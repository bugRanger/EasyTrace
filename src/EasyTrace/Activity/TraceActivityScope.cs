namespace EasyTrace.Activity;

public readonly struct TraceActivityScope(TraceActivity activity) : IDisposable
{
    public void Dispose()
    {
        if (activity == TraceActivity.Empty)
        {
            return;
        }

        activity.Source.Stop(activity);
    }
}