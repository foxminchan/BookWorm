namespace BookWorm.Catalog.Features.Books.Shared;

public sealed class ImageValidator : AbstractValidator<IFormFile>
{
    private const int MaxFileSize = 1048576;

    public ImageValidator()
    {
        RuleFor(x => x.Length)
            .LessThanOrEqualTo(MaxFileSize)
            .WithMessage($"The file size should not exceed {MaxFileSize / 1024} KB.");

        RuleFor(x => x.ContentType)
            .Must(x => x is MediaTypeNames.Image.Jpeg or MediaTypeNames.Image.Png)
            .WithMessage("File type is not allowed. Allowed file types are JPEG and PNG.");
    }
}
