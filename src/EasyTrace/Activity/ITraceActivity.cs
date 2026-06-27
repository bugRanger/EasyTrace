using System.Diagnostics;
using EasyTrace.Identifier;

namespace EasyTrace.Activity;

public interface ITraceActivity
{
    TraceIdentifier TraceId { get; }
    TraceIdentifier SpanId { get; }
    TraceActivitySource Source { get; }
    string OperationName { get; set; }
    ActivityKind Kind { get; set; }
    TimeSpan StartTime { get; set; }
    TimeSpan EndTime { get; set; }
    TimeSpan Duration { get; }
}