using System.Diagnostics;
using BenchmarkDotNet.Attributes;
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

    private static TraceActivitySource? _traceActivitySource;
    private static FakeInterceptor? _traceActivityInterceptor;

    private static SocketDiagnoser? _socketDiagnoser;

    private const int ExportBatchSize = 3;

    [GlobalSetup]
    public void Setup()
    {
        SetupSocketDiagnoser();
        SetupActivitySource();
        SetupTraceActivity();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _socketDiagnoser!.Report();
    }

    [Benchmark(Baseline = true, Description = "System.Diagnostics.Activity")]
    public ulong ActivitySource()
    {
        {
            using var activity1 = _activitySource!.StartActivity();
            using var activity2 = _activitySource.StartActivity();
            using var activity3 = _activitySource.StartActivity();
        }

        return _activityProcessor!.TotalEvents;
    }

    [Benchmark(Description = "EasyTrace.Activity")]
    public ulong TraceActivity()
    {
        {
            using var activity1 = _traceActivitySource!.Start();
            using var activity2 = _traceActivitySource.Start();
            using var activity3 = _traceActivitySource.Start();
        }

        return _traceActivityInterceptor!.TotalEvents;
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

    private static void SetupTraceActivity()
    {
        _traceActivityInterceptor = new FakeInterceptor();
        _traceActivitySource = new TraceActivitySourceBuilder()
            .SetBatchExportOptions(new BatchExportOptions
            {
                MaxExportSize = ExportBatchSize,
                MaxQueueSize = ExportBatchSize * 2,
            })
            .AddInterceptor(_traceActivityInterceptor)
            .AddOtlpExporter(new HttpExportParameters())
            .Build(nameof(OtlpExportBenchmark));
    }
}