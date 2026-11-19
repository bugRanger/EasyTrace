using System.Diagnostics;
using System.Globalization;
using EasyTrace.Activity;
using EasyTrace.Export.Otlp.Protobuf;

namespace EasyTrace.Tests.Export;

public class ProtobufCalculateTests
{
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void CalculateSpanSize(bool addTags)
    {
        // Arrange
        var source = SetupActivitySource();
        using var diagnosticActivity = source.StartActivity(nameof(ProtobufCalculateTests));
        diagnosticActivity!.SetStartTime(new DateTime(2020, 1, 1));
        diagnosticActivity.SetEndTime(new DateTime(2020, 1, 2));
        if (addTags)
        {
            diagnosticActivity.SetTag("tag_string", "https://example.com/some/path");
            diagnosticActivity.SetTag("tag_double", (double)1.123456789);
            diagnosticActivity.SetTag("tag_long", (long)123456789ul);
            diagnosticActivity.SetTag("tag_bool", (bool)true);
        }

        diagnosticActivity.Stop();

        var traceActivity = Migrate(diagnosticActivity);

        // Act
        var expectedSize = ProtobufCalculator.CalculateSpanSize(diagnosticActivity);
        var calculateSize = ProtobufCalculator.CalculateSpanSize(new TraceActivityRef(traceActivity));

        // Assert
        Assert.That(calculateSize, Is.EqualTo(expectedSize));
    }

    private static TraceActivity Migrate(System.Diagnostics.Activity activity)
    {
        var traceActivity = new TraceActivity(new TraceActivityLimits())
        {
            OperationName = activity.DisplayName,
            Kind = activity.Kind,
            StartTime = activity.StartTimeUtc,
            EndTime = activity.StartTimeUtc.Add(activity.Duration),
            Recorded = activity.Recorded,
            RemoteParent = activity.HasRemoteParent,
        };

        activity.TraceId.CopyTo(traceActivity.TraceId.AsSpan());
        activity.SpanId.CopyTo(traceActivity.SpanId.AsSpan());

        if (activity.ParentSpanId != default)
        {
            activity.ParentSpanId.CopyTo(traceActivity.ParentId.AsSpan());
        }

        foreach (var tag in activity.TagObjects)
        {
            switch (tag.Value)
            {
                case string value:
                    traceActivity.Attributes.Add(tag.Key, value);
                    break;

                case bool value:
                    traceActivity.Attributes.Add(tag.Key, value);
                    break;

                case double value:
                    traceActivity.Attributes.Add(tag.Key, value);
                    break;

                case long or ulong or int or uint or short or ushort or sbyte or byte:
                    long longVal = Convert.ToInt64(tag.Value, CultureInfo.InvariantCulture);
                    traceActivity.Attributes.Add(tag.Key, longVal);
                    break;
            }
        }

        return traceActivity;
    }

    private static ActivitySource SetupActivitySource()
    {
        ActivitySource.AddActivityListener(new ActivityListener
        {
            ShouldListenTo = s => s.Name == nameof(ProtobufCalculator),
            Sample = (ref _) => ActivitySamplingResult.AllData,
        });

        return new ActivitySource(nameof(ProtobufCalculator));
    }
}