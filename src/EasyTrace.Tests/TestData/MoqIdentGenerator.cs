using System.Collections;
using System.Diagnostics;
using EasyTrace.Identifier;

namespace EasyTrace.Tests.TestData;

public class MoqIdentGenerator(IEnumerator traceIds, IEnumerator spanIds) : ITraceIdentifierGenerator
{
    public static MoqIdentGenerator Set(ActivityTraceId traceId, params ActivitySpanId[] spanIds)
    {
        return new MoqIdentGenerator(new[] { traceId }.GetEnumerator(), spanIds.GetEnumerator());
    }

    public static MoqIdentGenerator Sequence(int activityCount)
    {
        const string traceIdStr = "0af7651916cd43dd8448eb211c80319c";
        const string spanIdStr = "b7ad6b7169203331";
        const string hexCharStr = "0123456789abcdef";

        var charIndex = 0;
        // count of activities, each activity will require its own traceId and spanId.
        activityCount *= 2;

        var traceIds = Enumerable.Range(0, activityCount)
            .Select(i =>
            {
                var traceIdChars = traceIdStr.ToCharArray();
                traceIdChars[i % 5 + 1] = hexCharStr[++charIndex % hexCharStr.Length];
                return ActivityTraceId.CreateFromString(new string(traceIdChars));
            })
            .ToArray();

        var spanIds = Enumerable.Range(0, activityCount)
            .Select(i =>
            {
                var spanIdChars = spanIdStr.ToCharArray();
                spanIdChars[i % 5 + 1] = hexCharStr[++charIndex % hexCharStr.Length];
                return ActivitySpanId.CreateFromString(spanIdChars);
            })
            .ToArray();

        return new MoqIdentGenerator(traceIds.GetEnumerator(), spanIds.GetEnumerator());
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
}