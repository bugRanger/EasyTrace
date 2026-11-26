namespace EasyTrace.Activity;

/// <summary>
/// TODO: Write it.
/// </summary>
public interface ITraceActivityContext
{
    TraceActivity? Current { get; set; }
    ITraceActivityPool Pool { get; }
}