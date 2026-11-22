using Aspire.Hosting.Yarp;

namespace BookWorm.AppHost.Extensions.Frontend;

internal static class FrontendExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        public (
            IResourceBuilder<IResourceWithEndpoints> Storefront,
            IResourceBuilder<IResourceWithEndpoints> Commerce,
            IResourceBuilder<IResourceWithEndpoints> Backoffice
        ) AddFrontendApps(
            IResourceBuilder<YarpResource> gateway,
            IResourceBuilder<EntraIdApplicationResource> entraId
        )
        {
            var workspace = builder
                .AddFrontendWorkspace(
                    Components.TurboRepo,
                    Path.GetFullPath("../../Clients", builder.AppHostDirectory)
                )
                .WithGateway(gateway)
                .WithEntraId(entraId);

            var storefront = workspace
                .AddApp(Clients.StoreFront, Clients.StoreFrontTurboApp)
                .WithCopilot()
                .Build();

            var commerce = workspace
                .AddApp(Clients.Commerce, Clients.CommerceTurboApp)
                .WithHealthCheckPath("/api/health")
                .WithoutEntraId()
                .Build();

            var backoffice = workspace
                .AddApp(Clients.BackOffice, Clients.BackOfficeTurboApp)
                .WithPathPrefix("/backoffice")
                .WithHealthCheckPath("/backoffice")
                .Build();

            return (storefront, commerce, backoffice);
        }

        private FrontendWorkspaceBuilder AddFrontendWorkspace(string name, string workingDirectory)
        {
            return new(builder, name, workingDirectory);
        }
    }
}
