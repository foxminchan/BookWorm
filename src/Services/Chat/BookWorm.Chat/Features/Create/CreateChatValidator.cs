namespace BookWorm.Chat.Features.Create;

public sealed class CreateChatValidator : AbstractValidator<CreateChatCommand>
{
    public CreateChatValidator()
    {
        RuleFor(x => x.Prompt.Text).NotEmpty().MaximumLength(DataSchemaLength.Max);
    }
}
