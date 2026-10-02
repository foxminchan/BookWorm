using BookWorm.Testing;

namespace BookWorm.Basket.ContractTests;

internal static class PactTests
{
    internal static PactTestHelper Helper { get; } = new(PactMessageSerializationContext.Default);
}
