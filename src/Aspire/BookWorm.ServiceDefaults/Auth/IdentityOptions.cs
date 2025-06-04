namespace BookWorm.ServiceDefaults.Auth;

public sealed class IdentityOptions : IEquatable<IdentityOptions>
{
    public const string ConfigurationSection = "Identity";
    public string ClientId { get; init; } = string.Empty;
    public string? ClientSecret { get; init; } = string.Empty;
    public Dictionary<string, string?> Scopes { get; init; } = [];

    public bool Equals(IdentityOptions? other)
    {
        return other is not null
            && ClientId == other.ClientId
            && ClientSecret == other.ClientSecret
            && Scopes.Count == other.Scopes.Count
            && Scopes.All(kvp =>
                other.Scopes.TryGetValue(kvp.Key, out var value) && kvp.Value == value
            );
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as IdentityOptions);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ClientId, ClientSecret, Scopes);
    }
}
