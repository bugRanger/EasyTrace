namespace EasyTrace.Export.Otlp.Protobuf;

/// <summary>
/// Wire types within protobuf encoding.
/// https://protobuf.dev/programming-guides/encoding/#structure.
/// </summary>
public enum ProtobufWireType : uint
{
    /// <summary>
    /// Variable-length integer.
    /// Used for int32, int64, uint32, uint64, sint32, sint64, bool, enum.
    /// </summary>
    VarInt = 0,

    /// <summary>
    /// A fixed-length 64-bit value.
    /// Used for fixed64, sfixed64, double.
    /// </summary>
    I64 = 1,

    /// <summary>
    /// A length-delimited value.
    /// Used for string, bytes, embedded messages, packed repeated fields.
    /// </summary>
    Len = 2,

    /// <summary>
    /// A fixed-length 32-bit value.
    /// Used for fixed32, sfixed32, float.
    /// </summary>
    I32 = 5,
}