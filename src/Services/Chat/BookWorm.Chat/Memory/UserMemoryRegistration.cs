using BookWorm.Chassis.EF;
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Chat.Memory.Episodic;
using BookWorm.Chat.Memory.Extraction;
using BookWorm.Chat.Memory.Semantic;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookWorm.Chat.Memory;

internal static class UserMemoryRegistration
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers BookAgent's durable user memory: semantic preferences in PostgreSQL (<c>chatdb</c>),
        ///     episodic memories in Qdrant, the context provider, and the background extraction pipeline.
        /// </summary>
        /// <remarks>
        ///     Memory is only enabled when <c>Chat:Memory:Enabled</c> is not <see langword="false" /> and the
        ///     chat database, vector database, chat and embedding connection strings are all configured.
        ///     Otherwise a no-op <see cref="IUserMemoryService" /> is registered and BookAgent runs without
        ///     cross-session memory.
        /// </remarks>
        public void AddUserMemory()
        {
            var services = builder.Services;

            builder.Configure<UserMemoryOptions>(UserMemoryOptions.SectionName);

            services.AddHttpContextAccessor();
            services.AddSingleton<IUserIdentityAccessor, HttpUserIdentityAccessor>();

            if (!builder.IsUserMemoryAvailable())
            {
                services.AddSingleton<IUserMemoryService, DisabledUserMemoryService>();
                return;
            }

            // Semantic memory (generalized preferences)
            builder.AddAzurePostgresDbContext<ChatDbContext>(
                Components.Database.Chat,
                _ => services.AddMigration<ChatDbContext>(),
                true
            );
            services.AddScoped<IPreferenceStore, PreferenceStore>();

            // Episodic memory (summaries of past interactions)
            builder.AddQdrantClient(Components.VectorDb);
            services.AddQdrantCollection<Guid, EpisodicMemory>(EpisodicMemory.CollectionName);
            services.AddSingleton<IEpisodicMemoryStore, QdrantEpisodicMemoryStore>();
            services.AddHostedService<EpisodicMemoryRetentionService>();

            // Recall
            services.TryAddSingleton(TimeProvider.System);
            services.AddSingleton<IUserMemoryService, UserMemoryService>();
            services.AddSingleton<UserMemoryContextProvider>();

            // Background extraction
            services.AddSingleton<IMemoryExtractionQueue, MemoryExtractionQueue>();
            services.AddSingleton<IMemoryExtractor, MemoryExtractor>();
            services.AddHostedService<MemoryExtractionService>();
        }

        private bool IsUserMemoryAvailable()
        {
            var configuration = builder.Configuration;

            return configuration.GetValue($"{UserMemoryOptions.SectionName}:Enabled", true)
                && !string.IsNullOrWhiteSpace(
                    configuration.GetConnectionString(Components.Database.Chat)
                )
                && !string.IsNullOrWhiteSpace(
                    configuration.GetConnectionString(Components.VectorDb)
                )
                && !string.IsNullOrWhiteSpace(
                    configuration.GetConnectionString(Components.Foundry.Chat)
                )
                && !string.IsNullOrWhiteSpace(
                    configuration.GetConnectionString(Components.Foundry.Embedding)
                );
        }
    }
}
