using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using EasyTrace.Activity;
using EasyTrace.Benchmarks.TestData;
using EasyTrace.Export.Batch;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace EasyTrace.Benchmarks;

[BenchmarkCategory("AllowOnCI")]
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

    private const int AttributeCount = 10;
    private static TestAttributeProvider? _traceAttributeProvider;

    [Params(
        TestAttributeProvider.Int32,
        TestAttributeProvider.Double,
        TestAttributeProvider.String,
        TestAttributeProvider.None
    )]
    public string AttributeType { get; set; }

    [Params(true, false)] public bool IsExporter { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        SetupActivitySource();
        SetupTraceActivitySource();

        _traceAttributeProvider = new TestAttributeProvider(AttributeCount);
    }

    [Benchmark(Baseline = true)]
    public ulong ActivitySource()
    {
        using var activity1 = _activitySource!.StartActivity();
        using var activity2 = _activitySource!.StartActivity();
        using var activity3 = _activitySource!.StartActivity();

        _traceAttributeProvider!.SetTags(AttributeType, activity1);
        _traceAttributeProvider!.SetTags(AttributeType, activity2);
        _traceAttributeProvider!.SetTags(AttributeType, activity3);

        return _activityProcessor!.TotalEvents;
    }

    [Benchmark]
    public ulong TraceActivityScope()
    {
        using var activity1 = _traceActivitySource!.Start();
        using var activity2 = _traceActivitySource!.Start();
        using var activity3 = _traceActivitySource!.Start();

        _traceAttributeProvider!.SetAttributes(AttributeType, activity1);
        _traceAttributeProvider!.SetAttributes(AttributeType, activity2);
        _traceAttributeProvider!.SetAttributes(AttributeType, activity3);

        return _traceActivityInterceptor!.TotalEvents;
    }

    private void SetupActivitySource()
    {
        _activityProcessor = new FakeProcessor();
        _activitySource = new ActivitySource(nameof(OtlpExportBenchmark));

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
            .SetLimits(new TraceActivityLimits
            {
                AttributeCount = AttributeCount,
            })
            .SetBatchExportOptions(new BatchExportOptions
            {
                // Disable scheduled export, because benchmarking between Interceptor vs Processor.
                ScheduledDelayMilliseconds = uint.MinValue,
            })
            .AddInterceptor(_traceActivityInterceptor);

        if (IsExporter)
        {
            builder.AddExporter(new FakeExporter());
        }

        _traceActivitySource = builder.Build(nameof(OtlpExportBenchmark));
    }
}