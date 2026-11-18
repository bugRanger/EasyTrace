using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using EasyTrace.Export.Batch;
using EasyTrace.Export.Otlp.Http;
using EasyTrace.Tests.TestData;
using NetCoreServer;

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
        var endPoint = new Uri("http://127.0.0.1:4318");

        // Start local HTTP-server.
        var cache = new TestRequestCache();
        var server = new TestHttpServer(IPAddress.Any, endPoint.Port, cache);
        Assert.True(server.Start());
        while (!server.IsStarted)
        {
            Thread.Yield();
        }

        try
        {
            // Make activity source.
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
                    BufferSize = 1024 * 4,
                })
                .Build(nameof(ActivityExportTests));

            // Make activity for batch export.
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

            // Wait sending.
            Task.Delay(500).Wait();
            // Check activity export in batch.
            return Verify(cache.GetValues(), Settings);
        }
        finally
        {
            // Stop HTTP-server.
            Assert.True(server.Stop());
            while (server.IsStarted)
            {
                Thread.Yield();
            }
        }
    }

    [Test]
    public Task HttpExportWithTwoSource()
    {
        var endPoint = new Uri("http://127.0.0.1:4318");

        // Start local HTTP-server.
        var cache = new TestRequestCache();
        var server = new TestHttpServer(IPAddress.Any, endPoint.Port, cache);
        Assert.True(server.Start());
        while (!server.IsStarted)
        {
            Thread.Yield();
        }

        try
        {
            // Make builder.
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
                    BufferSize = 1024 * 4,
                });

            // Make activity source.
            var source1 = builder.Build("Source1");
            var source2 = builder.Build("Source2");

            // Make activity for batch export.
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

            // Wait sending.
            Task.Delay(500).Wait();
            // Check activity export in batch.
            return Verify(cache.GetValues(), Settings);
        }
        finally
        {
            // Stop HTTP-server.
            Assert.True(server.Stop());
            while (server.IsStarted)
            {
                Thread.Yield();
            }
        }
    }
}

public class TestHttpSession(HttpServer server, TestRequestCache cache) : HttpSession(server)
{
    protected override void OnReceivedRequest(HttpRequest request)
    {
        if (request.Method == "POST")
        {
            var key = request.Url;
            var value = request.BodyBytes;

            // Decode the key value
            key = Uri.UnescapeDataString(key);

            cache.Set(key, value);
            SendResponseAsync(Response.MakeOkResponse());
        }
        else
        {
            SendResponseAsync(Response.MakeErrorResponse("Unsupported HTTP method: " + request.Method));
        }
    }

    protected override void OnReceivedRequestError(HttpRequest request, string error)
    {
        Console.WriteLine($"Request error: {error}");
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"HTTP session caught an error: {error}");
    }
}

public class TestHttpServer(IPAddress address, int port, TestRequestCache cache) : HttpServer(address, port)
{
    protected override TcpSession CreateSession() => new TestHttpSession(this, cache);

    protected override void OnError(SocketError error) => Console.WriteLine($"HTTP session caught an error: {error}");
}

public class TestRequestCache
{
    private readonly ConcurrentDictionary<string, List<string>> _cache = new();

    public void Set(string key, byte[] value)
    {
        var valueStr = Convert.ToHexStringLower(value);
        if (_cache.TryGetValue(key, out var list))
        {
            list.Add(valueStr);
            return;
        }

        _cache[key] = [valueStr];
    }

    public string GetValues()
    {
        var result = new StringBuilder();
        result.Append("[\n");
        foreach (var item in _cache)
        {
            result.Append("  {\n");
            result.AppendFormat($"    \"key\": \"{item.Key}\",\n");
            result.AppendFormat($"    \"value\": \"{string.Join(", \n", item.Value)}\",\n");
            result.Append("  },\n");
        }

        result.Append("]\n");
        return result.ToString();
    }
}