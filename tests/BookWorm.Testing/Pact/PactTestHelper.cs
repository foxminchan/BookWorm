using System.Text.Json.Serialization;
using BookWorm.Testing.Pact.Configuration;
using BookWorm.Testing.Pact.Verification;

namespace BookWorm.Testing.Pact;

public sealed class PactTestHelper
{
    private readonly PactConsumerVerifier _consumerVerifier;
    private readonly PactProviderVerifier _providerVerifier;

    public PactTestHelper(JsonSerializerContext messageJsonContext)
    {
        var options = new PactOptions(messageJsonContext);
        _consumerVerifier = new(options);
        _providerVerifier = new(options);
    }

    public Task VerifyConsumerMessageAsync<T>(
        string consumer,
        string provider,
        T expectedMessage,
        Func<T, Task> consume
    )
    {
        return _consumerVerifier.VerifyAsync(consumer, provider, expectedMessage, consume);
    }

    public Task VerifyConsumerMessageAsync<T>(string consumer, string provider, T expectedMessage)
    {
        return _consumerVerifier.VerifyAsync(consumer, provider, expectedMessage, static _ =>
            Task.CompletedTask
        );
    }

    public void VerifyProviderMessage<T>(string consumer, string provider, T message)
    {
        _providerVerifier.Verify(consumer, provider, message);
    }
}
