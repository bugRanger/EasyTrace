using System.Diagnostics;
using System.Globalization;
using System.Text;
using EasyTrace.Activity;
using EasyTrace.Attribute;

namespace EasyTrace.Export.Otlp.Protobuf;

// --- Калькулятор размера Protobuf OTLP Span ---
public static class ProtobufCalculator
{
    private const int WireTypeVarInt = 0;
    private const int WireType64Bit = 1; // fixed64, double
    private const int WireTypeLengthDelimited = 2; // string, bytes, embedded messages
    private const int WireType32Bit = 5; // fixed32, float

    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Удобная перегрузка для прямого вызова с <see cref="System.Diagnostics.Activity"/>.
    /// </summary>
    public static int CalculateSpanSize(in TraceActivityRef activity)
    {
        var span = new TraceActivitySpanAdapter(activity);
        return CalculateSpanSize(ref span);
    }

    /// <summary>
    /// Удобная перегрузка для прямого вызова с <see cref="System.Diagnostics.Activity"/>.
    /// </summary>
    public static int CalculateSpanSize(System.Diagnostics.Activity? activity)
    {
        if (activity is null) return 0;
        var span = new ActivitySpanAdapter(activity);
        return CalculateSpanSize(ref span);
    }

    /// <summary>
    /// Вычисляет точный размер сообщения opentelemetry.proto.trace.v1.Span в байтах (proto3)
    /// для любой реализации ISpanData.
    /// </summary>
    private static int CalculateSpanSize<T>(ref T span)
        where T : ISpanData,
        allows ref struct
    {
        int size = 0;

        // 1. bytes trace_id = 1 (16 байт)
        if (span.TraceId != default)
        {
            size += ComputeTagSize(1, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(16);
        }

        // 2. bytes span_id = 2 (8 байт)
        if (span.SpanId != default)
        {
            size += ComputeTagSize(2, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(8);
        }

        // 3. string trace_state = 3
        if (!string.IsNullOrEmpty(span.TraceStateString))
        {
            size += ComputeStringFieldSize(3, span.TraceStateString);
        }

        // 4. bytes parent_span_id = 4 (8 байт)
        if (span.ParentSpanId != default)
        {
            size += ComputeTagSize(4, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(8);
        }

        // 5. string name = 5
        string name = span.DisplayName;
        if (!string.IsNullOrEmpty(name))
        {
            size += ComputeStringFieldSize(5, name);
        }

        // 6. SpanKind kind = 6
        int otelKind = MapSpanKind(span.Kind);
        if (otelKind != 0)
        {
            size += ComputeTagSize(6, WireTypeVarInt) + ComputeVarInt32Size((uint)otelKind);
        }

        // 7. fixed64 start_time_unix_nano = 7
        ulong startUnixNano = ToUnixTimeNanoseconds(span.StartTimeUtc);
        if (startUnixNano != 0)
        {
            size += ComputeTagSize(7, WireType64Bit) + 8;
        }

        // 8. fixed64 end_time_unix_nano = 8
        if (span.Duration > TimeSpan.Zero)
        {
            ulong endUnixNano = ToUnixTimeNanoseconds(span.StartTimeUtc + span.Duration);
            if (endUnixNano != 0)
            {
                size += ComputeTagSize(8, WireType64Bit) + 8;
            }
        }

        // 9. repeated opentelemetry.proto.common.v1.KeyValue attributes = 9
        foreach (var tag in span.TagObjects)
        {
            int kvSize = ComputeKeyValueSize(tag.Key, tag.Value);
            size += ComputeTagSize(9, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(kvSize);
        }

        // 11. repeated Event events = 11
        foreach (var ev in span.Events)
        {
            int eventSize = ComputeEventSize(ev);
            size += ComputeTagSize(11, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(eventSize);
        }

        // 13. repeated Link links = 13
        foreach (var link in span.Links)
        {
            int linkSize = ComputeLinkSize(link);
            size += ComputeTagSize(13, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(linkSize);
        }

        // 15. Status status = 15
        int statusSize = ComputeStatusSize(span.Status, span.StatusDescription);
        if (statusSize > 0)
        {
            size += ComputeTagSize(15, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(statusSize);
        }

        // 16. fixed32 flags = 16
        uint flags = ComputeSpanFlags(span.ActivityTraceFlags, span.HasRemoteParent);
        if (flags != 0)
        {
            size += ComputeTagSize(16, WireType32Bit) + 4;
        }

        return size;
    }

    // --- Вложенные сообщения OTLP ---

    private static int ComputeEventSize(ActivityEvent ev)
    {
        int size = 0;

        // 1. fixed64 time_unix_nano = 1
        ulong timeNano = ToUnixTimeNanoseconds(ev.Timestamp.UtcDateTime);
        if (timeNano != 0)
        {
            size += ComputeTagSize(1, WireType64Bit) + 8;
        }

        // 2. string name = 2
        if (!string.IsNullOrEmpty(ev.Name))
        {
            size += ComputeStringFieldSize(2, ev.Name);
        }

        // 3. repeated KeyValue attributes = 3
        foreach (var tag in ev.Tags)
        {
            int kvSize = ComputeKeyValueSize(tag.Key, tag.Value);
            size += ComputeTagSize(3, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(kvSize);
        }

        return size;
    }

    private static int ComputeLinkSize(ActivityLink link)
    {
        int size = 0;

        // 1. bytes trace_id = 1 (16 байт)
        if (link.Context.TraceId != default)
        {
            size += ComputeTagSize(1, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(16);
        }

        // 2. bytes span_id = 2 (8 байт)
        if (link.Context.SpanId != default)
        {
            size += ComputeTagSize(2, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(8);
        }

        // 3. string trace_state = 3
        if (!string.IsNullOrEmpty(link.Context.TraceState))
        {
            size += ComputeStringFieldSize(3, link.Context.TraceState);
        }

        // 4. repeated KeyValue attributes = 4
        if (link.Tags is not null)
        {
            foreach (var tag in link.Tags)
            {
                int kvSize = ComputeKeyValueSize(tag.Key, tag.Value);
                size += ComputeTagSize(4, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(kvSize);
            }
        }

        // 6. fixed32 flags = 6
        uint flags = ComputeSpanFlags(link.Context.TraceFlags, link.Context.IsRemote);
        if (flags != 0)
        {
            size += ComputeTagSize(6, WireType32Bit) + 4;
        }

        return size;
    }

    private static int ComputeStatusSize(ActivityStatusCode status, string? description)
    {
        int code = status switch
        {
            ActivityStatusCode.Ok => 1,
            ActivityStatusCode.Error => 2,
            _ => 0
        };

        if (code == 0 && string.IsNullOrEmpty(description))
            return 0;

        int size = 0;

        // 2. string message = 2
        if (!string.IsNullOrEmpty(description))
        {
            size += ComputeStringFieldSize(2, description);
        }

        // 3. StatusCode code = 3
        if (code != 0)
        {
            size += ComputeTagSize(3, WireTypeVarInt) + ComputeVarInt32Size((uint)code);
        }

        return size;
    }

    private static int ComputeKeyValueSize(string key, object? value)
    {
        int size = 0;

        // 1. string key = 1
        if (!string.IsNullOrEmpty(key))
        {
            size += ComputeStringFieldSize(1, key);
        }

        // 2. AnyValue value = 2
        if (value is not null)
        {
            int anyValueSize = ComputeAnyValueSize(value);
            size += ComputeTagSize(2, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(anyValueSize);
        }

        return size;
    }

    private static int ComputeAnyValueSize(object value)
    {
        switch (value)
        {
            case string s:
                return s.Length == 0 ? 0 : ComputeStringFieldSize(1, s);
            case bool:
                return ComputeTagSize(2, WireTypeVarInt) + 1;
            case int or long or short or sbyte or byte or uint or ushort or ulong:
                long longVal = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                return ComputeTagSize(3, WireTypeVarInt) + ComputeVarInt64Size((ulong)longVal);
            case double or float or decimal:
                return ComputeTagSize(4, WireType64Bit) + 8;
            case Array array:
                int arrayInnerSize = 0;
                foreach (var item in array)
                {
                    int elemSize = item is null ? 0 : ComputeAnyValueSize(item);
                    arrayInnerSize += ComputeTagSize(1, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(elemSize);
                }

                return ComputeTagSize(5, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(arrayInnerSize);
            case ISpanFormattable formattable:
                Span<char> buffer = stackalloc char[128];
                if (formattable.TryFormat(buffer, out int written, default, CultureInfo.InvariantCulture))
                {
                    if (written == 0) return 0;
                    int utf8Bytes = Encoding.UTF8.GetByteCount(buffer[..written]);
                    return ComputeTagSize(1, WireTypeLengthDelimited) + ComputeLengthDelimitedSize(utf8Bytes);
                }

                break;
        }

        string? fallback = value.ToString();
        return string.IsNullOrEmpty(fallback) ? 0 : ComputeStringFieldSize(1, fallback);
    }

    // --- Маппинг и низкоуровневые примитивы ---
    private static int MapSpanKind(ActivityKind kind) => kind switch
    {
        ActivityKind.Internal => 1,
        ActivityKind.Server => 2,
        ActivityKind.Client => 3,
        ActivityKind.Producer => 4,
        ActivityKind.Consumer => 5,
        _ => 0
    };

    private static uint ComputeSpanFlags(ActivityTraceFlags traceFlags, bool isRemote)
    {
        uint flags = (uint)traceFlags & 0x000000FFu;
        flags |= 0x00000100u;
        if (isRemote)
        {
            flags |= 0x00000200u;
        }

        return flags;
    }

    private static ulong ToUnixTimeNanoseconds(DateTime utcDateTime)
    {
        if (utcDateTime <= UnixEpoch) return 0;
        return (ulong)(utcDateTime.Ticks - UnixEpoch.Ticks) * 100ul;
    }

    private static int ComputeStringFieldSize(int fieldNumber, string value)
    {
        int utf8ByteCount = Encoding.UTF8.GetByteCount(value);
        return ComputeTagSize(fieldNumber, WireTypeLengthDelimited)
               + ComputeLengthDelimitedSize(utf8ByteCount);
    }

    private static int ComputeTagSize(int fieldNumber, int wireType)
    {
        uint key = (uint)((fieldNumber << 3) | wireType);
        return ComputeVarInt32Size(key);
    }

    private static int ComputeLengthDelimitedSize(int payloadLength)
    {
        return ComputeVarInt32Size((uint)payloadLength) + payloadLength;
    }

    private static int ComputeVarInt32Size(uint value)
    {
        if (value < (1u << 7)) return 1;
        if (value < (1u << 14)) return 2;
        if (value < (1u << 21)) return 3;
        if (value < (1u << 28)) return 4;
        return 5;
    }

    private static int ComputeVarInt64Size(ulong value)
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

// --- 1. Абстракции для данных спана ---
public interface ISpanData
{
    ActivityTraceId TraceId { get; }
    ActivitySpanId SpanId { get; }
    ActivitySpanId ParentSpanId { get; }
    string? TraceStateString { get; }
    string DisplayName { get; }
    ActivityKind Kind { get; }
    DateTime StartTimeUtc { get; }
    TimeSpan Duration { get; }
    ActivityTraceFlags ActivityTraceFlags { get; }
    bool HasRemoteParent { get; }
    ActivityStatusCode Status { get; }
    string? StatusDescription { get; }
    IEnumerable<KeyValuePair<string, object?>> TagObjects { get; }
    IEnumerable<ActivityEvent> Events { get; }
    IEnumerable<ActivityLink> Links { get; }
}

// --- 1.1 Адаптер для System.Diagnostics.Activity ---
internal readonly ref struct TraceActivitySpanAdapter : ISpanData
{
    public TraceActivitySpanAdapter(TraceActivityRef activity)
    {
        TraceId = activity.TraceId.IsEmpty
            ? default
            : ActivityTraceId.CreateFromBytes(activity.TraceId.AsReadOnlySpan());
        SpanId = activity.SpanId.IsEmpty
            ? default
            : ActivitySpanId.CreateFromBytes(activity.SpanId.AsReadOnlySpan());
        ParentSpanId = activity.ParentId.IsEmpty
            ? default
            : ActivitySpanId.CreateFromBytes(activity.ParentId.AsReadOnlySpan());
        TraceStateString = null;
        DisplayName = activity.OperationName;
        Kind = activity.Kind;
        StartTimeUtc = activity.StartTime;
        Duration = activity.Duration;
        ActivityTraceFlags = activity.Recorded ? ActivityTraceFlags.Recorded : ActivityTraceFlags.None;
        HasRemoteParent = activity.RemoteParent;
        Status = ActivityStatusCode.Unset;
        StatusDescription = null;
        TagObjects = new Dictionary<string, object?>(
            activity.Attributes
                .AsSpan()
                .ToArray()
                .Select(s =>
                {
                    var valueTyped = s.Value.Kind switch
                    {
                        TraceAttributeKind.String => (object?)s.Value.AsChars(),
                        TraceAttributeKind.Double => (object?)s.Value.AsDouble(),
                        TraceAttributeKind.Long => (object?)s.Value.AsLong(),
                        TraceAttributeKind.Boolean => (object?)s.Value.AsBool(),
                        _ => null,
                    };

                    return KeyValuePair.Create(s.Name.ToString(), valueTyped);
                }));
        Events = [];
        Links = [];
    }

    public ActivityTraceId TraceId { get; }
    public ActivitySpanId SpanId { get; }
    public ActivitySpanId ParentSpanId { get; }
    public string? TraceStateString { get; }
    public string DisplayName { get; }
    public ActivityKind Kind { get; }
    public DateTime StartTimeUtc { get; }
    public TimeSpan Duration { get; }
    public ActivityTraceFlags ActivityTraceFlags { get; }
    public bool HasRemoteParent { get; }
    public ActivityStatusCode Status { get; }
    public string? StatusDescription { get; }
    public IEnumerable<KeyValuePair<string, object?>> TagObjects { get; }
    public IEnumerable<ActivityEvent> Events { get; }
    public IEnumerable<ActivityLink> Links { get; }
}

// --- 1.2 Адаптер для System.Diagnostics.Activity ---
internal readonly ref struct ActivitySpanAdapter(System.Diagnostics.Activity activity) : ISpanData
{
    public ActivityTraceId TraceId => activity.TraceId;
    public ActivitySpanId SpanId => activity.SpanId;
    public ActivitySpanId ParentSpanId => activity.ParentSpanId;
    public string? TraceStateString => activity.TraceStateString;
    public string DisplayName => activity.DisplayName;
    public ActivityKind Kind => activity.Kind;
    public DateTime StartTimeUtc => activity.StartTimeUtc;
    public TimeSpan Duration => activity.Duration;
    public ActivityTraceFlags ActivityTraceFlags => activity.ActivityTraceFlags;
    public bool HasRemoteParent => activity.HasRemoteParent;
    public ActivityStatusCode Status => activity.Status;
    public string? StatusDescription => activity.StatusDescription;
    public IEnumerable<KeyValuePair<string, object?>> TagObjects => activity.TagObjects;
    public IEnumerable<ActivityEvent> Events => activity.Events;
    public IEnumerable<ActivityLink> Links => activity.Links;
}