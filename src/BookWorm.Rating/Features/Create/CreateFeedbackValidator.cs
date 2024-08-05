using BookWorm.Shared.Constants;
using FluentValidation;

namespace BookWorm.Rating.Features.Create;

public sealed class CreateFeedbackValidator : AbstractValidator<CreateFeedbackCommand>
{
    public CreateFeedbackValidator()
    {
        RuleFor(x => x.BookId)
            .NotEmpty();

        RuleFor(x => x.Rating)
            .InclusiveBetween(0, 5);

        RuleFor(x => x.Comment)
            .MaximumLength(DataSchemaLength.Max);
    }
}
