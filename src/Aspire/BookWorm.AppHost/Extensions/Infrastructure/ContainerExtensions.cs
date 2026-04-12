namespace BookWorm.AppHost.Extensions.Infrastructure;

internal static class ContainerExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        public IResourceBuilder<ContainerRegistryResource> AddContainerRegistry()
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
}
