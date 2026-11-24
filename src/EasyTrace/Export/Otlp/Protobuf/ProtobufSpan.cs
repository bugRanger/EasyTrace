using System.Diagnostics;
using System.Globalization;
using EasyTrace.Activity;
using EasyTrace.Attribute;

namespace EasyTrace.Export.Otlp.Protobuf;

internal readonly ref struct ProtobufSpan(TraceActivityRef activity) : IProtobufSpan
{
    private readonly TraceAttributeList _attributeList = activity.Attributes;

    public ReadOnlySpan<byte> TraceId { get; } = activity.TraceId.AsReadOnlySpan();
    public ReadOnlySpan<byte> SpanId { get; } = activity.SpanId.AsReadOnlySpan();
    public ReadOnlySpan<byte> ParentSpanId { get; } = activity.ParentId.AsReadOnlySpan();
    public ReadOnlySpan<char> TraceStateString { get; } = null;
    public ReadOnlySpan<char> Name { get; } = activity.OperationName;
    public ActivityKind Kind { get; } = activity.Kind;
    public DateTime StartTime { get; } = activity.StartTime;
    public TimeSpan Duration { get; } = activity.Duration;

    public ActivityTraceFlags Flags { get; } =
        activity.Recorded ? ActivityTraceFlags.Recorded : ActivityTraceFlags.None;

    public bool HasRemoteParent { get; } = activity.RemoteParent;
    public ActivityStatusCode Status { get; } = ActivityStatusCode.Unset;
    public ReadOnlySpan<char> StatusDescription { get; } = null;

    public int ComputeAttributes(ProtobufCalculator calculator)
    {
        var size = 0;

        foreach (var attribute in _attributeList.AsSpan())
        {
            var valueSize = ComputeValueSize(calculator, attribute.Value);
            size += calculator.ComputeAttribute(attribute.Name, valueSize);
        }

        return size;
    }

    public int ComputeEvents(ProtobufCalculator calculator)
    {
        return 0;
    }

    public int ComputeLinks(ProtobufCalculator calculator)
    {
        return 0;
    }

    private static int ComputeValueSize(ProtobufCalculator calculator, TraceAttributeField value)
    {
        switch (value.Kind)
        {
            case TraceAttributeKind.String:
                return calculator.ComputeStringFieldSize(1, value.AsChars().Span);

            case TraceAttributeKind.Double:
                return calculator.ComputeTagSize(4, calculator.WireType64Bit) + 8;

            case TraceAttributeKind.Long:
                var longVal = Convert.ToInt64(value.AsLong(), CultureInfo.InvariantCulture);
                return calculator.ComputeTagSize(3, calculator.WireTypeVarInt) +
                       calculator.ComputeVarInt64Size((ulong)longVal);

            case TraceAttributeKind.Boolean:
                return calculator.ComputeTagSize(2, calculator.WireTypeVarInt) + 1;

            default:
                return 0;
        }
    }
}