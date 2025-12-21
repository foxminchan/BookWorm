namespace BookWorm.AppHost.Extensions.Infrastructure;

public static class ContainerExtensions
{
    /// <summary>
    ///     Adds a container registry resource to the distributed application builder.
    /// </summary>
    /// <param name="builder">The distributed application builder to configure.</param>
    /// <returns>
    ///     An <see cref="IResourceBuilder{ContainerRegistryResource}" /> instance representing the configured container
    ///     registry resource.
    /// </returns>
    public static IResourceBuilder<ContainerRegistryResource> AddContainerRegistry(
        this IDistributedApplicationBuilder builder
    )
    {
        var endpoint = builder
            .AddParameter("container-endpoint")
            .WithDescription(ParameterDescriptions.ContainerRegistry.Endpoint, true)
            .WithCustomInput(_ =>
                new()
                {
                    Name = "ContainerEndpointParameter",
                    Label = "Container Endpoint",
                    InputType = InputType.Text,
                    Description = "Enter your container registry endpoint here",
                }
            );

        var repository = builder
            .AddParameter("container-repository")
            .WithDescription(ParameterDescriptions.ContainerRegistry.Repository, true)
            .WithCustomInput(_ =>
                new()
                {
                    Name = "ContainerRepositoryParameter",
                    Label = "Container Repository",
                    InputType = InputType.Text,
                    Description = "Enter your container registry repository here",
                }
            );

        var registry = builder.AddContainerRegistry(
            Components.ContainerRegistry,
            endpoint,
            repository
        );

        return registry;
    }
}
