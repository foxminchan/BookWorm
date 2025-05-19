using BookWorm.Constants;
using BookWorm.Constants.Aspire;

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
        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            var mailpit = builder.ApplicationBuilder.AddMailPit(Components.MailPit);
            builder.WithReference(mailpit).WaitFor(mailpit);
            mailpit.WithParentRelationship(builder);
        }
        else
        {
            builder
                .WithEnvironment(
                    "SendGrid__ApiKey",
                    builder.ApplicationBuilder.AddParameter("api-key", true)
                )
                .WithEnvironment(
                    "SendGrid__SenderEmail",
                    builder.ApplicationBuilder.AddParameter("sender-email", true)
                )
                .WithEnvironment(
                    "SendGrid__SenderName",
                    builder.ApplicationBuilder.AddParameter("sender-name", true)
                );
        }

        return builder;
    }
}
