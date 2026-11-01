using System.Text.Json.Serialization;

namespace BookWorm.Rating.Infrastructure.Summarizer;

[JsonSerializable(typeof(FeedbackReview[]))]
[JsonSerializable(typeof(PartialRatingSummary[]))]
[JsonSerializable(typeof(RatingSummaryOutput))]
[JsonSerializable(typeof(RatingSummaryOutput[]))]
[JsonSerializable(typeof(QualityAssessmentOutput))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
internal sealed partial class RatingSummarizerSerializationContext : JsonSerializerContext;
