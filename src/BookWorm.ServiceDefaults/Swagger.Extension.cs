using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BookWorm.ServiceDefaults;

public static class SwaggerExtension
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

        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.UseSwaggerUI(options =>
        {
            app.DescribeApiVersions()
                .Select(desc => new
                {
                    url = $"/swagger/{desc.GroupName}/swagger.json",
                    name = desc.GroupName.ToUpperInvariant()
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

        return app;
    }
}
