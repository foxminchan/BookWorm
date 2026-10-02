using BookWorm.Testing;

namespace BookWorm.Ordering.ContractTests;

internal static class PactTests
{
    internal static PactTestHelper Helper { get; } = new(PactMessageSerializationContext.Default);
}
