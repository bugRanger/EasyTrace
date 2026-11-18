namespace EasyTrace.Export.Batch;

public sealed record BatchExportOptions
{
    private const uint DefaultMaxQueueSize = 2048;
    private const int DefaultMaxExportBatchSize = 512;
    private static readonly TimeSpan DefaultScheduledDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan DefaultShutdownDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The maximum batch size of every export. It must be smaller or equal to maxQueueSize. The default value is 512.
    /// </summary>
    public uint MaxExportSize { get; set; } = DefaultMaxExportBatchSize;

    /// <summary>
    /// The maximum queue size. After the size is reached data are dropped. The default value is 2048
    /// </summary>
    public uint MaxQueueSize { get; set; } = DefaultMaxQueueSize;

    /// <summary>
    /// The delay interval between two consecutive exports. The default value is 5s.
    /// </summary>
    /// <remarks>
    /// If a value less than <see cref="TimeSpan.Zero"/> is specified, the scheduled export is disabled.
    /// If the value is set to <see cref="TimeSpan.Zero"/>, the export is performed in the calling thread.
    /// </remarks>
    public TimeSpan ScheduledDelay { get; set; } = DefaultScheduledDelay;

    /// <summary>
    /// Interval for a graceful shutdown. The default value is 5s.
    /// </summary>
    /// <remarks>
    /// If the value is set to <see cref="TimeSpan.Zero"/> or less, graceful shutdown is disabled.
    /// </remarks>
    public TimeSpan ShutdownDelay { get; set; } = DefaultShutdownDelay;
}