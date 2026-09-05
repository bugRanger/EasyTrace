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
        // A maximum of 48 non-duplicate changes are allowed, after which duplicates begin.
        // This should be sufficient for testing
        const string saltCharStr = "0123456789abcdeffedcba987654321098765fedcba43210";

        var charIndex = 0;

        var traceIds = Enumerable.Range(0, activityCount)
            .Select(i =>
            {
                var traceIdChars = traceIdStr.ToCharArray();
                var charSelected = i % traceIdStr.Length;
                traceIdChars[charSelected == 0 ? 1 : charSelected] = saltCharStr[++charIndex % saltCharStr.Length];
                return ActivityTraceId.CreateFromString(traceIdChars);
            })
            .ToArray();

        charIndex = 0;
        var spanIds = Enumerable.Range(0, activityCount)
            .Select(i =>
            {
                var spanIdChars = spanIdStr.ToCharArray();
                var charSelected = i % spanIdStr.Length;
                spanIdChars[charSelected == 0 ? 1 : charSelected] = saltCharStr[++charIndex % saltCharStr.Length];
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