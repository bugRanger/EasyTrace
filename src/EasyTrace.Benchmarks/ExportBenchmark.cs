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

[MemoryDiagnoser]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
public class ExportBenchmark
{
    public static void Run() => BenchmarkRunner.Run<ExportBenchmark>();

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
        _activitySource = new ActivitySource(nameof(ExportBenchmark));
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
            .SetResources(new Dictionary<string, string>
            {
                ["telemetry.sdk.name"] = "test",
                ["telemetry.sdk.language"] = "dotnet",
                ["telemetry.sdk.version"] = "1.0.0.0",
                ["service.name"] = "unknown",
            })
            .SetBatchExportOptions(new BatchExportOptions
            {
                MaxQueueSize = ExportBatchSize * 2,
                MaxExportBatchSize = ExportBatchSize,
            })
            .AddInterceptor(_traceActivityInterceptor)
            .AddHttpExporter(new HttpExportParameters())
            .Build(nameof(ExportBenchmark));
    }
}