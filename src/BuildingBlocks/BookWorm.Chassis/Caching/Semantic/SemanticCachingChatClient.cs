using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;

namespace BookWorm.Chassis.Caching.Semantic;

internal sealed class SemanticCachingChatClient(
    IChatClient innerClient,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStoreCollection<string, SemanticCacheEntry> collection,
    IOptions<CachingOptions> options,
    ILogger<SemanticCachingChatClient> logger
) : DelegatingChatClient(innerClient)
{
    private readonly SemaphoreSlim _collectionLock = new(1, 1);
    private volatile bool _collectionEnsured;

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options1 = null,
        CancellationToken cancellationToken = default
    )
    {
        var messageList = messages as IReadOnlyList<ChatMessage> ?? [.. messages];

        if (!TryGetCacheablePrompt(messageList, out var prompt))
        {
            return await base.GetResponseAsync(messageList, options1, cancellationToken);
        }

        var (cachedResponse, embedding) = await TryGetCachedResponseAsync(
            prompt,
            cancellationToken
        );

        if (cachedResponse is not null)
        {
            return cachedResponse;
        }

        var response = await base.GetResponseAsync(messageList, options1, cancellationToken);

        if (IsCacheableResponse(response))
        {
            await StoreResponseAsync(prompt, embedding, response, cancellationToken);
        }

        return response;
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options1 = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var messageList = messages as IReadOnlyList<ChatMessage> ?? [.. messages];

        if (!TryGetCacheablePrompt(messageList, out var prompt))
        {
            await foreach (
                var update in base.GetStreamingResponseAsync(
                    messageList,
                    options1,
                    cancellationToken
                )
            )
            {
                yield return update;
            }

            yield break;
        }

        var (cachedResponse, embedding) = await TryGetCachedResponseAsync(
            prompt,
            cancellationToken
        );

        if (cachedResponse is not null)
        {
            foreach (var update in cachedResponse.ToChatResponseUpdates())
            {
                yield return update;
            }

            yield break;
        }

        List<ChatResponseUpdate> updates = [];

        await foreach (
            var update in base.GetStreamingResponseAsync(messageList, options1, cancellationToken)
        )
        {
            updates.Add(update);
            yield return update;
        }

        var response = updates.ToChatResponse();

        if (IsCacheableResponse(response))
        {
            await StoreResponseAsync(prompt, embedding, response, cancellationToken);
        }
    }

    private bool TryGetCacheablePrompt(IReadOnlyList<ChatMessage> messages, out string prompt)
    {
        prompt = string.Empty;

        if (!options.Value.SemanticCacheEnabled)
        {
            return false;
        }

        string? userText = null;
        var userCount = 0;

        foreach (var message in messages)
        {
            if (message.Role == ChatRole.Tool)
            {
                return false;
            }

            if (message.Role != ChatRole.User)
            {
                continue;
            }

            userCount++;
            if (userCount > 1)
            {
                return false;
            }

            userText = message.Text;
        }

        if (userCount != 1 || string.IsNullOrWhiteSpace(userText))
        {
            return false;
        }

        prompt = userText.Trim();
        return true;
    }

    private static bool IsCacheableResponse(ChatResponse response)
    {
        return !string.IsNullOrWhiteSpace(response.Text)
            && !response.Messages.SelectMany(m => m.Contents).OfType<FunctionCallContent>().Any();
    }

    private async Task<(
        ChatResponse? Response,
        ReadOnlyMemory<float> Embedding
    )> TryGetCachedResponseAsync(string prompt, CancellationToken cancellationToken)
    {
        await EnsureCollectionExistsOnceAsync(cancellationToken);

        var embedding = await embeddingGenerator.GenerateVectorAsync(
            prompt,
            cancellationToken: cancellationToken
        );

        var nowUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var threshold = options.Value.SemanticCacheSimilarityThreshold;

        await foreach (
            var result in collection.SearchAsync(embedding, 1, cancellationToken: cancellationToken)
        )
        {
            if (
                !(result.Score >= threshold)
                || result.Record.ExpiresAtUnixSeconds <= nowUnixSeconds
            )
            {
                continue;
            }

            Activity.Current?.SetTag("ai.cache.hit", true);
            Activity.Current?.SetTag("ai.cache.type", "semantic");
            Activity.Current?.SetTag("ai.cache.similarity", result.Score);

            logger.LogInformation("Semantic cache hit (similarity: {Similarity:F4})", result.Score);

            var cached = new ChatResponse(
                new ChatMessage(ChatRole.Assistant, result.Record.ResponseText)
            )
            {
                ModelId = result.Record.ModelId,
            };

            return (cached, embedding);
        }

        Activity.Current?.SetTag("ai.cache.hit", false);
        Activity.Current?.SetTag("ai.cache.type", "semantic");

        return (null, embedding);
    }

    private async Task StoreResponseAsync(
        string prompt,
        ReadOnlyMemory<float> embedding,
        ChatResponse response,
        CancellationToken cancellationToken
    )
    {
        await EnsureCollectionExistsOnceAsync(cancellationToken);

        var entry = new SemanticCacheEntry
        {
            Id = Guid.CreateVersion7().ToString("N"),
            Prompt = prompt,
            ResponseText = response.Text,
            ModelId = response.ModelId,
            ExpiresAtUnixSeconds = DateTimeOffset
                .UtcNow.Add(options.Value.Expiration)
                .ToUnixTimeSeconds(),
            Vector = embedding,
        };

        await collection.UpsertAsync(entry, cancellationToken);
    }

    private async Task EnsureCollectionExistsOnceAsync(CancellationToken cancellationToken)
    {
        if (_collectionEnsured)
        {
            return;
        }

        await _collectionLock.WaitAsync(cancellationToken);

        try
        {
            if (_collectionEnsured)
            {
                return;
            }

            await collection.EnsureCollectionExistsAsync(cancellationToken);
            _collectionEnsured = true;
        }
        finally
        {
            _collectionLock.Release();
        }
    }
}
