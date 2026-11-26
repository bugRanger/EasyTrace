namespace EasyTrace.Activity;

/// <summary>
/// <see cref="TraceActivity"/> pool to reduce GC load.
/// </summary>
public interface ITraceActivityPool
{
    TraceActivity Rent();

    void Return(TraceActivity item);
}