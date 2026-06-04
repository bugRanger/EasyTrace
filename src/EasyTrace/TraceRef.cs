namespace EasyTrace;

public class TraceProvider
{
    // TODO: Configure sampler.
    // TODO: Configure batcher.
    // TODO: Add resources.
    // TODO: Add exporters.
}

public struct TraceSource
{
    
}

public ref struct TraceScope(TraceProvider provider, Trace? trace) : IDisposable
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
    public Trace? Next()
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