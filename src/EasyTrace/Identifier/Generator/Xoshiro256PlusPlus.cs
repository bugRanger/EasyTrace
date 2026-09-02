using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace EasyTrace.Identifier.Generator;

public class Xoshiro256PlusPlus : ITraceIdentifierGenerator
{
    private ulong _s0;
    private ulong _s1;
    private ulong _s2;
    private ulong _s3;

    public Xoshiro256PlusPlus()
        : this(GenerateCryptoSeed())
    {
    }

    public Xoshiro256PlusPlus(ulong seed)
    {
        if (seed == 0)
            seed = 123456789;
        InitState(seed);
    }

    private void InitState(ulong seed)
    {
        var x = seed;
        _s0 = SplitMix64(ref x);
        _s1 = SplitMix64(ref x);
        _s2 = SplitMix64(ref x);
        _s3 = SplitMix64(ref x);
    }

    public ulong NextUInt64()
    {
        var result = Rotl(_s0 + _s3, 23) + _s0;

        var t = _s1 << 17;

        _s2 ^= _s0;
        _s3 ^= _s1;
        _s1 ^= _s2;
        _s0 ^= _s3;

        _s2 ^= t;

        _s3 = Rotl(_s3, 45);

        return result;
    }

    public void Generate(Span<byte> bytes)
    {
        if (bytes.Length != 8 && bytes.Length != 16)
        {
            throw new ArgumentOutOfRangeException(nameof(bytes));
        }

        Unsafe.WriteUnaligned(ref bytes[0], NextUInt64());

        if (bytes.Length == 16)
        {
            Unsafe.WriteUnaligned(ref bytes[8], NextUInt64());
        }
    }

    private static ulong SplitMix64(ref ulong x)
    {
        x += 0x9e3779b97f4a7c15;
        var z = x;
        z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9;
        z = (z ^ (z >> 27)) * 0x94d049bb133111eb;
        return z ^ (z >> 31);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Rotl(ulong x, int k)
    {
        return (x << k) | (x >> (64 - k));
    }

    private static ulong GenerateCryptoSeed()
    {
        using var rng = RandomNumberGenerator.Create();
        Span<byte> bytes = stackalloc byte[8];
        rng.GetBytes(bytes);
        return BitConverter.ToUInt64(bytes);
    }
}