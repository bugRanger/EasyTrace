using EasyTrace.Activity;
using NetCoreServer;
using FastHttpClient = NetCoreServer.HttpClient;

namespace EasyTrace.Export.Otlp.Grpc;

/// <summary>
/// [WIP] Export via GRPC.
/// </summary>
internal class GrpcExporter(GrpcExportParameters parameters)
    : FastHttpClient(parameters.EndPoint.Host, parameters.EndPoint.Port), ITraceActivityExporter
{
    private const string Url = "opentelemetry.proto.collector.trace.v1.TraceService/Export";
    private const string ContentType = "application/grpc";

    private readonly HttpRequest _request = new("POST", Url);

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

        // TODO: Send via HTTP2 (Grpc use HTTP2) 
        foreach (var (_, serializer) in _serializerBySource)
        {
            var bytes = serializer.Flush();

            _request.SetHeader("TE", "trailers");
            _request.SetHeader("Content-Type", ContentType);
            _request.SetBody(bytes);

            if (!IsConnected)
            {
                if (!Connect())
                {
                    // TODO: Write error in log.
                    continue;
                }
            }

            var byteCount = SendRequest(_request);
            if (byteCount == 0)
            {
                // TODO: Write error in log.
                continue;
            }
        }
    }
}