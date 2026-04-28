using Asp.Versioning.ApiExplorer;
using BookWorm.Chassis.Utilities.Configurations;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;

internal sealed class OpenApiInfoDefinitionsTransformer<T>(T appSettings)
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

        var versionedDescriptionProvider =
            context.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
        var apiDescription = versionedDescriptionProvider?.ApiVersionDescriptions.SingleOrDefault(
            description => description.GroupName == context.DocumentName
        );

        if (apiDescription is null)
        {
            return Task.CompletedTask;
        }

        document.Info.Version = apiDescription.ApiVersion.ToString();

        return Task.CompletedTask;
    }
}

public static class OpenApiInfoDefinitionsTransformerExtensions
{
    public static OpenApiOptions ApplyOpenApiInfoDefinitions<T>(this OpenApiOptions options)
        where T : AppSettings, new()
    {
        options.AddDocumentTransformer(new OpenApiInfoDefinitionsTransformer<T>(new()));
        return options;
    }
}
