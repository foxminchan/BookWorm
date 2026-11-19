namespace BookWorm.McpTools.Models;

[ValueObject<Guid>(Conversions.SystemTextJson)]
public readonly partial record struct AuthorId
{
    private static Validation Validate(Guid value)
    {
        return value == Guid.Empty
            ? Validation.Invalid("AuthorId cannot be empty.")
            : Validation.Ok;
    }
}
