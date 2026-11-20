using System.Text.Json;
using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions.Abstractions;

public static class DecisionClientExtensions
{
    extension(IDecisionClient client)
    {
        public Task<DecisionResponse> GetDecisionAsync<TState>(
            TState state,
            IEnumerable<DecisionQuestion> questions,
            IEnumerable<AIContent>? contents = null,
            DecisionOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            return client.GetDecisionAsync(
                state,
                AIJsonUtilities.DefaultOptions,
                questions,
                contents,
                options,
                cancellationToken
            );
        }

        public Task<DecisionResponse> GetDecisionAsync<TState>(
            TState state,
            JsonSerializerOptions serializerOptions,
            IEnumerable<DecisionQuestion> questions,
            IEnumerable<AIContent>? contents = null,
            DecisionOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(serializerOptions);
            ArgumentNullException.ThrowIfNull(questions);

            if (state is AIContent or IEnumerable<AIContent>)
            {
                throw new ArgumentException(
                    "AIContent instances must be passed via the 'contents' parameter rather than as 'state'.",
                    nameof(state)
                );
            }

            JsonElement? serializedState = state switch
            {
                null => null,
                JsonElement element => element,
                _ => JsonSerializer.SerializeToElement(state, serializerOptions),
            };

            var questionList = questions as IReadOnlyList<DecisionQuestion> ?? questions.ToList();
            var contentList = contents as IReadOnlyList<AIContent> ?? contents?.ToList();

            return client.GetDecisionAsync(
                new(serializedState, questionList, contentList),
                options,
                cancellationToken
            );
        }

        public TService? GetService<TService>(object? serviceKey = null)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.GetService(typeof(TService), serviceKey) is TService service
                ? service
                : default;
        }
    }
}
