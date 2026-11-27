<Columns cols={2}>

<Column>

## 1. List Active English Recognizers

An internal diagnostic check queries the **Presidio Analyzer** sidecar (`GET /recognizers`) for active English (`en`) PII recognizers.

### Payload details

- Query parameter: Optional `language` ISO 639-1 code (for example, `"en"`).
- Response body: JSON array of recognizer class names (`string[]`).

### Example response

```json
[
	"SpacyRecognizer",
	"EmailRecognizer",
	"PhoneRecognizer",
	"CreditCardRecognizer",
	"IpRecognizer",
	"UrlRecognizer"
]
```

### Using this example

Returns `200 OK` with a JSON array of active recognizer names configured for the requested language.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IPresidioAnalyzerMetadata
{
    [Get("/recognizers")]
    Task<string[]> GetRecognizersAsync(
        [Query] string? language = "en",
        CancellationToken cancellationToken = default
    );
}

// DI registration following BookWorm.Chassis PresidioExtensions pattern
builder.Services
    .AddRefitGeneratedClient<IPresidioAnalyzerMetadata>(
        settings,
        Components.Presidio.Analyzer
    )
    .ConfigureHttpClient(client => client.BaseAddress = new(analyzerConnectionString))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
string[] recognizers = await analyzerMetadata.GetRecognizersAsync("en", cancellationToken);
```

</CodeGroup>

</Column>

</Columns>
