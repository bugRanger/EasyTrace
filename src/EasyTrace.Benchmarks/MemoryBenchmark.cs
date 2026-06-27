using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BenchmarkDotNet.Attributes;
using EasyTrace.Activity;

namespace EasyTrace.Benchmarks;

[MemoryDiagnoser]
[ThreadingDiagnoser]
public class MemoryBenchmark
{
    public static void Run() => BenchmarkRunner.Run<MemoryBenchmark>();

    private static readonly ActivitySource Source = new(nameof(MemoryBenchmark));
    private static readonly TraceActivitySource TraceSource = new(nameof(MemoryBenchmark));

    private ActivityListener? _listener;

    [Params(1_000, 10_000)] public int Iterations { get; set; }

    [Params(true, false)] public bool IsExporter { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        if (IsExporter)
        {
            // TODO: add real export in httpClient.
            _listener = new ActivityListener
            {
                ShouldListenTo = s => s.Name == nameof(MemoryBenchmark),
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
            };

            ActivitySource.AddActivityListener(_listener);
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _listener?.Dispose();
    }

    [Benchmark]
    public void Activity() => Execute(() => Source.StartActivity());

    [Benchmark]
    public void TraceScope() => Execute(() => TraceSource.Start());

    private void Execute<T>(Func<T> factory)
        where T : IDisposable, allows ref struct
    {
        foreach (var _ in Enumerable.Repeat(0, Iterations))
        {
            using var activity1 = factory();
            using var activity2 = factory();

            Thread.SpinWait(100);
        }
    }

    private static void TraceSourceExport(in TraceActivityRef _)
    {
    }
}