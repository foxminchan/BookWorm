﻿using System.Text.RegularExpressions;
using Npgsql;

namespace BookWorm.Catalog.Infrastructure;

public sealed partial class CatalogDbContextSeed(
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
            context.Categories.AddRange(new CategoryData());
            await context.SaveChangesAsync();
        }

        if (!await context.Authors.AnyAsync())
        {
            logger.LogInformation("Seeding authors");
            context.Authors.AddRange(new AuthorData());
            await context.SaveChangesAsync();
        }

        if (!await context.Publishers.AnyAsync())
        {
            logger.LogInformation("Seeding publishers");
            context.Publishers.AddRange(new PublisherData());
            await context.SaveChangesAsync();
        }

        if (!await context.Books.AnyAsync())
        {
            logger.LogInformation("Seeding books");
            var authorIds = await context.Authors.Select(a => a.Id).ToListAsync();
            var publisherIds = await context.Publishers.Select(p => p.Id).ToListAsync();
            var categoryIds = await context.Categories.Select(c => c.Id).ToListAsync();

            var random = Random.Shared;
            var books = new BookData();

            foreach (var book in books)
            {
                var prompts = $"""
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

                var response = await chatClient.GetResponseAsync(
                    prompts,
                    new() { Temperature = 0.6f, ResponseFormat = ChatResponseFormat.Text }
                );

                var description =
                    response.Message.Text?.Trim('"', ' ', '\r', '\n', '\t') ?? string.Empty;

                description = RemoveExtraWhitespaceRegex().Replace(description, " ");
                description = RemoveThinkTagsRegex().Replace(description, string.Empty);

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
            }

            context.Books.AddRange(books);
            await context.SaveChangesAsync();
        }
    }

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex RemoveExtraWhitespaceRegex();

    [GeneratedRegex("<think>.*?</think>", RegexOptions.Singleline)]
    private static partial Regex RemoveThinkTagsRegex();
}
