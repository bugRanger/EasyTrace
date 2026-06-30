namespace EasyTrace.Identifier;

public interface ITraceIdentifierGenerator
{
    void Generate(Span<byte> bytes);
}
