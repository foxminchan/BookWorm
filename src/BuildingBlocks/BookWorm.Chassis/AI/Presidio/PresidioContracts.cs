using System.Text.Json.Serialization;

namespace BookWorm.Chassis.AI.Presidio;

public sealed record AnalyzerRequest(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("language")] string Language = "en"
);

public sealed record AnalyzerResponse(
    [property: JsonPropertyName("start")] [property: JsonRequired] int Start,
    [property: JsonPropertyName("end")] [property: JsonRequired] int End,
    [property: JsonPropertyName("score")] [property: JsonRequired] float Score,
    [property: JsonPropertyName("entity_type")] [property: JsonRequired] string EntityType
);

public sealed record AnonymizerRequest(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("analyzer_results")] AnalyzerResponse[] AnalyzerResults
);

public sealed record AnonymizerResponse(
    [property: JsonPropertyName("text")] [property: JsonRequired] string Text
);

[JsonSerializable(typeof(AnalyzerRequest))]
[JsonSerializable(typeof(AnalyzerResponse[]))]
[JsonSerializable(typeof(AnonymizerRequest))]
[JsonSerializable(typeof(AnonymizerResponse))]
internal sealed partial class PresidioSerializationContext : JsonSerializerContext;
