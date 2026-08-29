using EasyTrace.Activity;
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

    private readonly Dictionary<TraceActivitySource, GrpcSerializer> _serializerBySource = new();


    void ITraceActivityExporter.Export(scoped in TraceActivityRef activityRef)
    {
        if (!_serializerBySource.TryGetValue(activityRef.Source, out var serializer))
        {
            serializer = GrpcSerializer.Create(parameters.BufferSize, activityRef.Source);
            _serializerBySource[activityRef.Source] = serializer;
        }

        serializer.Write(activityRef);
    }

    void ITraceActivityExporter.Flush()
    {
        if (_serializerBySource.Count == 0)
        {
            return;
        }

        foreach (var (_, serializer) in _serializerBySource)
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
            if (byteCount == 0)
            {
                // TODO: Throw error.
                return;
            }
        }
    }
}