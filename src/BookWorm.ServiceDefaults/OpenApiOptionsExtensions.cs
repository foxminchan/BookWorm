using System.Text;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;

namespace BookWorm.ServiceDefaults;

internal static class OpenApiOptionsExtensions
{
    public static OpenApiOptions ApplyApiVersionInfo(this OpenApiOptions options)
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
                document.Info.Description = BuildDescription(apiDescription);
                return Task.CompletedTask;
            }
        );
        return options;
    }

    private static string BuildDescription(ApiVersionDescription api)
    {
        var text = new StringBuilder();

        if (api.IsDeprecated)
        {
            if (text.Length > 0)
            {
                if (text[^1] != '.')
                {
                    text.Append('.');
                }

                text.Append(' ');
            }

            text.Append("This API version has been deprecated.");
        }

        if (api.SunsetPolicy is not { } policy)
        {
            return text.ToString();
        }

        if (policy.Date is { } when)
        {
            if (text.Length > 0)
            {
                text.Append(' ');
            }

            text.Append("The API will be sunset on ")
                .Append(when.Date.ToShortDateString())
                .Append('.');
        }

        if (!policy.HasLinks)
        {
            return text.ToString();
        }

        text.AppendLine();

        var rendered = false;

        foreach (var link in policy.Links.Where(l => l.Type == "text/html"))
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
