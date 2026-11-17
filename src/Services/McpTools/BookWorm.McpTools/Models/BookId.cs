namespace BookWorm.McpTools.Models;

[ValueObject<Guid>(Conversions.SystemTextJson)]
public readonly partial record struct BookId
{
    private static Validation Validate(Guid value)
    {
        return value == Guid.Empty ? Validation.Invalid("BookId cannot be empty.") : Validation.Ok;
    }
}
