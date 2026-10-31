using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using BookWorm.Constants.Other;
using BookWorm.Rating.Domain.FeedbackAggregator.Specifications;
using BookWorm.Rating.Infrastructure.Summarizer.Models;
using BookWorm.Rating.Infrastructure.Summarizer.Providers;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace BookWorm.Rating.Infrastructure.Summarizer;

internal sealed class RatingSummarizer(
    [FromKeyedServices(Agents.RatingAgent)] AIAgent ratingAgent,
    IRatingPromptProvider promptProvider,
    IFeedbackRepository feedbackRepository
) : ISummarizer
{
    public async Task<SummarizeResult> SummarizeAsync(
        Guid bookId,
        CancellationToken cancellationToken = default
    )
    {
        var feedback = await feedbackRepository.ListAsync(
            new FeedbackFilterSpec(bookId, nameof(Feedback.Rating), false, 1, int.MaxValue),
            cancellationToken
        );

        var reviewCount = feedback.Count;

        if (reviewCount == 0)
        {
            return new("No feedback found for this book.");
        }

        var averageRating = feedback.Average(review => review.Rating);
        var reviewData = JsonSerializer.Serialize(
            [
                .. feedback.Select(review => new FeedbackReview(
                    (Guid)review.Id,
                    review.FirstName,
                    review.LastName,
                    review.Comment,
                    review.Rating,
                    review.BookId
                )),
            ],
            RatingSummarizerSerializationContext.Default.FeedbackReviewArray
        );

        var summaryPrompt = await promptProvider.GetSummaryPromptAsync(
            reviewData,
            cancellationToken
        );

        var summary = await RunStructuredWithRetryAsync(
            RatingSummarizerSerializationContext.Default.RatingSummaryOutput,
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
        var assessment = await RunStructuredWithRetryAsync(
            RatingSummarizerSerializationContext.Default.QualityAssessmentOutput,
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
            averageRating,
            reviewCount,
            assessment.Evidence,
            assessment.Confidence
        );
    }

    private async Task<T> RunStructuredWithRetryAsync<T>(
        JsonTypeInfo<T> jsonTypeInfo,
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
                        ToolMode = ChatToolMode.None,
                    },
                },
                cancellationToken
            );

            try
            {
                return JsonSerializer.Deserialize(response.Text, jsonTypeInfo)
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
