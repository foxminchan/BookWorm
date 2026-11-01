using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace BookWorm.Rating.Infrastructure.Summarizer;

internal enum RatingConfidenceLevel
{
    Unknown = -1,
    Low = 0,
    Medium = 1,
    High = 2,
}

[OptionsValidator]
internal sealed partial class RatingSummarizerOptions : IValidateOptions<RatingSummarizerOptions>
{
    public const string ConfigurationSection = "RatingSummarizer";

    [Range(1, int.MaxValue, ErrorMessage = nameof(BatchSize) + " must be greater than zero.")]
    public int BatchSize { get; set; } = 20;

    [Range(1, int.MaxValue, ErrorMessage = nameof(MaxConcurrency) + " must be greater than zero.")]
    public int MaxConcurrency { get; set; } = 4;

    [Range(0, int.MaxValue, ErrorMessage = nameof(MaxIterations) + " cannot be negative.")]
    public int MaxIterations { get; set; } = 2;

    [AllowedValues(
        RatingConfidenceLevel.Low,
        RatingConfidenceLevel.Medium,
        RatingConfidenceLevel.High,
        ErrorMessage = nameof(MinimumConfidence) + " must be Low, Medium, or High."
    )]
    public RatingConfidenceLevel MinimumConfidence { get; set; } = RatingConfidenceLevel.Medium;
}
