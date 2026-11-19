using System.Net;

namespace EasyTrace.Tests.Export.TestData;

public ref struct TestHttpServerRunner : IDisposable
{
    private TestHttpServer? _server;

    public TestHttpServerCache Cache { get; } = new();

    public TestHttpServerRunner(IPAddress address, int port)
    {
        var server = new TestHttpServer(address, port, Cache);
        Assert.True(server.Start());
        while (!server.IsStarted)
        {
            Thread.Yield();
        }

        _server = server;
    }

    public void Dispose()
    {
        // Stop HTTP-server.
        if (_server == null) return;

        Assert.True(_server.Stop());
        while (_server.IsStarted)
        {
            Thread.Yield();
        }

        _server.Dispose();
        _server = null;
    }
}