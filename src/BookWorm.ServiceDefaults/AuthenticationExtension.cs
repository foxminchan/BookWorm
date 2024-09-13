using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BookWorm.ServiceDefaults;

public static class AuthenticationExtensions
{
    public static IHostApplicationBuilder AddDefaultAuthentication(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var identity = configuration.GetSection(nameof(Identity)).Get<Identity>();

        if (identity is null)
        {
            return builder;
        }

        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = identity.Url;
                options.Audience = identity.Audience;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters.ValidIssuers = [identity.Url];
                options.TokenValidationParameters.ValidateAudience = false;
            });

        builder.Services.AddAuthorization();

        return builder;
    }
}
