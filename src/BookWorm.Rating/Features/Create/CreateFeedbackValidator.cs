using BookWorm.Constants;

namespace BookWorm.Rating.Features.Create;

internal sealed class CreateFeedbackValidator : AbstractValidator<CreateFeedbackCommand>
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
