using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace BookWorm.AppHost.Extensions.Network;

internal static class CorsExtensions
{
    private static readonly string[] _defaultHeaders =
    [
        HeaderNames.ContentType,
        HeaderNames.Authorization,
        HeaderNames.Accept,
        HeaderNames.Origin,
        HeaderNames.XRequestedWith,
        HeaderNames.XPoweredBy,
        Http.RequestIdHeader,
    ];

    private static readonly string[] _defaultMethods =
    [
        HttpMethods.Get,
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Delete,
        HttpMethods.Patch,
        HttpMethods.Options,
    ];

    extension(IResourceBuilder<ProjectResource> builder)
    {
        /// <summary>
        ///     Applies CORS origin configuration to a backend service project, allowing cross-origin
        ///     requests from the specified Storefront and Backoffice URLs.
        /// </summary>
        /// <param name="storefront">The Aspire Storefront resource.</param>
        /// <param name="backoffice">The Aspire Backoffice resource.</param>
        /// <param name="scheme">The endpoint scheme used by the frontend deployment.</param>
        /// <returns>The original <see cref="IResourceBuilder{ProjectResource}" /> with CORS configuration applied.</returns>
        public IResourceBuilder<ProjectResource> WithCorsOrigins(
            IResourceBuilder<IResourceWithEndpoints> storefront,
            IResourceBuilder<IResourceWithEndpoints> backoffice,
            string scheme
        )
        {
            builder
                .WithEnvironment("Cors__Origins__0", storefront.GetEndpoint(scheme))
                .WithEnvironment("Cors__Origins__1", backoffice.GetEndpoint(scheme))
                .WithEnvironment("Cors__AllowCredentials", "true");

            for (var i = 0; i < _defaultHeaders.Length; i++)
            {
                builder.WithEnvironment($"Cors__Headers__{i}", _defaultHeaders[i]);
            }

            for (var i = 0; i < _defaultMethods.Length; i++)
            {
                builder.WithEnvironment($"Cors__Methods__{i}", _defaultMethods[i]);
            }

            return builder;
        }
    }
}
