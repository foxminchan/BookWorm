namespace BookWorm.AppHost.Extensions.Security;

public static class SecretExtensions
{
    /// <summary>
    ///     Configures the resource builder to create a generated secret parameter and expose it as an environment variable.
    /// </summary>
    /// <param name="builder">The resource builder to extend.</param>
    /// <param name="secretName">Suffix used to form the parameter name; the final parameter will be <c>{builder.Resource.Name}-{secretName}</c>.</param>
    /// <param name="environmentVariableName">Name of the environment variable that will be bound to the created secret parameter.</param>
    /// <returns>The original <see cref="IResourceBuilder{ProjectResource}" /> after registering the parameter and mapping it to the environment variable.</returns>
    /// <remarks>
    ///     The generated parameter uses default constraints of minimum length 32 and no special characters.
    ///     The created parameter is returned as a parameter resource which is then mapped to the specified environment variable.
    /// </remarks>
    public static IResourceBuilder<ProjectResource> WithSecret(
        this IResourceBuilder<ProjectResource> builder,
        string secretName,
        string environmentVariableName
    )
    {
        var secret = builder
            .ApplicationBuilder.AddParameter($"{builder.Resource.Name}-{secretName}", true)
            .WithGeneratedDefault(new() { MinLength = 32, Special = false });

        return builder.WithEnvironment(environmentVariableName, secret);
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
