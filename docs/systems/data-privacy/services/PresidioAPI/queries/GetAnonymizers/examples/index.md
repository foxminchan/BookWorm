<Columns cols={2}>

<Column>

## 1. List Built-In Anonymizer Operators

An internal diagnostic or configuration check queries the **Presidio Anonymizer** sidecar (`GET /anonymizers`) to verify supported transformation operators.

### Payload details

- Response body: JSON array of built-in anonymizer operator names (`string[]`).

### Example response

```json
["replace", "redact", "mask", "hash", "encrypt", "keep"]
```

### Using this example

Returns `200 OK` with a JSON array of supported anonymizer operator names.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Refit;

public interface IPresidioAnonymizerMetadata
{
    [Get("/anonymizers")]
    Task<string[]> GetAnonymizersAsync(CancellationToken cancellationToken = default);
}

// DI registration following BookWorm.Chassis PresidioExtensions pattern
builder.Services
    .AddRefitGeneratedClient<IPresidioAnonymizerMetadata>(
        settings,
        Components.Presidio.Anonymizer
    )
    .ConfigureHttpClient(client => client.BaseAddress = new(anonymizerConnectionString))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }
    );

// Typed invocation
string[] operators = await anonymizerMetadata.GetAnonymizersAsync(cancellationToken);
```

</CodeGroup>

</Column>

</Columns>
