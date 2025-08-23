using System.ComponentModel.DataAnnotations;

namespace BookWorm.ServiceDefaults.Kestrel;

[OptionsValidator]
public sealed partial class CorsSettings : IValidateOptions<CorsSettings>
{
    public const string ConfigurationSection = "Cors";

    [Required]
    public IList<string> Origins { get; } = [];

    [Required]
    public IList<string> Headers { get; } = [];

    [Required]
    public IList<string> Methods { get; } = [];

    [Range(0, int.MaxValue)]
    public int? MaxAge { get; set; }

    public bool AllowCredentials { get; set; }
}
