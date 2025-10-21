using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace BookWorm.McpTools.Options;

public sealed partial class ServerInfoOptions : IValidateOptions<ServerInfoOptions>
{
    public const string ConfigurationSection = "ServerInfo";

    [Required]
    public required string Name { get; init; } = Constants.Aspire.Services.McpTools;

    [Required]
    [RegularExpression(
        @"^(\d+\.)?(\d+\.)?(\*|\d+)$",
        ErrorMessage = "Version must be in format 'major[.minor[.patch]]' (e.g., 1, 1.0, or 1.0.0)."
    )]
    public required string Version { get; init; }

    public string? Title { get; set; }

    [Url]
    public string? WebsiteUrl { get; set; }

    [ValidateEnumeratedItems]
    public IReadOnlyList<Icon>? Icons { get; set; }

    public sealed class Icon
    {
        [Url]
        [Required]
        public required string Src { get; set; }

        public IReadOnlyList<string>? Sizes { get; set; }

        public string? MimeType { get; set; }
    }

    public ValidateOptionsResult Validate(string? name, ServerInfoOptions options)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(options);

        if (!Validator.TryValidateObject(options, context, results, validateAllProperties: true))
        {
            var errors = results.Select(r => r.ErrorMessage).Where(e => e is not null);
            return ValidateOptionsResult.Fail(errors!);
        }

        if (options.Icons is null)
        {
            return ValidateOptionsResult.Success;
        }

        foreach (var icon in options.Icons)
        {
            if (icon.Sizes is null)
            {
                continue;
            }

            foreach (var size in icon.Sizes.Where(size => !IconSizeRegex().IsMatch(size)))
            {
                return ValidateOptionsResult.Fail(
                    $"Size '{size}' must be in the format 'WxH' (e.g., '64x64') or 'any'."
                );
            }
        }

        return ValidateOptionsResult.Success;
    }

    [GeneratedRegex(@"^(\d+x\d+|any)$")]
    private static partial Regex IconSizeRegex();
}
