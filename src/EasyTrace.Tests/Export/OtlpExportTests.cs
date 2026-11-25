using System.Net;
using EasyTrace.Export.Batch;
using EasyTrace.Export.Otlp.Http;
using EasyTrace.Tests.Export.TestData;
using EasyTrace.Tests.TestData;

namespace EasyTrace.Tests.Export;

public class OtlpExportTests
{
    private static readonly VerifySettings Settings;

    static OtlpExportTests()
    {
        Settings = new VerifySettings();
        Settings.UseDirectory("snapshots");
    }

    [Test]
    public Task HttpExportWithOneSource()
    {
        // Arrange
        var endPoint = new Uri("http://127.0.0.1:4318");
        var server = new TestHttpServerRunner(IPAddress.Any, endPoint.Port);

        try
        {
            // - Make activity source.
            var source = new TraceActivitySourceBuilder()
                .SetTimeProvider(new MoqTimeProvider())
                .SetIdentifierGenerator(MoqIdentGenerator.Sequence(9))
                .SetResources(new Dictionary<string, string>
                {
                    ["telemetry.sdk.name"] = "easytrace",
                    ["telemetry.sdk.language"] = "dotnet",
                    ["telemetry.sdk.version"] = "1.0.0",
                    ["service.name"] = "unknown",
                })
                .SetBatchExportOptions(new BatchExportOptions
                {
                    MaxExportSize = 3,
                })
                .AddOtlpExporter(new HttpExportParameters
                {
                    EndPoint = endPoint,
                    BufferCapacity = 1024 * 4,
                })
                .Build(nameof(ActivityExportTests));

            // Act
            // - Make activity for batch export.
            {
                {
                    using (var _ = source.Start()) {}
                    using (var _ = source.Start()) {}
                    using (var _ = source.Start()) {}
                }
                Task.Delay(100).Wait();

                {
                    using var _ = source.Start();
                    using (var __ = source.Start()) {}
                    using (var __ = source.Start()) {}
                }
                Task.Delay(100).Wait();

                {
                    using var _ = source.Start();
                    using var __ = source.Start();
                    using var ___ = source.Start();
                }
                Task.Delay(100).Wait();
            }

            // Assert
            // - Wait sending.
            Task.Delay(500).Wait();
            // - Check activity export in batch.
            return Verify(server.Cache.GetValues(), Settings);
        }
        finally
        {
            server.Dispose();
        }
    }

    [Test]
    public Task HttpExportWithTwoSource()
    {
        // Arrange
        var endPoint = new Uri("http://127.0.0.1:4318");
        var server = new TestHttpServerRunner(IPAddress.Any, endPoint.Port);

        try
        {
            // - Make builder.
            var builder = new TraceActivitySourceBuilder()
                .SetTimeProvider(new MoqTimeProvider())
                .SetIdentifierGenerator(MoqIdentGenerator.Sequence(6))
                .SetResources(new Dictionary<string, string>
                {
                    ["telemetry.sdk.name"] = "easytrace",
                    ["telemetry.sdk.language"] = "dotnet",
                    ["telemetry.sdk.version"] = "1.0.0",
                    ["service.name"] = "unknown",
                })
                .SetBatchExportOptions(new BatchExportOptions
                {
                    MaxExportSize = 3,
                })
                .AddOtlpExporter(new HttpExportParameters
                {
                    EndPoint = endPoint,
                    BufferCapacity = 1024 * 4,
                });

            // - Make activity source.
            var source1 = builder.Build("Source1");
            var source2 = builder.Build("Source2");

            // Act
            // - Make activity for batch export.
            {
                {
                    using var _ = source1.Start();
                    using var __ = source2.Start();
                    using var ___ = source2.Start();
                }
                Task.Delay(100).Wait();

                {
                    using var _ = source2.Start();
                    using var __ = source1.Start();
                    using var ___ = source1.Start();
                }
                Task.Delay(100).Wait();
            }

            // Assert
            // - Wait sending.
            Task.Delay(500).Wait();
            // - Check activity export in batch.
            return Verify(server.Cache.GetValues(), Settings);
        }
        finally
        {
            server.Dispose();
        }
    }
    
    [Test]
    public Task HttpExportWithHighStressLoad()
    {
        // Arrange
        var endPoint = new Uri("http://127.0.0.1:4318");
        var server = new TestHttpServerRunner(IPAddress.Any, endPoint.Port);

        try
        {
            // - Make activity source.
            var source = new TraceActivitySourceBuilder()
                .SetTimeProvider(new MoqTimeProvider())
                .SetIdentifierGenerator(MoqIdentGenerator.Sequence(500))
                .SetBatchExportOptions(new BatchExportOptions
                {
                    ScheduledDelay = TimeSpan.FromMilliseconds(200),
                })
                .AddOtlpExporter(new HttpExportParameters
                {
                    EndPoint = endPoint,
                    BufferCapacity = 1024,
                })
                .Build(nameof(HttpExportWithHighStressLoad));

            // Act
            for (var i = 0; i < 500; i++)
            {
                using var activity = source.Start();
                activity.SetAttribute("http.url", "https://example.com/some/path");
            }

            // Assert
            Task.Delay(1500).Wait();
            // - Check activity export in batch.
            return Verify(server.Cache.GetValues(), Settings);
        }
        finally
        {
            server.Dispose();
        }
    }
}