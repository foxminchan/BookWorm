using System.ComponentModel.DataAnnotations;

namespace BookWorm.ServiceDefaults.Kestrel;

[OptionsValidator]
public sealed partial class CorsSettings : IValidateOptions<CorsSettings>
{
    public const string ConfigurationSection = "Cors";

    [Required]
    [Url]
    public string BackOfficeUrl { get; set; } = string.Empty;

    [Required]
    [Url]
    public string StoreFrontUrl { get; set; } = string.Empty;
}
