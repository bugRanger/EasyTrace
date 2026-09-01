using System.Diagnostics;
using System.Runtime.CompilerServices;
using EasyTrace.Activity;
using EasyTrace.Export;
using EasyTrace.Identifier;
using EasyTrace.Identifier.Generator;
using EasyTrace.Interceptor;
using EasyTrace.Time;

namespace EasyTrace;

public class TraceActivitySource(string name, Version? version = null) : IDisposable
{
    private static readonly ThreadLocal<TraceActivity?> ParentActivityByThreadLocal = new();

    internal static readonly TraceActivitySource Empty = new(nameof(Empty));

    internal ITraceTimeProvider TimeProvider { get; init; } = new TraceTimeProvider();
    internal ITraceIdentifierGenerator IdentifierGenerator { get; init; } = new Xoshiro256PlusPlus();
    internal KeyValuePair<string, string>[] Resources { get; init; } = [];
    internal GroupExporter? GroupExporter { get; set; }
    internal GroupInterceptor? GroupInterceptor { get; set; }

    private bool _disposed;

    private static TraceActivity? Parent
    {
        get => ParentActivityByThreadLocal.Value;
        set => ParentActivityByThreadLocal.Value = value;
    }

    public string Name { get; } = name;

    public string? Version { get; } = version?.ToString();

    public TraceActivityScope? Start(
        [CallerMemberName] string operationName = "",
        ActivityKind kind = ActivityKind.Internal)
    {
        if (GroupExporter == null)
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

        Parent ??= activity;

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

            scoped var activityRef = new TraceActivityRef(activity);

            GroupInterceptor?.Stop(in activityRef);
            GroupExporter?.Handle(in activityRef);

            if (Parent == activity)
            {
                Parent = null;
            }

            activity.Clear();
        }
        finally
        {
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
            GroupExporter?.Dispose();
            GroupExporter = null;
            GroupInterceptor = null;
        }

        _disposed = true;
    }
}