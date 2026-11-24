using System.Diagnostics;
using System.Runtime.CompilerServices;
using EasyTrace.Activity;
using EasyTrace.Export.Batch;
using EasyTrace.Identifier;
using EasyTrace.Identifier.Generator;
using EasyTrace.Interceptor;
using EasyTrace.Time;

namespace EasyTrace;

public class TraceActivitySource(string name, Version? version = null) : IDisposable
{
    private readonly ThreadLocal<TraceActivityPool> _poolActivityByThreadLocal = new();

    internal static readonly TraceActivitySource Empty = new(nameof(Empty));

    internal TraceActivityParent Parent { get; init; } = new();
    internal TraceActivityFactory Factory { get; init; } = new(new TraceActivityLimits());
    internal ITraceTimeProvider TimeProvider { get; init; } = new TraceTimeProvider();
    internal ITraceIdentifierGenerator IdentifierGenerator { get; init; } = new Xoshiro256PlusPlus();
    internal KeyValuePair<string, string>[] Resources { get; init; } = [];
    internal BatchExporter? BatchExporter { get; set; }
    internal GroupInterceptor? GroupInterceptor { get; set; }

    private bool _disposed;

    private TraceActivityPool Pool
    {
        get
        {
            _poolActivityByThreadLocal.Value ??= new TraceActivityPool(256, Factory);
            return _poolActivityByThreadLocal.Value;
        }
    }

    public string Name { get; } = name;

    public string? Version { get; } = version?.ToString();

    public TraceActivityScope Start(
        [CallerMemberName] string operationName = "",
        ActivityKind kind = ActivityKind.Internal)
    {
        if (this == Empty || BatchExporter is not { IsActive: true } && GroupInterceptor is not { IsActive: true })
        {
            return new TraceActivityScope(null);   
        }

        var activity = Pool.Rent();

        if (Parent.Activity == null)
        {
            activity.Parent = null;
            activity.ParentId.Clear();
            activity.TraceId.Generate(IdentifierGenerator);
        }
        else
        {
            activity.Parent = Parent.Activity;
            activity.ParentId.CopyFrom(Parent.Activity.SpanId);
            activity.TraceId.CopyFrom(Parent.Activity.TraceId);
        }

        activity.SpanId.Generate(IdentifierGenerator);
        activity.OperationName = operationName;
        activity.Source = this;
        activity.Kind = kind;
        activity.StartTime = TimeProvider.GetDateTime();
        activity.Recorded = true;
        // TODO: Support mark if parent is remote.
        activity.RemoteParent = false;

        Parent.Activity = activity;

        scoped var activityRef = new TraceActivityRef(activity);
        GroupInterceptor?.Start(activityRef);

        return new TraceActivityScope(activity);
    }

    public void Stop(TraceActivity activity)
    {
        if (this == Empty)
        {
            return;
        }

        try
        {
            if (activity.EndTime == DateTime.MinValue)
            {
                activity.EndTime = TimeProvider.GetDateTime();
            }

            Parent.Activity = activity.Parent;
            scoped var activityRef = new TraceActivityRef(activity);

            GroupInterceptor?.Stop(in activityRef);

            if (BatchExporter is { IsActive: true })
            {
                BatchExporter?.Handle(in activityRef);
            }
        }
        finally
        {
            activity.Clear();
            Pool.Return(activity);
        }
    }

    ~TraceActivitySource()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            BatchExporter?.Dispose();
            BatchExporter = null;
            GroupInterceptor = null;
        }

        _disposed = true;
    }
}