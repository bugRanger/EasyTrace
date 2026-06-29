using System.Diagnostics;
using EasyTrace.Export.Batch.Buffer;
using EasyTrace.Identifier;

namespace EasyTrace.Activity;

public class TraceActivity : ITraceActivity, ICopiable<TraceActivity>
{
    internal static readonly TraceActivity Empty = new();

    public TraceIdentifier TraceId { get; } = TraceIdentifier.CreateTraceId();
    public TraceIdentifier SpanId { get; } = TraceIdentifier.CreateSpanId();
    public TraceActivitySource Source { get; internal set; }
    public string OperationName { get; set; }
    public ActivityKind Kind { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public bool Recorded { get; set; }

    public void Clear()
    {
        OperationName = string.Empty;
        StartTime = TimeSpan.Zero;
        EndTime = TimeSpan.Zero;
    }

    public void CopyFrom(TraceActivity source)
    {
        source.CopyTo(this);
    }

    public void CopyTo(TraceActivity destination)
    {
        destination.Source = Source;
        destination.OperationName = OperationName;
        destination.TraceId.CopyFrom(TraceId);
        destination.SpanId.CopyFrom(SpanId);
        destination.StartTime = StartTime;
        destination.EndTime = EndTime;
        destination.Recorded = Recorded;
        destination.Kind = Kind;
    }
}