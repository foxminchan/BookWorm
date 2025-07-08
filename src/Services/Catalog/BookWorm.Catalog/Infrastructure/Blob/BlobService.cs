using System.Text.RegularExpressions;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace BookWorm.Catalog.Infrastructure.Blob;

public sealed partial class BlobService(BlobContainerClient client, AppSettings appSettings)
    : IBlobService
{
    public async Task<string> UploadFileAsync(
        IFormFile file,
        CancellationToken cancellationToken = default
    )
    {
        var uniqueFileName = GenerateUniqueFileName(file.FileName);

        var blobClient = client.GetBlobClient(uniqueFileName);

        await blobClient.UploadAsync(
            file.OpenReadStream(),
            new BlobUploadOptions { HttpHeaders = new() { ContentType = file.ContentType } },
            cancellationToken
        );

        return $"urn:{client.AccountName}:{client.Name}:{uniqueFileName}";
    }

    public async Task DeleteFileAsync(string urn, CancellationToken cancellationToken = default)
    {
        var blobName = GetBlobNameFromUrn(urn);
        var blobClient = client.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync(
            DeleteSnapshotsOption.IncludeSnapshots,
            cancellationToken: cancellationToken
        );
    }

    public string GetFileSasUrl(string urn)
    {
        var blobName = GetBlobNameFromUrn(urn);
        var blobClient = client.GetBlobClient(blobName);

        var url = blobClient.GenerateSasUri(
            BlobSasPermissions.Read,
            DateTimeOffset.UtcNow.AddHours(appSettings.SasExpiryHours)
        );

        return url.ToString();
    }

    private string GetBlobNameFromUrn(string urn)
    {
        if (string.IsNullOrWhiteSpace(urn))
        {
            throw new ArgumentException("URN cannot be null or empty.", nameof(urn));
        }

        var parts = urn.Split(':').AsSpan();
        if (parts.Length != 4)
        {
            throw new ArgumentException(
                "URN must follow the format: urn:{account}:{container}:{blobName}",
                nameof(urn)
            );
        }

        var (prefix, account, container, blobName) = (parts[0], parts[1], parts[2], parts[3]);

        _ = (prefix, account, container, blobName) switch
        {
            var t when !string.Equals(t.prefix, "urn", StringComparison.OrdinalIgnoreCase) =>
                throw new UrnException("URN must start with 'urn:'"),
            var t
                when !string.Equals(
                    t.account,
                    client.AccountName,
                    StringComparison.OrdinalIgnoreCase
                ) => throw new UrnException($"URN account name must match '{client.AccountName}'"),
            var t
                when !string.Equals(t.container, client.Name, StringComparison.OrdinalIgnoreCase) =>
                throw new UrnException($"URN container name must match '{client.Name}'"),
            var t when string.IsNullOrWhiteSpace(t.blobName) => throw new UrnException(
                "URN must contain a valid blob name."
            ),
            _ => true,
        };

        return blobName;
    }

    private static string GenerateUniqueFileName(string originalFileName)
    {
        var name = Path.GetFileNameWithoutExtension(originalFileName);
        var ext = Path.GetExtension(originalFileName);
        var safeName = InvalidFileNameChars().Replace(name, "");
        return $"{Guid.CreateVersion7()}-{safeName}{ext}";
    }

    [GeneratedRegex(@"[^a-zA-Z0-9\.\-_]")]
    private static partial Regex InvalidFileNameChars();
}
