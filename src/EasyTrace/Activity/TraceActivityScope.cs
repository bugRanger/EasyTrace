using EasyTrace.Attribute;

namespace EasyTrace.Activity;

public readonly struct TraceActivityScope(TraceActivity activity) : IDisposable
{
    public void SetAttribute(in TraceAttributeFieldRef name, in TraceAttributeFieldRef field)
    {
        activity.Attributes.Add(name, field);
    }

    public void Dispose()
    {
        activity.Source.Stop(activity);
    }
}