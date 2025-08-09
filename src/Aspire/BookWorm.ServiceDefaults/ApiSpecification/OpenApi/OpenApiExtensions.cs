namespace BookWorm.ServiceDefaults.ApiSpecification.OpenApi;

public static class OpenApiExtensions
{
    public static void AddDefaultOpenApi(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        var sp = services.BuildServiceProvider();
        var document = sp.GetRequiredService<DocumentOptions>();
        var identity = sp.GetService<IdentityOptions>();

        foreach (var version in sp.GetApiVersionDescription())
        {
            services.AddOpenApi(
                version.GroupName,
                options =>
                {
                    options.ApplyApiVersionInfo(document, version);
                    options.ApplySchemaNullableFalse();

                    if (identity is not null)
                    {
                        options.ApplySecuritySchemeDefinitions();
                        options.ApplyAuthorizationChecks([.. identity.Scopes.Keys]);
                    }

                    options.ApplyOperationDeprecatedStatus();
                }
            );
        }
    }

    public static void UseDefaultOpenApi(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        app.MapOpenApi();
        app.MapGet("/", () => Results.Redirect("openapi/v1.json")).ExcludeFromDescription();
    }
}
