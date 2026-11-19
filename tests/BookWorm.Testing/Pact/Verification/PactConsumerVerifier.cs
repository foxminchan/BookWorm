using System.Net.Mime;
using System.Text.Json;
using BookWorm.Testing.Pact.Configuration;
using BookWorm.Testing.Pact.Messaging;
using PactNet;

namespace BookWorm.Testing.Pact.Verification;

internal sealed class PactConsumerVerifier(PactOptions options)
{
    public Task VerifyAsync<T>(
        string consumer,
        string provider,
        T expectedMessage,
        Func<T, Task> consume
    )
    {
        ArgumentNullException.ThrowIfNull(expectedMessage);
        ArgumentNullException.ThrowIfNull(consume);

        var messageType = PactMessageConventions.GetMessageType(expectedMessage.GetType());
        var description = PactMessageConventions.GetMessageDescription(messageType);
        var pact = PactNet
            .Pact.V3(consumer, provider, options.CreatePactConfig())
            .WithMessageInteractions();

        return pact.ExpectsToReceive(description)
            .WithMetadata("contentType", MediaTypeNames.Application.Json)
            .WithMetadata("type", messageType)
            .WithMetadata("source", PactMessageConventions.GetMessageSource(provider))
            .WithJsonContent(
                PactTypeMatcher.Create(
                    JsonSerializer.SerializeToElement(expectedMessage, options.MessageJsonOptions)
                )
            )
            .VerifyAsync(consume);
    }
}
