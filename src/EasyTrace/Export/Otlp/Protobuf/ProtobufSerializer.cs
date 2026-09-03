using System.Diagnostics;
using System.Runtime.CompilerServices;
using EasyTrace.Activity;

namespace EasyTrace.Export.Otlp.Protobuf;

public class ProtobufSerializer
{
    protected readonly ProtobufStream Stream;
    private readonly int _traceLengthPosition;
    private readonly int _spansLengthPosition;
    private readonly int _messageWritePosition;

    public ProtobufSerializer(int capacity, TraceActivitySource activitySource)
        : this(new ProtobufStream(capacity), activitySource)
    {
    }

    public ProtobufSerializer(ProtobufStream stream, TraceActivitySource activitySource)
    {
        Stream = stream;
        // Message: Trace + Resource + Spans + [Source + [Activity]]
        _traceLengthPosition = WriteTrace(Stream);
        WriteResource(Stream, activitySource.Resources);
        _spansLengthPosition = WriteSpans(Stream);
        WriteSource(Stream, activitySource);
        _messageWritePosition = Stream.Position;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(scoped in TraceActivityRef activityRef) => WriteActivity(Stream, activityRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual ReadOnlySpan<byte> Flush()
    {
        Stream.WriteLength(_traceLengthPosition);
        Stream.WriteLength(_spansLengthPosition);
        var bytes = Stream.AsSpan();
        Stream.Reset(_messageWritePosition);
        return bytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int WriteTrace(ProtobufStream stream)
    {
        stream.WriteTag(ProtobufFieldNumber.TracesData, ProtobufWireType.Len);
        return stream.ReserveForLength();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int WriteSpans(ProtobufStream stream)
    {
        stream.WriteTag(ProtobufFieldNumber.ResourceSpans, ProtobufWireType.Len);
        return stream.ReserveForLength();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteResource(ProtobufStream stream, KeyValuePair<string, string>[]? resources)
    {
        stream.WriteTag(ProtobufFieldNumber.Resource, ProtobufWireType.Len);
        using var resourceLengthScope = stream.WriteLengthScope();

        if (resources == null)
        {
            return;
        }

        foreach (var (attributeKey, attributeValue) in resources)
        {
            stream.WriteTag(ProtobufFieldNumber.ResourceAttributes, ProtobufWireType.Len);
            using var attributeLengthScope = stream.WriteLengthScope();
            stream.WriteKeyValueTag(attributeKey, attributeValue);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteSource(ProtobufStream stream, TraceActivitySource activitySource)
    {
        const int sourceName = 1;
        const int sourceVersion = 2;

        stream.WriteTag(ProtobufFieldNumber.Scope, ProtobufWireType.Len);
        using var sourceLengthScope = stream.WriteLengthScope();

        stream.WriteStringWithTag(sourceName, activitySource.Name);

        if (activitySource.Version != null)
        {
            stream.WriteStringWithTag(sourceVersion, activitySource.Version);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteActivity(ProtobufStream stream, scoped in TraceActivityRef activity)
    {
        stream.WriteTag(ProtobufFieldNumber.ScopeSpan, ProtobufWireType.Len);
        using var activityLengthScope = stream.WriteLengthScope();

        stream.WriteByteArrayWithTag(ProtobufFieldNumber.TraceId, activity.TraceId.AsReadOnlySpan());
        stream.WriteByteArrayWithTag(ProtobufFieldNumber.SpanId, activity.SpanId.AsReadOnlySpan());

        if (activity.ParentId.IsNotEmpty)
        {
            stream.WriteByteArrayWithTag(ProtobufFieldNumber.ParentId, activity.ParentId.AsReadOnlySpan());
        }

        var activityTraceFlags = activity.Recorded ? ActivityTraceFlags.Recorded : ActivityTraceFlags.None;
        var spanFlags = (uint)activityTraceFlags & 0x000000FF;

        spanFlags |= 0x00000100;
        if (activity.RemoteParent)
        {
            spanFlags |= 0x00000200;
        }

        stream.WriteFixed32WithTag(ProtobufFieldNumber.Flags, spanFlags);
        stream.WriteStringWithTag(ProtobufFieldNumber.Name, activity.OperationName);
        stream.WriteEnumWithTag(ProtobufFieldNumber.Kind, (int)activity.Kind + 1);
        stream.WriteFixed64WithTag(ProtobufFieldNumber.StartTimeUnixNano, ToUnixTimeNanoseconds(activity.StartTime));
        stream.WriteFixed64WithTag(ProtobufFieldNumber.EndTimeUnixNano, ToUnixTimeNanoseconds(activity.EndTime));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ToUnixTimeNanoseconds(DateTime dateTime)
    {
        const long nanosecondsPerTicks = 100;
        const long unixEpochTicks = 621355968000000000;

        return (ulong)(dateTime.Ticks - unixEpochTicks) * nanosecondsPerTicks;
    }
}