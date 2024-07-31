using BookWorm.Identity.Options;
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
            }
        ];
    }
}
