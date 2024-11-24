namespace BookWorm.Catalog.Infrastructure.Blob;

public sealed class AzuriteService(
    ResiliencePipelineProvider<string> pipeline,
    BlobServiceClient client
) : IAzuriteService
{
    private readonly ResiliencePipeline _policy = pipeline.GetPipeline(nameof(Blob));

    public async Task<string> UploadFileAsync(
        IFormFile file,
        CancellationToken cancellationToken = default
    )
    {
        var blobContainerClient = client.GetBlobContainerClient(nameof(Catalog));

        await blobContainerClient.CreateIfNotExistsAsync(
            PublicAccessType.BlobContainer,
            cancellationToken: cancellationToken
        );

        var blobName = Guid.CreateVersion7().ToString();

        var blobClient = blobContainerClient.GetBlobClient(blobName);

        await _policy.ExecuteAsync(
            async token =>
                await blobClient.UploadAsync(
                    file.OpenReadStream(),
                    new BlobHttpHeaders { ContentType = file.ContentType },
                    cancellationToken: token
                ),
            cancellationToken
        );

        return blobClient.Uri.ToString();
    }

    public async Task DeleteFileAsync(string url, CancellationToken cancellationToken = default)
    {
        var blobClient = new BlobClient(new(url));

        await _policy.ExecuteAsync(
            async token =>
                await blobClient.DeleteIfExistsAsync(
                    DeleteSnapshotsOption.IncludeSnapshots,
                    cancellationToken: token
                ),
            cancellationToken
        );
    }
}
