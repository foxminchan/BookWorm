using Microsoft.SemanticKernel;

namespace BookWorm.Catalog.Infrastructure.Ai;

internal static class Extension
{
    public static IHostApplicationBuilder AddAi(this IHostApplicationBuilder builder)
    {
        var modelName = builder.Configuration["AiOptions:OpenAi:EmbeddingName"] ?? "text-embedding-3-small";

        builder.AddAzureOpenAIClient(ServiceName.OpenAi);
        builder.Services.AddOpenAITextEmbeddingGeneration(modelName);

        builder.Services.AddSingleton<IAiService, AiService>();

        return builder;
    }
}
