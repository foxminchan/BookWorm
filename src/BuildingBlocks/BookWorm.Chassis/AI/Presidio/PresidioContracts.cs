using System.Text.Json.Serialization;

namespace BookWorm.Chassis.AI.Presidio;

/// <summary>
///     Request payload for the Presidio Analyzer API.
/// </summary>
/// <param name="Text">The text to analyze for PII entities.</param>
/// <param name="Language">The language of the text (default: "en").</param>
public sealed record AnalyzerRequest(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("language")] string Language = "en"
);

/// <summary>
///     Response from the Presidio Analyzer API representing a detected PII entity.
/// </summary>
/// <param name="Start">The start index of the PII entity in the text.</param>
/// <param name="End">The end index of the PII entity in the text.</param>
/// <param name="Score">The confidence score of the detection.</param>
/// <param name="EntityType">The type of PII entity detected.</param>
public sealed record AnalyzerResponse(
    [property: JsonPropertyName("start")] int Start,
    [property: JsonPropertyName("end")] int End,
    [property: JsonPropertyName("score")] float Score,
    [property: JsonPropertyName("entity_type")] string EntityType
);

/// <summary>
///     Request payload for the Presidio Anonymizer API.
/// </summary>
/// <param name="Text">The original text containing PII.</param>
/// <param name="AnalyzerResults">The analyzer results identifying PII locations.</param>
public sealed record AnonymizerRequest(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("analyzer_results")] AnalyzerResponse[] AnalyzerResults
);

/// <summary>
///     Response from the Presidio Anonymizer API containing the anonymized text.
/// </summary>
/// <param name="Text">The text with PII entities anonymized.</param>
public sealed record AnonymizerResponse([property: JsonPropertyName("text")] string Text);
