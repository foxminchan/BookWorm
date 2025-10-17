using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;

internal sealed class AuthorizationChecksTransformer(string[] scopes) : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;

        if (!metadata.OfType<IAuthorizeData>().Any())
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
