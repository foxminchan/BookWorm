namespace BookWorm.Chat.Features.Update;

public sealed class UpdateChatValidator : AbstractValidator<UpdateChatCommand>
{
    public UpdateChatValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Prompt.Text).NotEmpty().MaximumLength(DataSchemaLength.Max);
    }
}
