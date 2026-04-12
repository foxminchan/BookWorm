namespace BookWorm.Catalog.Infrastructure.Blob;

internal interface IBlobService
{
    Task<string> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string urn, CancellationToken cancellationToken = default);
    string GetFileSasUrl(string urn);
}
