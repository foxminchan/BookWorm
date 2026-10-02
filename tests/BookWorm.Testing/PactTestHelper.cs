using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using PactNet;
using PactNet.Verifier;
using Wolverine.Attributes;
using Match = PactNet.Matchers.Match;

namespace BookWorm.Testing;

public sealed partial class PactTestHelper
{
    private const string ProviderVerificationMutexName = "BookWorm.PactNet.ProviderVerification";

    private readonly JsonSerializerOptions _jsonOptions;
    private readonly JsonSerializerOptions _providerJsonSettings;

    public PactTestHelper(JsonSerializerContext messageJsonContext)
    {
        ArgumentNullException.ThrowIfNull(messageJsonContext);

        _jsonOptions = new(messageJsonContext.Options)
        {
            TypeInfoResolver = JsonTypeInfoResolver.Combine(
                messageJsonContext,
                PactSerializationContext.Default
            ),
        };

        _providerJsonSettings = new(_jsonOptions)
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

    private Task VerifyConsumerMessageAsync<T>(
        string consumer,
        string provider,
        string description,
        T expectedMessage,
        Func<T, Task> consume
    )
    {
        ArgumentNullException.ThrowIfNull(expectedMessage);
        ArgumentNullException.ThrowIfNull(consume);

        var pact = Pact.V3(consumer, provider, CreateConfig()).WithMessageInteractions();

        return pact.ExpectsToReceive(description)
            .WithMetadata("contentType", MediaTypeNames.Application.Json)
            .WithMetadata("type", GetMessageType(expectedMessage.GetType()))
            .WithMetadata("source", GetMessageSource(provider))
            .WithJsonContent(
                CreateTypeMatcher(JsonSerializer.SerializeToElement(expectedMessage, _jsonOptions))
            )
            .VerifyAsync(consume);
    }

    public Task VerifyConsumerMessageAsync<T>(
        string consumer,
        string provider,
        T expectedMessage,
        Func<T, Task> consume
    )
    {
        ArgumentNullException.ThrowIfNull(expectedMessage);
        var messageType = GetMessageType(expectedMessage.GetType());

        return VerifyConsumerMessageAsync(
            consumer,
            provider,
            $"{messageType} message",
            expectedMessage,
            consume
        );
    }

    public Task VerifyConsumerMessageAsync<T>(string consumer, string provider, T expectedMessage)
    {
        ArgumentNullException.ThrowIfNull(expectedMessage);
        var messageType = GetMessageType(expectedMessage.GetType());

        return VerifyConsumerMessageAsync(
            consumer,
            provider,
            $"{messageType} message",
            expectedMessage,
            static _ => Task.CompletedTask
        );
    }

    public void VerifyProviderMessage<T>(string consumer, string provider, T message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var messageType = GetMessageType(message.GetType());
        var pactPath = Path.Combine(GetPactDirectory(), $"{consumer}-{provider}.json");
        var description = $"{messageType} message";
        var pactDocument = JsonNode.Parse(File.ReadAllText(pactPath))!;
        var messages =
            pactDocument["messages"] as JsonArray
            ?? throw new InvalidDataException($"Pact file '{pactPath}' has no messages.");
        var matchingMessages = messages
            .Where(messageNode => messageNode?["description"]?.GetValue<string>() == description)
            .ToArray();

        if (matchingMessages.Length != 1)
        {
            throw new InvalidDataException(
                $"Expected exactly one '{description}' message in Pact file '{pactPath}', "
                    + $"but found {matchingMessages.Length}."
            );
        }

        pactDocument["messages"] = new JsonArray(matchingMessages[0]!.DeepClone());
        var verificationPactPath = Path.Combine(
            Path.GetTempPath(),
            $"bookworm-{Guid.CreateVersion7():N}.json"
        );
        File.WriteAllText(
            verificationPactPath,
            pactDocument.ToJsonString(new() { WriteIndented = true })
        );

        using var mutex = new Mutex(false, ProviderVerificationMutexName);
        mutex.WaitOne();

        try
        {
            using var verifier = new PactVerifier(provider);
            verifier
                // PactNet 5.x requires an HTTP transport before configuring message verification.
                .WithHttpEndpoint(new("http://localhost"))
                .WithMessages(
                    scenarios =>
                        scenarios.Add(
                            description,
                            builder =>
                            {
                                builder
                                    .WithMetadata(
                                        new PactMessageMetadata(
                                            MediaTypeNames.Application.Json,
                                            messageType,
                                            GetMessageSource(provider)
                                        )
                                    )
                                    .WithContent(() => message, _jsonOptions);
                            }
                        ),
                    _providerJsonSettings
                )
                .WithFileSource(new(verificationPactPath))
                .Verify();
        }
        finally
        {
            mutex.ReleaseMutex();
            File.Delete(verificationPactPath);
        }
    }

    private PactConfig CreateConfig()
    {
        var pactDirectory = GetPactDirectory();
        Directory.CreateDirectory(pactDirectory);

        return new() { PactDir = pactDirectory, DefaultJsonSettings = _providerJsonSettings };
    }

    private static string GetPactDirectory()
    {
        var repositoryRoot = FindRepositoryRoot();
        var configuredDirectory = Environment.GetEnvironmentVariable(
            "BOOKWORM_PACT_OUTPUT_DIRECTORY"
        );
        return configuredDirectory is null
            ? Path.Combine(repositoryRoot, "tests", "pacts")
            : Path.GetFullPath(Path.Combine(repositoryRoot, configuredDirectory));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "BookWorm.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the BookWorm repository root for Pact output."
        );
    }

    private static string GetMessageType(Type messageType)
    {
        return messageType.GetCustomAttribute<MessageIdentityAttribute>()?.Alias
            ?? messageType.FullName
            ?? messageType.Name;
    }

    private static object? CreateTypeMatcher(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element
                .EnumerateObject()
                .ToDictionary(
                    property => property.Name,
                    property => CreateTypeMatcher(property.Value)
                ),
            JsonValueKind.Array => element.EnumerateArray().Select(CreateTypeMatcher).ToArray(),
            JsonValueKind.String => Match.Type(element.GetString()!),
            JsonValueKind.Number => CreateNumberMatcher(element),
            JsonValueKind.True => Match.Type(true),
            JsonValueKind.False => Match.Type(false),
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => throw new InvalidOperationException(
                "Undefined JSON token in Pact message is not supported."
            ),
            _ => throw new InvalidOperationException(
                $"Unsupported JSON token in Pact message: {element.ValueKind}."
            ),
        };
    }

    private static object CreateNumberMatcher(JsonElement element)
    {
        return element.TryGetInt64(out var integer)
            ? Match.Type(integer)
            : Match.Type(element.GetDecimal());
    }

    private static string GetMessageSource(string provider)
    {
        var kebabCase = MessageSourceRegex()
            .Replace($"BookWorm.{provider}".Replace('.', '-'), "$1-$2");

        return $"urn:bookworm:{kebabCase.ToLowerInvariant()}";
    }

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex MessageSourceRegex();
}
