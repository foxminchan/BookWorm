using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;

internal sealed class AuthorizationChecksTransformer(IConfiguration configuration)
    : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var scopes = configuration[$"{Microsoft.Identity.Web.Constants.AzureAd}:Scopes"]
            ?.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (scopes is not { Length: > 0 })
        {
            return Task.CompletedTask;
        }

        var metadata = context.Description.ActionDescriptor.EndpointMetadata;

        if (
            !metadata.OfType<IAuthorizeData>().Any()
            && !metadata.OfType<AuthorizationPolicy>().Any()
            && !metadata.OfType<IAuthorizationRequirementData>().Any()
        )
        {
            return Task.CompletedTask;
        }

        operation.Responses ??= [];
        operation.Responses.TryAdd(
            $"{StatusCodes.Status401Unauthorized}",
            new OpenApiResponse { Description = "Unauthorized" }
        );
        operation.Responses.TryAdd(
            $"{StatusCodes.Status403Forbidden}",
            new OpenApiResponse { Description = "Forbidden" }
        );

        var oAuthScheme = new OpenApiSecuritySchemeReference(OAuthDefaults.DisplayName);

        operation.Security = [new() { [oAuthScheme] = [.. scopes] }];

        return Task.CompletedTask;
    }
}
