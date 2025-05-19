using BookWorm.Constants.Core;

namespace BookWorm.Rating.Features.Create;

public sealed class CreateFeedbackValidator : AbstractValidator<CreateFeedbackCommand>
{
    public CreateFeedbackValidator()
    {
        RuleFor(x => x.BookId).NotEmpty();

        RuleFor(x => x.FirstName).NotEmpty();

        RuleFor(x => x.LastName).NotEmpty();

        RuleFor(x => x.Comment).MaximumLength(DataSchemaLength.Max);

        RuleFor(x => x.Rating).InclusiveBetween(0, 5);
    }
}
