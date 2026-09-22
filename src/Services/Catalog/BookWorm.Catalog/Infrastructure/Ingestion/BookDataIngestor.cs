using BookWorm.Chassis.AI.Ingestion;
using BookWorm.Chassis.AI.Search;
using Microsoft.Extensions.VectorData;

namespace BookWorm.Catalog.Infrastructure.Ingestion;

internal sealed class BookDataIngestor(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStoreCollection<Guid, TextSnippet> vectorCollection,
    CatalogDbContext dbContext
) : IIngestionSource<Book>
{
    public async Task IngestDataAsync(Book data, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(data.Name);
        ArgumentException.ThrowIfNullOrEmpty(data.Description);

        await vectorCollection.EnsureCollectionExistsAsync(cancellationToken);

        var authorIds = data.BookAuthors.Select(bookAuthor => bookAuthor.AuthorId.Value).ToArray();

        var authorNames = await dbContext
            .Authors.Where(author => authorIds.AsEnumerable().Contains(author.Id.Value))
            .Select(author => author.Name)
            .ToListAsync(cancellationToken);

        var categoryName = data.CategoryId is not null
            ? await dbContext
                .Categories.Where(category => category.Id.Value == data.CategoryId.Value)
                .Select(category => category.Name)
                .SingleOrDefaultAsync(cancellationToken)
            : null;

        var publisherName = data.PublisherId is not null
            ? await dbContext
                .Publishers.Where(publisher => publisher.Id.Value == data.PublisherId.Value)
                .Select(publisher => publisher.Name)
                .SingleOrDefaultAsync(cancellationToken)
            : null;

        var content = $"""
            Title: {data.Name}
            Description: {data.Description}
            Authors: {string.Join(", ", authorNames)}
            Category: {categoryName}
            Publisher: {publisherName}
            """;

        var embeddings = await embeddingGenerator.GenerateVectorAsync(
            content,
            cancellationToken: cancellationToken
        );

        var record = new TextSnippet
        {
            Id = (Guid)data.Id,
            Content = content,
            Vector = embeddings,
        };

        await vectorCollection.UpsertAsync(record, cancellationToken);
    }
}
