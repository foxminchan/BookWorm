namespace BookWorm.Chassis.Security.Settings;

public sealed class IdentityOptions : IEquatable<IdentityOptions>
{
    public const string ConfigurationSection = "Identity";
    public string Realm { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public Dictionary<string, string?> Scopes { get; init; } = [];
    public Dictionary<string, string?> TokenExchangeTargets { get; init; } = [];

    public bool Equals(IdentityOptions? other)
    {
        return other is not null
            && Realm == other.Realm
            && ClientId == other.ClientId
            && ClientSecret == other.ClientSecret
            && Scopes.Count == other.Scopes.Count
            && Scopes.All(kvp =>
                other.Scopes.TryGetValue(kvp.Key, out var value) && kvp.Value == value
            )
            && TokenExchangeTargets.Count == other.TokenExchangeTargets.Count
            && TokenExchangeTargets.All(kvp =>
                other.TokenExchangeTargets.TryGetValue(kvp.Key, out var v2) && kvp.Value == v2
            );
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as IdentityOptions);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Scopes, TokenExchangeTargets);
    }
}
