using System.Runtime.CompilerServices;
using EasyTrace.Activity;
using EasyTrace.Activity.Context;
using EasyTrace.Export;
using EasyTrace.Export.Batch;
using EasyTrace.Export.Otlp.Http;
using EasyTrace.Identifier;
using EasyTrace.Identifier.Generator;
using EasyTrace.Interceptor;
using EasyTrace.Time;

namespace EasyTrace;

// TODO: Delete after testing.
public enum ContextMode
{
    ThreadLocal,
    AsyncLocal,
    AsyncState,
}

public class TraceActivitySourceBuilder
{
    private readonly List<ITraceActivityInterceptor> _interceptors = [];
    private readonly List<ITraceActivityExporter> _exporters = [];
    private BatchExportOptions? _batchExportOptions;
    private BatchExporter? _batchExporter;
    private Dictionary<string, string>? _resources;
    private ContextMode? _contextMode;
    private TraceActivityFactory? _factory;
    private TraceActivityLimits? _limits;
    private ITraceTimeProvider? _timeProvider;
    private ITraceIdentifierGenerator? _identifierGenerator;
    private ITraceActivityContext? _context;

    public TraceActivitySourceBuilder SetLimits(TraceActivityLimits limits)
    {
        ThrowIfNotNull(_limits);
        _limits = limits;
        return this;
    }

    public TraceActivitySourceBuilder SetTimeProvider(ITraceTimeProvider timeProvider)
    {
        ThrowIfNotNull(_timeProvider);
        _timeProvider = timeProvider;
        return this;
    }

    public TraceActivitySourceBuilder SetIdentifierGenerator(ITraceIdentifierGenerator identifierGenerator)
    {
        ThrowIfNotNull(_identifierGenerator);
        _identifierGenerator = identifierGenerator;
        return this;
    }

    public TraceActivitySourceBuilder SetBatchExportOptions(BatchExportOptions batchExportOptions)
    {
        ThrowIfNotNull(_batchExportOptions);
        batchExportOptions.Validate();
        _batchExportOptions = batchExportOptions;
        return this;
    }

    public TraceActivitySourceBuilder SetContextMode(ContextMode value)
    {
        ThrowIfNotNull(_contextMode);
        _contextMode ??= value;
        return this;
    }

    public TraceActivitySourceBuilder AddOtlpExporter(HttpExportParameters parameters)
    {
        ThrowIfNotNull(_batchExporter, "No changes are permitted after the first build");
        parameters.Validate();
        var exporter = new HttpExporter(parameters);
        exporter.WarmUp();
        AddExporter(exporter);
        return this;
    }

    public TraceActivitySourceBuilder AddExporter(ITraceActivityExporter exporter)
    {
        ThrowIfNotNull(_batchExporter, "No changes are permitted after the first build");
        _exporters.Add(exporter);
        return this;
    }

    public TraceActivitySourceBuilder AddInterceptor(ITraceActivityInterceptor interceptor)
    {
        ThrowIfNotNull(_batchExporter, "No changes are permitted after the first build");
        _interceptors.Add(interceptor);
        return this;
    }

    public TraceActivitySourceBuilder SetResources(IEnumerable<KeyValuePair<string, string>> resources)
    {
        ThrowIfNotNull(_resources);
        _resources = new Dictionary<string, string>(resources);
        return this;
    }

    public TraceActivitySourceBuilder AddResources(IEnumerable<KeyValuePair<string, string>> resources)
    {
        ThrowIfNull(_resources);

        foreach (var (key, value) in resources)
        {
            _resources!.Add(key, value);
        }

        return this;
    }

    public TraceActivitySource Build(string name, Version? version = null)
    {
        _resources ??= GetResourceDefault();
        _timeProvider ??= new TraceTimeProvider();
        _identifierGenerator ??= Xoshiro256PlusPlusShared.Instance;

        _factory ??= new TraceActivityFactory(_limits ?? new TraceActivityLimits());
        _context ??= _contextMode switch
        {
            null => new ContextThreadLocal(_factory),
            ContextMode.ThreadLocal => new ContextThreadLocal(_factory),
            ContextMode.AsyncLocal => new ContextAsyncLocal(_factory),
            ContextMode.AsyncState => new ContextAsyncState(_factory),
            _ => throw new ArgumentOutOfRangeException()
        };

        _batchExportOptions ??= new BatchExportOptions();
        _batchExporter ??= new BatchExporter(new GroupExporter([.. _exporters]), _factory, _batchExportOptions);

        return new TraceActivitySource
        {
            Name = name,
            Version = version?.ToString(),
            Context = _context,
            TimeProvider = _timeProvider,
            IdentifierGenerator = _identifierGenerator,
            Resources = [.. _resources],
            BatchExporter = _batchExporter,
            GroupInterceptor = new GroupInterceptor([.. _interceptors]),
        };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowIfNull<T>(T value, string? message = null)
    {
        if (value is null)
        {
            throw new InvalidOperationException(message ?? "Value must be not null.");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowIfNotNull<T>(T value, string? message = null)
    {
        if (value is not null)
        {
            throw new InvalidOperationException(message ?? "Value has already been set.");
        }
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