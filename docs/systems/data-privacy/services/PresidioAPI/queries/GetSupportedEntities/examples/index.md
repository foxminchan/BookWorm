<Columns cols={2}>

<Column>

## 1. List Detectable PII Entity Types

An internal diagnostic check queries the **Presidio Analyzer** sidecar (`GET /supportedentities`) to inspect the entity labels available for English (`en`).

### Payload details

- Query parameter: Optional `language` ISO 639-1 code (for example, `"en"`).
- Response body: JSON array of Presidio entity type strings (`string[]`).

### Example response

```json
["PERSON", "EMAIL_ADDRESS", "PHONE_NUMBER", "CREDIT_CARD", "IP_ADDRESS", "LOCATION", "DATE_TIME"]
```

### Using this example

Returns `200 OK` with a JSON array of supported PII entity labels for the requested language.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IPresidioAnalyzerMetadata
{
    [Get("/supportedentities")]
    Task<string[]> GetSupportedEntitiesAsync(
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
string[] entities = await analyzerMetadata.GetSupportedEntitiesAsync("en", cancellationToken);
```

</CodeGroup>

</Column>

</Columns>
