using System.Collections;
using System.Diagnostics;
using EasyTrace.Identifier;

namespace EasyTrace.Tests.TestData;

public class MoqIdentGenerator(ActivityTraceId traceId, params ActivitySpanId[] spanIds) : ITraceIdentifierGenerator
{
    private readonly IEnumerator _spanIds = spanIds.GetEnumerator();

    public void Generate(Span<byte> bytes)
    {
        switch (bytes.Length)
        {
            case 8:
                _spanIds.MoveNext();
                ((ActivitySpanId)_spanIds.Current).CopyTo(bytes);
                break;
            case 16:
                traceId.CopyTo(bytes);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(bytes.Length));
        }
    }
}