using EasyTrace.Export.Batch.Buffer;

namespace EasyTrace.Activity.Pool;

public class UnboundedQueue(int capacity, IFactory<TraceActivity> factory) : ITraceActivityPool
{
    private readonly Queue<TraceActivity> _queue = new(capacity);
    private TraceActivity? _fastItem;

    public TraceActivity Rent()
    {
        if (_fastItem == null)
        {
            return _queue.TryDequeue(out var activity) ? activity : factory.Create();
        }

        var item = _fastItem;
        _fastItem = null;
        return item;
    }

    public void Return(TraceActivity item)
    {
        if (_fastItem == null)
        {
            _fastItem = item;
            return;
        }

        _queue.Enqueue(item);
    }
}