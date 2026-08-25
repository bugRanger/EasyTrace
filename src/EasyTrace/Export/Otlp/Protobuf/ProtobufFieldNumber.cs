namespace EasyTrace.Export.Otlp.Protobuf;

public class ProtobufFieldNumber
{
    public static readonly ProtobufFieldNumber Scope = new(1);
    public static readonly ProtobufFieldNumber Span = new(2);

    public static readonly ProtobufFieldNumber TraceId = new(1);
    public static readonly ProtobufFieldNumber SpanId = new(2);
    public static readonly ProtobufFieldNumber TraceState = new(3);
    public static readonly ProtobufFieldNumber ParentId = new(4);
    public static readonly ProtobufFieldNumber Name = new(5);
    public static readonly ProtobufFieldNumber Kind = new(6);
    public static readonly ProtobufFieldNumber StartTimeUnixNano = new(7);
    public static readonly ProtobufFieldNumber EndTimeUnixNano = new(8);
    public static readonly ProtobufFieldNumber Attributes = new(9);
    public static readonly ProtobufFieldNumber DroppedAttributesCount = new(10);
    public static readonly ProtobufFieldNumber Events = new(11);
    public static readonly ProtobufFieldNumber DroppedEventsCount = new(12);
    public static readonly ProtobufFieldNumber Links = new(13);
    public static readonly ProtobufFieldNumber DroppedLinksCount = new(14);
    public static readonly ProtobufFieldNumber Status = new(15);
    public static readonly ProtobufFieldNumber Flags = new(16);
    
    private readonly byte _value;

    private ProtobufFieldNumber(byte value)
    {
        _value = value;
    }

    public static implicit operator int(ProtobufFieldNumber fieldNumber)
    {
        return fieldNumber._value;
    }
}