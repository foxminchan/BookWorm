namespace BookWorm.AppHost.Extensions.Security;

public static class SecretExtensions
{
    /// <summary>
    ///     Adds an HMAC secret key to the resource builder's environment configuration.
    /// </summary>
    /// <param name="builder">The resource builder to configure.</param>
    /// <returns>The configured <see cref="IResourceBuilder{ProjectResource}" /> instance.</returns>
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
    ///     The resource builder to configure.
    /// </param>
    /// <param name="generateParameterDefault">
    ///     A delegate used to generate the default parameter value.
    /// </param>
    /// <returns>
    ///     The configured <see cref="IResourceBuilder{ParameterResource}" /> instance.
    /// </returns>
    /// <remarks>
    ///     This method generates a default parameter value using the provided delegate and assigns it to the resource.
    ///     It also overrides the initial state of the resource to ensure parameters are hidden by default and properly bound
    ///     to the configuration source. This behavior is adapted from the internal static helper
    ///     <c>AddParameter(this IDistributedApplicationBuilder builder, ...)</c> in
    ///     Aspire.Hosting.ParameterResourceBuilderExtensions.
    /// </remarks>
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
