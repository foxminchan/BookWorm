using System.Diagnostics;
using BookWorm.Chassis.Security.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.Security.Keycloak;

internal sealed class KeycloakTokenIntrospectionMiddleware(
    IKeycloakApi keycloakApi,
    IdentityOptions identityOptions,
    ILogger<KeycloakTokenIntrospectionMiddleware> logger
) : IMiddleware
{
    private const string BearerPrefix = $"{JwtBearerDefaults.AuthenticationScheme} ";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();

        var requiresAuth = endpoint?.Metadata.GetMetadata<IAuthorizeData>() is not null;
        var allowsAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null;

        if (!requiresAuth || allowsAnonymous)
        {
            await next(context);
            return;
        }

        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        var cancellationToken = context.RequestAborted;

        var token = authHeader?.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase) is true
            ? authHeader[BearerPrefix.Length..].Trim()
            : null;

        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogWarning("Missing or invalid Authorization header");

            await WriteProblemAsync(context, "Authorization header missing or invalid", traceId);
            return;
        }

        using var response = await keycloakApi.IntrospectTokenAsync(
            identityOptions.Realm,
            new()
            {
                ["token"] = token,
                ["client_id"] = identityOptions.ClientId,
                ["client_secret"] = identityOptions.ClientSecret,
            },
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Token introspection returned {StatusCode}", response.StatusCode);

            await WriteProblemAsync(context, "Token introspection failed", traceId);
            return;
        }

        await response.EnsureSuccessfulAsync();

        if (response.Content?.Active is not true)
        {
            logger.LogInformation("Inactive token presented");

            await WriteProblemAsync(context, "Token is not active", traceId);
            return;
        }

        await next(context);
    }

    private static Task WriteProblemAsync(HttpContext context, string title, string traceId)
    {
        return TypedResults
            .Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: title,
                extensions: new Dictionary<string, object?> { { nameof(traceId), traceId } }
            )
            .ExecuteAsync(context);
    }
}

public static class KeycloakTokenIntrospectionMiddlewareExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Registers the Keycloak token introspection middleware in the dependency injection container.
        /// </summary>
        /// <returns>
        ///     The updated <see cref="IServiceCollection" /> instance.
        /// </returns>
        public IServiceCollection AddKeycloakTokenIntrospection()
        {
            return services.AddScoped<KeycloakTokenIntrospectionMiddleware>();
        }
    }

    extension(IApplicationBuilder app)
    {
        /// <summary>
        ///     Adds the Keycloak token introspection middleware to the application request pipeline.
        /// </summary>
        /// <returns>
        ///     The same <see cref="IApplicationBuilder" /> instance so additional middleware can be chained.
        /// </returns>
        public IApplicationBuilder UseKeycloakTokenIntrospection()
        {
            return app.UseMiddleware<KeycloakTokenIntrospectionMiddleware>();
        }
    }
}
