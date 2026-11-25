using System.Runtime.CompilerServices;
using EasyTrace.Attribute;

namespace EasyTrace.Activity;

public struct TraceActivityScope(TraceActivity? activity) : IDisposable
{
    private TraceActivity? _activity = activity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetAttribute(in TraceAttributeFieldRef name, in TraceAttributeFieldRef field)
    {
        _activity?.Attributes.Add(name, field);
    }

    public void Dispose()
    {
        var local = Interlocked.Exchange(ref _activity, null);
        local?.Source.Stop(local);
    }
}