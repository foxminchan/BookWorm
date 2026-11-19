using System.Text.Json;
using System.Text.Json.Nodes;

namespace BookWorm.Testing.Pact.Verification;

internal sealed class ScopedPactFile : IDisposable
{
    private static readonly JsonSerializerOptions _indentedJsonOptions = new()
    {
        WriteIndented = true,
    };

    private ScopedPactFile(string filePath)
    {
        FilePath = filePath;
    }

    public string FilePath { get; }

    public void Dispose()
    {
        File.Delete(FilePath);
    }

    public static ScopedPactFile Create(string pactPath, string description)
    {
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

        pactDocument["messages"] = new JsonArray(matchingMessages[0]?.DeepClone());

        var verificationPactPath = Path.Combine(
            Path.GetTempPath(),
            $"bookworm-{Guid.CreateVersion7():N}.json"
        );
        File.WriteAllText(verificationPactPath, pactDocument.ToJsonString(_indentedJsonOptions));

        return new(verificationPactPath);
    }
}
