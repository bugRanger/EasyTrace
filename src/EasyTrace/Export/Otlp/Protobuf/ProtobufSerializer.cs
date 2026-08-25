using System.Diagnostics;
using System.Runtime.CompilerServices;
using EasyTrace.Activity;

namespace EasyTrace.Export.Otlp.Protobuf;

public static class ProtobufSerializer
{
    private const int ReserveSizeForLength = 4;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void WriteSource(
        ProtobufStream stream,
        TraceActivitySource activitySource)
    {
        const int sourceName = 1;
        const int sourceVersion = 2;

        stream.WriteTag(ProtobufFieldNumber.Scope, ProtobufWireType.Len);
        var scopeLengthPosition = stream.Position;
        stream.Reserve(ReserveSizeForLength);

        stream.WriteStringWithTag(sourceName, activitySource.Name);

        if (activitySource.Version != null)
        {
            stream.WriteStringWithTag(sourceVersion, activitySource.Version);
        }

        stream.WriteLength(scopeLengthPosition, stream.Position - (scopeLengthPosition + ReserveSizeForLength));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteActivity(
        ProtobufStream stream,
        scoped in TraceActivityRef activity)
    {
        stream.WriteTag(ProtobufFieldNumber.Span, ProtobufWireType.Len);
        var spanLengthPosition = stream.Position;
        stream.Reserve(ReserveSizeForLength);

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
        stream.WriteLength(spanLengthPosition, stream.Position - (spanLengthPosition + ReserveSizeForLength));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ToUnixTimeNanoseconds(DateTime dateTime)
    {
        const long nanosecondsPerTicks = 100;
        const long unixEpochTicks = 621355968000000000;

        return (ulong)(dateTime.Ticks - unixEpochTicks) * nanosecondsPerTicks;
    }
}