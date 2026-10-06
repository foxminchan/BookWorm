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
        /// <param name="httpPort">The optional host port for the HTTP endpoint. Assigned dynamically when omitted.</param>
        /// <returns>An <see cref="IResourceBuilder{PresidioAnalyzerResource}" /> for further configuration.</returns>
        [AspireExport]
        public IResourceBuilder<PresidioAnalyzerResource> AddPresidioAnalyzer(
            [ResourceName] string name,
            int? httpPort = null
        )
        {
            var resource = new PresidioAnalyzerResource(name);

            return builder
                .AddResource(resource)
                .WithImage(PresidioAnalyzerContainerImageTags.Image)
                .WithImageRegistry(PresidioAnalyzerContainerImageTags.Registry)
                .WithImageTag(PresidioAnalyzerContainerImageTags.Tag)
                .WithIconName("SearchShield")
                .WithHttpEndpoint(
                    port: httpPort,
                    targetPort: 3000,
                    name: PresidioAnalyzerResource.HttpEndpointName
                )
                .WithHttpHealthCheck(
                    "/health",
                    endpointName: PresidioAnalyzerResource.HttpEndpointName
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
        /// <param name="httpPort">The optional host port for the HTTP endpoint. Assigned dynamically when omitted.</param>
        /// <returns>An <see cref="IResourceBuilder{PresidioAnonymizerResource}" /> for further configuration.</returns>
        [AspireExport]
        public IResourceBuilder<PresidioAnonymizerResource> AddPresidioAnonymizer(
            [ResourceName] string name,
            int? httpPort = null
        )
        {
            var resource = new PresidioAnonymizerResource(name);

            return builder
                .AddResource(resource)
                .WithImage(PresidioAnonymizerContainerImageTags.Image)
                .WithImageRegistry(PresidioAnonymizerContainerImageTags.Registry)
                .WithImageTag(PresidioAnonymizerContainerImageTags.Tag)
                .WithIconName("ShieldCheckmark")
                .WithHttpEndpoint(
                    port: httpPort,
                    targetPort: 3000,
                    name: PresidioAnonymizerResource.HttpEndpointName
                )
                .WithHttpHealthCheck(
                    "/health",
                    endpointName: PresidioAnonymizerResource.HttpEndpointName
                );
        }
    }
}
