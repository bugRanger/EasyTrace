namespace EasyTrace.Activity;

public interface ITraceActivityParent
{
    TraceActivity? Activity { get; set; }
}