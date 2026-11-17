namespace BookWorm.McpTools.Models;

[ValueObject<Guid>(Conversions.SystemTextJson)]
public readonly partial record struct CategoryId
{
    private static Validation Validate(Guid value)
    {
        return value == Guid.Empty
            ? Validation.Invalid("CategoryId cannot be empty.")
            : Validation.Ok;
    }
}
