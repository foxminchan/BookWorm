using Scalar.AspNetCore;

namespace BookWorm.ServiceDefaults;

public static class OpenApiExtension
{
    public static IHostApplicationBuilder AddOpenApi(this IHostApplicationBuilder builder)
    {
        var openApi = builder.Configuration.GetSection(nameof(OpenApi)).Get<OpenApi>();
        var identitySection = builder.Configuration.GetSection(nameof(Identity));

        var scopes = identitySection.Exists()
            ? identitySection
                .GetRequiredSection("Scopes")
                .GetChildren()
                .ToDictionary(p => p.Key, p => p.Value)
            : [];

        if (openApi is null)
        {
            return builder;
        }

        string[] versions = ["v1"];
        foreach (var description in versions)
        {
            builder.Services.AddOpenApi(
                description,
                options =>
                {
                    options.ApplyApiVersionInfo(
                        openApi.Document.Title,
                        openApi.Document.Description
                    );
                    options.ApplyAuthorizationChecks([.. scopes.Keys]);
                    options.ApplySecuritySchemeDefinitions();
                    options.ApplyOperationDeprecatedStatus();
                    options.ApplyApiVersionDescription();
                    options.ApplySchemaNullableFalse();
                    options.AddDocumentTransformer(
                        (document, context, cancellationToken) =>
                        {
                            document.Servers = [];
                            return Task.CompletedTask;
                        }
                    );
                }
            );
        }

        return builder;
    }

    public static IApplicationBuilder UseOpenApi(this WebApplication app)
    {
        var configuration = app.Configuration;
        var openApiSection = configuration.GetSection(nameof(OpenApi)).Get<OpenApi>();

        if (openApiSection is null)
        {
            return app;
        }

        app.MapOpenApi();

        if (app.Environment.IsDevelopment())
        {
            app.MapScalarApiReference(options =>
            {
                options.DefaultFonts = false;
            });
            app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
        }

        return app;
    }
}
