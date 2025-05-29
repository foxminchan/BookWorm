using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.Search;

public static class Extensions
{
    private const string ActivitySourceName = "Microsoft.SemanticKernel*";

    public static void AddSearchService(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        AppContext.SetSwitch(
            "Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive",
            builder.Environment.IsDevelopment()
        );

        services.AddScoped<ISearch, HybridSearch>();

        services
            .AddOpenTelemetry()
            .WithTracing(x => x.AddSource(ActivitySourceName))
            .WithMetrics(x => x.AddMeter(ActivitySourceName));
    }
}
