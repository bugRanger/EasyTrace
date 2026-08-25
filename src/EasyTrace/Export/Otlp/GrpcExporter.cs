using EasyTrace.Activity;
using EasyTrace.Export.Otlp.Protobuf;
using FastHttpClient = NetCoreServer.HttpClient;

namespace EasyTrace.Export.Otlp;

public class GrpcExporter(GrpcExportParameters parameters)
    : FastHttpClient(parameters.EndPoint.Host, parameters.EndPoint.Port), ITraceActivityExporter
{
    [ThreadStatic] private static readonly Stack<ProtobufStream> _streamPool;
    [ThreadStatic] private static readonly Dictionary<string, ProtobufStream> _streamBySource;

    static GrpcExporter()
    {
        _streamPool = new Stack<ProtobufStream>();
        _streamBySource = new Dictionary<string, ProtobufStream>();
    }

    private void Send(ProtobufStream stream)
    {
        // TODO: Add top-level elements (resources/traceData/etc).
        // TODO: Add send http message with content.
        // if (!IsConnected) Connect();
        // SendRequestBody(_buffer, 0, _buffer.Length);
    }

    void ITraceActivityExporter.Export(scoped in TraceActivityRef activityRef)
    {
        if (!_streamBySource.TryGetValue(activityRef.Source.Name, out var stream))
        {
            stream = _streamPool.Count > 0 ? _streamPool.Pop() : new ProtobufStream(parameters.BufferSize);
            _streamBySource[activityRef.Source.Name] = stream;

            ProtobufSerializer.WriteSource(stream, activityRef.Source);
        }

        ProtobufSerializer.WriteActivity(stream, activityRef);
    }

    void ITraceActivityExporter.Flush()
    {
        if (_streamBySource.Count == 0)
        {
            return;
        }

        foreach (var stream in _streamBySource.Values)
        {
            Send(stream);
            stream.Reset();
            _streamPool.Push(stream);
        }

        _streamBySource.Clear();
    }
}