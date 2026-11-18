namespace EasyTrace.Identifier.Generator;

public class Xoshiro256PlusPlusShared : ITraceIdentifierGenerator
{
    private readonly ThreadLocal<Xoshiro256PlusPlus> _shared = new(() => new Xoshiro256PlusPlus());

    public static Xoshiro256PlusPlusShared Instance { get; } = new();
    
    public void Generate(Span<byte> bytes) => _shared.Value!.Generate(bytes);
}