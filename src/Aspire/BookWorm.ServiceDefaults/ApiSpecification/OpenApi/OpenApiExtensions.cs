using BookWorm.ServiceDefaults.Auth;
using BookWorm.ServiceDefaults.Configuration;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi;

public static class OpenApiExtensions
{
    public static void AddDefaultOpenApi(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.Configure<IdentityOptions>(IdentityOptions.ConfigurationSection);

        var sp = services.BuildServiceProvider();
        var document = sp.GetRequiredService<DocumentOptions>();
        var identity = sp.GetRequiredService<IdentityOptions>();

        Span<string> versions = ["v1"];

        foreach (var description in versions)
        {
            services.AddOpenApi(
                description,
                options =>
                {
                    options.ApplyApiVersionInfo(document);
                    options.ApplySchemaNullableFalse();
                    options.ApplySecuritySchemeDefinitions();
                    options.ApplyAuthorizationChecks([.. identity.Scopes.Keys]);
                    options.ApplyOperationDeprecatedStatus();
                }
            );
        }
    }

    public static void UseDefaultOpenApi(this WebApplication app)
    {
        app.MapOpenApi();

        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        app.MapScalarApiReference(options =>
        {
            options.Theme = ScalarTheme.BluePlanet;
            options.DefaultFonts = false;
            options.AddAuthorizationCodeFlow(
                OAuthDefaults.DisplayName,
                flow =>
                {
                    var identity = app.Services.GetRequiredService<IdentityOptions>();
                    flow.Pkce = Pkce.Sha256;
                    flow.ClientId = identity.ClientId;
                    flow.ClientSecret = identity.ClientSecret;
                }
            );
        });

        app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
    }
}
