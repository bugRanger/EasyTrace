namespace EasyTrace.Attribute;

public sealed class TraceAttribute(int nameCapacity, int valueCapacity)
{
    private TraceAttributeField _name = new(nameCapacity);
    private TraceAttributeField _value = new(valueCapacity);

    public ReadOnlySpan<char> Name => _name.PackInChars();

    public TraceAttributeField Value => _value;

    internal void Write(in TraceAttributeFieldRef name, in TraceAttributeFieldRef field)
    {
        _name.Write(name);
        _value.Write(field);
    }

    internal void Clear()
    {
        _name.Clear();
        _value.Clear();
    }
}