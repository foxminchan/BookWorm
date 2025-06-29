namespace BookWorm.AppHost.Extensions.AI;

public static class ModelExtensions
{
    /// <summary>
    ///     Adds an Ollama instance to the distributed application.
    /// </summary>
    /// <param name="builder">The distributed application builder to add the Ollama instance to.</param>
    /// <param name="configure">Optional action to further configure the Ollama resource.</param>
    /// <returns>A configured container resource builder for the Ollama instance.</returns>
    /// <remarks>
    ///     This method configures an Ollama container with sensible defaults:
    ///     - Creates a persistent data volume
    ///     - Enables the OpenWebUI interface
    ///     - Always pulls the latest image
    ///     - Configures the container with persistent lifetime
    ///     - Automatically enables GPU support on non-macOS systems when useGpu is true
    /// </remarks>
    public static void AddOllama(
        this IDistributedApplicationBuilder builder,
        Action<IResourceBuilder<OllamaResource>>? configure = null
    )
    {
        var ollama = builder
            .AddOllama(Components.Ollama.Resource)
            .WithDataVolume()
            .WithOpenWebUI()
            .WithImagePullPolicy(ImagePullPolicy.Always)
            .WithLifetime(ContainerLifetime.Persistent)
            .PublishAsContainer();

        if (IsUseGpu())
        {
            ollama.WithGPUSupport();
        }

        configure?.Invoke(ollama);
    }

    /// <summary>
    ///     Configures a project resource to utilize Ollama models.
    /// </summary>
    /// <param name="builder">The project resource builder to be configured.</param>
    /// <returns>The configured project resource builder.</returns>
    /// <remarks>
    ///     This method locates an existing Ollama resource in the application,
    ///     adds the specified models to it, and configures the project to reference
    ///     and wait for these models to be loaded before proceeding.
    /// </remarks>
    public static IResourceBuilder<ProjectResource> WithOllama(
        this IResourceBuilder<ProjectResource> builder
    )
    {
        var models = builder
            .ApplicationBuilder.Resources.OfType<OllamaModelResource>()
            .Select(builder.ApplicationBuilder.CreateResourceBuilder);

        foreach (var model in models)
        {
            builder.WithReference(model).WaitFor(model);
        }

        return builder;
    }

    private static bool IsUseGpu()
    {
        if (OperatingSystem.IsMacOS())
        {
            return false;
        }

        var envVar = Environment.GetEnvironmentVariable("OLLAMA_USE_GPU");
        return int.TryParse(envVar, out var useGpu) && useGpu == 1;
    }
}
