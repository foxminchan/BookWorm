using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;

internal sealed class OpenApiInfoDefinitionsTransformer(
    DocumentOptions? openApiDocument,
    ApiVersionDescription apiDescription
) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        document.Info.License = new()
        {
            Name = openApiDocument?.LicenseName,
            Url = openApiDocument?.LicenseUrl,
        };

        document.Info.Contact = new()
        {
            Name = openApiDocument?.AuthorName,
            Url = openApiDocument?.AuthorUrl,
            Email = openApiDocument?.AuthorEmail,
        };

        if (!string.IsNullOrWhiteSpace(openApiDocument?.Title))
        {
            document.Info.Title = $"{openApiDocument.Title} {apiDescription.ApiVersion}";
        }

        document.Info.Version = apiDescription.ApiVersion.ToString();

        if (!string.IsNullOrWhiteSpace(openApiDocument?.Description))
        {
            document.Info.Description = ApiVersionDescriptionBuilder.BuildDescription(
                apiDescription,
                openApiDocument.Description
            );
        }

        return Task.FromResult(Task.CompletedTask);
    }
}
