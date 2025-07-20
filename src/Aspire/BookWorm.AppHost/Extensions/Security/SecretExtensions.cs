namespace BookWorm.AppHost.Extensions.Security;

public static class SecretExtensions
{
    /// <summary>
    ///     Adds an HMAC secret key to the resource builder's environment configuration.
    /// </summary>
    /// <param name="builder">The resource builder to configure with HMAC secret key.</param>
    /// <returns>The configured <see cref="IResourceBuilder{ProjectResource}" /> instance with HMAC key environment variable.</returns>
    public static IResourceBuilder<ProjectResource> WithHmacSecret(
        this IResourceBuilder<ProjectResource> builder
    )
    {
        var hmacKey = builder
            .ApplicationBuilder.AddParameter($"{builder.Resource.Name}-hmac-key", true)
            .WithGeneratedDefault(new() { MinLength = 32, Special = false });

        return builder.WithEnvironment("HMAC__Key", hmacKey);
    }

    /// <summary>
    ///     Configures the resource builder to generate a default parameter value and override the initial state of the
    ///     resource.
    /// </summary>
    /// <param name="builder">
    ///     The resource builder to configure with generated default parameter value.
    /// </param>
    /// <param name="generateParameterDefault">
    ///     A delegate used to generate the default parameter value with specified constraints.
    /// </param>
    /// <returns>
    ///     The configured <see cref="IResourceBuilder{ParameterResource}" /> instance with generated default value and proper
    ///     initial state.
    /// </returns>
    public static IResourceBuilder<ParameterResource> WithGeneratedDefault(
        this IResourceBuilder<ParameterResource> builder,
        GenerateParameterDefault generateParameterDefault
    )
    {
        var generatedParameter = ParameterResourceBuilderExtensions.CreateGeneratedParameter(
            builder.ApplicationBuilder,
            builder.Resource.Name,
            builder.Resource.Secret,
            generateParameterDefault
        );

        builder.Resource.Default = generatedParameter.Default;

        builder.WithInitialState(
            new()
            {
                ResourceType = "Parameter",
                IsHidden = true,
                Properties =
                [
                    new("parameter.secret", builder.Resource.Secret.ToString()),
                    new(
                        CustomResourceKnownProperties.Source,
                        $"Parameters:{builder.Resource.Name}"
                    ),
                ],
            }
        );

        return builder;
    }
}
