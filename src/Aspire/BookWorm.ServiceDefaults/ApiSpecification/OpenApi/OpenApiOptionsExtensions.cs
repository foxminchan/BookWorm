using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi;

internal static class OpenApiOptionsExtensions
{
    public static void ApplyApiVersionInfo(
        this OpenApiOptions options,
        DocumentOptions? openApiDocument,
        ApiVersionDescription apiDescription
    )
    {
        options.AddDocumentTransformer(
            (document, _, _) =>
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

                return Task.CompletedTask;
            }
        );
    }

    public static void ApplySecuritySchemeDefinitions(this OpenApiOptions options)
    {
        options.AddDocumentTransformer<SecuritySchemeDefinitionsTransformer>();
    }

    public static void ApplyOperationDeprecatedStatus(this OpenApiOptions options)
    {
        options.AddOperationTransformer(
            (operation, context, _) =>
            {
                var apiDescription = context.Description;
                operation.Deprecated |= apiDescription.IsDeprecated();
                return Task.CompletedTask;
            }
        );
    }

    public static void ApplySchemaNullableFalse(this OpenApiOptions options)
    {
        options.AddSchemaTransformer(
            (schema, _, _) =>
            {
                if (schema.Properties is null)
                {
                    return Task.CompletedTask;
                }

                foreach (var property in schema.Properties)
                {
                    if (schema.Required?.Contains(property.Key) == false)
                    {
                        property.Value.Nullable = false;
                    }
                }

                return Task.CompletedTask;
            }
        );
    }

    public static void ApplyAuthorizationChecks(this OpenApiOptions options, string[] scopes)
    {
        options.AddOperationTransformer(
            (operation, context, _) =>
            {
                var metadata = context.Description.ActionDescriptor.EndpointMetadata;

                if (!metadata.OfType<IAuthorizeData>().Any())
                {
                    return Task.CompletedTask;
                }

                operation.Responses.TryAdd(
                    $"{StatusCodes.Status401Unauthorized}",
                    new() { Description = "Unauthorized" }
                );
                operation.Responses.TryAdd(
                    $"{StatusCodes.Status403Forbidden}",
                    new() { Description = "Forbidden" }
                );

                var oAuthScheme = new OpenApiSecurityScheme
                {
                    Reference = new()
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = OAuthDefaults.DisplayName,
                    },
                };

                operation.Security = [new() { [oAuthScheme] = scopes }];

                return Task.CompletedTask;
            }
        );
    }

    private sealed class SecuritySchemeDefinitionsTransformer(IdentityOptions identityOptions)
        : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken
        )
        {
            var securityScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Description = "OAuth2 security scheme for the BookWorm API",
                Flows = new() { AuthorizationCode = new() { Scopes = identityOptions.Scopes } },
            };

            document.Components ??= new();
            document.Components.SecuritySchemes.Add(OAuthDefaults.DisplayName, securityScheme);

            return Task.CompletedTask;
        }
    }
}
