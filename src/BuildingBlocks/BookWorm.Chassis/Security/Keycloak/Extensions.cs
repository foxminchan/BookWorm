using BookWorm.Chassis.Utilities;
using BookWorm.Constants.Aspire;
using BookWorm.Constants.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;

namespace BookWorm.Chassis.Security.Keycloak;

public static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers the Refit client used to communicate with Keycloak.
        /// </summary>
        /// <returns>The same <see cref="IHostApplicationBuilder" /> instance.</returns>
        public IHostApplicationBuilder AddKeycloakClient()
        {
            var scheme = builder.Environment.IsDevelopment()
                ? Uri.UriSchemeHttp
                : Http.Schemes.HttpOrHttps;
            var keycloakUrl = HttpUtilities
                .AsUrlBuilder()
                .WithScheme(scheme)
                .WithHost(Components.KeyCloak)
                .Build();

            builder
                .Services.AddRefitGeneratedClient<IKeycloakApi>(
                    RefitSettings.ForJsonContext(KeycloakSerializationContext.Default),
                    Components.KeyCloak
                )
                .ConfigureHttpClient(client => client.BaseAddress = new(keycloakUrl));

            return builder;
        }
    }
}
