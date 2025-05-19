using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace BookWorm.Catalog.Infrastructure.Blob;

public sealed class BlobService(BlobContainerClient client) : IBlobService
{
    private const int SasExpiryHours = 1;

    public async Task<string> UploadFileAsync(
        IFormFile file,
        CancellationToken cancellationToken = default
    )
    {
        var uniqueFileName = $"{Guid.CreateVersion7()}-{file.FileName}";

        var blobClient = client.GetBlobClient(uniqueFileName);

        await blobClient.UploadAsync(file.OpenReadStream(), cancellationToken);

        return uniqueFileName;
    }

    public async Task DeleteFileAsync(
        string fileName,
        CancellationToken cancellationToken = default
    )
    {
        var blobClient = client.GetBlobClient(fileName);

        await blobClient.DeleteIfExistsAsync(
            DeleteSnapshotsOption.IncludeSnapshots,
            cancellationToken: cancellationToken
        );
    }

    public string GetFileUrl(string fileName, CancellationToken cancellationToken = default)
    {
        var blobClient = client.GetBlobClient(fileName);

        var url = blobClient.GenerateSasUri(
            BlobSasPermissions.Read,
            DateTimeOffset.UtcNow.AddHours(SasExpiryHours)
        );

        return url.ToString();
    }
}
