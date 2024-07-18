using BookWorm.Catalog.Domain;

namespace BookWorm.Catalog.Features.Publishers;

public static class EntityToDto
{
    public static PublisherDto ToPublisherDto(this Publisher publisher)
    {
        return new(publisher.Id, publisher.Name);
    }

    public static List<PublisherDto> ToPublisherDtos(this IEnumerable<Publisher> publishers)
    {
        return publishers.Select(ToPublisherDto).ToList();
    }
}
