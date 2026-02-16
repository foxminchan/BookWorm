using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using BookWorm.Common;
using BookWorm.Constants.Aspire;
using Projects;
using TUnit.Aspire;

namespace BookWorm.Catalog.IntegrationTests.Fixtures;

public sealed class CatalogAppFixture : AspireFixture<BookWorm_AppHost>
{
    protected override TimeSpan ResourceTimeout => TimeSpan.FromSeconds(120);

    protected override ResourceWaitBehavior WaitBehavior => ResourceWaitBehavior.Named;

    protected override IEnumerable<string> ResourcesToWaitFor()
    {
        yield return Services.Catalog;
    }

    protected override void ConfigureBuilder(IDistributedApplicationTestingBuilder builder)
    {
        builder
            .WithRandomParameterValues()
            .WithContainersLifetime(ContainerLifetime.Session)
            .WithRandomVolumeNames()
            .WithIncludeResources([
                Services.Catalog,
                Components.Postgres,
                Components.Database.Catalog,
                Components.Queue,
                Components.Redis,
                Components.VectorDb,
                Components.Azure.Storage.Resource,
                Components.Azure.Storage.BlobContainer(Services.Catalog),
            ]);
    }
}
