using BookWorm.Identity.Data;
using BookWorm.Identity.Data.CompliedModels;
using BookWorm.Identity.DataProtection;
using BookWorm.Identity.Models;
using BookWorm.ServiceDefaults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Identity;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var appSettings = new AppSettings();

        builder.Configuration.Bind(appSettings);

        builder.AddServiceDefaults();

        builder.AddRedisDataProtection();

        builder.Services.AddRazorPages();

        builder.Services.AddMigration<ApplicationDbContext, SeedData>();

        builder.AddNpgsqlDbContext<ApplicationDbContext>("identitydb",
            configureDbContextOptions: dbContextOptionsBuilder => dbContextOptionsBuilder.UseNpgsql()
                .UseModel(ApplicationDbContextModel.Instance));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var identityServerBuilder = builder.Services
            .AddIdentityServer(options =>
            {
                options.Authentication.CookieLifetime = TimeSpan.FromHours(2);
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.EmitStaticAudienceClaim = true;

                if (builder.Environment.IsDevelopment())
                {
                    options.KeyManagement.Enabled = false;
                }
            })
            .AddInMemoryIdentityResources(Config.GetResources())
            .AddInMemoryApiScopes(Config.GetApiScopes())
            .AddInMemoryApiResources(Config.GetApis())
            .AddInMemoryClients(Config.GetClients(appSettings.Services))
            .AddAspNetIdentity<ApplicationUser>();

        // not recommended for production - you need to store your key material somewhere secure
        if (builder.Environment.IsDevelopment())
        {
            identityServerBuilder.AddDeveloperSigningCredential();
        }

        builder.Services.AddAuthentication();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.MapDefaultEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCookiePolicy(new() { MinimumSameSitePolicy = SameSiteMode.Lax });

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();
        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }
}
