namespace EasyTrace.Export.Otlp;

public class GrpcExportParameters
{
    public Uri EndPoint { get; set; } = new("http://localhost:4317");

    public int BufferSize { get; set; } = 1024;

    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(EndPoint);
        ArgumentOutOfRangeException.ThrowIfLessThan(BufferSize, 1024);
    }
}