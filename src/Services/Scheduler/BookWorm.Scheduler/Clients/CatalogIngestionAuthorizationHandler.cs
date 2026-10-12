using System.Diagnostics.CodeAnalysis;
using BookWorm.Chassis.Security.TokenExchange;
using BookWorm.Constants.Aspire;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace BookWorm.Scheduler.Clients;

[ExcludeFromCodeCoverage]
internal sealed class CatalogIngestionAuthorizationHandler(ITokenExchange tokenExchange)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var exchangedToken = await tokenExchange.ExchangeClientCredentialsAsync(
            Services.CatalogIngestion,
            cancellationToken: cancellationToken
        );

        request.Headers.Authorization = new(JwtBearerDefaults.AuthenticationScheme, exchangedToken);
        return await base.SendAsync(request, cancellationToken);
    }
}
