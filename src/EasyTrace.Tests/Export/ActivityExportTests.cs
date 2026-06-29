using System.Diagnostics;
using EasyTrace.Activity;
using EasyTrace.Export;
using EasyTrace.Tests.TestData;

namespace EasyTrace.Tests.Export;

public class ActivityExportTests
{
    private class InMemoryExport : ITraceActivityExporter
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

    private static readonly VerifySettings Settings;

    static ActivityExportTests()
    {
        Settings = new VerifySettings();
        Settings.UseDirectory("snapshots");
    }

    [Test]
    [TestCase(ActivityKind.Server)]
    [TestCase(ActivityKind.Client)]
    [TestCase(ActivityKind.Consumer)]
    [TestCase(ActivityKind.Internal)]
    [TestCase(ActivityKind.Producer)]
    public Task SingleActivity(ActivityKind kind)
    {
        // Arrange
        var inMemoryExporter = new InMemoryExport();
        var source = new TraceActivitySourceBuilder()
            .SetTimeProvider(new MoqTimeProvider(DateTime.MinValue.ToUniversalTime()))
            .SetIdentifierGenerator(new MoqIdentGenerator(
                ActivityTraceId.CreateFromString("0af7651916cd43dd8448eb211c80319c"),
                ActivitySpanId.CreateFromString("b7ad6b7169203331")))
            .AddExporter(inMemoryExporter)
            .Build(nameof(ActivityExportTests));

        // Act
        {
            using var _ = source.Start("TestName", kind);
        }

        // Assert
        return Verify(inMemoryExporter.Items, Settings);
    }

    [Test]
    public Task GroupRelatedActivity()
    {
        // Arrange
        var inMemoryExporter = new InMemoryExport();
        var source = new TraceActivitySourceBuilder()
            .SetTimeProvider(new MoqTimeProvider(DateTime.MinValue.ToUniversalTime()))
            .SetIdentifierGenerator(new MoqIdentGenerator(
                ActivityTraceId.CreateFromString("0af7651916cd43dd8448eb211c80319c"),
                ActivitySpanId.CreateFromString("b7ad6b7169203331"),
                ActivitySpanId.CreateFromString("b8ad6b7169203331"),
                ActivitySpanId.CreateFromString("b9ad6b7169203331")
            ))
            .AddExporter(inMemoryExporter)
            .Build(nameof(ActivityExportTests));

        // Act
        {
            using var _ = source.Start("Parent");
            using var __ = source.Start("Child1");
            using var ___ = source.Start("Child2");
        }

        // Assert
        return Verify(inMemoryExporter.Items, Settings);
    }
}