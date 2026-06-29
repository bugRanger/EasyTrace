using System;
using EasyTrace.Activity;
using EasyTrace.Export;

namespace EasyTrace.Benchmarks.TestData;

internal sealed class FakeExporter(Action action) : ITraceActivityExporter
{
    public void Export(scoped in TraceActivityRef activityRef)
    {
        action();
    }
}