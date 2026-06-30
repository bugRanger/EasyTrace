using System.Diagnostics;
using System.Runtime.CompilerServices;
using EasyTrace.Activity;
using EasyTrace.Export;
using EasyTrace.Identifier;
using EasyTrace.Identifier.Generator;
using EasyTrace.Time;

namespace EasyTrace;

public class TraceActivitySource(string name, Version? version = null)
{
    private static readonly ThreadLocal<TraceActivity?> ParentActivityByThreadLocal = new();

    internal ITraceTimeProvider TimeProvider { get; init; } = new TraceTimeProvider();
    internal ITraceIdentifierGenerator IdentifierGenerator { get; init; } = new Xoshiro256PlusPlus();
    internal ITraceActivityExporter? Exporter { get; init; }

    private static TraceActivity? Parent
    {
        get => ParentActivityByThreadLocal.Value;
        set => ParentActivityByThreadLocal.Value = value;
    }

    public string Name { get; } = name;

    public string? Version { get; } = version?.ToString();

    public TraceActivityRef Start([CallerMemberName] string operationName = "", ActivityKind kind = ActivityKind.Internal)
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
        activity.Kind = kind;
        activity.OperationName = operationName;
        activity.SpanId.Generate(IdentifierGenerator);
        activity.StartTime = TimeProvider.GetTimestamp();
        activity.Recorded = true;

        Parent ??= activity;

        return new TraceActivityRef(this, activity);
    }

    public void Stop(scoped in TraceActivityRef activityRef, TraceActivity activity)
    {
        if (activity.EndTime == TimeSpan.Zero)
        {
            activity.EndTime = TimeProvider.GetTimestamp();
        }

        Exporter?.Export(activityRef);

        if (Parent == activity)
        {
            Parent = null;
        }

        activity.Clear();

        TraceActivityPool.Shared.Return(activity);
    }
}