using Microsoft.Extensions.Configuration;

namespace BookWorm.ServiceDefaults.Auth;

public static class IdentitySectionExtensions
{
    private const string IdentitySectionName = "Identity";

    public static string? GetIdentityUrl(this IConfiguration configuration)
    {
        return configuration.GetSection(IdentitySectionName).GetValue<string>("Url");
    }

    public static string? GetClientId(this IConfiguration configuration)
    {
        return configuration.GetSection(IdentitySectionName).GetValue<string>("ClientId");
    }

    public static string? GetClientSecret(this IConfiguration configuration)
    {
        return configuration.GetSection(IdentitySectionName).GetValue<string>("ClientSecret");
    }

    public static Dictionary<string, string?> GetScopes(this IConfiguration configuration)
    {
        var identitySection = configuration.GetSection(IdentitySectionName);

        if (!identitySection.Exists())
        {
            return new();
        }

        return identitySection
            .GetRequiredSection("Scopes")
            .GetChildren()
            .ToDictionary(p => p.Key, p => p.Value);
    }
}
