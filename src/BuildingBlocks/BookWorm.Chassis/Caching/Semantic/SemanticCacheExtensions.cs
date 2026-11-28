using BookWorm.Chassis.Utilities.Configurations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using StackExchange.Redis;

namespace BookWorm.Chassis.Caching.Semantic;

public static class SemanticCacheExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers Redis-backed semantic caching options and vector collection.
        ///     Reuses the <see cref="IConnectionMultiplexer" /> registered by Aspire's Redis client.
        /// </summary>
        public IHostApplicationBuilder WithSemanticCache()
        {
            builder.Configure<CachingOptions>(CachingOptions.ConfigurationSection);
            builder.Services.TryAddSingleton(sp =>
                sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase()
            );
            builder.Services.AddRedisHashSetCollection<SemanticCacheEntry>(
                SemanticCacheEntry.CollectionName
            );

            return builder;
        }
    }

    extension(ChatClientBuilder builder)
    {
        /// <summary>
        ///     Adds multi-level AI caching to the <see cref="IChatClient" /> pipeline:
        ///     exact-match result caching via <see cref="DistributedCachingChatClient" /> (when
        ///     <see cref="IDistributedCache" /> is registered), followed by Redis vector-similarity
        ///     semantic caching via <see cref="SemanticCachingChatClient" /> (when both an
        ///     <see cref="IEmbeddingGenerator{TInput, TEmbedding}" /> and a
        ///     <see cref="VectorStoreCollection{TKey,TRecord}" /> for <see cref="SemanticCacheEntry" />
        ///     are registered in DI).
        /// </summary>
        public ChatClientBuilder UseSemanticCache()
        {
            return builder
                .Use(
                    static (innerClient, sp) =>
                        sp.GetService<IDistributedCache>() is { } cache
                            ? new DistributedCachingChatClient(innerClient, cache)
                            : innerClient
                )
                .Use(
                    (innerClient, sp) =>
                    {
                        var embeddingGenerator = sp.GetService<
                            IEmbeddingGenerator<string, Embedding<float>>
                        >();
                        var collection = sp.GetService<
                            VectorStoreCollection<string, SemanticCacheEntry>
                        >();

                        if (embeddingGenerator is null || collection is null)
                        {
                            return innerClient;
                        }

                        if (sp.GetService<IDistributedCache>() is { } distributedCache)
                        {
                            embeddingGenerator = new DistributedCachingEmbeddingGenerator<
                                string,
                                Embedding<float>
                            >(embeddingGenerator, distributedCache);
                        }

                        var options = sp.GetService<IOptions<CachingOptions>>()
                            ?? Options.Create(new CachingOptions());
                        var logger = sp.GetRequiredService<ILogger<SemanticCachingChatClient>>();

                        return new SemanticCachingChatClient(
                            innerClient,
                            embeddingGenerator,
                            collection,
                            options,
                            logger
                        );
                    }
                );
        }
    }
}
