namespace EasyTrace.Export.Otlp.Http;

public class HttpExportParameters
{
    public Uri EndPoint { get; init; } = new("http://127.0.0.1:4318");

    /// <summary>
    /// Buffer capacity for sending messages.
    /// </summary>
    /// <remarks>
    /// If the capacity is insufficient, it will be recalculated as a power of 2.
    /// </remarks>
    public int BufferCapacity { get; init; } = 1024;

    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(EndPoint);
        ArgumentOutOfRangeException.ThrowIfLessThan(BufferCapacity, 1024);
    }
}