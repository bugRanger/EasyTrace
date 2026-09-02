using System.Runtime.CompilerServices;
using EasyTrace.Export.Otlp.Protobuf;

namespace EasyTrace.Export.Otlp.Grpc;

public sealed class GrpcSerializer : ProtobufSerializer
{
    private const int MessageLengthPosition = 1;
    private const int MessageWritePosition = 5;

    private GrpcSerializer(ProtobufStream stream, TraceActivitySource activitySource) : base(stream, activitySource)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GrpcSerializer Create(int capacity, TraceActivitySource activitySource)
    {
        var stream = new ProtobufStream(capacity);
        // Grpc payload consists of 3 parts:
        // byte 0 - Specifying if the payload is compressed.
        // 1-4 byte - Specifies the length of payload in big endian format.
        // 5 and above -  Protobuf serialized data.
        stream.Reserve(MessageWritePosition);
        return new GrpcSerializer(stream, activitySource);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override ReadOnlySpan<byte> Flush()
    {
        Stream.WriteFixed32BigEndianFormat(MessageLengthPosition, (uint)Stream.Position - MessageWritePosition);
        return base.Flush();
    }
}