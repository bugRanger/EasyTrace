using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace EasyTrace.Export.Otlp.Protobuf;

public class ProtobufStream(int capacity)
{
    private static readonly Encoding Utf8Encoding = Encoding.UTF8;

    private const uint UInt128 = 0x80;
    private const int Fixed32Size = 4;
    private const int Fixed64Size = 8;
    private const int MaskBitsLow = 0b_0111_1111;
    private const int MaskBitHigh = 0b_1000_0000;

    private readonly byte[] _buffer = new byte[capacity];

    public int Position { get; private set; }

    public int Length => _buffer.Length;

    public void Reset() => Position = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reserve(int length) => Position += length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteTag(int fieldNumber, ProtobufWireType type) =>
        WriteVarInt32(GetTagValue(fieldNumber, type));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLength(int position, int length)
    {
        var slice = _buffer.AsSpan(position, 4);
        slice[0] = (byte)((length & MaskBitsLow) | MaskBitHigh);
        slice[1] = (byte)(((length >> 7) & MaskBitsLow) | MaskBitHigh);
        slice[2] = (byte)(((length >> 14) & MaskBitsLow) | MaskBitHigh);
        slice[3] = (byte)((length >> 21) & MaskBitsLow);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteEnumWithTag(int fieldNumber, int value)
    {
        WriteTag(fieldNumber, ProtobufWireType.VarInt);
        _buffer[Position++] = (byte)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixed32WithTag(int fieldNumber, uint value)
    {
        WriteTag(fieldNumber, ProtobufWireType.I32);
        WriteFixed32LittleEndianFormat(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFixed64WithTag(int fieldNumber, ulong value)
    {
        WriteTag(fieldNumber, ProtobufWireType.I64);
        WriteFixed64LittleEndianFormat(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByteArrayWithTag(int fieldNumber, ReadOnlySpan<byte> value)
    {
        WriteTag(fieldNumber, ProtobufWireType.Len);
        WriteLength(value.Length);
        value.CopyTo(_buffer.AsSpan(Position));
        Position += value.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteStringWithTag(int fieldNumber, string value)
    {
        Debug.Assert(value != null, "value was null");
        WriteStringWithTag(fieldNumber, value.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteStringWithTag(int fieldNumber, ReadOnlySpan<char> value)
    {
        var numberOfUtf8CharsInString = GetNumberOfUtf8CharsInString(value);
        WriteStringWithTag(fieldNumber, numberOfUtf8CharsInString, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint GetTagValue(int fieldNumber, ProtobufWireType wireType) =>
        ((uint)(fieldNumber << 3)) | (uint)wireType;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteVarInt32(uint value)
    {
        while (value >= UInt128)
        {
            _buffer[Position++] = (byte)(MaskBitHigh | (value & MaskBitsLow));
            value >>= 7;
        }

        _buffer[Position++] = (byte)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteLength(int length) => WriteVarInt32((uint)length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteFixed32LittleEndianFormat(uint value)
    {
        Span<byte> span = new(_buffer, Position, Fixed32Size);
        BinaryPrimitives.WriteUInt32LittleEndian(span, value);
        Position += Fixed32Size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteFixed64LittleEndianFormat(ulong value)
    {
        Span<byte> span = new(_buffer, Position, Fixed64Size);
        BinaryPrimitives.WriteUInt64LittleEndian(span, value);
        Position += Fixed64Size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetNumberOfUtf8CharsInString(ReadOnlySpan<char> value)
    {
        return Utf8Encoding.GetByteCount(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteStringWithTag(
        int fieldNumber,
        int numberOfUtf8CharsInString,
        ReadOnlySpan<char> value)
    {
        WriteTag(fieldNumber, ProtobufWireType.Len);
        WriteLength(numberOfUtf8CharsInString);

        var bytesWritten = Utf8Encoding.GetBytes(value, _buffer.AsSpan(Position));
        Debug.Assert(bytesWritten == numberOfUtf8CharsInString, "bytesWritten did not match numberOfUtf8CharsInString");
        Position += bytesWritten;
    }
}