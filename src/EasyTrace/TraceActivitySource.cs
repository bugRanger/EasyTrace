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

    internal static readonly TraceActivitySource Empty = new(nameof(Empty));

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

    public TraceActivityScope? Start(
        [CallerMemberName] string operationName = "",
        ActivityKind kind = ActivityKind.Internal)
    {
        if (Exporter == null)
        {
            return null;
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

        return new TraceActivityScope(activity);
    }

    public void Stop(TraceActivity activity)
    {
        try
        {
            if (activity.EndTime == TimeSpan.Zero)
            {
                activity.EndTime = TimeProvider.GetTimestamp();
            }

            Exporter?.Export(new TraceActivityRef(activity));

            if (Parent == activity)
            {
                Parent = null;
            }

            activity.Clear();
        }
        finally
        {
            TraceActivityPool.Shared.Return(activity);
        }
    }
}