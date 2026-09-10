using System.Runtime.InteropServices;

namespace EasyTrace.Attribute;

[StructLayout(LayoutKind.Explicit)]
public readonly ref struct TraceAttributeFieldRef
{
    [FieldOffset(0)] internal readonly TraceAttributeKind _kind;
    [FieldOffset(8)] internal readonly long _int = 0;
    [FieldOffset(8)] internal readonly double _double = 0;
    [FieldOffset(16)] internal readonly ReadOnlyMemory<char> _string = string.Empty.AsMemory();

    private TraceAttributeFieldRef(int value)
    {
        _kind = TraceAttributeKind.Int;
        _int = value;
    }

    private TraceAttributeFieldRef(double value)
    {
        _kind = TraceAttributeKind.Double;
        _double = value;
    }

    private TraceAttributeFieldRef(string value)
    {
        _kind = TraceAttributeKind.String;
        _string = value.AsMemory();
    }

    private TraceAttributeFieldRef(ReadOnlyMemory<char> value)
    {
        _kind = TraceAttributeKind.String;
        _string = value;
    }

    public static implicit operator TraceAttributeFieldRef(int value) => new(value);
    public static implicit operator TraceAttributeFieldRef(double value) => new(value);
    public static implicit operator TraceAttributeFieldRef(string value) => new(value);
    public static implicit operator TraceAttributeFieldRef(ReadOnlyMemory<char> value) => new(value);
}