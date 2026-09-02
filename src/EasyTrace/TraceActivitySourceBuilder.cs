using EasyTrace.Export;
using EasyTrace.Export.Batch;
using EasyTrace.Export.Otlp.Grpc;
using EasyTrace.Export.Otlp.Http;
using EasyTrace.Identifier;
using EasyTrace.Identifier.Generator;
using EasyTrace.Interceptor;
using EasyTrace.Time;

namespace EasyTrace;

public class TraceActivitySourceBuilder
{
    private readonly List<ITraceActivityExporter> _exporters = [];
    private readonly List<ITraceActivityInterceptor> _interceptors = [];
    private Dictionary<string, string> _resources = GetResourceDefault();
    private BatchExportOptions? _batchExportOptions;
    private ITraceTimeProvider _timeProvider = new TraceTimeProvider();
    private ITraceIdentifierGenerator _identifierGenerator = new Xoshiro256PlusPlus();

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

    public TraceActivitySourceBuilder AddOtlpExporter(HttpExportParameters parameters)
    {
        AddExporter(new HttpExporter(parameters));
        return this;
    }

    public TraceActivitySourceBuilder AddExporter(ITraceActivityExporter exporter)
    {
        _exporters.Add(exporter);
        return this;
    }

    public TraceActivitySourceBuilder AddInterceptor(ITraceActivityInterceptor interceptor)
    {
        _interceptors.Add(interceptor);
        return this;
    }

    public TraceActivitySourceBuilder AddResources(IEnumerable<KeyValuePair<string, string>> resources)
    {
        foreach (var (key, value) in resources)
        {
            _resources.Add(key, value);
        }

        return this;
    }

    public TraceActivitySourceBuilder SetResources(IEnumerable<KeyValuePair<string, string>> resources)
    {
        _resources = new Dictionary<string, string>(resources);
        return this;
    }

    public TraceActivitySource Build(string name, Version? version = null)
    {
        BatchExporter<ITraceActivityExporter>? batchExporter = null;
        if (_exporters.Count > 0)
        {
            batchExporter = new BatchExporter<ITraceActivityExporter>(
                _exporters.Count == 1 ? _exporters[0] : new GroupExporter([.. _exporters]),
                _batchExportOptions ?? new BatchExportOptions());
        }

        GroupInterceptor? groupInterceptor = null;
        if (_interceptors.Count > 0)
        {
            groupInterceptor = new GroupInterceptor(_interceptors);
        }

        return new TraceActivitySource(name, version)
        {
            TimeProvider = _timeProvider,
            IdentifierGenerator = _identifierGenerator,
            Resources = [.. _resources],
            BatchExporter = batchExporter,
            GroupInterceptor = groupInterceptor,
        };
    }

    private static Dictionary<string, string> GetResourceDefault()
    {
        var entryAssemblyName = System.Reflection.Assembly.GetEntryAssembly()?.GetName();
        var executingAssemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
        return new Dictionary<string, string>
        {
            ["telemetry.sdk.name"] = executingAssemblyName.Name!.ToLower(),
            ["telemetry.sdk.language"] = "dotnet",
            ["telemetry.sdk.version"] = executingAssemblyName.Version!.ToString(),
            ["service.name"] = entryAssemblyName?.Name?.ToLower() ?? "unknown",
        };
    }
}