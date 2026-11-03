using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Agents;

public static class SubAgentFactory
{
    /// <summary>
    ///     Creates function tools that delegate requests to the specified agents.
    /// </summary>
    /// <param name="agents">The agents to expose as tools.</param>
    /// <returns>One function tool for each supplied agent.</returns>
    /// <exception cref="ArgumentNullException">
    ///     The agent collection or one of its entries is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     No agents are supplied, or an agent lacks a name or description.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Multiple agents produce the same tool name.
    /// </exception>
    public static AITool[] Create(params AIAgent[] agents)
    {
        ArgumentNullException.ThrowIfNull(agents);

        if (agents.Length == 0)
        {
            throw new ArgumentException("At least one subagent must be provided.", nameof(agents));
        }

        var toolNames = new HashSet<string>(StringComparer.Ordinal);
        var tools = new AITool[agents.Length];

        for (var index = 0; index < agents.Length; index++)
        {
            var agent = agents[index]
                ?? throw new ArgumentException(
                    "Subagents cannot contain null entries.",
                    nameof(agents)
                );

            if (string.IsNullOrWhiteSpace(agent.Name))
            {
                throw new ArgumentException(
                    "Each subagent must have a name.",
                    nameof(agents)
                );
            }

            if (string.IsNullOrWhiteSpace(agent.Description))
            {
                throw new ArgumentException(
                    $"Subagent '{agent.Name}' must have a description.",
                    nameof(agents)
                );
            }

            var toolName = $"delegate_{SanitizeName(agent.Name)}";

            if (!toolNames.Add(toolName))
            {
                throw new InvalidOperationException(
                    $"Multiple subagents produce the tool name '{toolName}'."
                );
            }

            tools[index] = CreateTool(agent, toolName);
        }

        return tools;
    }

    private static AITool CreateTool(AIAgent agent, string name)
    {
        async Task<string> InvokeAsync(string query, CancellationToken cancellationToken)
        {
            var response = await agent.RunAsync(
                query,
                cancellationToken: cancellationToken
            );

            return response.Text;
        }

        return AIFunctionFactory.Create(InvokeAsync, name, agent.Description);
    }

    private static string SanitizeName(string name)
    {
        return string.Concat(
            name.Select(character =>
                char.IsAsciiLetterOrDigit(character) ? character : '_'
            )
        );
    }
}
