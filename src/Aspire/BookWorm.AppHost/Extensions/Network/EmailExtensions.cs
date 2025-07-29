namespace BookWorm.AppHost.Extensions.Network;

public static class EmailExtensions
{
    /// <summary>
    ///     Configures the email provider for the application based on the execution context.
    /// </summary>
    /// <param name="builder">The resource builder for the project resource.</param>
    /// <returns>The updated resource builder with email provider configuration applied.</returns>
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
            var apiKey = applicationBuilder
                .AddParameter("api-key", true)
                .WithDescription(
                    """
                    SendGrid API key for authentication.
                    You can obtain it from your SendGrid account.

                    - For more information, visit: https://app.sendgrid.com/settings/api_keys
                    - To create a new API key, go to: https://app.sendgrid.com/settings/api_keys/create
                    - Ensure the API key has 'Mail Send' permissions.
                    """,
                    true
                )
                .WithCustomInput(_ =>
                    new()
                    {
                        Label = "SendGrid API Key",
                        InputType = InputType.SecretText,
                        Description = "Enter your SendGrid API key here",
                    }
                );

            var email = applicationBuilder
                .AddParameter("email")
                .WithDescription(
                    """
                    Sender email address for outgoing emails.
                    This should be a verified email address in your SendGrid account.

                    - For more information, visit: https://app.sendgrid.com/settings/sender_auth
                    - To verify a new sender email, go to: https://app.sendgrid.com/settings/sender_auth/single_sender
                    """,
                    true
                )
                .WithCustomInput(_ =>
                    new()
                    {
                        Label = "Sender Email",
                        InputType = InputType.Text,
                        Value = "noreply@yourdomain.com",
                        Description = "Enter the sender email address",
                    }
                );

            var senderName = applicationBuilder
                .AddParameter("sender-name")
                .WithDescription(
                    """
                    Sender name for outgoing emails.
                    This is the name that will appear in the 'From' field of the email.

                    - For more information, visit: https://app.sendgrid.com/settings/sender_auth
                    """,
                    true
                )
                .WithCustomInput(_ =>
                    new()
                    {
                        Label = "Sender Name",
                        InputType = InputType.Text,
                        Value = "BookWorm Support",
                        Description = "Enter the sender name",
                    }
                );

            builder
                .WithEnvironment("SendGrid__ApiKey", apiKey)
                .WithEnvironment("SendGrid__SenderEmail", email)
                .WithEnvironment("SendGrid__SenderName", senderName);
        }

        return builder;
    }
}
