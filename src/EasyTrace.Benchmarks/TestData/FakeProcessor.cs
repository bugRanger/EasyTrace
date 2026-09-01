using System.Threading;
using OpenTelemetry;

namespace EasyTrace.Benchmarks.TestData;

internal sealed class FakeProcessor : BaseProcessor<System.Diagnostics.Activity>
{
    private uint _startedCounter;
    private uint _stoppedCounter;

    public uint Started => _startedCounter;

    public uint Stopped => _stoppedCounter;

    public override void OnStart(System.Diagnostics.Activity activity) => Interlocked.Increment(ref _startedCounter);

    public override void OnEnd(System.Diagnostics.Activity activity) => Interlocked.Increment(ref _stoppedCounter);

    protected override bool OnForceFlush(int timeoutMilliseconds) => true;

    protected override bool OnShutdown(int timeoutMilliseconds) => true;
}