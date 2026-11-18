namespace EasyTrace.Activity;

public class TraceActivityParent
{
    private readonly ThreadLocal<TraceActivity?> _parentActivityByThreadLocal = new();
    
    public TraceActivity? Activity
    {
        get => _parentActivityByThreadLocal.Value;
        set => _parentActivityByThreadLocal.Value = value;
    }
}