using MediatR.Pipeline;

namespace BookWorm.Catalog.Features.Books.Create;

public sealed class CreateBookPreProcessor(IBlobService blobService)
    : IRequestPreProcessor<CreateBookCommand>
{
    public async Task Process(CreateBookCommand request, CancellationToken cancellationToken)
    {
        if (request.Image is not null)
        {
            var imageUrl = await blobService.UploadFileAsync(request.Image, cancellationToken);
            request.ImageName = imageUrl;
        }
    }
}
