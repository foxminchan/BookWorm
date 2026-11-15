namespace BookWorm.Constants.Aspire;

public static class ParameterDescriptions
{
    public static class Email
    {
        public const string SmtpEndpoint = """
            SMTP server endpoint used to deliver outgoing email.

            💡 Example: SendGrid uses 'smtp://smtp.sendgrid.net:587'.
            📋 Use the endpoint and port provided by your SMTP provider.
            ⚠️  Use an isolated test SMTP endpoint in staging to avoid delivering real email.
            """;

        public const string SmtpUsername = """
            Username used to authenticate with the configured SMTP server.

            💡 For SendGrid SMTP, use 'apikey'. Other SMTP providers supply their own username.
            """;

        public const string SmtpPassword = """
            🔐 Password or credential for the configured SMTP server.

            🔒 Keep this secret out of source control and use secure secret storage in production.
            """;

        public const string SenderEmail = """
            📧 Sender email address for outgoing email.
            The provider may require this address or its domain to be verified.

            📋 Example: 'notifications@yourdomain.com'
            """;

        public const string SenderName = """
            👤 Sender name for outgoing emails.
            This is the name that will appear in the 'From' field of the email.

            💡 Best practices:
            - Use a recognizable brand or application name
            - Keep it concise and professional
            - Avoid special characters or excessive length

            📋 Examples: 'BookWorm App', 'BookWorm Notifications', or 'Your Company Name'
            """;
    }
}
