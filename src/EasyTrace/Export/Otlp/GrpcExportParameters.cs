namespace EasyTrace.Export.Otlp;

public class GrpcExportParameters
{
    public Uri EndPoint { get; set; } = new("http://localhost:4317");

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    public int BufferSize { get; set; } = 1024;

    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(EndPoint);
        ArgumentOutOfRangeException.ThrowIfEqual(Timeout, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThan(BufferSize, 1024);
    }
}