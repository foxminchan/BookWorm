using MediatR.Pipeline;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Catalog.Features.Books.Update;

public sealed record UpdateBookCommand(
    [property: JsonIgnore] Guid Id,
    [FromForm] string Name,
    [FromForm] string Description,
    IFormFile? Image,
    [FromForm] decimal Price,
    [FromForm] decimal? PriceSale,
    [FromForm] Guid CategoryId,
    [FromForm] Guid PublisherId,
    [FromForm] Guid[] AuthorIds,
    [FromForm] bool IsRemoveImage = false
) : ICommand
{
    [JsonIgnore]
    public string? ImageName { get; set; }
}

public sealed class PreUpdateBookHandler(IBlobService blobService)
    : IRequestPreProcessor<UpdateBookCommand>
{
    public async Task Process(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        if (request.Image is not null)
        {
            var imageUrl = await blobService.UploadFileAsync(request.Image, cancellationToken);
            request.ImageName = imageUrl;
        }
    }
}

public sealed class UpdateBookHandler(IBookRepository repository)
    : ICommandHandler<UpdateBookCommand>
{
    public async Task<Unit> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        var book = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(book, $"Book with id {request.Id} not found.");

        var imageName = request.IsRemoveImage ? null : request.ImageName ?? book.Image;

        book.Update(
            request.Name,
            request.Description,
            request.Price,
            request.PriceSale,
            imageName,
            request.CategoryId,
            request.PublisherId,
            request.AuthorIds
        );

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}

public sealed class PostUpdateBookHandler(IBlobService blobService)
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
