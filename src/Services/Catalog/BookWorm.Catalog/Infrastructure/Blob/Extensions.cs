using BookWorm.Constants.Aspire;

namespace BookWorm.Catalog.Infrastructure.Blob;

internal static class Extensions
{
    public static void AddAzureBlobStorage(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        builder.AddAzureBlobContainerClient(Components.Azure.Storage.BlobContainer);
        services.AddScoped<IBlobService, BlobService>();
    }
}
