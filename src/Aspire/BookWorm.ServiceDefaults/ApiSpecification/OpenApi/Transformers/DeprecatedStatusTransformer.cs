using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;

internal sealed class DeprecatedStatusTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        operation.Deprecated =
            operation.Deprecated
            || context
                .Description.ActionDescriptor.EndpointMetadata.OfType<ObsoleteAttribute>()
                .Any();

        return Task.CompletedTask;
    }
}
