namespace BookWorm.Chat.Features.List;

public sealed class ListChatValidator : AbstractValidator<ListChatQuery>
{
    public ListChatValidator()
    {
        RuleFor(x => x.Name).MaximumLength(DataSchemaLength.Large);
    }
}
