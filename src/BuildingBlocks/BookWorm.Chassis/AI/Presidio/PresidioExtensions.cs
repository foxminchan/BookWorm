using BookWorm.Constants.Aspire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            var analyzerConnectionString = builder.Configuration.GetConnectionString(
                Components.Presidio.Analyzer
            );

            var anonymizerConnectionString = builder.Configuration.GetConnectionString(
                Components.Presidio.Anonymizer
            );

            if (
                string.IsNullOrWhiteSpace(analyzerConnectionString)
                || string.IsNullOrWhiteSpace(anonymizerConnectionString)
            )
            {
                return builder;
            }

            var services = builder.Services;

            services
                .AddHttpClient(
                    Components.Presidio.Analyzer,
                    client => client.BaseAddress = new(analyzerConnectionString)
                )
                .ConfigurePrimaryHttpMessageHandler(() =>
                    new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
                );

            services
                .AddHttpClient(
                    Components.Presidio.Anonymizer,
                    client => client.BaseAddress = new(anonymizerConnectionString)
                )
                .ConfigurePrimaryHttpMessageHandler(() =>
                    new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
                );

            services.AddSingleton<IPresidioService, PresidioService>();

            return builder;
        }
    }
}
