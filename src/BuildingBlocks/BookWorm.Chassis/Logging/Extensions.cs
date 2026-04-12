using System.Text;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.Logging;

public static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Configures log redaction services for the host builder.
        /// </summary>
        /// <remarks>
        ///     This method enables log redaction, applies <see cref="AsteriskRedactor" /> for sensitive data,
        ///     and configures HMAC-based redaction for PII using the configured `HMAC:Key` value.
        ///     The key is UTF-8 encoded and converted to Base64 because the HMAC redactor expects a Base64 key.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the `HMAC:Key` configuration value is missing or empty.
        /// </exception>
        public void AddRedaction()
        {
            var keyString = builder
                .Configuration.GetRequiredSection("HMAC")
                .GetValue<string>("Key")
                ?.Trim();

            if (string.IsNullOrEmpty(keyString))
            {
                throw new InvalidOperationException("HMAC key configuration is missing or empty");
            }

            // Convert configured key material to Base64 for HMAC redactor configuration.
            var keyBytes = Encoding.UTF8.GetBytes(keyString);
            var base64Key = Convert.ToBase64String(keyBytes);

            // Enable framework-level redaction support in logging.
            builder.Logging.EnableRedaction();

            // Register redactors by data classification.
            builder.Services.AddRedaction(x =>
            {
                // Mask sensitive fields with asterisks.
                x.SetRedactor<AsteriskRedactor>(
                    new DataClassificationSet(DataTaxonomy.SensitiveData)
                );

                // Apply deterministic HMAC redaction for PII fields.
                x.SetHmacRedactor(
                    options =>
                    {
                        options.KeyId = 10;
                        options.Key = base64Key;
                    },
                    new DataClassificationSet(DataTaxonomy.PIIData)
                );
            });
        }
    }
}
