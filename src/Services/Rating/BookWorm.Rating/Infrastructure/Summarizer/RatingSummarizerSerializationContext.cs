using System.Text.Json.Serialization;
using BookWorm.Rating.Infrastructure.Summarizer.Models;

namespace BookWorm.Rating.Infrastructure.Summarizer;

[JsonSerializable(typeof(FeedbackReview[]))]
[JsonSerializable(typeof(RatingSummaryOutput))]
[JsonSerializable(typeof(QualityAssessmentOutput))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
internal sealed partial class RatingSummarizerSerializationContext : JsonSerializerContext;
