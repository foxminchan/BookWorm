using System.Net.Mime;
using System.Text;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi;

internal static class OpenApiOptionsExtensions
{
    public static void ApplyApiVersionInfo(this OpenApiOptions options, Document? openApiDocument)
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
                    Name = openApiDocument?.LicenseName,
                    Url = openApiDocument?.LicenseUrl,
                };

                document.Info.Contact = new()
                {
                    Name = openApiDocument?.AuthorName,
                    Url = openApiDocument?.AuthorUrl,
                    Email = openApiDocument?.AuthorEmail,
                };

                document.Info.Version = apiDescription.ApiVersion.ToString();

                if (!string.IsNullOrWhiteSpace(openApiDocument?.Title))
                {
                    document.Info.Title = openApiDocument.Title;
                }

                if (!string.IsNullOrWhiteSpace(openApiDocument?.Description))
                {
                    document.Info.Description = BuildDescription(
                        apiDescription,
                        openApiDocument.Description
                    );
                }

                return Task.CompletedTask;
            }
        );
    }

    private static string BuildDescription(ApiVersionDescription api, string description)
    {
        var text = new StringBuilder(description);

        if (api.IsDeprecated)
        {
            text.AppendLine();
            text.AppendLine("**This API version has been deprecated.**");
        }

        if (api.SunsetPolicy is not { } policy)
        {
            return text.ToString();
        }

        text.AppendLine();

        if (policy.Date is { } when)
        {
            text.AppendLine($"**Sunset date:** {when:yyyy-MM-dd}");
        }

        if (!policy.HasLinks)
        {
            return text.ToString();
        }

        text.AppendLine();

        var rendered = false;

        foreach (var link in policy.Links.Where(l => l.Type == MediaTypeNames.Text.Html))
        {
            if (!rendered)
            {
                text.Append("<h4>Links</h4><ul>");
                rendered = true;
            }

            text.Append("<li><a href=\"");
            text.Append(link.LinkTarget.OriginalString);
            text.Append("\">");
            text.Append(
                StringSegment.IsNullOrEmpty(link.Title)
                    ? link.LinkTarget.OriginalString
                    : link.Title.ToString()
            );
            text.Append("</a></li>");
        }

        if (rendered)
        {
            text.Append("</ul>");
        }

        return text.ToString();
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
