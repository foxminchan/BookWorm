using Microsoft.Extensions.Configuration;

namespace BookWorm.AppHost.Extensions;

public static class ModelExtensions
{
    /// <summary>
    ///     Configures the builder to run as an Ollama instance with specific models and settings.
    /// </summary>
    /// <param name="builder">The resource builder to configure.</param>
    /// <param name="models">A dictionary of model names and their corresponding Ollama model names.</param>
    /// <returns>The configured resource builder.</returns>
    public static IResourceBuilder<ProjectResource> RunAsOllama(
        this IResourceBuilder<ProjectResource> builder,
        Dictionary<string, string> models
    )
    {
        var ollama = builder
            .ApplicationBuilder.AddOllama("ollama")
            .WithDataVolume()
            .WithOpenWebUI()
            .WithImagePullPolicy(ImagePullPolicy.Always)
            .WithLifetime(ContainerLifetime.Persistent)
            .PublishAsContainer();

        var enabledGraphics = builder.ApplicationBuilder.Configuration.GetValue<bool>(
            "Ollama:GPUSupport"
        );

        if (!OperatingSystem.IsMacOS() && enabledGraphics)
        {
            ollama.WithGPUSupport();
        }

        foreach (
            var ollamaModel in from model in models
            let name = model.Key
            let modelName = model.Value
            select ollama.AddModel(name, modelName)
        )
        {
            builder.WithReference(ollamaModel).WaitFor(ollamaModel);
        }

        return builder;
    }
}
