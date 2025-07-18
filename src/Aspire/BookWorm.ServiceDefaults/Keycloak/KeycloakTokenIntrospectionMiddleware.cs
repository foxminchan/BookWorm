using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.ServiceDefaults.Keycloak;

public sealed class KeycloakTokenIntrospectionMiddleware(
    IHttpClientFactory httpClientFactory,
    IdentityOptions identityOptions
) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(' ').Last();

        if (token is not null)
        {
            var authorityUri =
                $"{Protocol.HttpOrHttps}://{Components.KeyCloak}/realms/{identityOptions.Realm}";

            var introspectionEndpoint = $"{authorityUri}/protocol/openid-connect/token/introspect";

            using var httpClient = httpClientFactory.CreateClient();

            var requestContent = new FormUrlEncodedContent(
                [
                    new("token", token),
                    new("client_id", identityOptions.ClientId),
                    new("client_secret", identityOptions.ClientSecret),
                ]
            );

            var response = await httpClient.PostAsync(introspectionEndpoint, requestContent);
            if (!response.IsSuccessStatusCode)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var content = await response.Content.ReadAsStringAsync();
            using var tokenResponse = JsonDocument.Parse(content);
            var root = tokenResponse.RootElement;

            var isActive =
                root.TryGetProperty("active", out var activeElement) && activeElement.GetBoolean();
            if (!isActive)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Title = "The provided token is not active or valid",
                        Extensions = new Dictionary<string, object?>
                        {
                            { nameof(traceId), traceId },
                        },
                    }
                );
                return;
            }

            var scopes = root.TryGetProperty("scope", out var scopeElement)
                ? scopeElement.GetString()?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? []
                : [];

            if (!identityOptions.Scopes.Any(scope => scopes.Contains(scope.Key)))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Title = "The token does not include the required scopes",
                        Extensions = new Dictionary<string, object?>
                        {
                            { nameof(traceId), traceId },
                        },
                    }
                );
                return;
            }
        }

        await next(context);
    }
}
