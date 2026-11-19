using System.Net;
using EasyTrace.Export.Batch;
using EasyTrace.Export.Otlp.Http;
using EasyTrace.Tests.Export.TestData;
using EasyTrace.Tests.TestData;

namespace EasyTrace.Tests.Export;

public class StressExportTests
{
    private static readonly VerifySettings Settings;

    static StressExportTests()
    {
        Settings = new VerifySettings();
        Settings.UseDirectory("snapshots");
    }

    [Test]
    [TestCase(0)]
    [TestCase(1024)]
    public Task Stress1(int resizeFrame)
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
                    BufferSize = 1024,
                    ResizeFrame = resizeFrame,
                })
                .Build(nameof(StressExportTests));

            // Act
            for (var i = 0; i < 500; i++)
            {
                using var activity = source.Start();
                activity?.SetAttribute("http.url", "https://example.com/some/path");
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