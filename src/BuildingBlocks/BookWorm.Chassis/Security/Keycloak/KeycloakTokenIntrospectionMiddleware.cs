using System.Diagnostics;
using System.Text.Json;
using BookWorm.Chassis.Security.Settings;
using BookWorm.Constants.Aspire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.Security.Keycloak;

internal sealed class KeycloakTokenIntrospectionMiddleware(
    IHttpClientFactory httpClientFactory,
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

        var introspectionEndpoint = KeycloakEndpoints
            .Introspect(identityOptions.Realm)
            .TrimStart('/');

        using var httpClient = httpClientFactory.CreateClient(Components.KeyCloak);

        using var requestContent = new FormUrlEncodedContent([
            new("token", token),
            new("client_id", identityOptions.ClientId),
            new("client_secret", identityOptions.ClientSecret),
        ]);

        using var response = await httpClient.PostAsync(
            introspectionEndpoint,
            requestContent,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Token introspection returned {StatusCode}", response.StatusCode);

            await WriteProblemAsync(context, "Token introspection failed", traceId);
            return;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var tokenResponse = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: cancellationToken
        );

        var isActive =
            tokenResponse.RootElement.TryGetProperty("active", out var activeElement)
            && activeElement.GetBoolean();

        if (!isActive)
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
        public IServiceCollection AddKeycloakTokenIntrospection()
        {
            return services.AddScoped<KeycloakTokenIntrospectionMiddleware>();
        }
    }

    extension(IApplicationBuilder app)
    {
        public IApplicationBuilder UseKeycloakTokenIntrospection()
        {
            return app.UseMiddleware<KeycloakTokenIntrospectionMiddleware>();
        }
    }
}
