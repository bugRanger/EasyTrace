using System.Diagnostics;
using EasyTrace.Export.Batch;
using EasyTrace.Tests.TestData;

namespace EasyTrace.Tests.Export;

public class ActivityExportTests
{
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
            .SetIdentifierGenerator(MoqIdentGenerator.Set(
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
    public Task GroupActivity()
    {
        // Arrange
        var inMemoryExporter = new InMemoryExport();
        var source = new TraceActivitySourceBuilder()
            .SetTimeProvider(new MoqTimeProvider(DateTime.MinValue.ToUniversalTime()))
            .SetIdentifierGenerator(MoqIdentGenerator.Set(
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

    [Test]
    public Task SingleBatchActivity()
    {
        // Arrange
        var inMemoryExporter = new InMemoryExport();
        var source = new TraceActivitySourceBuilder()
            .SetTimeProvider(new MoqTimeProvider(DateTime.MinValue.ToUniversalTime()))
            .SetIdentifierGenerator(MoqIdentGenerator.Sequence(1))
            .SetBatchExportOptions(new BatchExportOptions
            {
                MaxExportBatchSize = 2,
                ScheduledDelayMilliseconds = uint.MaxValue,
            })
            .AddExporter(inMemoryExporter)
            .Build(nameof(ActivityExportTests));

        // Act
        {
            using var _ = source.Start("Parent");
            using var __ = source.Start("Child1");
        }

        // Assert
        // - wait for batch processing from another thread to complete.
        Task.Delay(500).Wait();
        return Verify(inMemoryExporter.Items, Settings);
    }

    [Test]
    public Task MultiBatchActivity()
    {
        // Arrange
        var inMemoryExporter = new InMemoryExport();
        var source = new TraceActivitySourceBuilder()
            .SetTimeProvider(new MoqTimeProvider(DateTime.MinValue.ToUniversalTime()))
            .SetIdentifierGenerator(MoqIdentGenerator.Sequence(10))
            .SetBatchExportOptions(new BatchExportOptions
            {
                MaxExportBatchSize = 2,
                ScheduledDelayMilliseconds = uint.MaxValue,
            })
            .AddExporter(inMemoryExporter)
            .Build(nameof(ActivityExportTests));

        // Act
        for (var i = 0; i < 10; i++)
        {
            using var _ = source.Start("Parent");
            using var __ = source.Start("Child1");
        }

        // Assert
        // - wait for batch processing from another thread to complete.
        Task.Delay(500).Wait();
        return Verify(inMemoryExporter.Items, Settings);
    }

    [Test]
    public Task ScheduledBatchDisable()
    {
        // Arrange
        var inMemoryExporter = new InMemoryExport();
        var source = new TraceActivitySourceBuilder()
            .SetTimeProvider(new MoqTimeProvider(DateTime.MinValue.ToUniversalTime()))
            .SetIdentifierGenerator(MoqIdentGenerator.Sequence(2))
            .SetBatchExportOptions(new BatchExportOptions
            {
                MaxExportBatchSize = uint.MaxValue,
                ScheduledDelayMilliseconds = uint.MaxValue,
            })
            .AddExporter(inMemoryExporter)
            .Build(nameof(ActivityExportTests));

        // Act
        {
            using var _ = source.Start("Parent");
            using var __ = source.Start("Child1");
        }

        // Assert
        // - wait for batch processing from another thread to complete.
        Task.Delay(500).Wait();
        return Verify(inMemoryExporter.Items, Settings);
    }

    [Test]
    public Task ScheduledBatchEnable()
    {
        // Arrange
        var scheduledDelayMilliseconds = TimeSpan.FromMilliseconds(500);

        var inMemoryExporter = new InMemoryExport();
        var source = new TraceActivitySourceBuilder()
            .SetTimeProvider(new MoqTimeProvider(DateTime.MinValue.ToUniversalTime()))
            .SetIdentifierGenerator(MoqIdentGenerator.Sequence(2))
            .SetBatchExportOptions(new BatchExportOptions
            {
                MaxExportBatchSize = uint.MaxValue,
                ScheduledDelayMilliseconds = (uint)scheduledDelayMilliseconds.Milliseconds,
            })
            .AddExporter(inMemoryExporter)
            .Build(nameof(ActivityExportTests));

        // Act
        {
            using var _ = source.Start("Parent");
            using var __ = source.Start("Child1");
        }

        // Assert
        // - wait for batch processing from another thread to complete.
        Task.Delay(scheduledDelayMilliseconds.Multiply(2)).Wait();
        return Verify(inMemoryExporter.Items, Settings);
    }
}