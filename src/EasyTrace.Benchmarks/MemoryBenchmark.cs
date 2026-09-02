using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using EasyTrace.Benchmarks.TestData;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace EasyTrace.Benchmarks;

[MemoryDiagnoser]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
public class MemoryBenchmark
{
    public static void Run() => BenchmarkRunner.Run<MemoryBenchmark>();

    private static ActivitySource? _activitySource;
    private static FakeProcessor? _activityProcessor;

    private static TraceActivitySource? _traceActivitySource;
    private static FakeInterceptor? _traceActivityInterceptor;

    [Params(1_000)] public int Iterations { get; set; }

    [Params(4, 8, 16)] public int ParallelLimit { get; set; }

    [Params(true, false)] public bool IsExporter { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        SetupActivitySource();
        SetupTraceActivitySource();
    }

    [Benchmark(Baseline = true)]
    public ulong ActivitySource()
    {
        Parallel.For(0,
            ParallelLimit,
            i =>
            {
                foreach (var _ in Enumerable.Repeat(0, Iterations))
                {
                    using var activity1 = _activitySource!.StartActivity();
                    using var activity2 = _activitySource!.StartActivity();
                    using var activity3 = _activitySource!.StartActivity();
                }
            });

        return _activityProcessor!.TotalEvents;
    }

    [Benchmark]
    public ulong TraceActivityScope()
    {
        Parallel.For(0,
            ParallelLimit,
            i =>
            {
                foreach (var _ in Enumerable.Repeat(0, Iterations))
                {
                    using var activity1 = _traceActivitySource!.Start();
                    using var activity2 = _traceActivitySource!.Start();
                    using var activity3 = _traceActivitySource!.Start();
                }
            });

        return _traceActivityInterceptor!.TotalEvents;
    }

    private void SetupActivitySource()
    {
        _activityProcessor = new FakeProcessor();
        _activitySource = new ActivitySource(nameof(ExportBenchmark));
        var builder = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault())
            .AddSource(_activitySource.Name);

        if (IsExporter)
        {
            // In their implementation, the processor acts as an exporter that listens to activity states.
            builder.AddProcessor(_activityProcessor);
        }

        _ = builder.Build();
    }

    private void SetupTraceActivitySource()
    {
        _traceActivityInterceptor = new FakeInterceptor();

        var builder = new TraceActivitySourceBuilder()
            .AddInterceptor(_traceActivityInterceptor);

        if (IsExporter)
        {
            builder.AddExporter(new FakeExporter());
        }

        _traceActivitySource = builder.Build(nameof(ExportBenchmark));
    }
}