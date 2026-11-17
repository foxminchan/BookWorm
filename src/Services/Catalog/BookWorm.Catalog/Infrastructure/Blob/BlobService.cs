using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Text.RegularExpressions;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using BookWorm.Catalog.Configurations;

namespace BookWorm.Catalog.Infrastructure.Blob;

internal sealed partial class BlobService(
    BlobContainerClient client,
    CatalogAppSettings appSettings
) : IBlobService
{
    private const int MaxFileNameLength = 100;

    private static readonly FrozenSet<string> _allowedContentTypes = FrozenSet.Create(
        StringComparer.OrdinalIgnoreCase,
        MediaTypeNames.Image.Jpeg,
        MediaTypeNames.Image.Png,
        MediaTypeNames.Image.Webp
    );

    private static readonly ConcurrentDictionary<string, UserDelegationKey> _delegationKeys = new(
        StringComparer.OrdinalIgnoreCase
    );

    public async Task<string> UploadFileAsync(
        IFormFile file,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(file);

        if (file.Length <= 0)
        {
            throw new ArgumentException("File must not be empty.", nameof(file));
        }

        if (
            string.IsNullOrWhiteSpace(file.ContentType)
            || !_allowedContentTypes.Contains(file.ContentType)
        )
        {
            throw new ArgumentException(
                $"Content type '{file.ContentType}' is not allowed.",
                nameof(file)
            );
        }

        var uniqueFileName = GenerateUniqueFileName(file.FileName);
        var blobClient = client.GetBlobClient(uniqueFileName);

        await using var stream = file.OpenReadStream();

        await blobClient.UploadAsync(
            stream,
            new BlobUploadOptions
            {
                HttpHeaders = new() { ContentType = file.ContentType },
                Conditions = new() { IfNoneMatch = ETag.All },
            },
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
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(appSettings.SasExpiryHours);

        var blobName = GetBlobNameFromUrn(urn);
        var blobClient = client.GetBlobClient(blobName);

        var now = DateTimeOffset.UtcNow;
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = client.Name,
            BlobName = blobName,
            Resource = "b",
            StartsOn = now.AddMinutes(-5),
            ExpiresOn = now.AddHours(appSettings.SasExpiryHours),
            Protocol = client.Uri.Scheme.Equals(
                Uri.UriSchemeHttps,
                StringComparison.OrdinalIgnoreCase
            )
                ? SasProtocol.Https
                : SasProtocol.HttpsAndHttp,
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        if (blobClient.CanGenerateSasUri)
        {
            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }

        var userDelegationKey = GetOrRefreshUserDelegationKey(now);
        var uriBuilder = new BlobUriBuilder(blobClient.Uri)
        {
            Sas = sasBuilder.ToSasQueryParameters(userDelegationKey, client.AccountName),
        };

        return uriBuilder.ToUri().ToString();
    }

    private UserDelegationKey GetOrRefreshUserDelegationKey(DateTimeOffset now)
    {
        if (
            _delegationKeys.TryGetValue(client.AccountName, out var cachedKey)
            && cachedKey.SignedExpiresOn > now.AddHours(1)
        )
        {
            return cachedKey;
        }

        var serviceClient = client.GetParentBlobServiceClient();
        var options = new BlobGetUserDelegationKeyOptions(now.AddDays(1))
        {
            StartsOn = now.AddMinutes(-5),
        };
        var key = serviceClient.GetUserDelegationKey(options).Value;
        _delegationKeys[client.AccountName] = key;
        return key;
    }

    private string GetBlobNameFromUrn(string urn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(urn);

        if (urn.Split(':') is not [var prefix, var account, var container, var blobName])
        {
            throw new UrnException(
                "URN must follow the format: urn:{account}:{container}:{blobName}"
            );
        }

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
            var t
                when string.IsNullOrWhiteSpace(t.blobName)
                    || t.blobName.Contains('/')
                    || t.blobName.Contains('\\')
                    || t.blobName.Contains("..", StringComparison.Ordinal) =>
                throw new UrnException("URN must contain a valid blob name."),
            _ => true,
        };

        return blobName;
    }

    private static string GenerateUniqueFileName(string originalFileName)
    {
        var name = Path.GetFileNameWithoutExtension(originalFileName);
        var ext = Path.GetExtension(originalFileName);
        var safeName = InvalidFileNameChars().Replace(name, string.Empty);
        var safeExt = InvalidFileNameChars().Replace(ext, string.Empty);

        if (safeName.Length > MaxFileNameLength)
        {
            safeName = safeName[..MaxFileNameLength];
        }

        return string.IsNullOrEmpty(safeName)
            ? $"{Guid.CreateVersion7()}{safeExt}"
            : $"{Guid.CreateVersion7()}-{safeName}{safeExt}";
    }

    [GeneratedRegex(@"[^a-zA-Z0-9\.\-_]")]
    private static partial Regex InvalidFileNameChars();
}
