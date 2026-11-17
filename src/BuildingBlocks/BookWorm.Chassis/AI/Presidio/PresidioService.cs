using System.Collections.Frozen;
using System.Text.Json;

namespace BookWorm.Chassis.AI.Presidio;

internal sealed class PresidioService(IPresidioAnalyzer analyzer, IPresidioAnonymizer anonymizer)
    : IPresidioService
{
    private static readonly FrozenDictionary<string, ReplaceAnonymizer> DefaultAnonymizers =
        new Dictionary<string, ReplaceAnonymizer>
        {
            ["DEFAULT"] = new("<PII>"),
        }.ToFrozenDictionary();

    public async Task<string> AnonymizeAsync(
        string text,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var analyzerResults = await analyzer.AnalyzeAsync(new(text), cancellationToken)
            ?? throw new JsonException("The Presidio analyzer returned a null response.");

        if (analyzerResults.Length == 0)
        {
            return text;
        }

        var request = new AnonymizerRequest(text, analyzerResults, DefaultAnonymizers);

        var result = await anonymizer.AnonymizeAsync(request, cancellationToken)
            ?? throw new JsonException("The Presidio anonymizer returned a null response.");

        return result.Text;
    }
}
