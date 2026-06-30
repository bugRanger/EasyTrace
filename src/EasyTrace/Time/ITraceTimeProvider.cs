namespace EasyTrace.Time;

public interface ITraceTimeProvider
{
    TimeSpan GetTimestamp();
}