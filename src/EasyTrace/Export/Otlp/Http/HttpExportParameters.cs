namespace EasyTrace.Export.Otlp.Http;

public class HttpExportParameters
{
    public Uri EndPoint { get; init; } = new("http://127.0.0.1:4318");

    public int BufferSize { get; init; } = 1024;

    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(EndPoint);
        ArgumentOutOfRangeException.ThrowIfLessThan(BufferSize, 1024);
    }
}