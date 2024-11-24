using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Catalog.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Catalog.Features.Books.Create;

public sealed class CreateBookEndpoint : IEndpoint<Created<Guid>, CreateBookCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/books",
                async ([FromForm] CreateBookCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .AddEndpointFilter<FileValidationFilter>()
            .Produces<Created<Guid>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .DisableAntiforgery()
            .WithOpenApi()
            .WithTags(nameof(Book))
            .WithFormOptions(bufferBody: true)
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Created<Guid>> HandleAsync(
        CreateBookCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Created(
            new UrlBuilder().WithVersion().WithResource(nameof(Books)).WithId(result.Value).Build(),
            result.Value
        );
    }
}
