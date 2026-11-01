using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Agents.AI;

namespace BookWorm.Rating.Infrastructure.Summarizer.Workflows.Runners;

internal sealed class StructuredRatingResponseRunner(AIAgent ratingAgent)
{
    public async Task<T> RunAsync<T>(
        JsonTypeInfo<T> jsonTypeInfo,
        ChatMessage message,
        CancellationToken cancellationToken
    )
        where T : class
    {
        JsonException? firstFailure = null;

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var session = await ratingAgent.CreateSessionAsync(cancellationToken);
            var response = await ratingAgent.RunAsync(
                [message],
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
                        firstFailure
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
