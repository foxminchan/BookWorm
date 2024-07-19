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
            new("Ordering", "Ordering Service"),
            new("basket", "Basket Service"),
            new("catalog", "Catalog Service"),
            new("rating", "Rating Service")
        ];
    }

    public static IEnumerable<ApiScope> GetApiScopes()
    {
        return
        [
            new("ordering", "Ordering Service"),
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
                ClientId = "catalogswaggerui",
                ClientName = "Catalog Swagger UI",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,
                RedirectUris = { $"{service.Catalog}/swagger/catalog-api/oauth2-redirect.html" },
                PostLogoutRedirectUris = { $"{service.Catalog}/swagger/" },
                AllowedScopes = { "catalog" }
            },
            new()
            {
                ClientId = "orderingswaggerui",
                ClientName = "Ordering Swagger UI",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,
                RedirectUris = { $"{service.Ordering}/swagger/ordering-api/oauth2-redirect.html" },
                PostLogoutRedirectUris = { $"{service.Ordering}/swagger/" },
                AllowedScopes = { "ordering" }
            },
            new()
            {
                ClientId = "basketswaggerui",
                ClientName = "Basket Swagger UI",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,
                RedirectUris = { $"{service.Basket}/swagger/basket-api/oauth2-redirect.html" },
                PostLogoutRedirectUris = { $"{service.Basket}/swagger/" },
                AllowedScopes = { "basket" }
            },
            new()
            {
                ClientId = "ratingswaggerui",
                ClientName = "Rating Swagger UI",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,
                RedirectUris = { $"{service.Rating}/swagger/rating-api/oauth2-redirect.html" },
                PostLogoutRedirectUris = { $"{service.Rating}/swagger/" },
                AllowedScopes = { "rating" }
            },
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
                    "ordering",
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
