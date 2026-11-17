using System.Net.Mime;
using BookWorm.Testing.Pact.Configuration;
using BookWorm.Testing.Pact.Messaging;
using PactNet.Verifier;

namespace BookWorm.Testing.Pact.Verification;

internal sealed class PactProviderVerifier(PactOptions options)
{
    private const string ProviderVerificationMutexName = "BookWorm.PactNet.ProviderVerification";

    public void Verify<T>(string consumer, string provider, T message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var messageType = PactMessageConventions.GetMessageType(message.GetType());
        var description = PactMessageConventions.GetMessageDescription(messageType);
        var pactPath = Path.Combine(
            PactDirectoryResolver.GetPactDirectory(),
            $"{consumer}-{provider}.json"
        );

        using var scopedPact = ScopedPactFile.Create(pactPath, description);
        using var mutex = new Mutex(false, ProviderVerificationMutexName);
        mutex.WaitOne();

        try
        {
            using var verifier = new PactVerifier(provider);
            verifier
                // PactNet 5.x requires an HTTP transport before configuring message verification.
                .WithHttpEndpoint(new("http://localhost"))
                .WithMessages(scenarios => scenarios.Add(description, builder =>
                        {
                            builder
                                .WithMetadata(
                                    new PactMessageMetadata(
                                        MediaTypeNames.Application.Json,
                                        messageType,
                                        PactMessageConventions.GetMessageSource(provider)
                                    )
                                )
                                .WithContent(() => message, options.MessageJsonOptions);
                        }), options.ProviderJsonSettings)
                .WithFileSource(new(scopedPact.FilePath))
                .Verify();
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }
}
