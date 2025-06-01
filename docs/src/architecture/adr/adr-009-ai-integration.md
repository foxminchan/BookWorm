---
category:
  - Architecture Decisions Records
tag:
  - ADR
---

# ADR-009: AI Integration Strategy

## Status

**Accepted** - December 2024

## Context

The system requires AI capabilities for search enhancement, recommendations, and chat functionality while maintaining cost efficiency and performance.

## Decision

Integrate external AI services (Nomic Embed Text, Gemma 3) through dedicated service abstractions.

## Rationale

- **Specialized Models**: Use purpose-built models for specific tasks
- **Cost Efficiency**: Pay-per-use model for AI capabilities
- **Flexibility**: Easy model switching and A/B testing
- **Performance**: External specialized hardware for AI processing
- **Abstraction**: Service interfaces allow for implementation changes

## Implementation

### AI Service Abstraction

```cs
public sealed class BookDataIngestor(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStore vectorStore
) : IIngestionSource<Book>
{
    private readonly string _collectionName = nameof(Book).ToLowerInvariant();

    public async Task IngestDataAsync(Book data, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(data.Name);
        ArgumentException.ThrowIfNullOrEmpty(data.Description);

        // Generate embeddings for book content
        var embedding = await embeddingGenerator.GenerateAsync(
            $"{data.Name} {data.Description}",
            cancellationToken);

        // Store in vector database
        var record = new VectorRecord<Book>
        {
            Id = data.Id.ToString(),
            Vector = embedding.Vector,
            Data = data
        };

        await vectorStore.UpsertAsync(_collectionName, record, cancellationToken);
    }
}
```

### Chat Service Integration

```csharp
public sealed class ChatService(
    IChatClient chatClient,
    IVectorStore vectorStore
) : IChatService
{
    public async IAsyncEnumerable<string> StreamChatAsync(
        string message,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Retrieve relevant context from vector store
        var context = await GetRelevantContextAsync(message, cancellationToken);

        // Stream response from AI model
        await foreach (var chunk in chatClient.CompleteStreamingAsync(message, context, cancellationToken))
        {
            yield return chunk;
        }
    }

    private async Task<string> GetRelevantContextAsync(string query, CancellationToken cancellationToken)
    {
        var searchResults = await vectorStore.SearchAsync(query, limit: 5, cancellationToken);
        return string.Join("\n", searchResults.Select(r => r.Data.ToString()));
    }
}
```

## Consequences

### Positive

- Access to state-of-the-art AI models
- No GPU infrastructure requirements
- Flexible model selection
- Clear service boundaries

### Negative

- External service dependencies
- Network latency for AI operations
- Potential cost scaling issues
- API rate limiting considerations

## Related Decisions

- [ADR-001: Microservices Architecture](adr-001-microservices-architecture.md)
- [ADR-004: PostgreSQL as Primary Database](adr-004-postgresql-database.md)
