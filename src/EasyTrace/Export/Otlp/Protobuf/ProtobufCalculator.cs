using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace EasyTrace.Export.Otlp.Protobuf;

// --- Калькулятор размера Protobuf OTLP Span ---
public class ProtobufCalculator
{
    private const int HeaderSize = 5;
    public readonly int WireTypeVarInt = 0;
    public readonly int WireType64Bit = 1; // fixed64, double
    public readonly int WireTypeLengthDelimited = 2; // string, bytes, embedded messages
    public readonly int WireType32Bit = 5; // fixed32, float

    public static ProtobufCalculator Instance { get; } = new();

    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Вычисляет точный размер сообщения opentelemetry.proto.trace.v1.Span в байтах (proto3)
    /// для любой реализации ISpanData.
    /// </summary>
    public int Calculate<T>(ref T span)
        where T : IProtobufSpan, allows ref struct
    {
        var size = 0;

        // 1. bytes trace_id = 1 (16 байт)
        if (!span.TraceId.IsEmpty)
        {
            size += ComputeTagSize(1, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(16);
        }

        // 2. bytes span_id = 2 (8 байт)
        if (!span.SpanId.IsEmpty)
        {
            size += ComputeTagSize(2, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(8);
        }

        // 3. string trace_state = 3
        if (!span.TraceStateString.IsEmpty)
        {
            size += ComputeStringFieldSize(3, span.TraceStateString);
        }

        // 4. bytes parent_span_id = 4 (8 байт)
        if (!span.ParentSpanId.IsEmpty)
        {
            size += ComputeTagSize(4, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(8);
        }

        // 5. string name = 5
        if (!span.Name.IsEmpty)
        {
            size += ComputeStringFieldSize(5, span.Name);
        }

        // 6. SpanKind kind = 6
        var kind = MapSpanKind(span.Kind);
        if (kind != 0)
        {
            size += ComputeTagSize(6, WireTypeVarInt) + ComputeVarInt32Size(kind);
        }

        // 7. fixed64 start_time_unix_nano = 7
        var startUnixNano = ToUnixTimeNanoseconds(span.StartTime);
        if (startUnixNano != 0)
        {
            size += ComputeTagSize(7, WireType64Bit) + 8;
        }

        // 8. fixed64 end_time_unix_nano = 8
        if (span.Duration > TimeSpan.Zero)
        {
            var endUnixNano = ToUnixTimeNanoseconds(span.StartTime + span.Duration);
            if (endUnixNano != 0)
            {
                size += ComputeTagSize(8, WireType64Bit) + 8;
            }
        }

        // 9. repeated opentelemetry.proto.common.v1.KeyValue attributes = 9
        size += span.ComputeAttributes(this);

        // 11. repeated Event events = 11
        size += span.ComputeEvents(this);

        // 13. repeated Link links = 13
        size += span.ComputeLinks(this);

        // 15. Status status = 15
        var statusSize = ComputeStatusSize(span.Status, span.StatusDescription);
        if (statusSize > 0)
        {
            size += ComputeTagSize(15, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(statusSize);
        }

        // 16. fixed32 flags = 16
        var flags = ComputeSpanFlags(span.Flags, span.HasRemoteParent);
        if (flags != 0)
        {
            size += ComputeTagSize(16, WireType32Bit) + 4;
        }

        return HeaderSize + size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ComputeLink(
        ReadOnlySpan<byte> traceId,
        ReadOnlySpan<byte> spanId,
        ReadOnlySpan<char> traceState,
        ActivityTraceFlags flags,
        bool isRemote,
        int sizeTags)
    {
        var size = 0;

        // 1. bytes trace_id = 1 (16 байт)
        if (!traceId.IsEmpty)
        {
            size += ComputeTagSize(1, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(16);
        }

        // 2. bytes span_id = 2 (8 байт)
        if (!spanId.IsEmpty)
        {
            size += ComputeTagSize(2, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(8);
        }

        // 3. string trace_state = 3
        if (!traceState.IsEmpty)
        {
            size += ComputeStringFieldSize(3, traceState);
        }

        // 4. repeated KeyValue attributes = 4
        size += sizeTags;

        // 6. fixed32 flags = 6
        if (ComputeSpanFlags(flags, isRemote) != 0)
        {
            size += ComputeTagSize(6, WireType32Bit) + 4;
        }

        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ComputeEvent(DateTime dateTime, ReadOnlySpan<char> name, int sizeTags)
    {
        var size = 0;

        // 1. fixed64 time_unix_nano = 1
        var timeNano = ToUnixTimeNanoseconds(dateTime);
        if (timeNano != 0)
        {
            size += ComputeTagSize(1, WireType64Bit) + 8;
        }

        // 2. string name = 2
        if (!name.IsEmpty)
        {
            size += ComputeStringFieldSize(2, name);
        }

        // 3. repeated KeyValue attributes = 3
        size += sizeTags;

        size += ComputeTagSize(11, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(size);
        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ComputeAttribute(ReadOnlySpan<char> name, int valueSize)
    {
        if (name.IsEmpty) return 0;

        var size = ComputeStringFieldSize(1, name);
        if (valueSize > 0)
        {
            size += ComputeTagSize(2, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(valueSize);
        }

        size += ComputeTagSize(9, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(size);
        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ComputeStatusSize(ActivityStatusCode status, ReadOnlySpan<char> description)
    {
        var code = status switch
        {
            ActivityStatusCode.Ok => 1,
            ActivityStatusCode.Error => 2,
            _ => 0
        };

        if (code == 0 && description.IsEmpty)
        {
            return 0;
        }

        var size = 0;

        if (!description.IsEmpty)
        {
            size += ComputeStringFieldSize(2, description);
        }

        if (code != 0)
        {
            size += ComputeTagSize(3, WireTypeVarInt) + ComputeVarInt32Size((uint)code);
        }

        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint MapSpanKind(ActivityKind kind) => kind switch
    {
        ActivityKind.Internal => 1,
        ActivityKind.Server => 2,
        ActivityKind.Client => 3,
        ActivityKind.Producer => 4,
        ActivityKind.Consumer => 5,
        _ => 0
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint ComputeSpanFlags(ActivityTraceFlags traceFlags, bool isRemote)
    {
        uint flags = (uint)traceFlags & 0x000000FFu;
        flags |= 0x00000100u;
        if (isRemote)
        {
            flags |= 0x00000200u;
        }

        return flags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong ToUnixTimeNanoseconds(DateTime utcDateTime)
    {
        if (utcDateTime <= UnixEpoch) return 0;
        return (ulong)(utcDateTime.Ticks - UnixEpoch.Ticks) * 100ul;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ComputeStringFieldSize(int fieldNumber, ReadOnlySpan<char> value)
    {
        var utf8ByteCount = Encoding.UTF8.GetByteCount(value);
        return ComputeTagSize(fieldNumber, WireTypeLengthDelimited)
               + ComputeLengthDelimitedSize(utf8ByteCount) + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ComputeTagSize(int fieldNumber, int wireType)
    {
        uint key = (uint)((fieldNumber << 3) | wireType);
        return ComputeVarInt32Size(key);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ComputeLengthDelimitedSize(int payloadLength)
    {
        return ComputeVarInt32Size((uint)payloadLength) + payloadLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ComputeVarInt32Size(uint value)
    {
        if (value < (1u << 7)) return 1;
        if (value < (1u << 14)) return 2;
        if (value < (1u << 21)) return 3;
        if (value < (1u << 28)) return 4;
        return 5;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ComputeVarInt64Size(ulong value)
    {
        int size = 1;
        while (value >= 0x80)
        {
            value >>= 7;
            size++;
        }

        return size;
    }
}

public interface IProtobufSpan
{
    ReadOnlySpan<byte> TraceId { get; }
    ReadOnlySpan<byte> SpanId { get; }
    ReadOnlySpan<byte> ParentSpanId { get; }
    ReadOnlySpan<char> TraceStateString { get; }
    ReadOnlySpan<char> Name { get; }
    ActivityKind Kind { get; }
    DateTime StartTime { get; }
    TimeSpan Duration { get; }
    ActivityTraceFlags Flags { get; }
    bool HasRemoteParent { get; }
    ActivityStatusCode Status { get; }
    ReadOnlySpan<char> StatusDescription { get; }
    int ComputeAttributes(ProtobufCalculator calculator);
    int ComputeEvents(ProtobufCalculator calculator);
    int ComputeLinks(ProtobufCalculator calculator);
}