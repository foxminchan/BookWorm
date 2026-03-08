using System.Net.Http.Json;
using BookWorm.Constants.Aspire;

namespace BookWorm.Chassis.AI.Presidio;

internal sealed class PresidioService(IHttpClientFactory httpClientFactory) : IPresidioService
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

        var analyzerRequest = new AnalyzerRequest(text);

        using var analyzerClient = httpClientFactory.CreateClient(Components.Presidio.Analyzer);

        using var analyzerResponse = await analyzerClient.PostAsJsonAsync(
            "/analyze",
            analyzerRequest,
            cancellationToken
        );

        analyzerResponse.EnsureSuccessStatusCode();

        var analyzerResults =
            await analyzerResponse.Content.ReadFromJsonAsync<AnalyzerResponse[]>(cancellationToken)
            ?? [];

        if (analyzerResults.Length == 0)
        {
            return text;
        }

        var anonymizerRequest = new AnonymizerRequest(text, analyzerResults);

        using var anonymizerClient = httpClientFactory.CreateClient(Components.Presidio.Anonymizer);

        using var anonymizerResponse = await anonymizerClient.PostAsJsonAsync(
            "/anonymize",
            anonymizerRequest,
            cancellationToken
        );

        anonymizerResponse.EnsureSuccessStatusCode();

        var result = await anonymizerResponse.Content.ReadFromJsonAsync<AnonymizerResponse>(
            cancellationToken
        );

        return result?.Text ?? text;
    }
}
