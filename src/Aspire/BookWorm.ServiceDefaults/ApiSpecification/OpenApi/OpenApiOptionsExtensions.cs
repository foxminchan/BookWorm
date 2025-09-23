using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi;

public static class OpenApiOptionsExtensions
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
            var keycloakUrl = ServiceDiscoveryUtilities.GetServiceEndpoint(
                Components.KeyCloak,
                Protocols.Http
            );

            if (string.IsNullOrWhiteSpace(keycloakUrl))
            {
                return Task.CompletedTask;
            }

            var realmPath = $"realms/{identityOptions.Realm}";

            var authorizationUrl = $"{keycloakUrl}/{realmPath}/protocol/openid-connect/auth";

            // Please refer: https://github.com/scalar/scalar/issues/6225
            var tokenUrl =
                $"{Protocols.Http}://{Components.KeyCloak}/{realmPath}/protocol/openid-connect/token";

            var securityScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Description = "OAuth2 security scheme for the BookWorm API",
                Flows = new()
                {
                    AuthorizationCode = new()
                    {
                        Scopes = identityOptions.Scopes!,
                        AuthorizationUrl = new(authorizationUrl),
                        TokenUrl = new(tokenUrl),
                    },
                },
            };

            document.Components ??= new();
            document.Components.SecuritySchemes ??=
                new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes.Add(OAuthDefaults.DisplayName, securityScheme);

            return Task.CompletedTask;
        }
    }
}
