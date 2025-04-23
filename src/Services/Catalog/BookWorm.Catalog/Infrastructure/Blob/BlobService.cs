using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace BookWorm.Catalog.Infrastructure.Blob;

public sealed class BlobService(BlobServiceClient client) : IBlobService
{
    private const int SasExpiryHours = 1;
    private readonly string _container = nameof(Catalog).ToLower();

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

    public async Task<string> GetFileUrl(
        string fileName,
        CancellationToken cancellationToken = default
    )
    {
        var blobContainerClient = client.GetBlobContainerClient(_container);

        var blobClient = blobContainerClient.GetBlobClient(fileName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _container,
            BlobName = fileName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(SasExpiryHours),
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var userDelegationKey = await client.GetUserDelegationKeyAsync(
            DateTimeOffset.UtcNow.AddMinutes(-5),
            DateTimeOffset.UtcNow.AddHours(SasExpiryHours),
            cancellationToken
        );

        var sasToken = sasBuilder
            .ToSasQueryParameters(userDelegationKey, client.AccountName)
            .ToString();

        return $"{blobClient.Uri}?{sasToken}";
    }
}
