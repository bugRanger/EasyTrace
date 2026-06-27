using EasyTrace.Activity;
using EasyTrace.Export;
using EasyTrace.Identifier;
using EasyTrace.Identifier.Generator;
using EasyTrace.Time;

namespace EasyTrace;

// TODO: Configure sampler.
// TODO: Configure batcher.
// TODO: Add resources.
// TODO: Add exporters.
public class TraceActivitySource(
    string name,
    Version? version = null,
    ITraceTimeProvider? traceTimeProvider = null,
    ITraceIdentifierGenerator? identifierProvider = null)
{
    private static readonly ThreadLocal<TraceActivity?> CurrentThreadLocal = new();

    private ITraceTimeProvider TimeProvider { get; } = traceTimeProvider ?? new TraceTimeProvider();
    private ITraceIdentifierGenerator IdentifierGenerator { get; } = identifierProvider ?? new Xoshiro256PlusPlus();

    public string Name { get; } = name;

    public string? Version { get; } = version?.ToString();

    public ITraceActivityExporter? Exporter { get; set; }

    public static TraceActivity? Parent
    {
        get => CurrentThreadLocal.Value;
        private set => CurrentThreadLocal.Value = value;
    }

    public TraceActivityRef Start(string operationName = "")
    {
        if (Exporter == null)
        {
            return new TraceActivityRef(this, TraceActivity.Empty);
        }

        var activity = TraceActivityPool.Shared.Rent();

        if (Parent == null)
        {
            activity.TraceId.Generate(IdentifierGenerator);
        }
        else
        {
            activity.TraceId.CopyFrom(Parent.TraceId);
        }

        activity.Source = this;
        activity.OperationName = operationName;
        activity.SpanId.Generate(IdentifierGenerator);
        activity.StartTime = TimeProvider.GetTimestamp();
        activity.Recorded = true;

        Parent ??= activity;

        return new TraceActivityRef(this, activity);
    }

    public void Stop(scoped in TraceActivityRef activityRef, TraceActivity activity)
    {
        activity.EndTime = TimeProvider.GetTimestamp();

        Exporter?.Export(activityRef);

        if (Parent == activity)
        {
            Parent = null;
        }

        activity.Clear();

        TraceActivityPool.Shared.Return(activity);
    }
}

// public ref struct TraceData
// {
// }
//
// public interface TraceExporter
// {
//     void Export(scoped TraceData trace);
// }