using Aspire.Hosting.ApplicationModel;
using BookWorm.Hosting.Presidio;

namespace Aspire.Hosting;

public static class PresidioAnalyzerResourceBuilderExtensions
{
    /// <summary>
    ///     Adds a Presidio Analyzer container resource to the distributed application.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="httpPort">The optional host port for the HTTP endpoint. Defaults to container port 3000.</param>
    /// <returns>An <see cref="IResourceBuilder{PresidioAnalyzerResource}" /> for further configuration.</returns>
    public static IResourceBuilder<PresidioAnalyzerResource> AddPresidioAnalyzer(
        this IDistributedApplicationBuilder builder,
        string name,
        int? httpPort = null
    )
    {
        var resource = new PresidioAnalyzerResource(name);

        return builder
            .AddResource(resource)
            .WithImage(PresidioAnalyzerContainerImageTags.Image)
            .WithImageRegistry(PresidioAnalyzerContainerImageTags.Registry)
            .WithImageTag(PresidioAnalyzerContainerImageTags.Tag)
            .WithHttpEndpoint(
                targetPort: httpPort ?? 3000,
                name: PresidioAnalyzerResource.HttpEndpointName
            );
    }
}

public static class PresidioAnonymizerResourceBuilderExtensions
{
    /// <summary>
    ///     Adds a Presidio Anonymizer container resource to the distributed application.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="httpPort">The optional host port for the HTTP endpoint. Defaults to container port 3000.</param>
    /// <returns>An <see cref="IResourceBuilder{PresidioAnonymizerResource}" /> for further configuration.</returns>
    public static IResourceBuilder<PresidioAnonymizerResource> AddPresidioAnonymizer(
        this IDistributedApplicationBuilder builder,
        string name,
        int? httpPort = null
    )
    {
        var resource = new PresidioAnonymizerResource(name);

        return builder
            .AddResource(resource)
            .WithImage(PresidioAnonymizerContainerImageTags.Image)
            .WithImageRegistry(PresidioAnonymizerContainerImageTags.Registry)
            .WithImageTag(PresidioAnonymizerContainerImageTags.Tag)
            .WithHttpEndpoint(
                targetPort: httpPort ?? 3000,
                name: PresidioAnonymizerResource.HttpEndpointName
            );
    }
}
