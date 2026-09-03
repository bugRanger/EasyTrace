namespace EasyTrace.Identifier;

public class TraceIdentifier(int byteLength)
{
    private readonly byte[] _bytes = new byte[byteLength];

    public bool IsEmpty { get; private set; } = true;
    public bool IsNotEmpty => !IsEmpty;

    public static TraceIdentifier CreateTraceId() => new(16);

    public static TraceIdentifier CreateSpanId() => new(8);

    public void Generate(ITraceIdentifierGenerator generator)
    {
        generator.Generate(_bytes.AsSpan(0, byteLength));
        IsEmpty = false;
    }

    public void Clear()
    {
        IsEmpty = true;
    }

    public void CopyFrom(TraceIdentifier source)
    {
        IsEmpty = source.IsEmpty;
        source.AsReadOnlySpan().CopyTo(_bytes);
    }

    public ReadOnlySpan<byte> AsReadOnlySpan()
    {
        return IsEmpty ? ReadOnlySpan<byte>.Empty : new ReadOnlySpan<byte>(_bytes, 0, byteLength);
    }

    public bool SequenceEqual(TraceIdentifier other)
    {
        return AsReadOnlySpan().SequenceEqual(other.AsReadOnlySpan());
    }
}