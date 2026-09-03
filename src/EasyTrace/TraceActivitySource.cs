using System.Diagnostics;
using System.Runtime.CompilerServices;
using EasyTrace.Activity;
using EasyTrace.Export;
using EasyTrace.Export.Batch;
using EasyTrace.Identifier;
using EasyTrace.Identifier.Generator;
using EasyTrace.Interceptor;
using EasyTrace.Time;

namespace EasyTrace;

public class TraceActivitySource(string name, Version? version = null) : IDisposable
{
    private readonly ThreadLocal<TraceActivity?> _parentActivityByThreadLocal = new();

    internal static readonly TraceActivitySource Empty = new(nameof(Empty));

    internal ITraceTimeProvider TimeProvider { get; init; } = new TraceTimeProvider();
    internal ITraceIdentifierGenerator IdentifierGenerator { get; init; } = new Xoshiro256PlusPlus();
    internal KeyValuePair<string, string>[] Resources { get; init; } = [];
    internal BatchExporter<ITraceActivityExporter>? BatchExporter { get; set; }
    internal GroupInterceptor? GroupInterceptor { get; set; }

    private bool _disposed;

    private TraceActivity? Parent
    {
        get => _parentActivityByThreadLocal.Value;
        set => _parentActivityByThreadLocal.Value = value;
    }

    public string Name { get; } = name;

    public string? Version { get; } = version?.ToString();

    public TraceActivityScope? Start(
        [CallerMemberName] string operationName = "",
        ActivityKind kind = ActivityKind.Internal)
    {
        if (BatchExporter == null)
        {
            return null;
        }

        var activity = TraceActivityPool.Shared.Rent();

        if (Parent == null)
        {
            activity.TraceId.Generate(IdentifierGenerator);
        }
        else
        {
            activity.Parent = Parent;
            activity.ParentId.CopyFrom(Parent.SpanId);
            activity.TraceId.CopyFrom(Parent.TraceId);
        }

        activity.SpanId.Generate(IdentifierGenerator);
        activity.Source = this;
        activity.Kind = kind;
        activity.OperationName = operationName;
        activity.StartTime = TimeProvider.GetDateTime();
        activity.Recorded = true;
        // TODO: Support mark if parent is remote.
        activity.RemoteParent = false;

        Parent = activity;

        scoped var activityRef = new TraceActivityRef(activity);
        GroupInterceptor?.Start(activityRef);

        return new TraceActivityScope(activity);
    }

    public void Stop(TraceActivity activity)
    {
        try
        {
            if (activity.EndTime == DateTime.MinValue)
            {
                activity.EndTime = TimeProvider.GetDateTime();
            }

            Parent = activity.Parent;
            scoped var activityRef = new TraceActivityRef(activity);

            GroupInterceptor?.Stop(in activityRef);
            BatchExporter?.Handle(in activityRef);
        }
        finally
        {
            activity.Clear();
            TraceActivityPool.Shared.Return(activity);
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