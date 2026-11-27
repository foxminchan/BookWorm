<Columns cols={2}>

<Column>

## 1. Detect PII Entities in Customer Input via Refit

**Chat API** or **Rating API** invokes `IPresidioAnalyzer.AnalyzeAsync` through `BookWorm.Chassis` before anonymizing user-supplied text.

### Payload details

- `text`: Input text string (or batch of strings) to scan for PII.
- `language`: Two-character ISO 639-1 language code (`"en"` by default in `AnalyzerRequest`).
- `entities`: Optional allowlist of Presidio entity labels (e.g., `PERSON`, `EMAIL_ADDRESS`).
- `score_threshold`: Optional minimum confidence score (`0.0`–`1.0`).

### Example payload

```json
{
	"text": "Please contact Minh Nguyen at minh.nguyen@bookworm.dev regarding order 01960a1b.",
	"language": "en",
	"entities": ["PERSON", "EMAIL_ADDRESS"],
	"score_threshold": 0.6
}
```

### Using this example

Returns `200 OK` with an array of `AnalyzerResponse` detections containing `entity_type`, `start`, `end`, and `score`.

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using BookWorm.Chassis.AI.Presidio;
using BookWorm.Chassis.AI.Presidio.Clients;

// 1. DI registration via BookWorm.Chassis (PresidioExtensions.AddPresidio)
builder.AddPresidio();

// 2. Typed Refit invocation (IPresidioAnalyzer)
AnalyzerResponse[] detections = await analyzer.AnalyzeAsync(
    new AnalyzerRequest(
        "Please contact Minh Nguyen at minh.nguyen@bookworm.dev regarding order 01960a1b.",
        Language: "en"
    ),
    cancellationToken
);

foreach (var detection in detections)
{
    Console.WriteLine(
        $"{detection.EntityType}: {detection.Start}-{detection.End} (score {detection.Score:F2})"
    );
}
```

</CodeGroup>

</Column>

</Columns>
