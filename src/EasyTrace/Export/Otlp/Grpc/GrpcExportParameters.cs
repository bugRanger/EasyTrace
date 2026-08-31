namespace EasyTrace.Export.Otlp.Grpc;

public class GrpcExportParameters
{
    public Uri EndPoint { get; init; } = new("http://localhost:4317");

    public int BufferSize { get; init; } = 1024;

    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(EndPoint);
        ArgumentOutOfRangeException.ThrowIfLessThan(BufferSize, 1024);
    }
}