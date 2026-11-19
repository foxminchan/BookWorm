using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Finance.Domain.AggregatesModel.OrderAggregate;

public sealed class CustomerContact(string? fullName, string? email) : ValueObject
{
    public string? FullName { get; } = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim();

    public string? Email { get; } = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

    public bool CanNotify =>
        !string.IsNullOrWhiteSpace(FullName) && !string.IsNullOrWhiteSpace(Email);

    public CustomerContact WithEmail(string? email)
    {
        return new(FullName, email);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FullName;
        yield return Email;
    }
}
