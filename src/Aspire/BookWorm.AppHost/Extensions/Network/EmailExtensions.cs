namespace BookWorm.AppHost.Extensions.Network;

internal static class EmailExtensions
{
    extension(IResourceBuilder<ProjectResource> builder)
    {
        public IResourceBuilder<ProjectResource> WithEmailProvider()
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

                apiKey.WithParentRelationship(builder);
                email.WithParentRelationship(builder);
                senderName.WithParentRelationship(builder);
            }

            return builder;
        }
    }
}
