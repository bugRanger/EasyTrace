using System.Globalization;

namespace EasyTrace.Activity;

public readonly struct TraceActivityScope(TraceActivity activity) : IDisposable
{
    public void SetTag(ReadOnlySpan<char> name, ReadOnlySpan<char> value)
    {
        activity.Tags.Add(name, value);
    }
    
    public void SetTag(ReadOnlySpan<char> name, double value)
    {
        Span<char> stringValue = stackalloc char[32];
        if (value.TryFormat(stringValue, out _, "R", CultureInfo.InvariantCulture))
        {
            return;
        }

        activity.Tags.Add(name, stringValue);
    }

    public void SetTag(ReadOnlySpan<char> name, int value)
    {
        Span<char> stringValue = stackalloc char[11];
        if (value.TryFormat(stringValue, out _, provider: CultureInfo.InvariantCulture))
        {
            return;
        }

        activity.Tags.Add(name, stringValue);
    }

    public void Dispose()
    {
        activity.Source.Stop(activity);
    }
}