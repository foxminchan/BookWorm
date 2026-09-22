using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Agents.AI;
using Npgsql;

namespace BookWorm.Catalog.Infrastructure;

internal sealed class CatalogDbContextSeed(
    IChatClient chatClient,
    ILogger<CatalogDbContextSeed> logger
) : IDbSeeder<CatalogDbContext>
{
    public async Task SeedAsync(CatalogDbContext context)
    {
        await context.Database.OpenConnectionAsync();
        await ((NpgsqlConnection)context.Database.GetDbConnection()).ReloadTypesAsync();

        if (!await context.Categories.AnyAsync())
        {
            logger.LogInformation("Seeding categories");
            context.Categories.AddRange(
                (
                    await LoadSetupDataAsync(
                        "categories.json",
                        CatalogSeedSerializationContext.Default.ListCategorySeed
                    )
                ).Select(static seed => new Category(seed.Name))
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Authors.AnyAsync())
        {
            logger.LogInformation("Seeding authors");
            context.Authors.AddRange(
                (
                    await LoadSetupDataAsync(
                        "authors.json",
                        CatalogSeedSerializationContext.Default.ListAuthorSeed
                    )
                ).Select(static seed => new Author(seed.Name))
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Publishers.AnyAsync())
        {
            logger.LogInformation("Seeding publishers");
            context.Publishers.AddRange(
                (
                    await LoadSetupDataAsync(
                        "publishers.json",
                        CatalogSeedSerializationContext.Default.ListPublisherSeed
                    )
                ).Select(static seed => new Publisher(seed.Name))
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Books.AnyAsync())
        {
            logger.LogInformation("Seeding books");
            var authorIds = await context.Authors.Select(a => a.Id).ToListAsync();
            var publisherIds = await context.Publishers.Select(p => p.Id).ToListAsync();
            var categoryIds = await context.Categories.Select(c => c.Id).ToListAsync();

            var random = Random.Shared;
            var books = new List<Book>();

            foreach (
                var seed in await LoadSetupDataAsync(
                    "books.json",
                    CatalogSeedSerializationContext.Default.ListBookSeed
                )
            )
            {
                var book = new Book(
                    seed.Name,
                    null,
                    null,
                    seed.Price,
                    seed.PriceSale,
                    CategoryId.From(Guid.Empty),
                    PublisherId.From(Guid.Empty),
                    []
                );

                var instructions = $"""
                    You are a professional book metadata writer.
                    Task: Generate a compelling description for a book titled "{book.Name}".

                    Requirements:
                    - Write 2-3 sentences that capture the essence of the book
                    - Include the likely genre based on the title
                    - Mention probable main themes and a brief plot overview
                    - Keep the description under 700 characters
                    - Use engaging, professional language appropriate for a book catalog
                    - Focus on creating a description that would interest potential readers

                    Return only the description text with no additional formatting or commentary.
                    """;

                var agent = new ChatClientAgent(chatClient, instructions);
                var options = new ChatClientAgentRunOptions(new() { Temperature = 0.6f });
                var thread = await agent.CreateSessionAsync();

                var response = await agent.RunAsync(
                    $"Generate a book description for the title: '{book.Name}'",
                    thread,
                    options
                );

                var assistantMessage = response.Messages.LastOrDefault(static message =>
                    message.Role == ChatRole.Assistant
                );

                var description = assistantMessage?.Text;

                if (string.IsNullOrWhiteSpace(description))
                {
                    logger.LogWarning(
                        "No assistant description generated for book {Name}, using fallback",
                        book.Name
                    );
                    description =
                        $"A captivating book titled '{book.Name}' that offers readers an engaging literary experience.";
                }

                logger.LogDebug(
                    "Generated description for book {Name}: {Description}",
                    book.Name,
                    description
                );

                book.SetMetadata(
                    description,
                    categoryIds[random.Next(0, categoryIds.Count)],
                    publisherIds[random.Next(0, publisherIds.Count)],
                    [authorIds[random.Next(0, authorIds.Count)]]
                );

                books.Add(book);
            }

            context.Books.AddRange(books);
            await context.SaveChangesAsync();
        }
    }

    private static async Task<List<T>> LoadSetupDataAsync<T>(
        string fileName,
        JsonTypeInfo<List<T>> typeInfo
    )
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Setup", fileName);
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync(stream, typeInfo)
            ?? throw new InvalidOperationException($"Setup data file '{fileName}' is empty.");
    }
}

internal sealed record CategorySeed(string Name);

internal sealed record AuthorSeed(string Name);

internal sealed record PublisherSeed(string Name);

internal sealed record BookSeed(string Name, decimal Price, decimal? PriceSale);

[JsonSerializable(typeof(List<CategorySeed>))]
[JsonSerializable(typeof(List<AuthorSeed>))]
[JsonSerializable(typeof(List<PublisherSeed>))]
[JsonSerializable(typeof(List<BookSeed>))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class CatalogSeedSerializationContext : JsonSerializerContext;
