﻿using MediatR.Pipeline;

namespace BookWorm.Catalog.Features.Books.Update;

public sealed class UpdateBookPostProcessor(IBlobService blobService)
    : IRequestPostProcessor<UpdateBookCommand, Unit>
{
    public async Task Process(
        UpdateBookCommand request,
        Unit response,
        CancellationToken cancellationToken
    )
    {
        if (request is { IsRemoveImage: true, ImageName: not null })
        {
            await blobService.DeleteFileAsync(request.ImageName, cancellationToken);
        }
    }
}
