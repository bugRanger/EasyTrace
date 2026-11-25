using System.Globalization;
using System.Runtime.InteropServices;

namespace EasyTrace.Attribute;

[StructLayout(LayoutKind.Explicit)]
public struct TraceAttributeField(int capacity)
{
    [FieldOffset(0)] private TraceAttributeKind _kind;
    [FieldOffset(4)] private int _charLength = 0;
    [FieldOffset(8)] private bool _bool = false;
    [FieldOffset(8)] private long _long = 0;
    [FieldOffset(8)] private double _double = 0;
    [FieldOffset(16)] private readonly char[] _chars = new char[capacity];

    public TraceAttributeKind Kind => _kind;

    public TraceAttributeFieldRef AsRef()
    {
        return _kind switch
        {
            TraceAttributeKind.String => _chars.AsSpan(0, _charLength),
            TraceAttributeKind.Double => _double,
            TraceAttributeKind.Long => _long,
            TraceAttributeKind.Boolean => _bool,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    internal ReadOnlyMemory<char> AsChars() => _chars.AsMemory(0, _charLength);

    internal double AsDouble() => _double;

    internal long AsLong() => _long;

    internal bool AsBool() => _bool;

    internal ReadOnlySpan<char> PackInChars()
    {
        switch (_kind)
        {
            case TraceAttributeKind.String:
                return _chars.AsSpan(0, _charLength);

            case TraceAttributeKind.Double:
                _double.TryFormat(_chars, out _charLength, "R", CultureInfo.InvariantCulture);
                _kind = TraceAttributeKind.String;
                return _chars.AsSpan(0, _charLength);

            case TraceAttributeKind.Long:
                _long.TryFormat(_chars, out _charLength, provider: CultureInfo.InvariantCulture);
                _kind = TraceAttributeKind.String;
                return _chars.AsSpan(0, _charLength);
            
            case TraceAttributeKind.Boolean:
                _bool.TryFormat(_chars, out _charLength);
                _kind = TraceAttributeKind.String;
                return _chars.AsSpan(0, _charLength);

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

            case TraceAttributeKind.Long:
                _long = fieldRef._long;
                break;
            
            case TraceAttributeKind.Boolean:
                _bool = fieldRef._bool;
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

    public override string ToString()
    {
        return _kind switch
        {
            TraceAttributeKind.String => _chars.AsSpan(0, _charLength).ToString(),
            TraceAttributeKind.Double => _double.ToString(CultureInfo.InvariantCulture),
            TraceAttributeKind.Long => _long.ToString(CultureInfo.InvariantCulture),
            TraceAttributeKind.Boolean => _bool.ToString(CultureInfo.InvariantCulture),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}