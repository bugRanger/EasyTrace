using System;
using System.Collections;
using System.Diagnostics;
using EasyTrace.Identifier;

namespace EasyTrace.Benchmarks.TestData;

internal class FakeIdentGenerator(IEnumerator traceIds, IEnumerator spanIds) : ITraceIdentifierGenerator
{
    public static FakeIdentGenerator Infinity()
    {
        return new FakeIdentGenerator(Cycle(ActivityTraceId.CreateRandom()), Cycle(ActivitySpanId.CreateRandom()));
    }
    
    public void Generate(Span<byte> bytes)
    {
        switch (bytes.Length)
        {
            case 8:
                spanIds.MoveNext();
                ((ActivitySpanId)spanIds.Current).CopyTo(bytes);
                break;
            case 16:
                traceIds.MoveNext();
                ((ActivityTraceId)traceIds.Current).CopyTo(bytes);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(bytes.Length));
        }
    }
    
    private static IEnumerator Cycle<T>(T item)
    {
        while (true)
        {
            yield return item;
        }
    }
}