using EasyTrace.Identifier.Generator;

namespace EasyTrace.Tests.Identifier;

public class Xoshiro256PlusPlusTests
{
    /// <summary>
    /// Тест на лавинный эффект (Avalanche effect). 
    /// Проверяет, что два генератора, отличающиеся всего на 1 бит в seed,
    /// выдают абсолютно разные, не пересекающиеся последовательности чисел (нет коллизий между потоками).
    /// </summary>
    /// <remarks>
    /// В хорошем PRNG совпадение чисел при минимальном изменении сида 
    /// на короткой дистанции стремится к нулю.
    /// </remarks>
    [Test]
    public void DifferentSeeds_ProduceEntirelyDifferentOutputs()
    {
        ulong seed1 = 0x123456789ABCDEF0UL;
        ulong seed2 = 0x123456789ABCDEF1UL; // Отличие строго в 1 младший бит

        var rng1 = new Xoshiro256PlusPlus(seed1);
        var rng2 = new Xoshiro256PlusPlus(seed2);

        int checkCount = 1000;
        int collisions = 0;

        for (int i = 0; i < checkCount; i++)
        {
            if (rng1.NextUInt64() == rng2.NextUInt64())
            {
                collisions++;
            }
        }

        Assert.That(collisions, Is.LessThanOrEqualTo(1), $"Слишком много коллизий между близкими сидами: {collisions}");
    }

    /// <summary>
    /// Тест гарантирует, что при одном и том же seed генератор выдает 
    /// строго идентичную последовательность (детерминированность).
    /// </summary>
    [Test]
    public void Deterministic_SequencesMatch()
    {
        ulong seed = 42UL;

        var rng1 = new Xoshiro256PlusPlus(seed);
        var rng2 = new Xoshiro256PlusPlus(seed);

        for (int i = 0; i < 1000; i++)
        {
            Assert.That(rng1.NextUInt64(), Is.EqualTo(rng2.NextUInt64()));
        }
    }
}