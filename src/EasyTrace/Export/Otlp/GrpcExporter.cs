using EasyTrace.Activity;
using EasyTrace.Export.Otlp.Protobuf;
using NetCoreServer;
using FastHttpClient = NetCoreServer.HttpClient;

namespace EasyTrace.Export.Otlp;

public class GrpcExporter(GrpcExportParameters parameters)
    : FastHttpClient(parameters.EndPoint.Host, parameters.EndPoint.Port), ITraceActivityExporter
{
    private const string Url = "opentelemetry.proto.collector.trace.v1.TraceService/Export";
    private const string ContentType = "application/grpc";

    private readonly HttpRequest _request = new(
        "POST",
        string.Concat(parameters.EndPoint.AbsoluteUri, Url));

    private static readonly Dictionary<TraceActivitySource, ProtobufSerializer> SerializerBySource;

    static GrpcExporter()
    {
        SerializerBySource = new Dictionary<TraceActivitySource, ProtobufSerializer>();
    }

    void ITraceActivityExporter.Export(scoped in TraceActivityRef activityRef)
    {
        if (!SerializerBySource.TryGetValue(activityRef.Source, out var serializer))
        {
            serializer = new ProtobufSerializer(parameters.BufferSize);
            serializer.CreateGrpc(activityRef.Source);
            SerializerBySource[activityRef.Source] = serializer;
        }

        serializer.Write(activityRef);
    }

    void ITraceActivityExporter.Flush()
    {
        if (SerializerBySource.Count == 0)
        {
            return;
        }

        foreach (var (_, serializer) in SerializerBySource)
        {
            var bytes = serializer.Flush();

            _request.SetHeader("TE", "trailers");
            _request.SetHeader("Content-Type", ContentType);
            _request.SetBody(bytes);

            Connect();

            if (!IsConnected)
            {
                // TODO: Throw error.
                return;
            }

            var byteCount = SendRequest(_request);

            if (bytes.Length != byteCount)
            {
                // TODO: Throw error.
                return;
            }
        }
    }
}