namespace BookWorm.Catalog.Infrastructure.Blob;

public sealed class AzuriteService(ResiliencePipelineProvider<string> pipeline, IConfiguration configuration)
    : IAzuriteService
{
    private readonly BlobContainerClient _container = new(configuration.GetConnectionString(ServiceName.Blob),
        nameof(Catalog));

    private readonly ResiliencePipeline _policy = pipeline.GetPipeline(nameof(Blob));

    public async Task<string> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        await _container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobName = Guid.NewGuid().ToString();

        var blobClient = _container.GetBlobClient(blobName);

        await _policy.ExecuteAsync(
            async token => await blobClient.UploadAsync(
                file.OpenReadStream(),
                new BlobHttpHeaders { ContentType = file.ContentType },
                cancellationToken: token),
            cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteFileAsync(string url, CancellationToken cancellationToken = default)
    {
        var blobClient = new BlobClient(new(url));

        await _policy.ExecuteAsync(
            async token =>
                await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: token),
            cancellationToken);
    }
}
