using EasyTrace.Time;

namespace EasyTrace.Tests.TestData;

public class MoqTimeProvider : ITraceTimeProvider
{
    private readonly DateTime _startTime = new (2000, 1, 1);

    private long _ticks;
    private const long TickStep = 100;

    public TimeSpan GetTimestamp()
    {
        _ticks += TickStep;
        return new TimeSpan(_ticks);
    }

    public DateTime GetDateTime() => _startTime.Add(GetTimestamp());
}