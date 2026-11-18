using BookWorm.Testing.Pact;

namespace BookWorm.Finance.ContractTests;

internal static class PactTests
{
    internal static PactTestHelper Helper { get; } = new(PactMessageSerializationContext.Default);
}
