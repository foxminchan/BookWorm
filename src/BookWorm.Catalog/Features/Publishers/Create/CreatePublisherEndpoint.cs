using BookWorm.Catalog.Domain;

namespace BookWorm.Catalog.Features.Publishers.Create;

public sealed record CreatePublisherRequest(string Name);

public sealed class CreatePublisherEndpoint : IEndpoint<Created<Guid>, CreatePublisherRequest, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/publishers",
                async (CreatePublisherRequest request, ISender sender) => await HandleAsync(request, sender))
            .Produces<Created<Guid>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithTags(nameof(Publisher))
            .WithName("Create Publisher")
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Created<Guid>> HandleAsync(CreatePublisherRequest request, ISender sender,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new CreatePublisherCommand(request.Name), cancellationToken);

        return TypedResults.Created($"/api/v1/publishers/{result.Value}", result.Value);
    }
}
