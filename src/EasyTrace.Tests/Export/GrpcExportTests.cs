using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using EasyTrace.Export.Batch;
using EasyTrace.Export.Otlp;
using EasyTrace.Tests.TestData;
using NetCoreServer;

namespace EasyTrace.Tests.Export;

public class GrpcExportTests
{
    private static readonly VerifySettings Settings;

    static GrpcExportTests()
    {
        Settings = new VerifySettings();
        Settings.UseDirectory("snapshots");
    }

    [Test]
    public Task Export()
    {
        var endPoint = new Uri("http://127.0.0.1:4317");

        // Start local HTTP-server.
        var server = new TestHttpServer(IPAddress.Any, endPoint.Port);
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
                .SetIdentifierGenerator(MoqIdentGenerator.Set(
                    ActivityTraceId.CreateFromString("0af7651916cd43dd8448eb211c80319c"),
                    ActivitySpanId.CreateFromString("b7ad6b7169203331"),
                    ActivitySpanId.CreateFromString("b9ad6b7169203331")
                ))
                .SetBatchExportOptions(new BatchExportOptions
                {
                    MaxExportBatchSize = 2,
                    ScheduledDelayMilliseconds = uint.MaxValue,
                })
                .AddGrpcExporter(new GrpcExportParameters
                {
                    EndPoint = endPoint,
                    BufferSize = 1024 * 4,
                })
                .Build(nameof(ActivityExportTests));

            // Make activity for batch export.
            {
                using var _ = source.Start();
                using var __ = source.Start();
                Task.Delay(100).Wait();
            }

            // Wait sending.
            Task.Delay(500).Wait();
            // Check activity export in batch.
            return Verify(server.RequestCache.GetAllCache(), Settings);
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

class TestHttpSession(HttpServer server) : HttpSession(server)
{
    public class RequestCache
    {
        public static RequestCache GetInstance()
        {
            _instance ??= new RequestCache();
            return _instance;
        }

        public void Set(string key, byte[] value)
        {
            _cache[key] = value;
        }

        public string GetAllCache()
        {
            var result = new StringBuilder();
            result.Append("[\n");
            foreach (var item in _cache)
            {
                result.Append("  {\n");
                result.AppendFormat($"    \"key\": \"{item.Key}\",\n");
                result.AppendFormat($"    \"value\": \"{string.Join(", ", item.Value)}\",\n");
                result.Append("  },\n");
            }

            result.Append("]\n");
            return result.ToString();
        }

        private readonly ConcurrentDictionary<string, byte[]> _cache = new();
        private static RequestCache? _instance;
    }

    protected override void OnReceivedRequest(HttpRequest request)
    {
        if (request.Method == "POST")
        {
            var key = request.Url;
            var value = request.BodyBytes;

            // Decode the key value
            key = Uri.UnescapeDataString(key);

            RequestCache.GetInstance().Set(key, value);
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

class TestHttpServer(IPAddress address, int port) : HttpServer(address, port)
{
    public TestHttpSession.RequestCache RequestCache => TestHttpSession.RequestCache.GetInstance();

    protected override TcpSession CreateSession() => new TestHttpSession(this);

    protected override void OnError(SocketError error) => Console.WriteLine($"HTTP session caught an error: {error}");
}