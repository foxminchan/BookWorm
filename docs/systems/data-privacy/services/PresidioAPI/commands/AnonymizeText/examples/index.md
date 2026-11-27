<Columns cols={2}>

<Column>

## 1. Redact Detected PII Entities with `<PII>` via Refit

`PresidioService.AnonymizeAsync` forwards the original text, `AnalyzerResponse[]` detections, and `DEFAULT` `ReplaceAnonymizer("<PII>")` configuration to `IPresidioAnonymizer.AnonymizeAsync`.

### Payload details

- `text`: Original text string containing PII spans.
- `analyzer_results`: Array of detections returned by `IPresidioAnalyzer.AnalyzeAsync`.
- `anonymizers`: Dictionary mapping `"DEFAULT"` or entity names to anonymizer operators (`replace`, `redact`, `mask`, `hash`, or `encrypt`).

### Example payload

```json
{
	"text": "Please contact Minh Nguyen at minh.nguyen@bookworm.dev.",
	"analyzer_results": [
		{
			"entity_type": "PERSON",
			"start": 15,
			"end": 26,
			"score": 0.85
		},
		{
			"entity_type": "EMAIL_ADDRESS",
			"start": 30,
			"end": 54,
			"score": 0.99
		}
	],
	"anonymizers": {
		"DEFAULT": {
			"type": "replace",
			"new_value": "<PII>"
		}
	}
}
```

### Using this example

Returns `200 OK` with `AnonymizerResponse` (`"text": "Please contact <PII> at <PII>."`).

</Column>

<Column>

<CodeGroup dropdown>

```csharp RefitClient.cs
using System.Collections.Frozen;
using BookWorm.Chassis.AI.Presidio;
using BookWorm.Chassis.AI.Presidio.Clients;

// 1. DI registration via BookWorm.Chassis (PresidioExtensions.AddPresidio)
builder.AddPresidio();

// 2. Typed Refit invocation (IPresidioAnonymizer) matching PresidioService
var anonymizers = new Dictionary<string, ReplaceAnonymizer>
{
    ["DEFAULT"] = new("<PII>"),
}.ToFrozenDictionary();

var request = new AnonymizerRequest(rawText, analyzerResults, anonymizers);
AnonymizerResponse result = await anonymizer.AnonymizeAsync(request, cancellationToken);

// 3. Or invoke the combined BookWorm.Chassis facade directly
string sanitized = await presidioService.AnonymizeAsync(rawText, cancellationToken);
```

</CodeGroup>

</Column>

</Columns>
