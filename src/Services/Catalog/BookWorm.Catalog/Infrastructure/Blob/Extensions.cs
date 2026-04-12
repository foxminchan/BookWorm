using BookWorm.Constants.Aspire;

namespace BookWorm.Catalog.Infrastructure.Blob;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddAzureBlobStorage()
        {
            var services = builder.Services;
            builder.AddAzureBlobContainerClient(
                Components.Azure.Storage.BlobContainer(Services.Catalog)
            );
            services.AddScoped<IBlobService, BlobService>();
        }
    }
}
