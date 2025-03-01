using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.ServiceDefaults;

public static class CorsExtensions
{
    public static void AddDefaultCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowAll",
                builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
            );
        });
    }

    public static void UseDefaultCors(this IApplicationBuilder app)
    {
        app.UseCors("AllowAll");
    }
}
