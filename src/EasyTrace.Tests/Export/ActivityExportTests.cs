using System.Diagnostics;
using EasyTrace.Activity;
using EasyTrace.Export.Batch;
using EasyTrace.Tests.TestData;

namespace EasyTrace.Tests.Export;

[TestFixture]
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
            .SetTimeProvider(new MoqTimeProvider())
            .SetIdentifierGenerator(MoqIdentGenerator.Set(
                ActivityTraceId.CreateFromString("0af7651916cd43dd8448eb211c80319c"),
                ActivitySpanId.CreateFromString("b7ad6b7169203331")))
            .SetBatchExportOptions(new BatchExportOptions
            {
                MaxExportSize = 1,
                ScheduledDelayMilliseconds = uint.MaxValue,
            })
            .AddExporter(inMemoryExporter)
            .Build(nameof(ActivityExportTests));

        // Act
        {
            using var _ = source.Start("TestName", kind);
        }

        // Assert
        Task.Delay(500).Wait();
        return Verify(inMemoryExporter.Items, Settings);
    }

    [Test]
    public Task GroupActivity()
    {
        // Arrange
        var inMemoryExporter = new InMemoryExport();
        var source = new TraceActivitySourceBuilder()
            .SetTimeProvider(new MoqTimeProvider())
            .SetIdentifierGenerator(MoqIdentGenerator.Set(
                ActivityTraceId.CreateFromString("0af7651916cd43dd8448eb211c80319c"),
                ActivitySpanId.CreateFromString("b7ad6b7169203331"),
                ActivitySpanId.CreateFromString("b8ad6b7169203331"),
                ActivitySpanId.CreateFromString("b9ad6b7169203331")
            ))
            .SetBatchExportOptions(new BatchExportOptions
            {
                MaxExportSize = 3,
                ScheduledDelayMilliseconds = uint.MaxValue,
            })
            .AddExporter(inMemoryExporter)
            .Build(nameof(ActivityExportTests));

        // Act
        {
            using var _ = source.Start("Parent");
            using var __ = source.Start("Child1");
            using var ___ = source.Start("Child2");
        }

        // Assert
        Task.Delay(500).Wait();
        return Verify(inMemoryExporter.Items, Settings);
    }

    [Test]
    public Task SingleBatchActivity()
    {
        // Arrange
        var inMemoryExporter = new InMemoryExport();
        var source = new TraceActivitySourceBuilder()
            .SetTimeProvider(new MoqTimeProvider())
            .SetIdentifierGenerator(MoqIdentGenerator.Sequence(2))
            .SetBatchExportOptions(new BatchExportOptions
            {
                MaxExportSize = 2,
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
            .SetTimeProvider(new MoqTimeProvider())
            .SetIdentifierGenerator(MoqIdentGenerator.Sequence(20))
            .SetBatchExportOptions(new BatchExportOptions
            {
                MaxExportSize = 2,
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
            .SetTimeProvider(new MoqTimeProvider())
            .SetIdentifierGenerator(MoqIdentGenerator.Sequence(2))
            .SetBatchExportOptions(new BatchExportOptions
            {
                MaxExportSize = uint.MaxValue,
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
            .SetTimeProvider(new MoqTimeProvider())
            .SetIdentifierGenerator(MoqIdentGenerator.Sequence(2))
            .SetBatchExportOptions(new BatchExportOptions
            {
                MaxExportSize = uint.MaxValue,
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

    [Test]
    public Task SingleActivityWithAttributes()
    {
        // Arrange
        var inMemoryExporter = new InMemoryExport();
        var source = new TraceActivitySourceBuilder()
            .SetLimits(new TraceActivityLimits
            {
                AttributeCount = 3,
                AttributeNameLen = 255,
                AttributeValueLen = AttributeSymbols.Length,
            })
            .SetTimeProvider(new MoqTimeProvider())
            .SetIdentifierGenerator(MoqIdentGenerator.Set(
                ActivityTraceId.CreateFromString("0af7651916cd43dd8448eb211c80319c"),
                ActivitySpanId.CreateFromString("b7ad6b7169203331")))
            .SetBatchExportOptions(new BatchExportOptions
            {
                MaxExportSize = 1,
                ScheduledDelayMilliseconds = uint.MaxValue,
            })
            .AddExporter(inMemoryExporter)
            .Build(nameof(ActivityExportTests));

        // Act
        {
            using var scope = source.Start("TestName");
            scope?.SetAttribute("tag_string", AttributeSymbols);
            scope?.SetAttribute("tag_double", 1.123456789);
            scope?.SetAttribute("tag_int", 123456789);
            // > dropped: 2
            scope?.SetAttribute("tag_drop1", "dropped");
            scope?.SetAttribute("tag_drop2", "dropped");
        }

        // Assert
        Task.Delay(500).Wait();
        return Verify(inMemoryExporter.Items, Settings);
    }

    [Test]
    public Task GroupActivityWithAttributes()
    {
        // Arrange
        var inMemoryExporter = new InMemoryExport();
        var source = new TraceActivitySourceBuilder()
            .SetLimits(new TraceActivityLimits
            {
                AttributeCount = 3,
                AttributeNameLen = 255,
                AttributeValueLen = AttributeSymbols.Length,
            })
            .SetTimeProvider(new MoqTimeProvider())
            .SetIdentifierGenerator(MoqIdentGenerator.Set(
                ActivityTraceId.CreateFromString("0af7651916cd43dd8448eb211c80319c"),
                ActivitySpanId.CreateFromString("b7ad6b7169203331"),
                ActivitySpanId.CreateFromString("b8ad6b7169203331"),
                ActivitySpanId.CreateFromString("b9ad6b7169203331")
            ))
            .SetBatchExportOptions(new BatchExportOptions
            {
                MaxExportSize = 3,
                ScheduledDelayMilliseconds = uint.MaxValue,
            })
            .AddExporter(inMemoryExporter)
            .Build(nameof(ActivityExportTests));

        // Act
        {
            using var parentScope = source.Start("Parent");
            // > A string equal to the established limit
            parentScope?.SetAttribute("tag_string", AttributeSymbols);
            parentScope?.SetAttribute("tag_double", 1.123456789);
            parentScope?.SetAttribute("tag_int", 123456789);
            // > dropped: 1
            parentScope?.SetAttribute("tag_drop1", "dropped");
            using var childScope1 = source.Start("Child1");
            // > A string is shorter than the limit.
            childScope1?.SetAttribute("tag_string", $"{AttributeSymbols.AsMemory(0, AttributeSymbols.Length / 2)}");
            childScope1?.SetAttribute("tag_double", 2.123456789);
            childScope1?.SetAttribute("tag_int", 213456789);
            // > dropped: 2
            childScope1?.SetAttribute("tag_drop1", "dropped");
            childScope1?.SetAttribute("tag_drop2", "dropped");
            using var childScope2 = source.Start("Child2");
            // > A string exceeds the established limit
            childScope2?.SetAttribute("tag_string",
                $"{AttributeSymbols.AsMemory(AttributeSymbols.Length / 2)}{AttributeSymbols}");
            childScope2?.SetAttribute("tag_double", 3.123456789);
            childScope2?.SetAttribute("tag_int", 312456789);
            // > dropped: 3
            childScope2?.SetAttribute("tag_drop1", "dropped");
            childScope2?.SetAttribute("tag_drop2", "dropped");
            childScope2?.SetAttribute("tag_drop2", "dropped");
        }

        // Assert
        Task.Delay(500).Wait();
        return Verify(inMemoryExporter.Items, Settings);
    }

    private const string AttributeSymbols =
        " !\"#$%&'()*+,-./0123456789:;<=>?@" +
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" +
        "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрствуфхцчшщъыьэюя" +
        "₽$€¥©®™№★✔❌" +
        "😀😃😄😁😆😅😂🤣";
}