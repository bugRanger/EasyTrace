using EasyTrace.Activity;
using EasyTrace.Export.Otlp.Protobuf;
using NetCoreServer;
using FastHttpClient = NetCoreServer.HttpClient;

namespace EasyTrace.Export.Otlp.Http;

/// <summary>
/// Export via HTTP/1.1 + Protobuf.
/// </summary>
public class HttpExporter(HttpExportParameters parameters)
    : FastHttpClient(parameters.EndPoint.Host, parameters.EndPoint.Port), ITraceActivityExporter
{
    private const string Url = "/v1/traces";
    private const string ContentType = "application/x-protobuf";

    private readonly HttpRequest _request = new();
    private readonly string _hostRequest = $"{parameters.EndPoint.Host}:{parameters.EndPoint.Port}";
    private readonly Dictionary<TraceActivitySource, ProtobufSerializer> _serializerBySource = new();

    void ITraceActivityExporter.Export(scoped in TraceActivityRef activityRef)
    {
        if (!_serializerBySource.TryGetValue(activityRef.Source, out var serializer))
        {
            serializer = new ProtobufSerializer(parameters.BufferSize, activityRef.Source);
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

            _request.SetBegin("POST", Url);
            _request.SetHeader("Host", _hostRequest);
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