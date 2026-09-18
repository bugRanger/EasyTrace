using System.Globalization;
using System.Text;
using EasyTrace.Activity;
using EasyTrace.Export.Otlp.Protobuf;
using NetCoreServer;
using Buffer = NetCoreServer.Buffer;
using FastHttpClient = NetCoreServer.HttpClient;

namespace EasyTrace.Export.Otlp.Http;

/// <summary>
/// Export via HTTP/1.1 + Protobuf.
/// </summary>
public class HttpExporter : FastHttpClient, ITraceActivityExporter
{
    private readonly Buffer _requestBuffer;
    private readonly long _requestOffset;
    private readonly Dictionary<TraceActivitySource, ProtobufSerializer> _serializerBySource = new();
    private readonly HttpExportParameters _parameters;

    /// <summary>
    /// Export via HTTP/1.1 + Protobuf.
    /// </summary>
    public HttpExporter(HttpExportParameters parameters) : base(parameters.EndPoint.Host, parameters.EndPoint.Port)
    {
        _parameters = parameters;
        var request = new HttpRequest();
        request.SetBegin("POST", "/v1/traces");
        request.SetHeader("Host", $"{parameters.EndPoint.Host}:{parameters.EndPoint.Port}");
        request.SetHeader("Content-Type", "application/x-protobuf");
        request.Cache.Append("Content-Length: ");
        _requestBuffer = request.Cache;
        _requestOffset = request.Cache.Size;
    }

    void ITraceActivityExporter.Export(scoped in TraceActivityRef activityRef)
    {
        if (!_serializerBySource.TryGetValue(activityRef.Source, out var serializer))
        {
            // TODO: Add buffer size configure from builder that accounts for constraints (resource size, tag size, etc.).
            serializer = new ProtobufSerializer(_parameters.BufferSize, activityRef.Source);
            _serializerBySource[activityRef.Source] = serializer;
        }

        serializer.Write(activityRef);
    }

    void ITraceActivityExporter.Flush()
    {
        const string newLine = "\r\n";

        if (_serializerBySource.Count == 0)
        {
            return;
        }

        foreach (var (_, serializer) in _serializerBySource)
        {
            var bytes = serializer.Flush();
            if (_requestBuffer.Size < _requestOffset + bytes.Length + 11 + newLine.Length * 2)
            {
                _requestBuffer.Resize(_requestOffset + bytes.Length + 11 + newLine.Length * 2);
            }

            var offset = (int)_requestOffset;

            bytes.Length.TryFormat(_requestBuffer.AsSpan()[offset..], out var writeBytes,
                provider: CultureInfo.InvariantCulture);
            offset += writeBytes;

            Encoding.UTF8.GetBytes(newLine, 0, newLine.Length, _requestBuffer.Data, offset);
            offset += newLine.Length;
            Encoding.UTF8.GetBytes(newLine, 0, newLine.Length, _requestBuffer.Data, offset);
            offset += newLine.Length;

            bytes.CopyTo(_requestBuffer.Data.AsSpan(offset));
            offset += bytes.Length;

            if (!IsConnected)
            {
                if (!Connect())
                {
                    // TODO: Write error in log.
                    continue;
                }
            }

            var byteCount = Send(_requestBuffer.Data.AsSpan(0, offset));
            if (byteCount == 0)
            {
                // TODO: Write error in log.
                continue;
            }
        }
    }
}