namespace BookWorm.Catalog.Features.Publishers;

public static class DomainToDtoMapper
{
    public static PublisherDto ToPublisherDto(this Publisher publisher)
    {
        return new(publisher.Id, publisher.Name);
    }

    public static IReadOnlyList<PublisherDto> ToPublisherDtos(
        this IEnumerable<Publisher> publishers
    )
    {
        return [.. publishers.AsValueEnumerable().Select(ToPublisherDto)];
    }
}
