using System.Threading;
using EasyTrace.Activity;
using EasyTrace.Interceptor;

namespace EasyTrace.Benchmarks.TestData;

internal sealed class FakeInterceptor : ITraceActivityInterceptor
{
    private uint _startedCounter;
    private uint _stoppedCounter;

    public uint Started => _startedCounter;

    public uint Stopped => _stoppedCounter;

    public void Start(scoped in TraceActivityRef activityRef) => Interlocked.Increment(ref _startedCounter);

    public void Stop(scoped in TraceActivityRef activityRef) => Interlocked.Increment(ref _stoppedCounter);
}