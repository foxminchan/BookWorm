namespace BookWorm.Chassis.Security.TokenExchange;

public sealed class AgentIdentityOptions
{
    public const string ConfigurationSection = "AgentIdentities";

    public string? DefaultAgentId { get; init; }

    public Dictionary<string, string> Agents { get; } = [with(StringComparer.OrdinalIgnoreCase)];

    /// <summary>
    ///     Resolves the configured agent application ID for the given key, falling back to <see cref="DefaultAgentId" />.
    /// </summary>
    /// <param name="key">Optional service key or agent name to look up.</param>
    /// <returns>The resolved agent application ID, or <see langword="null" /> if none is configured.</returns>
    public string? ResolveAgentId(string? key = null)
    {
        if (
            !string.IsNullOrWhiteSpace(key)
            && Agents.TryGetValue(key, out var agentId)
            && !string.IsNullOrWhiteSpace(agentId)
        )
        {
            return agentId;
        }

        return string.IsNullOrWhiteSpace(DefaultAgentId) ? null : DefaultAgentId;
    }
}
