using BookWorm.Chassis.AI.Search;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Npgsql;

namespace BookWorm.Catalog.Ingestion;

internal sealed class VectorBatchProcessor(
    NpgsqlDataSource dataSource,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStoreCollection<Guid, TextSnippet> collection,
    ILogger<VectorBatchProcessor> logger
) : IVectorBatchProcessor
{
    private const int BatchSize = 200;
    private const long LockId = 740031001;

    public async Task<int> ProcessAsync(CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var acquireLock = new NpgsqlCommand(
            "SELECT pg_try_advisory_lock($1)",
            connection
        );
        acquireLock.Parameters.AddWithValue(LockId);
        if (await acquireLock.ExecuteScalarAsync(cancellationToken) is not true)
        {
            return 0;
        }

        try
        {
            var pending = await ReadBatchAsync(connection, cancellationToken);
            if (pending.Count == 0)
            {
                return 0;
            }

            await collection.EnsureCollectionExistsAsync(cancellationToken);
            var active = pending.Where(item => item.Content is not null).ToArray();
            var embeddings =
                active.Length == 0
                    ? null
                    : await embeddingGenerator.GenerateAsync(
                        active.Select(item => item.Content!),
                        cancellationToken: cancellationToken
                    );

            if (embeddings is not null && embeddings.Count != active.Length)
            {
                throw new InvalidOperationException(
                    "The embedding service returned an incomplete batch."
                );
            }

            var index = 0;
            var processed = 0;
            foreach (var item in pending)
            {
                if (item.Content is null)
                {
                    await collection.DeleteAsync(item.BookId, cancellationToken);
                }
                else
                {
                    await collection.UpsertAsync(
                        new TextSnippet
                        {
                            Id = item.BookId,
                            Content = item.Content,
                            Vector = embeddings![index++].Vector,
                        },
                        cancellationToken
                    );
                }

                // Concurrent edits change revision and must survive this acknowledgement.
                await using var acknowledge = new NpgsqlCommand(
                    "DELETE FROM pending_vector_updates WHERE book_id = $1 AND revision = $2",
                    connection
                );
                acknowledge.Parameters.AddWithValue(item.BookId);
                acknowledge.Parameters.AddWithValue(item.Revision);
                await acknowledge.ExecuteNonQueryAsync(cancellationToken);
                processed++;
            }

            logger.LogInformation("Processed {Count} pending vector updates", processed);
            return processed;
        }
        finally
        {
            await using var releaseLock = new NpgsqlCommand(
                "SELECT pg_advisory_unlock($1)",
                connection
            );
            releaseLock.Parameters.AddWithValue(LockId);
            await releaseLock.ExecuteScalarAsync(CancellationToken.None);
        }
    }

    private async Task<List<PendingUpdate>> ReadBatchAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken
    )
    {
        await using var command = new NpgsqlCommand(
            """
            SELECT pending.book_id, pending.revision,
                CASE WHEN book.id IS NULL OR book.is_deleted THEN NULL ELSE
                    concat('Title: ', book.name, E'\nDescription: ', book.description,
                        E'\nAuthors: ', (
                            SELECT string_agg(author.name, ', ' ORDER BY author.name, author.id)
                            FROM book_authors link JOIN authors author ON author.id = link.author_id
                            WHERE link.book_id = book.id
                        ), E'\nCategory: ', category.name, E'\nPublisher: ', publisher.name)
                END AS content
            FROM pending_vector_updates pending
            LEFT JOIN books book ON book.id = pending.book_id
            LEFT JOIN categories category ON category.id = book.category_id
            LEFT JOIN publishers publisher ON publisher.id = book.publisher_id
            ORDER BY pending.updated_at, pending.book_id
            LIMIT $1
            """,
            connection
        );
        command.Parameters.AddWithValue(BatchSize);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        List<PendingUpdate> pending = [];
        while (await reader.ReadAsync(cancellationToken))
        {
            pending.Add(
                new(
                    reader.GetGuid(0),
                    reader.GetGuid(1),
                    reader.IsDBNull(2) ? null : reader.GetString(2)
                )
            );
        }

        return pending;
    }

    private sealed record PendingUpdate(Guid BookId, Guid Revision, string? Content);
}
