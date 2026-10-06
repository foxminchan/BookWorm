using System.Text.Json;
using BookWorm.Chassis.AI.Presidio.Clients;

namespace BookWorm.Chassis.AI.Presidio;

internal sealed class PresidioService(IPresidioAnalyzer analyzer, IPresidioAnonymizer anonymizer)
    : IPresidioService
{
    public async Task<string> AnonymizeAsync(
        string text,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var analyzerResults =
            await analyzer.AnalyzeAsync(new(text), cancellationToken)
            ?? throw new JsonException("The Presidio analyzer returned a null response.");

        if (analyzerResults.Length == 0)
        {
            return text;
        }

        var request = new AnonymizerRequest(
            text,
            analyzerResults,
            new Dictionary<string, ReplaceAnonymizer> { ["DEFAULT"] = new("<PII>") }
        );

        var result =
            await anonymizer.AnonymizeAsync(request, cancellationToken)
            ?? throw new JsonException("The Presidio anonymizer returned a null response.");

        return result.Text;
    }
}
