using FluentValidation;

namespace BookWorm.Catalog.Infrastructure.Blob;

public sealed class AzuriteOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}

public sealed class AzuriteOptionsValidator : AbstractValidator<AzuriteOptions>
{
    public AzuriteOptionsValidator()
    {
        RuleFor(x => x.ConnectionString).NotEmpty();
        RuleFor(x => x.ContainerName).NotEmpty();
    }
}
