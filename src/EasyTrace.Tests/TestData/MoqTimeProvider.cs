using EasyTrace.Time;

namespace EasyTrace.Tests.TestData;

public class MoqTimeProvider(DateTime startTimeInUtc) : ITraceTimeProvider
{
    private long _ticks;
    private const long TickStep = 100;

    public TimeSpan GetTimestamp()
    {
        _ticks += TickStep;
        return new TimeSpan(_ticks);
    }
}