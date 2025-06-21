using Bogus;

namespace Catalyst.Tests.Unit.Common;

public abstract class BaseTest : IDisposable
{
    protected Faker Faker { get; }
    protected ITestOutputHelper? Output { get; } // Optional: for writing output during tests

    protected BaseTest(ITestOutputHelper? output = null)
    {
        Faker = new Faker();
        Output = output;
        // Seed Bogus for reproducible tests if desired, though generally not for unit tests
        // Randomizer.Seed = new Random(GetDeterministicHashCode(GetType().FullName));
    }

    // Optional: Helper for seeding Bogus deterministically per test class
    // private static int GetDeterministicHashCode(string str)
    // {
    //     unchecked
    //     {
    //         int hash1 = (5381 << 16) + 5381;
    //         int hash2 = hash1;
    //
    //         for (int i = 0; i < str.Length; i += 2)
    //         {
    //             hash1 = ((hash1 << 5) + hash1) ^ str[i];
    //             if (i == str.Length - 1)
    //                 break;
    //             hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
    //         }
    //
    //         return hash1 + (hash2 * 1566083941);
    //     }
    // }

    public virtual void Dispose()
    {
        // Common cleanup logic if any
        GC.SuppressFinalize(this);
    }
}