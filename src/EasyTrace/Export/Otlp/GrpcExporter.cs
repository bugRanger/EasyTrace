using EasyTrace.Activity;
using EasyTrace.Export.Otlp.Protobuf;
using FastHttpClient = NetCoreServer.HttpClient;

namespace EasyTrace.Export.Otlp;

public class GrpcExporter(GrpcExportParameters parameters)
    : FastHttpClient(parameters.EndPoint.Host, parameters.EndPoint.Port), ITraceActivityExporter
{
    [ThreadStatic] private static readonly Stack<ProtobufStream> StreamPool;

    [ThreadStatic] private static readonly Dictionary<TraceActivitySource, ProtobufStream> StreamBySource;

    static GrpcExporter()
    {
        StreamPool = new Stack<ProtobufStream>();
        StreamBySource = new Dictionary<TraceActivitySource, ProtobufStream>();
    }

    private void Send(ProtobufStream stream)
    {
        // TODO: Add send http message with content.
        if (!IsConnected) Connect();
        SendRequestBody(stream.AsSpan());
    }

    void ITraceActivityExporter.Export(scoped in TraceActivityRef activityRef)
    {
        if (!StreamBySource.TryGetValue(activityRef.Source, out var stream))
        {
            if (!StreamPool.TryPop(out stream))
            {
                stream = new ProtobufStream(parameters.BufferSize);
            }

            StreamBySource[activityRef.Source] = stream;

            // TODO: Add top-level elements (traceData/resources/etc).
            // ProtobufSerializer.WriteTrace(stream);
            ProtobufSerializer.WriteResource(stream, activityRef.Source.GetResources());
            ProtobufSerializer.WriteSource(stream, activityRef.Source);
        }

        ProtobufSerializer.WriteActivity(stream, activityRef);
    }

    void ITraceActivityExporter.Flush()
    {
        if (StreamBySource.Count == 0)
        {
            return;
        }

        foreach (var (_, stream) in StreamBySource)
        {
            try
            {
                stream.Flush();
                Send(stream);
            }
            finally
            {
                stream.Reset();
                StreamPool.Push(stream);
            }
        }

        StreamBySource.Clear();
    }
}