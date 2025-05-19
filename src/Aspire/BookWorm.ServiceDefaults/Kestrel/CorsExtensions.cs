using System.Text;
using BookWorm.Constants;
using BookWorm.Constants.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookWorm.ServiceDefaults.Kestrel;

public static class CorsExtensions
{
    private const string AllowAllCorsPolicy = "AllowAll";
    private const string AllowSpecificCorsPolicy = "AllowSpecific";

    public static void AddDefaultCors(this IHostApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    AllowAllCorsPolicy,
                    policyBuilder =>
                    {
                        policyBuilder.SetIsOriginAllowed(origin =>
                            new Uri(origin).Host == Restful.Host.Localhost
                        );
                    }
                );
            });
        }
        else
        {
            var environmentName = builder.Environment.EnvironmentName;
            var storeFront = GetOrigin(false, environmentName);
            var backOffice = GetOrigin(true, environmentName);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    AllowSpecificCorsPolicy,
                    policyBuilder =>
                    {
                        policyBuilder
                            .WithOrigins(storeFront, backOffice)
                            .WithMethods(
                                Restful.Methods.Get,
                                Restful.Methods.Post,
                                Restful.Methods.Put,
                                Restful.Methods.Patch,
                                Restful.Methods.Delete,
                                Restful.Methods.Options
                            )
                            .AllowAnyHeader()
                            .AllowCredentials();
                    }
                );
            });
        }
    }

    public static void UseDefaultCors(this WebApplication app)
    {
        app.UseCors(app.Environment.IsDevelopment() ? AllowAllCorsPolicy : AllowSpecificCorsPolicy);
    }

    private static string GetOrigin(bool isAdmin, string environmentName)
    {
        var builder = new StringBuilder();

        builder.Append("https://");

        environmentName = environmentName.ToLowerInvariant();

        if (
            !string.Equals(
                environmentName,
                Environments.Production,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            builder.Append($"{environmentName}.");
        }

        if (isAdmin)
        {
            builder.Append("admin.");
        }

        builder.Append($"{nameof(BookWorm).ToLowerInvariant()}.com");

        return builder.ToString();
    }
}
