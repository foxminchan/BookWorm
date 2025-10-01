namespace BookWorm.AppHost.Extensions.AI;

public static class ModelExtensions
{
    /// <summary>
    ///     Adds an Ollama instance to the distributed application.
    /// </summary>
    /// <param name="builder">The distributed application builder to add the Ollama instance to.</param>
    /// <param name="configure">Optional action to further configure the Ollama resource.</param>
    /// <param name="useOpenWebUI">Flag to indicate whether to enable the OpenWebUI for Ollama.</param>
    public static async Task AddOllama(
        this IDistributedApplicationBuilder builder,
        Action<IResourceBuilder<OllamaResource>>? configure = null,
        bool useOpenWebUI = false
    )
    {
        var ollama = builder
            .AddOllama(Components.Ollama.Resource)
            .WithDataVolume()
            .WithIconName("BrainSparkle")
            .WithImagePullPolicy(ImagePullPolicy.Always)
            .WithLifetime(ContainerLifetime.Persistent)
            .PublishAsAzureContainerApp((_, app) => app.Template.Scale.MinReplicas = 0);

        if (await ollama.IsUseGpu())
        {
            ollama.WithGPUSupport();
        }

        if (useOpenWebUI)
        {
            ollama.WithOpenWebUI();
        }

        configure?.Invoke(ollama);
    }

    /// <summary>
    ///     Configures a project resource to utilize Ollama models.
    /// </summary>
    /// <param name="builder">The project resource builder to be configured.</param>
    /// <returns>The configured project resource builder with Ollama model references.</returns>
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

    private static async Task<bool> IsUseGpu(this IResourceBuilder<OllamaResource> builder)
    {
        if (OperatingSystem.IsMacOS())
        {
            return false;
        }

        var parameter = builder.ApplicationBuilder.AddParameterFromConfiguration(
            $"{builder.Resource.Name}-use-gpu",
            "UseGPU"
        );

        parameter.WithParentRelationship(builder);

        var envVar = await parameter.Resource.GetValueAsync(CancellationToken.None);

        return int.TryParse(envVar, out var useGpu) && useGpu == 1;
    }
}
