namespace BookWorm.Rating.Features.Create;

internal sealed class CreateFeedbackValidator : AbstractValidator<CreateFeedbackCommand>
{
    public CreateFeedbackValidator()
    {
        RuleFor(x => x.BookId).NotEmpty();

        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(DataSchemaLength.Medium);

        RuleFor(x => x.LastName).NotEmpty().MaximumLength(DataSchemaLength.Medium);

        RuleFor(x => x.Comment).MaximumLength(DataSchemaLength.Max);

        RuleFor(x => x.Rating).InclusiveBetween(0, 5);
    }
}
