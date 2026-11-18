using Aspire.Hosting.Azure.AppContainers;
using Azure.Provisioning.AppContainers;
using Azure.Provisioning.OperationalInsights;
using Microsoft.Extensions.Hosting;

namespace BookWorm.AppHost.Extensions.Infrastructure;

internal static partial class AzureExtensions
{
    extension(IResourceBuilder<AzureContainerAppEnvironmentResource> builder)
    {
        public void ProvisionAsService(IHostEnvironment environment)
        {
            builder
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
    }
}
