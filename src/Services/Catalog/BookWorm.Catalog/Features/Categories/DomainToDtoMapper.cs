namespace BookWorm.Catalog.Features.Categories;

public static class DomainToDtoMapper
{
    public static CategoryDto ToCategoryDto(this Category category)
    {
        return new(category.Id, category.Name);
    }

    public static IReadOnlyList<CategoryDto> ToCategoryDtos(this IEnumerable<Category> categories)
    {
        return [.. categories.AsValueEnumerable().Select(ToCategoryDto)];
    }
}
