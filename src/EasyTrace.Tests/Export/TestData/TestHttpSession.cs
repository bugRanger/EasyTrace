using System.Net.Sockets;
using NetCoreServer;

namespace EasyTrace.Tests.Export.TestData;

public class TestHttpSession(HttpServer server, TestHttpServerCache cache) : HttpSession(server)
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