using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BenchmarkDotNet.Attributes;
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
    private static TraceActivitySource? _traceActivitySource;

    private const int ExportIterations = 100;
    private const int ExportBatchSize = 3;
    private const int ExportDelayInMs = 50;

    // TODO: Add validation via export counter.
    private ulong _activityCounter;

    [GlobalSetup]
    public void Setup()
    {
        SetupActivitySource();
        SetupTraceActivity();
    }

    [Benchmark(Baseline = true)]
    public ulong ActivitySource()
    {
        _activityCounter = 0;
    
        foreach (var _ in Enumerable.Repeat(0, ExportIterations))
        {
            using var activity1 = _activitySource!.StartActivity();
            using var activity2 = _activitySource!.StartActivity();
            using var activity3 = _activitySource!.StartActivity();
            Thread.Sleep(ExportDelayInMs);
        }
    
        return Interlocked.Read(ref _activityCounter);
    }

    [Benchmark]
    public ulong TraceActivity()
    {
        _activityCounter = 0;
    
        foreach (var _ in Enumerable.Repeat(0, ExportIterations))
        {
            using var activity1 = _traceActivitySource!.Start();
            using var activity2 = _traceActivitySource!.Start();
            using var activity3 = _traceActivitySource!.Start();
            Thread.Sleep(ExportDelayInMs);
        }
    
        return Interlocked.Read(ref _activityCounter);
    }

    private static void SetupActivitySource()
    {
        _activitySource = new ActivitySource(nameof(ExportBenchmark));
        _ = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault())
            .AddSource(_activitySource.Name)
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
            .AddHttpExporter(new HttpExportParameters())
            .Build(nameof(ExportBenchmark));
    }
}