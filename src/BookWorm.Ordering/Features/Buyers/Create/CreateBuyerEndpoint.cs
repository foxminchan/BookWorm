using BookWorm.Ordering.Domain.BuyerAggregate;
using BookWorm.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Ordering.Features.Buyers.Create;

public sealed record CreateBuyerRequest(string? Street, string? City, string? Province);

public sealed class CreateBuyerEndpoint : IEndpoint<Created<Guid>, CreateBuyerRequest, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/buyers",
                async (CreateBuyerRequest request, ISender sender) => await HandleAsync(request, sender))
            .Produces<Created<Guid>>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithTags(nameof(Buyer))
            .WithName("Create Buyer")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Created<Guid>> HandleAsync(CreateBuyerRequest request, ISender sender,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new CreateBuyerCommand(request.Street, request.City, request.Province),
            cancellationToken);

        return TypedResults.Created($"/api/v1/buyer/{result.Value}", result.Value);
    }
}
