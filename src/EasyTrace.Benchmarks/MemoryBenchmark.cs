using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using EasyTrace.Benchmarks.TestData;

namespace EasyTrace.Benchmarks;

[MemoryDiagnoser]
public class MemoryBenchmark
{
    public static void Run() => BenchmarkRunner.Run<MemoryBenchmark>();

    private static ActivitySource? _activitySource;
    private static TraceActivitySource? _traceActivitySource;

    private ActivityListener? _listener;
    private ulong _activityCounter;

    [Params(1_000, 10_000)] public int Iterations { get; set; }

    [Params(4)] public int ParallelLimit { get; set; }

    [Params(true, false)] public bool IsExporter { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _activitySource = new ActivitySource(nameof(MemoryBenchmark));
        if (IsExporter)
        {
            _listener = new ActivityListener
            {
                ShouldListenTo = s => s.Name == nameof(MemoryBenchmark),
                Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                    ActivitySamplingResult.AllData
            };
            _listener.ActivityStopped += _ => Interlocked.Increment(ref _activityCounter);
            System.Diagnostics.ActivitySource.AddActivityListener(_listener);
        }

        var traceActivitySourceBuilder = new TraceActivitySourceBuilder();
        if (IsExporter)
        {
            traceActivitySourceBuilder.AddExporter(new FakeExporter(() => Interlocked.Increment(ref _activityCounter)));
        }

        _traceActivitySource = traceActivitySourceBuilder.Build(nameof(MemoryBenchmark));
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _listener?.Dispose();
    }

    [Benchmark]
    public ulong ActivitySource() => Execute(() => _activitySource!.StartActivity());

    [Benchmark]
    public ulong TraceActivityRef() => Execute(() => _traceActivitySource!.Start());

    private ulong Execute<T>(Func<T> factory)
        where T : IDisposable?, allows ref struct
    {
        _activityCounter = 0;

        Parallel.For(0,
            ParallelLimit,
            i =>
            {
                foreach (var _ in Enumerable.Repeat(0, Iterations))
                {
                    using var activity1 = factory();
                    using var activity2 = factory();
                    using var activity3 = factory();
                }
            });

        return Interlocked.Read(ref _activityCounter);
    }
}