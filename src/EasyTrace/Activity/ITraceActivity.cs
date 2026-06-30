using System.Diagnostics;
using EasyTrace.Identifier;

namespace EasyTrace.Activity;

public interface ITraceActivity
{
    TraceIdentifier TraceId { get; }
    TraceIdentifier SpanId { get; }
    TraceActivitySource Source { get; }
    string OperationName { get; }
    ActivityKind Kind { get; }
    TimeSpan StartTime { get; }
    TimeSpan EndTime { get; }
    TimeSpan Duration { get; }
}