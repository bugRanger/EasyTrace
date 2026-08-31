using EasyTrace.Export;
using EasyTrace.Export.Batch;
using EasyTrace.Export.Otlp.Grpc;
using EasyTrace.Export.Otlp.Http;
using EasyTrace.Identifier;
using EasyTrace.Identifier.Generator;
using EasyTrace.Time;

namespace EasyTrace;

// TODO: Configure sampler.
// TODO: Configure/Add resources (export in Jaeger).
public class TraceActivitySourceBuilder
{
    private readonly List<ITraceActivityExporter> _exporters = [];
    private BatchExportOptions? _batchExportOptions;
    private ITraceTimeProvider _timeProvider = new TraceTimeProvider();
    private ITraceIdentifierGenerator _identifierGenerator = new Xoshiro256PlusPlus();
    private KeyValuePair<string, string>[] _resources = [];

    public TraceActivitySourceBuilder SetTimeProvider(ITraceTimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        return this;
    }

    public TraceActivitySourceBuilder SetIdentifierGenerator(ITraceIdentifierGenerator identifierGenerator)
    {
        _identifierGenerator = identifierGenerator;
        return this;
    }

    public TraceActivitySourceBuilder SetBatchExportOptions(BatchExportOptions batchExportOptions)
    {
        _batchExportOptions = batchExportOptions;
        return this;
    }

    public TraceActivitySourceBuilder AddHttpExporter(HttpExportParameters parameters)
    {
        AddExporter(new HttpExporter(parameters));
        return this;
    }

    public TraceActivitySourceBuilder AddGrpcExporter(GrpcExportParameters parameters)
    {
        AddExporter(new GrpcExporter(parameters));
        return this;
    }

    public TraceActivitySourceBuilder AddExporter(ITraceActivityExporter exporter)
    {
        _exporters.Add(exporter);
        return this;
    }

    public TraceActivitySourceBuilder SetResources(IEnumerable<KeyValuePair<string, string>> resources)
    {
        _resources = [.. resources];
        return this;
    }

    public TraceActivitySource Build(string name, Version? version = null)
    {
        BatchExporter<ITraceActivityExporter>? batchExporter = null;

        if (_exporters.Count > 0)
        {
            batchExporter = new BatchExporter<ITraceActivityExporter>(
                _exporters.Count == 1 ? _exporters[0] : new GroupExporter(_exporters.ToArray()),
                _batchExportOptions ?? new BatchExportOptions());
        }

        return new TraceActivitySource(name, version)
        {
            TimeProvider = _timeProvider,
            IdentifierGenerator = _identifierGenerator,
            Resources = _resources,
            BatchExporter = batchExporter,
        };
    }
}