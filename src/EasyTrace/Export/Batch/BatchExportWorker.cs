using EasyTrace.Activity;
using EasyTrace.Export.Batch.Buffer;

namespace EasyTrace.Export.Batch;

public sealed class BatchExportWorker<T> : IDisposable
    where T : ITraceActivityExporter
{
    private readonly T _exporter;
    private readonly AutoResetEvent _exportNotifier = new(false);
    private bool _disposed;

    /// <summary>
    /// Gets the delay between exports in milliseconds.
    /// </summary>
    private readonly uint _scheduledDelayMilliseconds;

    /// <summary>
    /// Gets the maximum batch size for exports.
    /// </summary>
    private readonly ulong _maxExportBatchSize;

    /// <summary>
    /// Gets the circular buffer for storing telemetry objects.
    /// </summary>
    private readonly CircularBuffer<TraceActivity> _circularBuffer;

    private bool IsActive => _scheduledDelayMilliseconds != uint.MinValue;

    private bool IsCurrentThread => _scheduledDelayMilliseconds == uint.MaxValue;

    public BatchExportWorker(T exporter, TraceActivityFactory factory, BatchExportOptions options)
    {
        _exporter = exporter;
        _circularBuffer = new CircularBuffer<TraceActivity>(options.MaxQueueSize, factory);
        _maxExportBatchSize = options.MaxExportBatchSize;
        _scheduledDelayMilliseconds = options.ScheduledDelayMilliseconds;

        if (!IsActive || IsCurrentThread)
        {
            return;
        }

        var schedulerThread = new Thread(SchedulerLoop)
        {
            IsBackground = true, Name = $"Batch-Export-For-{exporter.GetType().Name}",
        };
        schedulerThread.Start();
    }

    ~BatchExportWorker()
    {
        Dispose(false);
    }

    public bool Push(in TraceActivityRef activityRef)
    {
        return IsActive && _circularBuffer.Push(in activityRef, 50_000);
    }

    public bool TryExport()
    {
        if (!IsActive)
        {
            return false;
        }

        if (_circularBuffer.Count < _maxExportBatchSize)
        {
            return false;
        }

        try
        {
            if (IsCurrentThread)
            {
                PerformExport();
            }
            else
            {
                _exportNotifier.Set();
            }
        }
        catch (ObjectDisposedException)
        {
            return false;
        }

        return true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _exportNotifier.Set();
            _exportNotifier.Dispose();
        }

        _disposed = true;
    }

    private void SchedulerLoop()
    {
        var triggers = new WaitHandle[]
        {
            _exportNotifier
        };

        while (true)
        {
            if (_circularBuffer.Count < _maxExportBatchSize)
            {
                try
                {
                    WaitHandle.WaitAny(triggers, (int)_scheduledDelayMilliseconds);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            }

            PerformExport();
        }
    }

    private void PerformExport()
    {
        if (_circularBuffer.Count <= 0)
        {
            return;
        }

        var batchSize = 0ul;
        while (batchSize < _maxExportBatchSize)
        {
            if (!_circularBuffer.Pop(out var bufferSlot))
            {
                break;
            }

            try
            {
                scoped var activityRef = new TraceActivityRef(bufferSlot.Item);
                _exporter.Export(in activityRef);
                batchSize++;
            }
            finally
            {
                bufferSlot.Clear();
            }
        }

        _exporter.Flush();
    }
}