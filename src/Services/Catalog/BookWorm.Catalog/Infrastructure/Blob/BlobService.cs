using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BookWorm.Catalog.Infrastructure.Blob;

public sealed class BlobService(BlobServiceClient client) : IBlobService
{
    private readonly string _container = nameof(Catalog).ToLowerInvariant();

    public async Task<string> UploadFileAsync(
        IFormFile file,
        CancellationToken cancellationToken = default
    )
    {
        var blobContainerClient = client.GetBlobContainerClient(_container);

        await blobContainerClient.CreateIfNotExistsAsync(
            PublicAccessType.BlobContainer,
            cancellationToken: cancellationToken
        );

        var uniqueFileName = $"{Guid.CreateVersion7()}-{file.FileName}";

        var blobClient = blobContainerClient.GetBlobClient(uniqueFileName);

        await blobClient.UploadAsync(file.OpenReadStream(), cancellationToken);

        return uniqueFileName;
    }

    public async Task DeleteFileAsync(
        string fileName,
        CancellationToken cancellationToken = default
    )
    {
        var blobContainerClient = client.GetBlobContainerClient(_container);

        var blobClient = blobContainerClient.GetBlobClient(fileName);

        await blobClient.DeleteIfExistsAsync(
            DeleteSnapshotsOption.IncludeSnapshots,
            cancellationToken: cancellationToken
        );
    }

    public string GetFileUrl(string fileName)
    {
        var blobContainerClient = client.GetBlobContainerClient(_container);

        var blobClient = blobContainerClient.GetBlobClient(fileName);

        return blobClient.Uri.AbsoluteUri;
    }
}
