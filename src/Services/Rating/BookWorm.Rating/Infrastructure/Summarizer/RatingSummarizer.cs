using System.Text.Json;
using BookWorm.Constants.Other;
using BookWorm.Rating.Features.List;
using BookWorm.Rating.Infrastructure.Summarizer.Models;
using BookWorm.Rating.Infrastructure.Summarizer.Providers;
using Mediator;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace BookWorm.Rating.Infrastructure.Summarizer;

internal sealed class RatingSummarizer(
    [FromKeyedServices(Agents.RatingAgent)] AIAgent ratingAgent,
    IRatingPromptProvider promptProvider,
    ISender sender
) : ISummarizer
{
    public async Task<SummarizeResult> SummarizeAsync(
        Guid bookId,
        CancellationToken cancellationToken = default
    )
    {
        var feedback = await sender.Send(
            new ListFeedbacksQuery(bookId, 1, int.MaxValue),
            cancellationToken
        );

        var reviewCount = feedback.Count;
        var averageRating =
            reviewCount == 0 ? double.NaN : feedback.Average(review => review.Rating);
        var reviewData = JsonSerializer.Serialize(feedback);

        var summaryPrompt = await promptProvider.GetSummaryPromptAsync(
            reviewData,
            cancellationToken
        );

        var summary = await RunStructuredWithRetryAsync<RatingSummaryOutput>(
            [summaryPrompt],
            cancellationToken
        );
        var summaryText = summary.ToText();

        var qualityPrompt = await promptProvider.GetQualityPromptAsync(
            averageRating,
            reviewCount,
            summaryText,
            cancellationToken
        );
        var assessment = await RunStructuredWithRetryAsync<QualityAssessmentOutput>(
            [qualityPrompt],
            cancellationToken
        );
        var classification = RatingClassifier.Classify(
            averageRating,
            reviewCount,
            assessment.DominantNegativePattern
        );

        return new(
            summaryText,
            classification,
            double.IsNaN(averageRating) ? null : averageRating,
            reviewCount,
            assessment.Evidence,
            assessment.Confidence
        );
    }

    private async Task<T> RunStructuredWithRetryAsync<T>(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken
    )
        where T : class
    {
        var messageList = messages.ToArray();
        JsonException? firstFailure = null;

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var session = await ratingAgent.CreateSessionAsync(cancellationToken);
            var response = await ratingAgent.RunAsync(
                messageList,
                session,
                new ChatClientAgentRunOptions
                {
                    ChatOptions = new()
                    {
                        ResponseFormat = ChatResponseFormat.ForJsonSchema<T>(),
                        Tools = [],
                    },
                },
                cancellationToken
            );

            try
            {
                return JsonSerializer.Deserialize<T>(response.Text, JsonSerializerOptions.Web)
                    ?? throw new JsonException("The structured response was null.");
            }
            catch (JsonException exception)
            {
                if (attempt != 0)
                {
                    throw new InvalidOperationException(
                        "The rating agent returned invalid structured output after one retry.",
                        firstFailure ?? exception
                    );
                }

                firstFailure = exception;
            }
        }

        throw new InvalidOperationException(
            "The rating agent returned invalid structured output after one retry.",
            firstFailure
        );
    }
}
