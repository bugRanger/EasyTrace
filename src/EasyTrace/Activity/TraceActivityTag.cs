namespace EasyTrace.Activity;

public sealed class TraceActivityTag
{
    // TODO: Add configure from in builder.
    private const int MaxStringLen = byte.MaxValue;
    private readonly char[] _name = new char[MaxStringLen];
    private int _nameLen;
    private readonly char[] _value = new char[MaxStringLen];
    private int _valueLen;

    public ReadOnlySpan<char> Name => _name.AsSpan(0, _nameLen);
    public ReadOnlySpan<char> Value => _value.AsSpan(0, _valueLen);

    internal void Write(ReadOnlySpan<char> name, ReadOnlySpan<char> value)
    {
        var nameLen = Math.Min(name.Length, MaxStringLen);
        name[..nameLen].CopyTo(_name);
        _nameLen = nameLen;
        var valueLen = Math.Min(value.Length, MaxStringLen);
        value[..valueLen].CopyTo(_value);
        _valueLen = valueLen;
    }

    internal void Clear()
    {
        _nameLen = 0;
        _valueLen = 0;
    }
}