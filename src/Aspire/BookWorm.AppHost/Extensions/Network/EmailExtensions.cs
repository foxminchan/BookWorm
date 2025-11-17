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
            var mailpit = applicationBuilder
                .AddMailPit(Components.MailPit, smtpPort: 587)
                .WithIconName("Mail");

            builder.WithReference(mailpit).WaitFor(mailpit);
            mailpit.WithParentRelationship(builder);
        }
        else
        {
            var apiKey = applicationBuilder
                .AddParameter("sendgrid-api-key", true)
                .WithDescription(ParameterDescriptions.SendGrid.ApiKey, true)
                .WithCustomInput(_ =>
                    new()
                    {
                        Name = "SendGridApiKeyParameter",
                        Label = "SendGrid API Key",
                        InputType = InputType.SecretText,
                        Description = "Enter your SendGrid API key here",
                    }
                );

            var email = applicationBuilder
                .AddParameter("sendgrid-email", true)
                .WithDescription(ParameterDescriptions.SendGrid.SenderEmail, true)
                .WithCustomInput(_ =>
                    new()
                    {
                        Name = "SendGridSenderEmailParameter",
                        Label = "Sender Email",
                        InputType = InputType.Text,
                        Value = "noreply@yourdomain.com",
                        Description = "Enter the sender email address",
                    }
                );

            var senderName = applicationBuilder
                .AddParameter("sendgrid-sender-name", true)
                .WithDescription(ParameterDescriptions.SendGrid.SenderName, true)
                .WithCustomInput(_ =>
                    new()
                    {
                        Name = "SenderNameParameter",
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
