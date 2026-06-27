using System.Diagnostics;

namespace EasyTrace.Time;

public class TraceTimeProvider : ITraceTimeProvider
{
    private readonly DateTime _startTimeInUtc = DateTime.UtcNow;
    private readonly long _startTimestamp = Stopwatch.GetTimestamp();

    public TimeSpan GetTimestamp() => Stopwatch.GetElapsedTime(_startTimestamp, Stopwatch.GetTimestamp());

    private DateTime ToDateTime(TimeSpan duration) => _startTimeInUtc.Add(duration);
}