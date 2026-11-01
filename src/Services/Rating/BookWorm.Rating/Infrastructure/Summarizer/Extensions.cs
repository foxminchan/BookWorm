using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Rating.Infrastructure.Summarizer.Providers;
using BookWorm.Rating.Infrastructure.Summarizer.Workflows;

namespace BookWorm.Rating.Infrastructure.Summarizer;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddSummarizer()
        {
            builder.Configure<RatingSummarizerOptions>(
                RatingSummarizerOptions.ConfigurationSection
            );

            var services = builder.Services;
            services.AddScoped<ISummarizer, RatingSummarizer>();
            services.AddScoped<IRatingPromptProvider, McpRatingPromptProvider>();
            services.AddScoped<IRatingSummarizerWorkflowFactory, RatingSummarizerWorkflowFactory>();
        }
    }
}
