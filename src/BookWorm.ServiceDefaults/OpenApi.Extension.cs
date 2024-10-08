﻿using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

namespace BookWorm.ServiceDefaults;

public static class OpenApiExtension
{
    public static IHostApplicationBuilder AddOpenApi(this IHostApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();

        builder.Services
            .AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGenOptions>()
            .AddFluentValidationRulesToSwagger()
            .AddSwaggerGen(options => options.OperationFilter<OpenApiDefaultValues>());

        return builder;
    }

    public static WebApplication UseOpenApi(this WebApplication app)
    {
        app.UseSwagger(c => c.PreSerializeFilters.Add((swagger, httpReq) =>
        {
            ArgumentNullException.ThrowIfNull(httpReq);

            swagger.Servers =
            [
                new()
                {
                    Url = $"{httpReq.Scheme}://{httpReq.Host.Value}",
                    Description = string.Join(
                        " ",
                        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Production,
                        nameof(Environment)
                    )
                }
            ];
        }));

        app.UseReDoc(options =>
        {
            app.DescribeApiVersions()
                .Select(desc => $"/swagger/{desc.GroupName}/swagger.json")
                .ToList()
                .ForEach(spec => options.SpecUrl(spec));

            options.EnableUntrustedSpec();
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerUI(options =>
            {
                app.DescribeApiVersions()
                    .Select(desc => new
                    {
                        url = $"/swagger/{desc.GroupName}/swagger.json", name = desc.GroupName.ToUpperInvariant()
                    })
                    .ToList()
                    .ForEach(endpoint => options.SwaggerEndpoint(endpoint.url, endpoint.name));

                var auth = app.Configuration.GetSection(nameof(OpenApi)).Get<OpenApi>()?.Auth;

                if (auth is null)
                {
                    return;
                }

                options.DocumentTitle = auth.AppName;
                options.OAuthClientId(auth.ClientId);
                options.OAuthClientSecret(auth.ClientSecret);
                options.OAuthAppName(auth.AppName);
                options.OAuthUsePkce();
                options.EnableValidator();
            });

            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
        }
        else
        {
            app.MapGet("/", () => Results.Redirect("/api-docs")).ExcludeFromDescription();
        }

        return app;
    }
}
