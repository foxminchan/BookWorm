using Aspire.Hosting.Dotnet;

namespace BookWorm.AppHost.Extensions.Network;

internal static class EmailExtensions
{
    extension(IResourceBuilder<DotnetProjectResource> builder)
    {
        public IResourceBuilder<DotnetProjectResource> WithEmailProvider()
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
                var smtpEndpoint = applicationBuilder
                    .AddParameter("email-smtp-endpoint")
                    .WithDescription(ParameterDescriptions.Email.SmtpEndpoint)
                    .WithCustomInput(_ =>
                        new()
                        {
                            Name = "EmailSmtpEndpointParameter",
                            Label = "SMTP Endpoint",
                            InputType = InputType.Text,
                            Value = "smtp://smtp.sendgrid.net:587",
                            Description = "SMTP endpoint for the email provider",
                        }
                    );

                var smtpUsername = applicationBuilder
                    .AddParameter("email-smtp-username")
                    .WithDescription(ParameterDescriptions.Email.SmtpUsername)
                    .WithCustomInput(_ =>
                        new()
                        {
                            Name = "EmailSmtpUsernameParameter",
                            Label = "SMTP Username",
                            InputType = InputType.Text,
                            Value = "apikey",
                            Description = "Username for SMTP authentication",
                        }
                    );

                var smtpPassword = applicationBuilder
                    .AddParameter("email-smtp-password", true)
                    .WithDescription(ParameterDescriptions.Email.SmtpPassword, true)
                    .WithCustomInput(_ =>
                        new()
                        {
                            Name = "EmailSmtpPasswordParameter",
                            Label = "SMTP Password",
                            InputType = InputType.SecretText,
                            Description = "Password or credential for SMTP authentication",
                        }
                    );

                var email = applicationBuilder
                    .AddParameter("email-sender-address", true)
                    .WithDescription(ParameterDescriptions.Email.SenderEmail, true)
                    .WithCustomInput(_ =>
                        new()
                        {
                            Name = "EmailSenderAddressParameter",
                            Label = "Sender Email",
                            InputType = InputType.Text,
                            Value = "noreply@yourdomain.com",
                            Description = "Enter the sender email address",
                        }
                    );

                var senderName = applicationBuilder
                    .AddParameter("email-sender-name", true)
                    .WithDescription(ParameterDescriptions.Email.SenderName, true)
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
                    .WithEnvironment("Email__Endpoint", smtpEndpoint)
                    .WithEnvironment("Email__Username", smtpUsername)
                    .WithEnvironment("Email__Password", smtpPassword)
                    .WithEnvironment("Email__From", email)
                    .WithEnvironment("Email__Name", senderName);

                smtpEndpoint.WithParentRelationship(builder);
                smtpUsername.WithParentRelationship(builder);
                smtpPassword.WithParentRelationship(builder);
                email.WithParentRelationship(builder);
                senderName.WithParentRelationship(builder);
            }

            return builder;
        }
    }
}
