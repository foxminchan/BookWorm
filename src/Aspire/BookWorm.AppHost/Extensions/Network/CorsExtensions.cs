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

    /// <summary>
    ///     Configures CORS origin parameters for the application to allow cross-origin requests
    ///     from the Storefront and Backoffice frontend applications in production.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <returns>
    ///     A tuple containing the Storefront URL and Backoffice URL parameter resource builders.
    /// </returns>
    /// <remarks>
    ///     This method creates two deployment parameters (<c>storefront-url</c> and <c>backoffice-url</c>)
    ///     with custom input prompts for the Aspire deploy experience. These parameters are consumed
    ///     by <see cref="WithCorsOrigins" /> to configure the <c>Cors</c> configuration section on each
    ///     backend service.
    /// </remarks>
    public static (
        IResourceBuilder<ParameterResource> StorefrontUrl,
        IResourceBuilder<ParameterResource> BackofficeUrl
    ) AddCorsOriginParameters(this IDistributedApplicationBuilder builder)
    {
        var storefrontUrl = builder
            .AddParameter("storefront-url")
            .WithDescription(ParameterDescriptions.Cors.StorefrontUrl, true)
            .WithCustomInput(_ =>
                new()
                {
                    Name = "StorefrontUrlParameter",
                    Label = "Storefront URL",
                    InputType = InputType.Text,
                    Description =
                        "Enter the Storefront application URL for CORS (e.g., https://bookworm.com)",
                }
            );

        var backofficeUrl = builder
            .AddParameter("backoffice-url")
            .WithDescription(ParameterDescriptions.Cors.BackofficeUrl, true)
            .WithCustomInput(_ =>
                new()
                {
                    Name = "BackofficeUrlParameter",
                    Label = "Backoffice URL",
                    InputType = InputType.Text,
                    Description =
                        "Enter the Backoffice application URL for CORS (e.g., https://admin.bookworm.com)",
                }
            );

        return (storefrontUrl, backofficeUrl);
    }

    /// <summary>
    ///     Applies CORS origin configuration to a backend service project, allowing cross-origin
    ///     requests from the specified Storefront and Backoffice URLs.
    /// </summary>
    /// <param name="builder">The resource builder for the project resource to configure.</param>
    /// <param name="storefrontUrl">The Storefront URL parameter resource builder.</param>
    /// <param name="backofficeUrl">The Backoffice URL parameter resource builder.</param>
    /// <returns>The original <see cref="IResourceBuilder{ProjectResource}" /> with CORS configuration applied.</returns>
    /// <remarks>
    ///     Sets environment variables that map to the <c>Cors</c> configuration section consumed by
    ///     <c>CorsSettings</c> in each service. This includes allowed origins, headers, methods,
    ///     and credential support for the production CORS policy.
    /// </remarks>
    public static IResourceBuilder<ProjectResource> WithCorsOrigins(
        this IResourceBuilder<ProjectResource> builder,
        IResourceBuilder<ParameterResource> storefrontUrl,
        IResourceBuilder<ParameterResource> backofficeUrl
    )
    {
        builder
            .WithEnvironment("Cors__Origins__0", storefrontUrl)
            .WithEnvironment("Cors__Origins__1", backofficeUrl)
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
