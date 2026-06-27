namespace EasyTrace.Identifier;

public class TraceIdentifier(int byteLength)
{
    private readonly byte[] _bytes = new byte[byteLength];

    public static TraceIdentifier CreateTraceId() => new(16);

    public static TraceIdentifier CreateSpanId() => new(8);

    public void Generate(ITraceIdentifierGenerator generator)
    {
        generator.Generate(_bytes.AsSpan(0, byteLength));
    }

    public void CopyFrom(TraceIdentifier source)
    {
        source.AsReadOnlySpan().CopyTo(_bytes);
    }

    public ReadOnlySpan<byte> AsReadOnlySpan()
    {
        return new ReadOnlySpan<byte>(_bytes, 0, byteLength);
    }

    public bool SequenceEqual(TraceIdentifier other)
    {
        return AsReadOnlySpan().SequenceEqual(other.AsReadOnlySpan());
    }
}