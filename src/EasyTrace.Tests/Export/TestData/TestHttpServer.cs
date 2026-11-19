using System.Net;
using System.Net.Sockets;
using NetCoreServer;

namespace EasyTrace.Tests.Export.TestData;

public class TestHttpServer(IPAddress address, int port, TestHttpServerCache cache) : HttpServer(address, port)
{
    protected override TcpSession CreateSession() => new TestHttpSession(this, cache);

    protected override void OnError(SocketError error) => Console.WriteLine($"HTTP session caught an error: {error}");
}