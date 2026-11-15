using Microsoft.Identity.Abstractions;

namespace BookWorm.Chassis.Security.TokenExchange;

public sealed class AgentTokenExchangeOptions
{
    public string[] Scopes { get; set; } = [];

    public string? AgentApplicationId { get; set; }

    public bool IsAppOnly { get; set; }

    public Action<AuthorizationHeaderProviderOptions>? ConfigureHeaderOptions { get; set; }
}
