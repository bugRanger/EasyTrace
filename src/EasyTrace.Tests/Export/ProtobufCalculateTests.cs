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

        var diagnosticRef = new DiagnosticProtobufSpan(diagnosticActivity);
        var traceActivityRef = new ProtobufSpan(Migrate(diagnosticActivity));

        // Act
        var expectedSize = ProtobufCalculator.Instance.Calculate(ref diagnosticRef);
        var calculateSize = ProtobufCalculator.Instance.Calculate(ref traceActivityRef);

        // Assert
        Assert.That(calculateSize, Is.EqualTo(expectedSize));
    }

    private static TraceActivityRef Migrate(System.Diagnostics.Activity activity)
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

        return new TraceActivityRef(traceActivity);
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

    private readonly ref struct DiagnosticProtobufSpan : IProtobufSpan
    {
        private readonly Span<byte> _traceId = new byte[16];
        private readonly Span<byte> _spanId = new byte[8];
        private readonly Span<byte> _parentSpanId = new byte[8];

        private readonly IEnumerable<KeyValuePair<string, object?>> _tagObjects;
        private readonly IEnumerable<ActivityEvent> _events;
        private readonly IEnumerable<ActivityLink> _links;

        public ReadOnlySpan<byte> TraceId => _traceId;
        public ReadOnlySpan<byte> SpanId => _spanId;
        public ReadOnlySpan<byte> ParentSpanId => _parentSpanId;
        public ReadOnlySpan<char> TraceStateString { get; }
        public ReadOnlySpan<char> Name { get; }
        public ActivityKind Kind { get; }
        public DateTime StartTime { get; }
        public TimeSpan Duration { get; }
        public ActivityTraceFlags Flags { get; }
        public bool HasRemoteParent { get; }
        public ActivityStatusCode Status { get; }
        public ReadOnlySpan<char> StatusDescription { get; }

        public DiagnosticProtobufSpan(System.Diagnostics.Activity activity)
        {
            activity.TraceId.CopyTo(_traceId);
            activity.SpanId.CopyTo(_spanId);

            if (activity.ParentSpanId == default)
            {
                _parentSpanId = default;
            }
            else
            {
                activity.ParentSpanId.CopyTo(_parentSpanId);
            }

            TraceStateString = activity.TraceStateString;
            Name = activity.OperationName;
            Kind = activity.Kind;
            StartTime = activity.StartTimeUtc;
            Duration = activity.Duration;
            Flags = activity.ActivityTraceFlags;
            HasRemoteParent = activity.HasRemoteParent;
            Status = activity.Status;
            StatusDescription = activity.StatusDescription;
            _tagObjects = activity.TagObjects;
            _events = activity.Events;
            _links = activity.Links;
        }

        public int ComputeAttributes(ProtobufCalculator calculator)
        {
            var size = 0;
            foreach (var (name, value) in _tagObjects)
            {
                var valueSize = ComputeAnyValueSize(calculator, value);
                size += calculator.ComputeAttribute(name, valueSize);
            }

            return size;
        }

        public int ComputeEvents(ProtobufCalculator calculator)
        {
            var size = 0;
            foreach (var ev in _events)
            {
                var sizeTags = ComputeSizeTags(calculator, ev.Tags);
                size += calculator.ComputeEvent(ev.Timestamp.UtcDateTime, ev.Name, sizeTags);
            }

            return size;
        }

        public int ComputeLinks(ProtobufCalculator calculator)
        {
            var size = 0;

            Span<byte> traceIdBytes = stackalloc byte[16];
            Span<byte> spanIdBytes = stackalloc byte[8];

            foreach (var link in _links)
            {
                link.Context.TraceId.CopyTo(traceIdBytes);
                link.Context.SpanId.CopyTo(spanIdBytes);

                var sizeTags = ComputeSizeTags(calculator, link.Tags ?? []);
                size += calculator.ComputeLink(
                    traceIdBytes,
                    spanIdBytes,
                    link.Context.TraceState,
                    link.Context.TraceFlags,
                    link.Context.IsRemote,
                    sizeTags);
            }

            return size;
        }


        private static int ComputeSizeTags(ProtobufCalculator calculator,
            IEnumerable<KeyValuePair<string, object?>> tags)
        {
            var size = 0;

            foreach (var (name, value) in tags)
            {
                var valueSize = ComputeAnyValueSize(calculator, value);
                size += calculator.ComputeAttribute(name, valueSize);
            }

            return size;
        }

        private static int ComputeAnyValueSize(ProtobufCalculator calculator, object? value)
        {
            if (value is null) return 0;

            switch (value)
            {
                case string s:
                    return s.Length == 0 ? 0 : calculator.ComputeStringFieldSize(1, s);

                case bool:
                    return calculator.ComputeTagSize(2, calculator.WireTypeVarInt) + 1;

                case int or long or short or sbyte or byte or uint or ushort or ulong:
                    long longVal = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                    return calculator.ComputeTagSize(3, calculator.WireTypeVarInt) +
                           calculator.ComputeVarInt64Size((ulong)longVal);

                case double or float or decimal:
                    return calculator.ComputeTagSize(4, calculator.WireType64Bit) + 8;

                case Array array:
                    var arrayInnerSize = 0;
                    foreach (var item in array)
                    {
                        int elemSize = item is null ? 0 : ComputeAnyValueSize(calculator, item);
                        arrayInnerSize += calculator.ComputeTagSize(1, calculator.WireTypeLengthDelimited) +
                                          calculator.ComputeLengthDelimitedSize(elemSize);
                    }

                    return calculator.ComputeTagSize(5, calculator.WireTypeLengthDelimited) +
                           calculator.ComputeLengthDelimitedSize(arrayInnerSize);
            }

            var fallback = value?.ToString();
            return string.IsNullOrEmpty(fallback) ? 0 : calculator.ComputeStringFieldSize(1, fallback);
        }
    }
}