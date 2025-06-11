namespace BookWorm.AppHost.Extensions;

public static class EmailExtensions
{
    /// <summary>
    ///     Configures the email provider for the application based on the execution context.
    /// </summary>
    /// <param name="builder">The resource builder for the project resource.</param>
    /// <returns>The updated resource builder.</returns>
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
