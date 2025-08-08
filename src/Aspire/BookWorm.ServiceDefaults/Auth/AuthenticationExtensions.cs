namespace BookWorm.ServiceDefaults.Auth;

public static class AuthenticationExtensions
{
    public static IHostApplicationBuilder AddDefaultAuthentication(
        this IHostApplicationBuilder builder
    )
    {
        var services = builder.Services;

        services.Configure<IdentityOptions>(IdentityOptions.ConfigurationSection);

        var realm = services.BuildServiceProvider().GetRequiredService<IdentityOptions>().Realm;

        services.AddHttpClient(
            Components.KeyCloak,
            client => client.BaseAddress = new($"{Protocols.HttpOrHttps}://{Components.KeyCloak}")
        );

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddKeycloakJwtBearer(
                Components.KeyCloak,
                realm,
                options =>
                {
                    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                    options.Audience = "account";
                }
            );

        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                Authorization.Policies.Admin,
                policy => policy.RequireRole(Authorization.Roles.Admin)
            )
            .AddPolicy(Authorization.Policies.User, policy => policy.RequireAuthenticatedUser())
            .SetDefaultPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        return builder;
    }
}
