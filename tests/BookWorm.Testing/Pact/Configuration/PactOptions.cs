using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using PactNet;

namespace BookWorm.Testing.Pact.Configuration;

internal sealed class PactOptions
{
    public PactOptions(JsonSerializerContext messageJsonContext)
    {
        ArgumentNullException.ThrowIfNull(messageJsonContext);

        MessageJsonOptions = new(messageJsonContext.Options)
        {
            TypeInfoResolver = JsonTypeInfoResolver.Combine(
                messageJsonContext,
                PactSerializationContext.Default
            ),
        };

        ProviderJsonSettings = new(MessageJsonOptions)
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            // PactNet also serializes matcher objects outside our generated message contracts.
            TypeInfoResolver = JsonTypeInfoResolver.Combine(
                messageJsonContext,
                PactSerializationContext.Default,
                new DefaultJsonTypeInfoResolver()
            ),
        };
    }

    public JsonSerializerOptions MessageJsonOptions { get; }

    public JsonSerializerOptions ProviderJsonSettings { get; }

    public PactConfig CreatePactConfig()
    {
        var pactDirectory = PactDirectoryResolver.GetPactDirectory();
        Directory.CreateDirectory(pactDirectory);

        return new() { PactDir = pactDirectory, DefaultJsonSettings = ProviderJsonSettings };
    }
}
