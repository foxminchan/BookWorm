﻿using BookWorm.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.Enrichment;

namespace BookWorm.SharedKernel.Logging;

public sealed class ApplicationEnricher(IHttpContextAccessor httpContextAccessor) : ILogEnricher
{
    public void Enrich(IEnrichmentTagCollector collector)
    {
        collector.Add(LoggingConstant.MachineName, Environment.MachineName);

        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            collector.Add(
                LoggingConstant.UserId,
                httpContext.User.Claims.First(c => c.Type == "sub").Value
            );
        }
    }
}
