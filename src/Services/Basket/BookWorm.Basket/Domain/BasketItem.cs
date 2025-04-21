using System.ComponentModel.DataAnnotations;

namespace BookWorm.Basket.Domain;

[method: JsonConstructor]
public sealed class BasketItem() : IValidatableObject
{
    public BasketItem(string id, int quantity)
        : this()
    {
        Id = id;
        Quantity = quantity;
    }

    public string? Id { get; private set; }

    public int Quantity { get; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Quantity <= 0)
        {
            yield return new("Quantity must be greater than zero.", [nameof(Quantity)]);
        }
    }
}
