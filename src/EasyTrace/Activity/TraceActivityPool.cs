namespace EasyTrace.Activity;

/// <summary>
/// <see cref="TraceActivity"/> pool to reduce GC load.
/// </summary>
public class TraceActivityPool(int capacity, TraceActivityFactory factory)
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