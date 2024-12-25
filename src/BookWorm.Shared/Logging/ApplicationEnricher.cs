﻿namespace BookWorm.Shared.Logging;

public sealed class ApplicationEnricher(IHttpContextAccessor httpContextAccessor) : ILogEnricher
{
    public void Enrich(IEnrichmentTagCollector collector)
    {
        collector.Add("MachineName", Environment.MachineName);

        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            collector.Add("IsAuthenticated", httpContext.User.Identity?.IsAuthenticated);
        }
    }
}
