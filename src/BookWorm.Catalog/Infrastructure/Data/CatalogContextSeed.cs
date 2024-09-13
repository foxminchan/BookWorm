using BookWorm.Catalog.Domain;

namespace BookWorm.Catalog.Infrastructure.Data;

public sealed class CatalogContextSeed : IDbSeeder<CatalogContext>
{
    public async Task SeedAsync(CatalogContext context)
    {
        if (!context.Categories.Any())
        {
            await context.Categories.AddRangeAsync(GetPreconfiguredCategories());
            await context.SaveChangesAsync();
        }

        if (!context.Publishers.Any())
        {
            await context.Publishers.AddRangeAsync(GetPreconfiguredPublishers());
            await context.SaveChangesAsync();
        }

        if (!context.Authors.Any())
        {
            await context.Authors.AddRangeAsync(GetPreconfiguredAuthors());
            await context.SaveChangesAsync();
        }
    }

    private static IEnumerable<Category> GetPreconfiguredCategories()
    {
        yield return new("Technology");
        yield return new("Personal Development");
        yield return new("Business");
        yield return new("Science");
        yield return new("Psychology");
        yield return new("Light Novel");
    }

    private static IEnumerable<Publisher> GetPreconfiguredPublishers()
    {
        yield return new("O'Reilly Media");
        yield return new("Manning Publications");
        yield return new("Packt Publishing");
        yield return new("Apress");
        yield return new("No Starch Press");
        yield return new("Pragmatic Bookshelf");
    }

    private static IEnumerable<Author> GetPreconfiguredAuthors()
    {
        yield return new("Martin Fowler");
        yield return new("Eric Evans");
        yield return new("Robert C. Martin");
        yield return new("Kent Beck");
        yield return new("Don Box");
        yield return new("Joshua Bloch");
    }
}
