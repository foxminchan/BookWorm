namespace BookWorm.Catalog.Features.Categories;

internal static class DomainToDtoMapper
{
    extension(Category category)
    {
        public CategoryDto ToCategoryDto()
        {
            return new(category.Id, category.Name);
        }
    }

    extension(IEnumerable<Category> categories)
    {
        public IReadOnlyList<CategoryDto> ToCategoryDtos()
        {
            return [.. categories.Select(ToCategoryDto)];
        }
    }
}
