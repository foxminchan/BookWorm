using System.Security.Claims;
using BookWorm.Constants.Other;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Enrichment;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.Logging;

internal sealed class ApplicationEnricher(IHttpContextAccessor httpContextAccessor) : ILogEnricher
{
    public void Enrich(IEnrichmentTagCollector collector)
    {
        collector.Add(LoggingConstant.MachineName, Environment.MachineName);

        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            collector.Add(
                LoggingConstant.UserId,
                httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
            );
        }
    }
}

public static class ApplicationEnricherExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers the <see cref="ApplicationEnricher" /> to enrich log events with application-specific properties such
        ///     as the machine name and user id.
        /// </summary>
        public void AddApplicationEnricher()
        {
            builder.Services.AddLogEnricher<ApplicationEnricher>();
        }
    }
}
