using System.Diagnostics;
using System.Runtime.CompilerServices;
using EasyTrace.Activity;

namespace EasyTrace.Export.Otlp.Protobuf;

public class ProtobufSerializer(int capacity)
{
    private readonly ProtobufStream _stream = new(capacity);
    private int _messageLengthPosition;
    private int _messageStartPosition;
    private int _traceLengthPosition;
    private int _spansLengthPosition;

    public void CreateGrpc(TraceActivitySource activitySource)
    {
        _stream.Reset();
        // Grpc payload consists of 3 parts:
        // byte 0 - Specifying if the payload is compressed.
        // 1-4 byte - Specifies the length of payload in big endian format.
        // 5 and above -  Protobuf serialized data.
        _messageLengthPosition = 1;
        _messageStartPosition = 5;
        _stream.Reserve(_messageStartPosition);
        // Message: Trace + Resource + Spans + [Source + [Activity]]
        _traceLengthPosition = WriteTrace(_stream);
        WriteResource(_stream, activitySource.GetResources());
        _spansLengthPosition = WriteSpans(_stream);
        WriteSource(_stream, activitySource);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(TraceActivityRef activityRef) => WriteActivity(_stream, activityRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> Flush()
    {
        _stream.WriteLength(_messageLengthPosition);
        _stream.WriteLength(_traceLengthPosition);
        _stream.WriteLength(_spansLengthPosition);
        var bytes = _stream.AsSpan();
        _stream.Reset(_messageStartPosition);
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
    private static void WriteResource(ProtobufStream stream, IEnumerable<KeyValuePair<string, string>>? resources)
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