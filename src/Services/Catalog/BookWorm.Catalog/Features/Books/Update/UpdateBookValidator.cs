namespace BookWorm.Catalog.Features.Books.Update;

internal sealed class UpdateBookValidator : BookValidatorBase<UpdateBookCommand>
{
    public UpdateBookValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
