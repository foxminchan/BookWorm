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

            var keyBytes = Encoding.UTF8.GetBytes(keyString);
            var base64Key = Convert.ToBase64String(keyBytes);

            builder.Logging.EnableRedaction();

            builder.Services.AddRedaction(x =>
            {
                x.SetRedactor<AsteriskRedactor>(
                    new DataClassificationSet(DataTaxonomy.SensitiveData)
                );

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
