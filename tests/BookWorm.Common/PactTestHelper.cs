using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PactNet;
using PactNet.Verifier;
using Wolverine.Attributes;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Match = PactNet.Matchers.Match;

namespace BookWorm.Common;

public static partial class PactTestHelper
{
    private const string ProviderVerificationMutexName = "BookWorm.PactNet.ProviderVerification";

    private static readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    private static readonly JsonSerializerOptions providerJsonSettings = new(
        JsonSerializerDefaults.Web
    )
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    private static Task VerifyConsumerMessageAsync<T>(
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
                CreateTypeMatcher(JsonSerializer.SerializeToElement(expectedMessage, jsonOptions))
            )
            .VerifyAsync(consume);
    }

    public static Task VerifyConsumerMessageAsync<T>(
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

    public static Task VerifyConsumerMessageAsync<T>(
        string consumer,
        string provider,
        T expectedMessage
    )
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

    public static void VerifyProviderMessage<T>(string consumer, string provider, T message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var messageType = GetMessageType(message.GetType());
        var pactPath = Path.Combine(GetPactDirectory(), $"{consumer}-{provider}.json");
        var description = $"{messageType} message";
        var pactDocument = JObject.Parse(File.ReadAllText(pactPath));
        var messages =
            pactDocument["messages"] as JArray
            ?? throw new InvalidDataException($"Pact file '{pactPath}' has no messages.");
        var matchingMessages = messages
            .Where(jToken => jToken["description"]?.Value<string>() == description)
            .ToArray();

        if (matchingMessages.Length != 1)
        {
            throw new InvalidDataException(
                $"Expected exactly one '{description}' message in Pact file '{pactPath}', "
                    + $"but found {matchingMessages.Length}."
            );
        }

        pactDocument["messages"] = new JArray(matchingMessages[0].DeepClone());
        var verificationPactPath = Path.Combine(
            Path.GetTempPath(),
            $"bookworm-{Guid.CreateVersion7():N}.json"
        );
        File.WriteAllText(verificationPactPath, pactDocument.ToString(Formatting.Indented));

        using var mutex = new Mutex(false, ProviderVerificationMutexName);
        mutex.WaitOne();

        try
        {
            using var verifier = new PactVerifier(provider);
            verifier
                // PactNet 5.x requires an HTTP transport before configuring message verification.
                .WithHttpEndpoint(new Uri("http://localhost"))
                .WithMessages(
                    scenarios =>
                        scenarios.Add(
                            description,
                            builder =>
                            {
                                builder
                                    .WithMetadata(
                                        new
                                        {
                                            contentType = MediaTypeNames.Application.Json,
                                            type = messageType,
                                            source = GetMessageSource(provider),
                                        }
                                    )
                                    .WithContent(() => message);
                            }
                        ),
                    providerJsonSettings
                )
                .WithFileSource(new FileInfo(verificationPactPath))
                .Verify();
        }
        finally
        {
            mutex.ReleaseMutex();
            File.Delete(verificationPactPath);
        }
    }

    private static PactConfig CreateConfig()
    {
        var pactDirectory = GetPactDirectory();
        Directory.CreateDirectory(pactDirectory);

        return new() { PactDir = pactDirectory, DefaultJsonSettings = providerJsonSettings };
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
