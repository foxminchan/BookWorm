using Aspire.Hosting.ApplicationModel;
using BookWorm.Hosting.Presidio;

namespace Aspire.Hosting;

public static class PresidioAnalyzerResourceBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        /// <summary>
        ///     Adds a Presidio Analyzer container resource to the distributed application.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <param name="httpsPort">The optional host port for the HTTPS endpoint. Assigned dynamically when omitted.</param>
        /// <returns>An <see cref="IResourceBuilder{PresidioAnalyzerResource}" /> for further configuration.</returns>
        [AspireExport]
        public IResourceBuilder<PresidioAnalyzerResource> AddPresidioAnalyzer(
            [ResourceName] string name,
            int? httpsPort = null
        )
        {
            var resource = new PresidioAnalyzerResource(name);

            return builder
                .AddResource(resource)
                .ConfigurePresidioContainer(
                    PresidioAnalyzerContainerImageTags.Registry,
                    PresidioAnalyzerContainerImageTags.Image,
                    PresidioAnalyzerContainerImageTags.Tag,
                    "SearchShield",
                    httpsPort,
                    PresidioResource.HttpsEndpointName
                );
        }
    }
}

public static class PresidioAnonymizerResourceBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        /// <summary>
        ///     Adds a Presidio Anonymizer container resource to the distributed application.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <param name="httpsPort">The optional host port for the HTTPS endpoint. Assigned dynamically when omitted.</param>
        /// <returns>An <see cref="IResourceBuilder{PresidioAnonymizerResource}" /> for further configuration.</returns>
        [AspireExport]
        public IResourceBuilder<PresidioAnonymizerResource> AddPresidioAnonymizer(
            [ResourceName] string name,
            int? httpsPort = null
        )
        {
            var resource = new PresidioAnonymizerResource(name);

            return builder
                .AddResource(resource)
                .ConfigurePresidioContainer(
                    PresidioAnonymizerContainerImageTags.Registry,
                    PresidioAnonymizerContainerImageTags.Image,
                    PresidioAnonymizerContainerImageTags.Tag,
                    "ShieldCheckmark",
                    httpsPort,
                    PresidioResource.HttpsEndpointName
                );
        }
    }
}

internal static class PresidioContainerBuilderExtensions
{
    internal static IResourceBuilder<TResource> ConfigurePresidioContainer<TResource>(
        this IResourceBuilder<TResource> builder,
        string registry,
        string image,
        string tag,
        string iconName,
        int? port,
        string httpsEndpointName
    )
        where TResource : ContainerResource, IResourceWithConnectionString
    {
        builder
            .WithImage(image)
            .WithImageRegistry(registry)
            .WithImageTag(tag)
            .WithIconName(iconName);

        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            return builder
                .WithHttpsEndpoint(port, 3000, httpsEndpointName)
                .WithHttpsDeveloperCertificate()
                .WithHttpsCertificateConfiguration(ctx =>
                {
                    ctx.EnvironmentVariables["GUNICORN_CMD_ARGS"] = ReferenceExpression.Create(
                        $"--certfile={ctx.CertificatePath} --keyfile={ctx.KeyPath}"
                    );
                    return Task.CompletedTask;
                })
                .WithHttpHealthCheck("/health", endpointName: httpsEndpointName);
        }

        const string httpEndpointName = "http";

        return builder
            .WithHttpEndpoint(port, 3000, httpEndpointName)
            .WithHttpHealthCheck("/health", endpointName: httpEndpointName);
    }
}
