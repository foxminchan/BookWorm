namespace BookWorm.Catalog.Features.Books;

internal static class ImageRules
{
    private const int MaxFileSize = 1048576;

    public static void ApplyImageRules<T>(this IRuleBuilder<T, IFormFile?> ruleBuilder)
    {
        ruleBuilder
            .NotNull()
            .Must(x => x!.Length <= MaxFileSize)
            .WithMessage($"The file size should not exceed {MaxFileSize / 1024} KB.")
            .Must(x => x!.ContentType is MediaTypeNames.Image.Jpeg or MediaTypeNames.Image.Png)
            .WithMessage("File type is not allowed. Allowed file types are JPEG and PNG.");
    }
}
