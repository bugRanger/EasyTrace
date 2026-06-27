using System.Diagnostics;
using EasyTrace.Identifier;

namespace EasyTrace.Activity;

public class TraceActivity : ITraceActivity
{
    public static readonly TraceActivity Empty = new ();
    
    public TraceIdentifier TraceId { get; } = TraceIdentifier.CreateTraceId();
    public TraceIdentifier SpanId { get; } = TraceIdentifier.CreateSpanId();
    public TraceActivitySource Source { get; set; }
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
}