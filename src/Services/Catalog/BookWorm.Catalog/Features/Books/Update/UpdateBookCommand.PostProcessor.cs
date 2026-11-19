namespace BookWorm.Catalog.Features.Books.Update;

internal sealed class UpdateBookPostProcessor(IBlobService blobService)
    : MessagePostProcessor<UpdateBookCommand, Unit>
{
    protected override ValueTask Handle(
        UpdateBookCommand request,
        Unit response,
        CancellationToken cancellationToken
    )
    {
        return request is { IsRemoveImage: true, ImageUrn: not null }
            ? new(blobService.DeleteFileAsync(request.ImageUrn, cancellationToken))
            : ValueTask.CompletedTask;
    }
}
