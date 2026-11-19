namespace BookWorm.McpTools.Models;

[ValueObject<Guid>(Conversions.SystemTextJson)]
public readonly partial record struct PublisherId
{
    private static Validation Validate(Guid value)
    {
        return value == Guid.Empty
            ? Validation.Invalid("PublisherId cannot be empty.")
            : Validation.Ok;
    }
}
