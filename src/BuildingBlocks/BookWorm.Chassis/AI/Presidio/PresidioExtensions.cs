using BookWorm.Chassis.AI.Presidio.Clients;
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;

namespace BookWorm.Chassis.AI.Presidio;

public static class PresidioExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Adds the Presidio PII detection and anonymization service to the DI container.
        ///     Reads connection strings for the analyzer and anonymizer from configuration.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        public IHostApplicationBuilder AddPresidio()
        {
            var analyzerConnectionString = builder.Configuration.GetRequiredConnectionString(
                Components.Presidio.Analyzer
            );

            var anonymizerConnectionString = builder.Configuration.GetRequiredConnectionString(
                Components.Presidio.Anonymizer
            );

            var services = builder.Services;
            var settings = RefitSettings.ForJsonContext(PresidioSerializationContext.Default);

            services
                .AddRefitGeneratedClient<IPresidioAnalyzer>(settings, Components.Presidio.Analyzer)
                .ConfigureHttpClient(client => client.BaseAddress = new(analyzerConnectionString))
                .ConfigurePrimaryHttpMessageHandler(() =>
                    new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
                );

            services
                .AddRefitGeneratedClient<IPresidioAnonymizer>(
                    settings,
                    Components.Presidio.Anonymizer
                )
                .ConfigureHttpClient(client => client.BaseAddress = new(anonymizerConnectionString))
                .ConfigurePrimaryHttpMessageHandler(() =>
                    new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
                );

            services.AddTransient<IPresidioService, PresidioService>();

            return builder;
        }
    }
}
