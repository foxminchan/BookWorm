﻿namespace BookWorm.Catalog.Infrastructure.Blob;

public interface IBlobService
{
    Task<string> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string fileName, CancellationToken cancellationToken = default);
    string GetFileUrl(string fileName);
}
