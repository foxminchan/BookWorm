using BookWorm.Testing;

namespace BookWorm.Catalog.ContractTests;

internal static class PactTests
{
    internal static PactTestHelper Helper { get; } = new(PactMessageSerializationContext.Default);
}
