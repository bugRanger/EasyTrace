using System.Threading;
using EasyTrace.Activity;
using EasyTrace.Export;

namespace EasyTrace.Benchmarks.TestData;

internal sealed class FakeExporter : ITraceActivityExporter
{
    private int _exportedCounter;

    public void Export(scoped in TraceActivityRef activityRef) => Interlocked.Increment(ref _exportedCounter);

    public void Flush()
    {
    }
}