using BookWorm.Catalog.Domain;

namespace BookWorm.Catalog.Features.Authors;

public static class EntityToDto
{
    public static AuthorDto ToAuthorDto(this Author author)
    {
        return new(author.Id, author.Name);
    }

    public static List<AuthorDto> ToAuthorDtos(this IEnumerable<Author> authors)
    {
        return authors.Select(ToAuthorDto).ToList();
    }
}
