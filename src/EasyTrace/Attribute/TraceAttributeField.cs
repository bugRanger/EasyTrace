using System.Globalization;
using System.Runtime.InteropServices;

namespace EasyTrace.Attribute;

[StructLayout(LayoutKind.Explicit)]
public struct TraceAttributeField
{
    [FieldOffset(0)] private TraceAttributeKind _kind;
    [FieldOffset(4)] private int _charLength = 0;
    [FieldOffset(8)] private long _int = 0;
    [FieldOffset(8)] private double _double = 0;
    [FieldOffset(16)] private readonly char[] _chars;

    public TraceAttributeField(int capacity)
    {
        _chars = new char[capacity];
    }

    internal ReadOnlyMemory<char> Get()
    {
        switch (_kind)
        {
            case TraceAttributeKind.String:
                return _chars.AsMemory(0, _charLength);

            case TraceAttributeKind.Double:
                _double.TryFormat(_chars, out _charLength, "R", CultureInfo.InvariantCulture);
                _kind = TraceAttributeKind.String;
                return _chars.AsMemory(0, _charLength);

            case TraceAttributeKind.Int:
                _int.TryFormat(_chars, out _charLength, provider: CultureInfo.InvariantCulture);
                _kind = TraceAttributeKind.String;
                return _chars.AsMemory(0, _charLength);

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal void Write(in TraceAttributeFieldRef fieldRef)
    {
        switch (fieldRef._kind)
        {
            case TraceAttributeKind.String:
                var charLength = Math.Min(fieldRef._string.Length, _chars.Length);
                fieldRef._string[..charLength].CopyTo(_chars);
                _charLength = charLength;
                break;

            case TraceAttributeKind.Double:
                _double = fieldRef._double;
                break;

            case TraceAttributeKind.Int:
                _int = fieldRef._int;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        _kind = fieldRef._kind;
    }

    internal void Clear()
    {
        _kind = TraceAttributeKind.String;
        _charLength = 0;
    }
}