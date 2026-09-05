using System.Diagnostics;
using EasyTrace.Export.Batch.Buffer;
using EasyTrace.Identifier;

namespace EasyTrace.Activity;

public class TraceActivity : ITraceActivity, ICopiable<TraceActivity>
{
    internal static readonly TraceActivity Empty = new();

    public TraceIdentifier TraceId { get; } = TraceIdentifier.CreateTraceId();
    public TraceIdentifier SpanId { get; } = TraceIdentifier.CreateSpanId();
    public TraceIdentifier ParentId { get; } = TraceIdentifier.CreateSpanId();
    public TraceActivitySource Source { get; internal set; } = TraceActivitySource.Empty;
    public string OperationName { get; set; } = string.Empty;
    public ActivityKind Kind { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public bool Recorded { get; set; }
    public bool RemoteParent { get; set; }
    public TraceActivity? Parent { get; set; }
    
    public void Clear()
    {
        OperationName = string.Empty;
        StartTime = DateTime.MinValue;
        EndTime = DateTime.MinValue;
        ParentId.Clear();
        Parent = null;
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
        destination.ParentId.CopyFrom(ParentId);
        destination.StartTime = StartTime;
        destination.EndTime = EndTime;
        destination.Recorded = Recorded;
        destination.RemoteParent = RemoteParent;
        destination.Kind = Kind;
    }
}