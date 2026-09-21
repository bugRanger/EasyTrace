using System;
using System.Net.Sockets;
using System.Threading;
using BenchmarkDotNet.Configs;

namespace EasyTrace.Benchmarks.Metrics;

public class SocketDiagnoser
{
    private const string MetricPrefix = $"{nameof(SocketDiagnoser)}";
    private const string SendPrefix = $"[{MetricPrefix}-{nameof(Socket.Send)}]:";

    private long _sendCount;
    
    public static IConfig AddColumns(IConfig config)
    {
        return config.AddColumn(new SocketSendColumn(SendPrefix));
    }

    public void Report()
    {
        Console.WriteLine($"{SendPrefix}{_sendCount}");
        _sendCount = 0;
    }

    internal void IncrementSend() => Interlocked.Increment(ref _sendCount);
}