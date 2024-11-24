using Microsoft.Extensions.AI;
using OpenAI;

namespace BookWorm.Catalog.Infrastructure.Ai;

internal static class Extension
{
    public static IHostApplicationBuilder AddAi(this IHostApplicationBuilder builder)
    {
        builder.AddOpenAIClientFromConfiguration(ServiceName.OpenAi);
        builder
            .Services.AddEmbeddingGenerator(sp =>
                sp.GetRequiredService<OpenAIClient>()
                    .AsEmbeddingGenerator(
                        builder.Configuration["AiOptions:OpenAi:EmbeddingName"]
                            ?? "text-embedding-3-small"
                    )
            )
            .UseOpenTelemetry()
            .UseLogging()
            .Build();
        builder.Services.AddSingleton<IAiService, AiService>();

        return builder;
    }
}
