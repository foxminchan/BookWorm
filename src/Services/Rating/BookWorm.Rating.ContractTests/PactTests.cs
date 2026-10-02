using BookWorm.Testing;

namespace BookWorm.Rating.ContractTests;

internal static class PactTests
{
    internal static PactTestHelper Helper { get; } = new(PactMessageSerializationContext.Default);
}
