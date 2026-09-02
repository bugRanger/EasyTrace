using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BenchmarkDotNet.Attributes;
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
    public static void Run() => BenchmarkRunner.Run<OtlpExportBenchmark>();

    private static ActivitySource? _activitySource;
    private static FakeProcessor? _activityProcessor;

    private static TraceActivitySource? _traceActivitySource;
    private static FakeInterceptor? _traceActivityInterceptor;

    private const int ExportBatchSize = 3;
    private const int ExportDelayInMs = 50;

    [Params(100)] public int Iterations { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        SetupActivitySource();
        SetupTraceActivity();
    }

    [Benchmark(Baseline = true)]
    public ulong ActivitySource()
    {
        foreach (var _ in Enumerable.Repeat(0, Iterations))
        {
            using var activity1 = _activitySource!.StartActivity();
            using var activity2 = _activitySource!.StartActivity();
            using var activity3 = _activitySource!.StartActivity();
            Thread.Sleep(ExportDelayInMs);
        }

        return _activityProcessor!.TotalEvents;
    }

    [Benchmark]
    public ulong TraceActivity()
    {
        foreach (var _ in Enumerable.Repeat(0, Iterations))
        {
            using var activity1 = _traceActivitySource!.Start();
            using var activity2 = _traceActivitySource!.Start();
            using var activity3 = _traceActivitySource!.Start();
            Thread.Sleep(ExportDelayInMs);
        }

        return _traceActivityInterceptor!.TotalEvents;
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
                MaxQueueSize = ExportBatchSize * 2,
                MaxExportBatchSize = ExportBatchSize,
            })
            .AddInterceptor(_traceActivityInterceptor)
            .AddOtlpExporter(new HttpExportParameters())
            .Build(nameof(OtlpExportBenchmark));
    }
}