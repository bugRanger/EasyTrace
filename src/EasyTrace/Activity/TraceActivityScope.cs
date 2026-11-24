using System.Runtime.CompilerServices;
using EasyTrace.Attribute;

namespace EasyTrace.Activity;

public ref struct TraceActivityScope(TraceActivity? activity) : IDisposable
{
    private TraceActivity? _activity = activity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetAttribute(in TraceAttributeFieldRef name, in TraceAttributeFieldRef field)
    {
        _activity?.Attributes.Add(name, field);
    }

    public void Dispose()
    {
        var local = _activity;
        _activity = null;
        local?.Source.Stop(local);
    }
}