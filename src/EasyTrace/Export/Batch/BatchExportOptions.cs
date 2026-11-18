namespace EasyTrace.Export.Batch;

public sealed record BatchExportOptions
{
    private const uint DefaultQueueSize = 2048;
    private const uint DefaultExportBatchSize = 512;
    private static readonly TimeSpan DefaultScheduledDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan DefaultShutdownDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The maximum batch size of every export. It must be smaller or equal to maxQueueSize. The default value is 512.
    /// </summary>
    public uint MaxExportSize { get; set; } = DefaultExportBatchSize;

    /// <summary>
    /// The maximum queue size. After the size is reached data are dropped. The default value is 2048
    /// </summary>
    public uint MaxQueueSize { get; set; } = DefaultQueueSize;

    /// <summary>
    /// The delay interval between two consecutive exports. The default value is 5s.
    /// </summary>
    /// <remarks>
    /// If the value is set to <see cref="TimeSpan.Zero"/> or less, the scheduled export is disabled.
    /// </remarks>
    public TimeSpan ScheduledDelay { get; set; } = DefaultScheduledDelay;

    /// <summary>
    /// Interval for a graceful shutdown. The default value is 5s.
    /// </summary>
    /// <remarks>
    /// If the value is set to <see cref="TimeSpan.Zero"/> or less, graceful shutdown is disabled.
    /// </remarks>
    public TimeSpan ShutdownDelay { get; set; } = DefaultShutdownDelay;

    public void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxExportSize, 1u);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxQueueSize, DefaultQueueSize);
        ArgumentOutOfRangeException.ThrowIfLessThan(ScheduledDelay, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(ShutdownDelay, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxQueueSize, MaxExportSize * 2);
    }
}