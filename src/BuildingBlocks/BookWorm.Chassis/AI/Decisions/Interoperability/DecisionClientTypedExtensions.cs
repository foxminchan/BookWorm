using System.Text.Json;
using BookWorm.Chassis.AI.Decisions.Abstractions;
using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions.Interoperability;

public static class DecisionClientTypedExtensions
{
    public static Task<DecisionResponse<TResult>> GetDecisionAsync<TState, TResult>(
        this IDecisionClient client,
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

        JsonElement? serializedState = state switch
        {
            null => null,
            JsonElement element => element,
            _ => JsonSerializer.SerializeToElement(state, definition.SerializerOptions),
        };

        return ExecuteTypedAsync(
            client,
            serializedState,
            definition,
            contents,
            options,
            cancellationToken
        );
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
