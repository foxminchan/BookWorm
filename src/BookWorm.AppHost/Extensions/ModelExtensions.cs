using BookWorm.Constants;

namespace BookWorm.AppHost.Extensions;

public static class ModelExtensions
{
    /// <summary>
    ///     Configures the builder to run as an Ollama instance with specific models and settings.
    /// </summary>
    /// <param name="builder">The resource builder to configure.</param>
    /// <returns>The configured resource builder.</returns>
    public static IResourceBuilder<ProjectResource> RunAsOllama(
        this IResourceBuilder<ProjectResource> builder
    )
    {
        var ollama = builder
            .ApplicationBuilder.AddOllama("ollama")
            .WithDataVolume()
            .WithOpenWebUI()
            .WithImagePullPolicy(ImagePullPolicy.Always)
            .WithLifetime(ContainerLifetime.Persistent)
            .PublishAsContainer();

        if (!OperatingSystem.IsMacOS())
        {
            ollama.WithGPUSupport();
        }

        var embeddings = ollama.AddModel(Components.Ollama.Embedding, "nomic-embed-text:latest");

        var chat = ollama.AddModel(Components.Ollama.Chat, "deepseek-r1:1.5b");

        builder.WithReference(embeddings).WaitFor(embeddings).WithReference(chat).WaitFor(chat);

        return builder;
    }
}
