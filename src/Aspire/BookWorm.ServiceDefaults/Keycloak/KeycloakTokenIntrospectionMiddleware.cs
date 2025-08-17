using System.Diagnostics;
using System.Text.Json;

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
        var cancellationToken = context.RequestAborted;

        var token = authHeader?.StartsWith(
            $"{JwtBearerDefaults.AuthenticationScheme} ",
            StringComparison.OrdinalIgnoreCase
        )
            is true
            ? authHeader[$"{JwtBearerDefaults.AuthenticationScheme} ".Length..].Trim()
            : null;

        if (!string.IsNullOrWhiteSpace(token))
        {
            var realmPath = $"realms/{identityOptions.Realm}";
            var introspectionEndpoint = $"{realmPath}/protocol/openid-connect/token/introspect";

            using var httpClient = httpClientFactory.CreateClient(Components.KeyCloak);

            var requestContent = new FormUrlEncodedContent(
                [
                    new("token", token),
                    new("client_id", identityOptions.ClientId),
                    new("client_secret", identityOptions.ClientSecret),
                ]
            );

            var response = await httpClient.PostAsync(
                introspectionEndpoint,
                requestContent,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                await TypedResults
                    .Problem(
                        statusCode: StatusCodes.Status401Unauthorized,
                        title: "Token introspection failed",
                        extensions: new Dictionary<string, object?> { { nameof(traceId), traceId } }
                    )
                    .ExecuteAsync(context);
                return;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using var tokenResponse = JsonDocument.Parse(content);

            var isActive =
                tokenResponse.RootElement.TryGetProperty("active", out var activeElement)
                && activeElement.GetBoolean();

            if (!isActive)
            {
                await TypedResults
                    .Problem(
                        statusCode: StatusCodes.Status401Unauthorized,
                        title: "Token is not active",
                        extensions: new Dictionary<string, object?> { { nameof(traceId), traceId } }
                    )
                    .ExecuteAsync(context);
                return;
            }
        }

        await next(context);
    }
}
