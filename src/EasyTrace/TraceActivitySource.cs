using System.Diagnostics;
using System.Runtime.CompilerServices;
using EasyTrace.Activity;
using EasyTrace.Export.Batch;
using EasyTrace.Identifier;
using EasyTrace.Interceptor;
using EasyTrace.Time;

namespace EasyTrace;

public class TraceActivitySource : IDisposable
{
    private bool _disposed;

    public string Name { get; init; } = null!;

    public string? Version { get; init; }

    internal ITraceActivityContext Context { get; init; } = null!;
    internal ITraceTimeProvider TimeProvider { get; init; } = null!;
    internal ITraceIdentifierGenerator IdentifierGenerator { get; init; } = null!;
    internal KeyValuePair<string, string>[] Resources { get; init; } = [];
    internal BatchExporter? BatchExporter { get; set; }
    internal GroupInterceptor? GroupInterceptor { get; set; }

    public TraceActivityScope Start(
        [CallerMemberName] string operationName = "",
        ActivityKind kind = ActivityKind.Internal)
    {
        if (BatchExporter is not { IsActive: true } && GroupInterceptor is not { IsActive: true })
        {
            return new TraceActivityScope(null);
        }

        var activity = Context.Pool.Rent();
        activity.Source = this;

        if (Context.Current == null)
        {
            activity.Parent = null;
            activity.ParentId.Clear();
            activity.TraceId.Generate(IdentifierGenerator);
        }
        else
        {
            activity.Parent = Context.Current;
            activity.ParentId.CopyFrom(Context.Current.SpanId);
            activity.TraceId.CopyFrom(Context.Current.TraceId);
        }

        activity.SpanId.Generate(IdentifierGenerator);
        activity.OperationName = operationName;
        activity.Kind = kind;
        activity.StartTime = TimeProvider.GetDateTime();
        activity.Recorded = true;
        // TODO: Support mark if parent is remote.
        activity.RemoteParent = false;

        Context.Current = activity;

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
            
            Context.Current = activity.Parent;
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
            Context.Pool.Return(activity);
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