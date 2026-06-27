using System.Diagnostics;
using EasyTrace.Identifier;

namespace EasyTrace.Activity;

public readonly ref struct TraceActivityRef(TraceActivitySource activitySource, TraceActivity activity)
    : IDisposable, ITraceActivity
{
    public TraceIdentifier TraceId => activity.TraceId;
    public TraceIdentifier SpanId => activity.TraceId;
    public TraceActivitySource Source => activity.Source;

    public string OperationName
    {
        get => activity.OperationName;
        set => activity.OperationName = value;
    }

    public ActivityKind Kind
    {
        get => activity.Kind;
        set => activity.Kind = value;
    }

    public TimeSpan StartTime
    {
        get => activity.StartTime;
        set => activity.StartTime = value;
    }

    public TimeSpan EndTime
    {
        get => activity.EndTime;
        set => activity.EndTime = value;
    }

    public TimeSpan Duration => activity.Duration;

    public void Dispose()
    {
        if (activity == TraceActivity.Empty)
        {
            return;
        }

        activitySource.Stop(this, activity);
    }
}