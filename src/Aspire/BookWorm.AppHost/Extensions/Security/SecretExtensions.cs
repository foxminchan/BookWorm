namespace BookWorm.AppHost.Extensions.Security;

public static class SecretExtensions
{
    /// <summary>
    ///     Adds an HMAC secret key to the resource builder's environment configuration.
    /// </summary>
    /// <param name="builder">The resource builder to configure with HMAC secret key.</param>
    /// <returns>The configured <see cref="IResourceBuilder{ProjectResource}" /> instance with HMAC key environment variable.</returns>
    /// <remarks>
    ///     This method provides secure HMAC key generation and configuration:
    ///     - Creates a secure parameter with the project name as prefix (e.g., "api-hmac-key")
    ///     - Generates a cryptographically secure 32-character key without special characters
    ///     - Marks the parameter as secret for secure handling in deployment
    ///     - Sets the <c>HMAC__Key</c> environment variable for application consumption
    ///     - Ensures consistent HMAC key availability across application restarts
    ///     - Follows .NET configuration naming conventions with double underscore syntax
    /// </remarks>
    /// <example>
    ///     <code>
    ///     builder.AddProject&lt;WebApi&gt;("api")
    ///            .WithHmacSecret();
    ///     </code>
    /// </example>
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
    /// <remarks>
    ///     This method provides comprehensive parameter generation and state management:
    ///     - <strong>Default value generation:</strong> Uses the provided delegate to create cryptographically secure
    ///     parameter values
    ///     - <strong>State management:</strong> Configures initial resource state with proper visibility and binding settings
    ///     - <strong>Security handling:</strong> Respects the parameter's secret flag for secure parameter management
    ///     - <strong>Configuration binding:</strong> Establishes proper configuration source binding using
    ///     <c>Parameters:{parameterName}</c> format
    ///     - <strong>Resource hiding:</strong> Sets resources as hidden by default to prevent accidental exposure in UI
    ///     - <strong>Aspire integration:</strong> Follows Aspire hosting patterns for parameter resource management
    ///     - Implementation is based on internal Aspire helper <c>AddParameter</c> from
    ///     <c>ParameterResourceBuilderExtensions</c>
    /// </remarks>
    /// <example>
    ///     <code>
    ///     var secretParam = builder.AddParameter("my-secret", true)
    ///                              .WithGeneratedDefault(new() { MinLength = 32, Special = false });
    ///
    ///     // With custom generation options
    ///     var apiKey = builder.AddParameter("api-key", true)
    ///                         .WithGeneratedDefault(new() {
    ///                             MinLength = 64,
    ///                             Upper = true,
    ///                             Lower = true,
    ///                             Numeric = true
    ///                         });
    ///     </code>
    /// </example>
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
