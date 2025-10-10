using Mediator;

namespace BookWorm.Catalog.Features.Books.Update;

public sealed class UpdateBookPostProcessor(IBlobService blobService)
    : MessagePostProcessor<UpdateBookCommand, Unit>
{
    protected override async ValueTask Handle(
        UpdateBookCommand request,
        Unit response,
        CancellationToken cancellationToken
    )
    {
        if (request is { IsRemoveImage: true, ImageUrn: not null })
        {
            await blobService.DeleteFileAsync(request.ImageUrn, cancellationToken);
        }
    }
}
