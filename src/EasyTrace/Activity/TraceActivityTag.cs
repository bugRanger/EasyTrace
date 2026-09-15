namespace EasyTrace.Activity;

public sealed class TraceActivityTag(int nameCapacity, int valueCapacity)
{
    private readonly char[] _name = new char[nameCapacity];
    private int _nameLen;
    private readonly char[] _value = new char[valueCapacity];
    private int _valueLen;

    public ReadOnlySpan<char> Name => _name.AsSpan(0, _nameLen);

    public ReadOnlySpan<char> Value => _value.AsSpan(0, _valueLen);

    internal void Write(ReadOnlySpan<char> name, ReadOnlySpan<char> value)
    {
        var nameLen = Math.Min(name.Length, nameCapacity);
        name[..nameLen].CopyTo(_name);
        _nameLen = nameLen;
        var valueLen = Math.Min(value.Length, valueCapacity);
        value[..valueLen].CopyTo(_value);
        _valueLen = valueLen;
    }

    internal void Clear()
    {
        _nameLen = 0;
        _valueLen = 0;
    }
}