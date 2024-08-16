using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Basket.Features.Create;

public sealed record CreateBasketRequest(Guid BookId, int Quantity);

public sealed class CreateBasketEndpoint : IEndpoint<Created<Guid>, CreateBasketRequest, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/baskets",
                async (CreateBasketRequest request, ISender sender) => await HandleAsync(request, sender))
            .Produces<Created<Guid>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithTags(nameof(Basket))
            .WithName("Create Basket")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Created<Guid>> HandleAsync(CreateBasketRequest request, ISender sender,
        CancellationToken cancellationToken = default)
    {
        CreateBasketCommand command = new(request.BookId, request.Quantity);

        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Created($"/api/v1/baskets/{result.Value}", result.Value);
    }
}
