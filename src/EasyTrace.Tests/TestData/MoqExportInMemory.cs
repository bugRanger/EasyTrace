using EasyTrace.Activity;
using EasyTrace.Export;

namespace EasyTrace.Tests.TestData;

public class InMemoryExport(TimeSpan? flushDelay = null) : ITraceActivityExporter
{
    private const char NewLine = '\n';

    private readonly List<string> _flushItems = [];

    /// <summary>
    /// List of items exported into batches.
    /// </summary>
    public readonly List<string> Batches = [];

    /// <summary>
    /// List of all items.
    /// </summary>
    public readonly List<string> Items = [];

    public void Export(scoped in TraceActivityRef activityRef)
    {
        var attributes = activityRef.Attributes.AsSpan().ToArray();
        var content =
            $"{nameof(ITraceActivity.TraceId)}: {Convert.ToHexStringLower(activityRef.TraceId.AsReadOnlySpan())}|" +
            $"{nameof(ITraceActivity.SpanId)}: {Convert.ToHexStringLower(activityRef.SpanId.AsReadOnlySpan())}|" +
            $"{nameof(ITraceActivity.ParentId)}: {Convert.ToHexStringLower(activityRef.ParentId.AsReadOnlySpan())}|" +
            $"{nameof(ITraceActivity.Source)}: {activityRef.Source.Name} {activityRef.Source.Version}|" +
            $"{nameof(ITraceActivity.OperationName)}: {activityRef.OperationName}|" +
            $"{nameof(ITraceActivity.Kind)}: {activityRef.Kind}|" +
            $"{nameof(ITraceActivity.StartTime)}: {activityRef.StartTime.TimeOfDay}|" +
            $"{nameof(ITraceActivity.EndTime)}: {activityRef.EndTime.TimeOfDay}|" +
            $"{nameof(ITraceActivity.Attributes)} ({attributes.Length}/{activityRef.Attributes.Dropped}): {string.Join(",", attributes.Select(s => $"{s.Name}: {s.Value}"))}";

        Items.Add(content);
        _flushItems.Add(content);
    }

    public void Flush()
    {
        Batches.Add($"< batch: {_flushItems.Count} >" + NewLine + string.Join(NewLine, _flushItems));
        _flushItems.Clear();

        if (flushDelay.HasValue)
        {
            Thread.Sleep(flushDelay.Value);   
        }
    }
}