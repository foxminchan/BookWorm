using Aspire.Hosting.Azure.AppContainers;
using Azure.Provisioning.AppContainers;
using Azure.Provisioning.OperationalInsights;
using Microsoft.Extensions.Hosting;

namespace BookWorm.AppHost.Extensions.Infrastructure;

internal static partial class AzureExtensions
{
    extension(IResourceBuilder<AzureContainerAppEnvironmentResource> builder)
    {
        public IResourceBuilder<AzureContainerAppEnvironmentResource> ProvisionAsService(
            IHostEnvironment environment
        )
        {
            return builder
                .WithDashboard()
                .WithAzdResourceNaming()
                .ConfigureInfrastructure(infra =>
                {
                    var resource = infra
                        .GetProvisionableResources()
                        .OfType<ContainerAppManagedEnvironment>()
                        .FirstOrDefault();

                    if (
                        resource is not null
                        && !resource.WorkloadProfiles.Any(wp =>
                            string.Equals(
                                wp.Value?.Name.Value,
                                "Consumption",
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                    )
                    {
                        resource.WorkloadProfiles.Add(
                            new ContainerAppWorkloadProfile
                            {
                                Name = "Consumption",
                                WorkloadProfileType = "Consumption",
                            }
                        );
                    }

                    var workspace = infra
                        .GetProvisionableResources()
                        .OfType<OperationalInsightsWorkspace>()
                        .FirstOrDefault();

                    workspace?.RetentionInDays = environment.IsProduction() ? 90 : 30;
                });
        }

        public void AssignComputeResources()
        {
            foreach (
                var resource in builder.ApplicationBuilder.Resources.OfType<IComputeResource>()
            )
            {
                builder
                    .ApplicationBuilder.CreateResourceBuilder(resource)
                    .WithComputeEnvironment(builder);
            }
        }
    }
}
