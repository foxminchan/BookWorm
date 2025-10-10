using Mediator;

namespace BookWorm.Catalog.Features.Books.Create;

public sealed class CreateBookPreProcessor(IBlobService blobService)
    : MessagePreProcessor<CreateBookCommand, Guid>
{
    protected override async ValueTask Handle(
        CreateBookCommand request,
        CancellationToken cancellationToken
    )
    {
        if (request.Image is not null)
        {
            var url = await blobService.UploadFileAsync(request.Image, cancellationToken);
            request.ImageName = url;
        }
    }
}
