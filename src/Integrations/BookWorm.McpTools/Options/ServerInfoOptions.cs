using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace BookWorm.McpTools.Options;

[OptionsValidator]
public sealed partial class ServerInfoOptions
    : IValidateOptions<ServerInfoOptions>
{
    public const string ConfigurationSection = "ServerInfo";

    [Required]
    public required string Name { get; init; } = Constants.Aspire.Services.McpTools;

    [Required]
    [RegularExpression(
        @"^(\d+\.)?(\d+\.)?(\*|\d+)$",
        ErrorMessage = "Version must be in the format Semantic Versioning (e.g., 1.0.0, 1.2, 2.0.1)."
    )]
    public required string Version { get; set; }

    public string? Title { get; set; }

    [Url]
    public string? WebsiteUrl { get; set; }

    [ValidateEnumeratedItems]
    public List<Icon>? Icons { get; set; } = [];

    public sealed class Icon
    {
        [Url]
        [Required]
        public required string Src { get; set; }

        [RegularExpression(
            @"^(\d+x\d+|any)$",
            ErrorMessage = "Size must be in the format 'WxH' (e.g., '64x64') or 'any'."
        )]
        public List<string>? Sizes { get; set; }

        public string? MimeType { get; set; }
    }
}
