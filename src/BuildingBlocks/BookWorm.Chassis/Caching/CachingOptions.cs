using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace BookWorm.Chassis.Caching;

[OptionsValidator]
public sealed partial class CachingOptions : IValidateOptions<CachingOptions>
{
    public const string ConfigurationSection = "Caching";

    [Required]
    [Range(typeof(TimeSpan), "00:00:01", "365.00:00:00")]
    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(30);

    public bool SemanticCacheEnabled { get; set; } = true;

    [Range(0.0, 1.0)]
    public float SemanticCacheSimilarityThreshold { get; set; } = 0.92f;
}
