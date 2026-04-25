namespace BookWorm.Catalog.Features.Authors;

internal static class DomainToDtoMapper
{
    extension(Author author)
    {
        public AuthorDto ToAuthorDto()
        {
            return new(author.Id, author.Name);
        }
    }

    extension(IEnumerable<Author> authors)
    {
        public IReadOnlyList<AuthorDto> ToAuthorDtos()
        {
            return [.. authors.Select(ToAuthorDto)];
        }
    }
}
