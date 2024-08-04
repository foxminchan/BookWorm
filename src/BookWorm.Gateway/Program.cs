using BookWorm.Gateway;
using BookWorm.ServiceDefaults;
using Duende.Bff.Yarp;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddBff()
    .AddRemoteApis()
    .AddServerSideSessions();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddBffExtensions();

Configuration config = new();
builder.Configuration.Bind("BFF", config);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        options.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "__Host-bff";
        options.Cookie.SameSite = SameSiteMode.Strict;
    })
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Authority = config.Authority;
        options.ClientId = config.ClientId;
        options.ClientSecret = config.ClientSecret;
        options.UsePkce = true;

        options.ResponseType = OpenIdConnectResponseType.Code;
        options.ResponseMode = OpenIdConnectResponseMode.Query;

        options.GetClaimsFromUserInfoEndpoint = true;
        options.MapInboundClaims = false;
        options.SaveTokens = true;

        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        options.Scope.Clear();
        foreach (var scope in config.Scopes)
        {
            options.Scope.Add(scope);
        }

        options.TokenValidationParameters.ValidIssuers = [config.Authority];
        options.TokenValidationParameters.ValidateAudience = false;
    });


var app = builder.Build();

app.MapDefaultEndpoints();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseBff();
app.UseRouting();
app.UseAuthorization();

app.MapBffManagementEndpoints();

app.MapBffReverseProxy();

if (config.Apis.Any())
{
    foreach (var api in config.Apis)
    {
        app.MapRemoteBffApiEndpoint(api.LocalPath, api.RemoteUrl!)
            .RequireAccessToken(api.RequiredToken);
    }
}

app.Run();
