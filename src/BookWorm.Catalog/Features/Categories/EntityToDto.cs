using BookWorm.Catalog.Domain;

namespace BookWorm.Catalog.Features.Categories;

public static class EntityToDto
{
    public static CategoryDto ToCategoryDto(this Category category)
    {
        return new(category.Id, category.Name);
    }

    public static List<CategoryDto> ToCategoryDtos(this IEnumerable<Category> categories)
    {
        return categories.Select(ToCategoryDto).ToList();
    }
}
