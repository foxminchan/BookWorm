using BookWorm.Shared.Constants;
using FluentValidation;

namespace BookWorm.Catalog.Features.Authors.Create;

public sealed class CreateAuthorValidator : AbstractValidator<CreateAuthorCommand>
{
    public CreateAuthorValidator() =>
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(DataSchemaLength.Large);
}
