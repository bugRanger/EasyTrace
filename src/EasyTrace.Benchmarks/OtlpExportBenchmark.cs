using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using EasyTrace.Benchmarks.Metrics;
using EasyTrace.Benchmarks.TestData;
using EasyTrace.Export.Batch;
using EasyTrace.Export.Otlp.Http;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace EasyTrace.Benchmarks;

/// <summary>
/// Comparative analysis of export to a running process with export via OTLP.
/// </summary>
/// <remarks>
/// This benchmark does NOT start the process that will receive data via OTLP.
/// This process must be running before running the benchmark.  
/// </remarks>
[MemoryDiagnoser]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
public class OtlpExportBenchmark
{
    public static void Run() =>
        BenchmarkRunner.Run<OtlpExportBenchmark>(SocketDiagnoser.AddColumns);

    private static ActivitySource? _activitySource;
    private static FakeProcessor? _activityProcessor;

    private static FakeInterceptor? _traceActivityInterceptor;
    private static TraceActivitySource? _traceActivitySourceThreadLocal;
    private static TraceActivitySource? _traceActivitySourceAsyncLocal;
    private static TraceActivitySource? _traceActivitySourceAsyncState;

    private static SocketDiagnoser? _socketDiagnoser;

    private const int ExportBatchSize = 5;

    private readonly Consumer _consumer = new();

    [Params(true, false)] public bool Async { get; set; }

    [Params(5, 25, 100)] public int ActivityCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        SetupSocketDiagnoser();
        SetupActivitySource();

        _traceActivitySourceThreadLocal = SetupTraceActivity(ContextMode.ThreadLocal);
        _traceActivitySourceAsyncLocal = SetupTraceActivity(ContextMode.AsyncLocal);
        _traceActivitySourceAsyncState = SetupTraceActivity(ContextMode.AsyncState);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _socketDiagnoser!.Report();
    }

    [Benchmark(Baseline = true, Description = "System.Diagnostics.Activity")]
    public async Task ActivitySource()
    {
        for (var i = 0; i < ActivityCount; i++)
        {
            using var activity1 = _activitySource!.StartActivity();
            // Имитация асинхронного переключения контекста
            if (Async) await Task.Yield();
            using var activity2 = _activitySource.StartActivity();
        }

        _consumer.Consume(_activityProcessor!.TotalEvents);
    }

    [Benchmark(Description = "EasyTrace.Activity.ThreadLocal")]
    public async Task TraceActivity_ThreadLocal() => await TraceActivity(_traceActivitySourceThreadLocal!);

    [Benchmark(Description = "EasyTrace.Activity.AsyncLocal")]
    public async Task TraceActivity_AsyncLocal() => await TraceActivity(_traceActivitySourceAsyncLocal!);

    [Benchmark(Description = "EasyTrace.Activity.AsyncState")]
    public async Task TraceActivity_AsyncState() => await TraceActivity(_traceActivitySourceAsyncState!);

    private async Task TraceActivity<T>(T traceActivitySource)
        where T : TraceActivitySource
    { 
        for (var i = 0; i < ActivityCount; i++)
        {
            using var activity1 = traceActivitySource!.Start();
            // Имитация асинхронного переключения контекста
            if (Async)
            {
                if (traceActivitySource == _traceActivitySourceThreadLocal)
                {
                    // TODO: Правильный путь сделать через билдер два типа источника (thread/async), в случае первого сделать ошибки сборки.
                    throw new NotSupportedException();
                }

                await Task.Yield();
            }
            using var activity2 = traceActivitySource.Start();
        }

        _consumer.Consume(_traceActivityInterceptor!.TotalEvents);
    }

    private static void SetupSocketDiagnoser()
    {
        _socketDiagnoser = SocketPatch.Initialize();
    }

    private static void SetupActivitySource()
    {
        _activityProcessor = new FakeProcessor();
        _activitySource = new ActivitySource(nameof(OtlpExportBenchmark));
        _ = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault())
            .AddSource(_activitySource.Name)
            .AddProcessor(_activityProcessor)
            .AddOtlpExporter(options =>
            {
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
                options.BatchExportProcessorOptions.MaxQueueSize = ExportBatchSize * 2;
                options.BatchExportProcessorOptions.MaxExportBatchSize = ExportBatchSize;
            })
            .Build();
    }

    private static TraceActivitySource SetupTraceActivity(ContextMode contextMode)
    {
        _traceActivityInterceptor ??= new FakeInterceptor();
        return new TraceActivitySourceBuilder()
            .SetBatchExportOptions(new BatchExportOptions
            {
                MaxQueueSize = ExportBatchSize * 2,
                MaxExportSize = ExportBatchSize,
            })
            .SetContextMode(contextMode)
            .AddInterceptor(_traceActivityInterceptor)
            .AddOtlpExporter(new HttpExportParameters())
            .Build(nameof(OtlpExportBenchmark));
    }
}