using Aspire.Hosting.Azure.AppContainers;
using Azure.Core;
using Azure.Provisioning.AppContainers;

namespace BookWorm.AppHost.Extensions.Infrastructure;

internal static partial class AzureExtensions
{
    extension(IResourceBuilder<AzureContainerAppEnvironmentResource> builder)
    {
        public void ProvisionAsService()
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

                    if (resource is null)
                    {
                        return;
                    }

                    resource.WorkloadProfiles.Add(
                        new ContainerAppWorkloadProfile
                        {
                            Name = "Consumption",
                            WorkloadProfileType = "Consumption",
                        }
                    );

                    resource.Location = AzureLocation.SoutheastAsia;
                });
        }
    }
}
