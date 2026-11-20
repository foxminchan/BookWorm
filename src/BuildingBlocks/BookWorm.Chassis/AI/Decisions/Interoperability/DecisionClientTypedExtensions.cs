using System.Text.Json;
using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions;

public static class DecisionClientTypedExtensions
{
    extension(IDecisionClient client)
    {
        public Task<DecisionResponse<TResult>> GetDecisionAsync<TState, TResult>(
            TState state,
            DecisionDefinition<TResult> definition,
            IEnumerable<AIContent>? contents = null,
            DecisionOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(definition);

            if (state is AIContent or IEnumerable<AIContent>)
            {
                throw new ArgumentException(
                    "AIContent instances must be passed via the 'contents' parameter rather than as 'state'.",
                    nameof(state)
                );
            }

            JsonElement? serializedState =
                state is null ? null
                : state is JsonElement element ? element
                : JsonSerializer.SerializeToElement(state, definition.SerializerOptions);

            return ExecuteTypedAsync(
                client,
                serializedState,
                definition,
                contents,
                options,
                cancellationToken
            );
        }

        public Task<DecisionResponse<TResult>> GetDecisionAsync<TResult>(
            JsonElement state,
            DecisionDefinition<TResult> definition,
            IEnumerable<AIContent>? contents = null,
            DecisionOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(definition);

            return ExecuteTypedAsync(
                client,
                state,
                definition,
                contents,
                options,
                cancellationToken
            );
        }
    }

    private static async Task<DecisionResponse<TResult>> ExecuteTypedAsync<TResult>(
        IDecisionClient client,
        JsonElement? state,
        DecisionDefinition<TResult> definition,
        IEnumerable<AIContent>? contents,
        DecisionOptions? options,
        CancellationToken cancellationToken
    )
    {
        var contentList = contents as IReadOnlyList<AIContent> ?? contents?.ToList();
        var request = new DecisionRequest(state, definition.Questions, contentList);
        var response = await client
            .GetDecisionAsync(request, options, cancellationToken)
            .ConfigureAwait(false);

        return definition.BindResponse(response);
    }
}
