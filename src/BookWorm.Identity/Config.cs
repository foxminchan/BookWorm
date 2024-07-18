using BookWorm.Identity.Options;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace BookWorm.Identity;

public static class Config
{
    public static IEnumerable<IdentityResource> GetResources()
    {
        return
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        ];
    }

    public static IEnumerable<ApiResource> GetApis()
    {
        return
        [
            new("orders", "Orders Service"),
            new("basket", "Basket Service"),
            new("catalog", "Catalog Service"),
            new("rating", "Rating Service")
        ];
    }

    public static IEnumerable<ApiScope> GetApiScopes()
    {
        return
        [
            new("orders", "Orders Service"),
            new("basket", "Basket Service"),
            new("catalog", "Catalog Service"),
            new("rating", "Rating Service")
        ];
    }

    public static IEnumerable<Client> GetClients(ServiceOptions service)
    {
        return
        [
            new()
            {
                ClientId = "bff",
                ClientName = "Backend For Frontend",
                ClientSecrets = { new("secret".Sha256()) },
                AllowedGrantTypes = [GrantType.AuthorizationCode],
                RedirectUris = { $"{service.Bff}/signin-oidc" },
                BackChannelLogoutUri = $"{service.Bff}/bff/backchannel",
                PostLogoutRedirectUris = { $"{service.Bff}/signout-callback-oidc" },
                AllowedCorsOrigins = { $"{service.Bff}", $"{service.BackOffice}", $"{service.StoreFront}" },
                AllowOfflineAccess = true,
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "orders",
                    "basket",
                    "catalog",
                    "rating"
                },
                AccessTokenLifetime = 60 * 60 * 2,
                IdentityTokenLifetime = 60 * 60 * 2
            }
        ];
    }
}
