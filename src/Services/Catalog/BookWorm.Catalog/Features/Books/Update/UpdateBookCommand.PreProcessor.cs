using Mediator;

namespace BookWorm.Catalog.Features.Books.Update;

public sealed class UpdateBookPreProcessor(IBlobService blobService)
    : MessagePreProcessor<UpdateBookCommand, Unit>
{
    protected override async ValueTask Handle(
        UpdateBookCommand request,
        CancellationToken cancellationToken
    )
    {
        if (request.Image is not null)
        {
            var urn = await blobService.UploadFileAsync(request.Image, cancellationToken);
            request.ImageUrn = urn;
        }
    }
}
