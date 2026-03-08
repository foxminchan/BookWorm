namespace BookWorm.Catalog.Features.Authors;

internal static class DomainToDtoMapper
{
    public static AuthorDto ToAuthorDto(this Author author)
    {
        return new(author.Id, author.Name);
    }

    public static IReadOnlyList<AuthorDto> ToAuthorDtos(this IEnumerable<Author> authors)
    {
        return [.. authors.Select(ToAuthorDto)];
    }
}
