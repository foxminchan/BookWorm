using BookWorm.Catalog.Features.Books.Get;
using BookWorm.Constants.Core;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Catalog.Features.Books.Create;

public sealed class CreateBookEndpoint
    : IEndpoint<Created<Guid>, CreateBookCommand, ISender, LinkGenerator>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/books",
                async (
                    [FromForm] CreateBookCommand command,
                    ISender sender,
                    LinkGenerator linker
                ) => await HandleAsync(command, sender, linker)
            )
            .Accepts<CreateBookCommand>(MediaTypeNames.Multipart.FormData)
            .ProducesPost<Guid>()
            .WithTags(nameof(Book))
            .WithName(nameof(CreateBookEndpoint))
            .WithSummary("Create Book")
            .WithDescription("Create a book")
            .WithFormOptions(true)
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin, Authorization.Policies.Vendor)
            .RequirePerUserRateLimit();
    }

    public async Task<Created<Guid>> HandleAsync(
        CreateBookCommand command,
        ISender sender,
        LinkGenerator linker,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        var path = linker.GetPathByName(nameof(GetBookEndpoint), new { id = result });

        return TypedResults.Created(path, result);
    }
}
