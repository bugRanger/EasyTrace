using EasyTrace.Activity;
using EasyTrace.Export;

namespace EasyTrace.Tests.TestData;

public class InMemoryExport : ITraceActivityExporter
{
    public readonly List<string> Items = [];

    public void Export(scoped in TraceActivityRef activityRef)
    {
        Items.Add(
            $"{nameof(ITraceActivity.TraceId)}: {Convert.ToHexStringLower(activityRef.TraceId.AsReadOnlySpan())}|" +
            $"{nameof(ITraceActivity.SpanId)}: {Convert.ToHexStringLower(activityRef.SpanId.AsReadOnlySpan())}|" +
            $"{nameof(ITraceActivity.Source)}: {activityRef.Source.Name} {activityRef.Source.Version}|" +
            $"{nameof(ITraceActivity.OperationName)}: {activityRef.OperationName}|" +
            $"{nameof(ITraceActivity.Kind)}: {activityRef.Kind}|" +
            $"{nameof(ITraceActivity.StartTime)}: {activityRef.StartTime}|" +
            $"{nameof(ITraceActivity.EndTime)}: {activityRef.EndTime}");
    }
}