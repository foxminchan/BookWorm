namespace BookWorm.AppHost.Extensions.Network;

public static class EmailExtensions
{
    /// <summary>
    ///     Configures the email provider for the application based on the execution context.
    /// </summary>
    /// <param name="builder">The resource builder for the project resource.</param>
    /// <returns>The updated resource builder with email provider configuration applied.</returns>
    /// <remarks>
    ///     This method provides dual email provider configuration based on execution context:
    ///     - <strong>Run mode (local development):</strong> Configures MailPit as a local SMTP server for email testing and
    ///     debugging
    ///     - Uses SMTP port 587 for standard email delivery
    ///     - Establishes reference and wait dependency for proper startup order
    ///     - Creates parent-child relationship for resource management
    ///     - Provides web interface for viewing sent emails during development
    ///     - <strong>Publish mode (cloud deployment):</strong> Configures SendGrid as the production email service
    ///     - Sets up secure API key parameter for authentication
    ///     - Configures sender email address parameter for outbound emails
    ///     - Sets sender name parameter for email branding
    ///     - All parameters are marked as secret for security
    /// </remarks>
    /// <example>
    ///     <code>
    ///     builder.AddProject&lt;WebApi&gt;("api")
    ///            .WithEmailProvider();
    ///     </code>
    /// </example>
    public static IResourceBuilder<ProjectResource> WithEmailProvider(
        this IResourceBuilder<ProjectResource> builder
    )
    {
        var applicationBuilder = builder.ApplicationBuilder;

        if (applicationBuilder.ExecutionContext.IsRunMode)
        {
            var mailpit = applicationBuilder.AddMailPit(Components.MailPit, smtpPort: 587);
            builder.WithReference(mailpit).WaitFor(mailpit);
            mailpit.WithParentRelationship(builder);
        }
        else
        {
            builder
                .WithEnvironment(
                    "SendGrid__ApiKey",
                    applicationBuilder.AddParameter("api-key", true)
                )
                .WithEnvironment(
                    "SendGrid__SenderEmail",
                    applicationBuilder.AddParameter("sender-email", true)
                )
                .WithEnvironment(
                    "SendGrid__SenderName",
                    applicationBuilder.AddParameter("sender-name", true)
                );
        }

        return builder;
    }
}
