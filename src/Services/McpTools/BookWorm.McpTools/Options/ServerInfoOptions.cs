using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using BookWorm.Constants.Aspire;
using Microsoft.Extensions.Options;

namespace BookWorm.McpTools.Options;

internal sealed partial class ServerInfoOptions : IValidateOptions<ServerInfoOptions>
{
    public const string ConfigurationSection = "ServerInfo";

    public static string Name => Services.McpTools;

    [Required]
    [RegularExpression(
        @"^(\d+\.)?(\d+\.)?(\*|\d+)$",
        ErrorMessage = "Version must be in format 'major[.minor[.patch]]' (e.g., 1, 1.0, 1.0.0, or 1.2.*)."
    )]
    public required string Version { get; init; }

    public string? Title { get; set; }

    [Url]
    public string? WebsiteUrl { get; set; }

    [ValidateEnumeratedItems]
    public IReadOnlyList<Icon>? Icons { get; set; }

    public ValidateOptionsResult Validate(string? name, ServerInfoOptions options)
    {
        if (!TryValidate(options, out var failure))
        {
            return failure;
        }

        if (options.Icons is null)
        {
            return ValidateOptionsResult.Success;
        }

        foreach (var icon in options.Icons)
        {
            if (!TryValidate(icon, out var iconFailure))
            {
                return iconFailure;
            }

            var invalidSize = icon.Sizes?.FirstOrDefault(size => !IconSizeRegex().IsMatch(size));

            if (invalidSize is not null)
            {
                return ValidateOptionsResult.Fail(
                    $"Size '{invalidSize}' must be in the format 'WxH' (e.g., '64x64') or 'any'."
                );
            }
        }

        return ValidateOptionsResult.Success;
    }

    private static bool TryValidate(
        object instance,
        [NotNullWhen(false)] out ValidateOptionsResult? failure
    )
    {
        List<ValidationResult> results = [];
        if (Validator.TryValidateObject(instance, new(instance), results, true))
        {
            failure = null;
            return true;
        }

        failure = ValidateOptionsResult.Fail(
            results.Where(r => !string.IsNullOrEmpty(r.ErrorMessage)).Select(r => r.ErrorMessage!)
        );
        return false;
    }

    [GeneratedRegex(@"^(\d+x\d+|any)$")]
    private static partial Regex IconSizeRegex();

    public sealed class Icon
    {
        [Url]
        [Required]
        public required string Src { get; set; }

        public IReadOnlyList<string>? Sizes { get; set; }

        public string? MimeType { get; set; }
    }
}
