using System.Collections.Frozen;

namespace BookWorm.Catalog.Features.Books;

internal static class ImageRules
{
    private const int MaxFileSize = 1048576;

    private static readonly FrozenDictionary<string, FrozenSet<string>> _allowedTypes =
        new Dictionary<string, FrozenSet<string>>
        {
            [MediaTypeNames.Image.Jpeg] = new[] { ".jpg", ".jpeg" }.ToFrozenSet(
                StringComparer.OrdinalIgnoreCase
            ),
            [MediaTypeNames.Image.Png] = new[] { ".png" }.ToFrozenSet(
                StringComparer.OrdinalIgnoreCase
            ),
            [MediaTypeNames.Image.Webp] = new[] { ".webp" }.ToFrozenSet(
                StringComparer.OrdinalIgnoreCase
            ),
        }.ToFrozenDictionary();

    public static void ApplyImageRules<T>(this IRuleBuilderInitial<T, IFormFile?> ruleBuilder)
    {
        ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(x => x?.Length > 0)
            .WithMessage("The file must not be empty.")
            .Must(x => x?.Length <= MaxFileSize)
            .WithMessage($"The file size should not exceed {MaxFileSize / 1024} KB.")
            .Must(x => x?.ContentType is not null && _allowedTypes.ContainsKey(x.ContentType))
            .WithMessage("File type is not allowed. Allowed file types are JPEG, PNG, and WebP.")
            .Must(x =>
            {
                var ext = Path.GetExtension(x!.FileName);

                return _allowedTypes.TryGetValue(x.ContentType, out var extensions)
                    && extensions.Contains(ext);
            })
            .WithMessage("The file extension does not match its content type.");
    }
}
