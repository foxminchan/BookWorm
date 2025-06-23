using System.Text;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.Logging;

public static class Extensions
{
    public static void AddRedaction(this IHostApplicationBuilder builder)
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

        builder.Services.AddRedaction(x =>
        {
            x.SetRedactor<AsteriskRedactor>(new DataClassificationSet(DataTaxonomy.SensitiveData));

            x.SetHmacRedactor(
                options =>
                {
                    options.KeyId = 10;
                    options.Key = base64Key;
                },
                new DataClassificationSet(DataTaxonomy.PiiData)
            );
        });
    }
}
