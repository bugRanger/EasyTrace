using System.Diagnostics;
using EasyTrace.Activity;
using EasyTrace.Export.Batch.Buffer;

namespace EasyTrace.Export.Batch;

public sealed class BatchExportWorker<T> : IDisposable
    where T : ITraceActivityExporter
{
    private readonly T _exporter;
    private readonly AutoResetEvent _exportNotifier = new(false);
    private Thread? _schedulerThread;
    private bool _disposed;

    /// <summary>
    /// Gets the delay between exports.
    /// </summary>
    private readonly TimeSpan _scheduledDelay;

    /// <summary>
    /// Gets the delay for shutdown.
    /// </summary>
    private readonly TimeSpan _shutdownDelay;

    /// <summary>
    /// Gets the maximum batch size for exports.
    /// </summary>
    private readonly ulong _maxExportSize;

    /// <summary>
    /// Gets the circular buffer for storing telemetry objects.
    /// </summary>
    private readonly CircularBuffer<TraceActivity> _circularBuffer;

    private readonly bool _isCurrentThread;
    private bool _isActive;

    public BatchExportWorker(T exporter, TraceActivityFactory factory, BatchExportOptions options)
    {
        _exporter = exporter;
        _circularBuffer = new CircularBuffer<TraceActivity>(options.MaxQueueSize, factory);
        _maxExportSize = options.MaxExportSize;
        _scheduledDelay = options.ScheduledDelay;
        _shutdownDelay = options.ShutdownDelay;

        _isActive = options.ScheduledDelay >= TimeSpan.Zero;
        _isCurrentThread = options.ScheduledDelay == TimeSpan.Zero;

        if (_isActive && !_isCurrentThread)
        {
            _schedulerThread = new Thread(SchedulerLoop)
            {
                IsBackground = true, Name = $"Batch-Export-For-{exporter.GetType().Name}",
            };
            _schedulerThread.Start();
        }
    }

    ~BatchExportWorker()
    {
        Dispose(false);
    }

    public bool Push(in TraceActivityRef activityRef)
    {
        return _isActive && _circularBuffer.Push(in activityRef, 50_000);
    }

    public bool TryExport()
    {
        if (!_isActive)
        {
            return false;
        }

        if (_circularBuffer.Count < _maxExportSize)
        {
            return false;
        }

        try
        {
            if (_isCurrentThread)
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
            if (_shutdownDelay > TimeSpan.Zero)
            {
                Volatile.Write(ref _isActive, false);

                _exportNotifier.Set();
                _schedulerThread?.Join(_shutdownDelay);
                _schedulerThread = null;
            }

            _exportNotifier.Dispose();
        }

        _disposed = true;
    }

    private void SchedulerLoop()
    {
        var triggers = new WaitHandle[]
        {
            _exportNotifier,
        };

        while (true)
        {
            if (_circularBuffer.Count < _maxExportSize)
            {
                try
                {
                    WaitHandle.WaitAny(triggers, _scheduledDelay);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            }

            var active = Volatile.Read(ref _isActive);
            if (active)
            {
                PerformExport();
            }
            else
            {
                while (_circularBuffer.Count > 0)
                {
                    PerformExport();
                }

                return;
            }
        }
    }

    private void PerformExport()
    {
        if (_circularBuffer.Count <= 0)
        {
            return;
        }

        var batchSize = 0ul;
        while (batchSize < _maxExportSize)
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