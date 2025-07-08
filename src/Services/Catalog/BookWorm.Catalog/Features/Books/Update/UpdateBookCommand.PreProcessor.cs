using MediatR.Pipeline;

namespace BookWorm.Catalog.Features.Books.Update;

public sealed class UpdateBookPreProcessor(IBlobService blobService)
    : IRequestPreProcessor<UpdateBookCommand>
{
    public async Task Process(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        if (request.Image is not null)
        {
            var urn = await blobService.UploadFileAsync(request.Image, cancellationToken);
            request.ImageUrn = urn;
        }
    }
}
