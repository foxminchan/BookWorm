namespace BookWorm.Catalog.Features.Publishers;

internal static class DomainToDtoMapper
{
    extension(Publisher publisher)
    {
        public PublisherDto ToPublisherDto()
        {
            return new(publisher.Id, publisher.Name);
        }
    }

    extension(IEnumerable<Publisher> publishers)
    {
        public IReadOnlyList<PublisherDto> ToPublisherDtos()
        {
            return [.. publishers.Select(ToPublisherDto)];
        }
    }
}
