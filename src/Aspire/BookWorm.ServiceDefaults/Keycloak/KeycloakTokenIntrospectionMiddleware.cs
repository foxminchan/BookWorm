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
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        var token = authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) is true
            ? authHeader["Bearer ".Length..].Trim()
            : null;

        if (!string.IsNullOrWhiteSpace(token))
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
                        Title = "Token is not active",
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
