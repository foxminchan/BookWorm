using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi;

internal static class OpenApiOptionsExtensions
{
    public static void ApplyApiVersionInfo(
        this OpenApiOptions options,
        string? title,
        string? description
    )
    {
        options.AddDocumentTransformer(
            (document, context, _) =>
            {
                var apiDescription = context
                    .ApplicationServices.GetService<IApiVersionDescriptionProvider>()
                    ?.ApiVersionDescriptions.SingleOrDefault(versionDescription =>
                        versionDescription.GroupName == context.DocumentName
                    );

                if (apiDescription is null)
                {
                    return Task.CompletedTask;
                }

                document.Info.License = new()
                {
                    Name = "MIT",
                    Url = new("https://opensource.org/licenses/MIT"),
                };

                document.Info.Contact = new()
                {
                    Name = "Nhan Nguyen",
                    Url = new("https://github.com/foxminchan"),
                };

                document.Info.Version = apiDescription.ApiVersion.ToString();

                if (!string.IsNullOrWhiteSpace(title))
                {
                    document.Info.Title = title;
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    document.Info.Description = description;
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
                    if (schema.Required?.Contains(property.Key) != true)
                    {
                        property.Value.Nullable = false;
                    }
                }

                return Task.CompletedTask;
            }
        );
    }

    private sealed class SecuritySchemeDefinitionsTransformer(
        IAuthenticationSchemeProvider authenticationSchemeProvider
    ) : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken
        )
        {
            var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
            if (
                authenticationSchemes.Any(authScheme =>
                    authScheme.Name == JwtBearerDefaults.AuthenticationScheme
                )
            )
            {
                var requirements = new Dictionary<string, OpenApiSecurityScheme>
                {
                    [JwtBearerDefaults.AuthenticationScheme] = new()
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = JwtBearerDefaults.AuthenticationScheme,
                        In = ParameterLocation.Header,
                        BearerFormat = "Json Web Token",
                    },
                };
                document.Components ??= new();
                document.Components.SecuritySchemes = requirements;
            }
        }
    }
}
