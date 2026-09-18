namespace EasyTrace.Export.Batch;

public sealed record BatchExportOptions
{
    private const uint DefaultMaxQueueSize = 2048;
    private const int DefaultScheduledDelayMilliseconds = 5000;
    private const int DefaultMaxExportBatchSize = 512;

    /// <summary>
    /// The maximum batch size of every export. It must be smaller or equal to maxQueueSize. The default value is 512.
    /// </summary>
    public uint MaxExportSize { get; set; } = DefaultMaxExportBatchSize;

    /// <summary>
    /// The maximum queue size. After the size is reached data are dropped. The default value is 2048
    /// </summary>
    public uint MaxQueueSize { get; set; } = DefaultMaxQueueSize;

    /// <summary>
    /// The delay interval in milliseconds between two consecutive exports. The default value is 5000.
    /// </summary>
    /// <remarks>
    /// If the value is set to <see cref="uint.MinValue"/>, scheduled export is disabled.
    /// If the value is set to <see cref="uint.MaxValue"/>, the export is performed in the calling thread.
    /// </remarks>
    public uint ScheduledDelayMilliseconds { get; set; } = DefaultScheduledDelayMilliseconds;
}