namespace EasyTrace;

public class TraceProvider
{
    // TODO: Add sampler.
    // TODO: Add batcher.
    // TODO: Add exporters.
}

public ref struct TraceRef(TraceProvider provider, Trace? trace) : IDisposable
{
    public void Dispose()
    {
        // TODO: flush current trace.
    }
}

public class Trace
{
}

public ref struct TraceData: IDisposable
{
    public TraceRef Next()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        // TODO: Unlock all slots in circle buffer.
    }
}

public interface TraceExporter
{
    void Export(scoped TraceData traceData);
}