using System.Diagnostics;
using EasyTrace.Activity;
using EasyTrace.Export.Otlp.Protobuf;
using EasyTrace.Tests.TestData;

namespace EasyTrace.Tests.Export;

[TestFixture]
public class ProtobufSerializeTests
{
    private static readonly VerifySettings Settings;

    static ProtobufSerializeTests()
    {
        Settings = new VerifySettings();
        Settings.UseDirectory("snapshots");
    }

    [Test]
    public Task SourceWithResources()
    {
        // Arrange
        var moqIdentGenerator = MoqIdentGenerator.Set(
            ActivityTraceId.CreateFromString("0af7651916cd43dd8448eb211c80319c"),
            ActivitySpanId.CreateFromString("b7ad6b7169203331"));

        var sourceEmpty = new TraceActivitySourceBuilder()
            .SetResources(new Dictionary<string, string>
            {
                ["telemetry.sdk.name"] = "easytrace",
                ["telemetry.sdk.language"] = "dotnet",
                ["telemetry.sdk.version"] = "1.0.0",
                ["service.name"] = "unknown:dotnet",
            })
            .Build("TestSource");

        var serializer = new ProtobufSerializer(new ProtobufStream(1_024), sourceEmpty);
        var activity = new TraceActivity
        {
            OperationName = "TestActivity",
            Kind = ActivityKind.Internal,
            StartTime = DateTime.MinValue,
            EndTime = DateTime.MinValue.AddSeconds(1),
            Recorded = true,
        };
        activity.TraceId.Generate(moqIdentGenerator);
        activity.SpanId.Generate(moqIdentGenerator);

        // Act
        serializer.Write(new TraceActivityRef(activity));

        // Assert - verify snapshot.
        return Verify(Convert.ToBase64String(serializer.Flush()), Settings);
    }

    [Test]
    public Task SourceWithoutResources()
    {
        // Arrange
        var moqIdentGenerator = MoqIdentGenerator.Set(
            ActivityTraceId.CreateFromString("0af7651916cd43dd8448eb211c80319c"),
            ActivitySpanId.CreateFromString("b7ad6b7169203331"));

        var sourceEmpty = new TraceActivitySourceBuilder()
            .SetResources([])
            .Build("TestSource");

        var serializer = new ProtobufSerializer(new ProtobufStream(1_024), sourceEmpty);
        var activity = new TraceActivity
        {
            OperationName = "TestActivity",
            Kind = ActivityKind.Internal,
            StartTime = DateTime.MinValue.AddTicks(200),
            EndTime = DateTime.MinValue.AddSeconds(1),
            Recorded = true,
        };
        activity.TraceId.Generate(moqIdentGenerator);
        activity.SpanId.Generate(moqIdentGenerator);

        // Act
        serializer.Write(new TraceActivityRef(activity));

        // Assert - verify snapshot.
        return Verify(Convert.ToBase64String(serializer.Flush()), Settings);
    }
}