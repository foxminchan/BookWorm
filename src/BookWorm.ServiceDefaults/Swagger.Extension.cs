using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
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

        return app;
    }
}
