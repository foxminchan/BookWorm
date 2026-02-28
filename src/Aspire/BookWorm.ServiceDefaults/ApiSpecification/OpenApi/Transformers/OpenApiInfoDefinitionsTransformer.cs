using BookWorm.Chassis.Utilities.Configurations;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;

public sealed class OpenApiInfoDefinitionsTransformer<T>(T appSettings)
    : IOpenApiDocumentTransformer
    where T : AppSettings, new()
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        if (appSettings.OpenApi is null)
        {
            return Task.CompletedTask;
        }

        document.Info = appSettings.OpenApi;

        return Task.CompletedTask;
    }
}
