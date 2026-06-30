using System.Diagnostics;
using EasyTrace.Export.Batch.Buffer;
using EasyTrace.Identifier;

namespace EasyTrace.Activity;

public readonly ref struct TraceActivityRef(TraceActivity activity)
    : ITraceActivity, ICopiable<TraceActivity>
{
    public TraceIdentifier TraceId => activity.TraceId;
    public TraceIdentifier SpanId => activity.SpanId;
    public TraceActivitySource Source => activity.Source;

    public string OperationName => activity.OperationName;

    public ActivityKind Kind => activity.Kind;

    public TimeSpan StartTime => activity.StartTime;

    public TimeSpan EndTime => activity.EndTime;

    public TimeSpan Duration => activity.Duration;

    void ICopiable<TraceActivity>.CopyFrom(TraceActivity source)
    {
        activity.CopyFrom(source);
    }

    void ICopiable<TraceActivity>.CopyTo(TraceActivity destination)
    {
        activity.CopyTo(destination);
    }
}