using BookWorm.Contracts;
using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.UnitTests.Fakers;

public static class FakerExtensions
{
    public static Outbox AsSent(this Faker<Outbox> faker)
    {
        var outbox = faker.Generate();
        outbox.MarkAsSent();
        return outbox;
    }

    public static PlaceOrderCommand WithNullEmail()
    {
        return new Faker<PlaceOrderCommand>()
            .CustomInstantiator(f =>
                new(
                    f.Random.Guid(),
                    f.Person.FullName,
                    null!,
                    f.Random.Guid(),
                    f.Random.Decimal(10, 1000)
                )
            )
            .Generate();
    }

    public static PlaceOrderCommand WithEmptyEmailAddress()
    {
        return new Faker<PlaceOrderCommand>()
            .CustomInstantiator(f =>
                new(
                    f.Random.Guid(),
                    f.Person.FullName,
                    string.Empty,
                    f.Random.Guid(),
                    f.Random.Decimal(10, 1000)
                )
            )
            .Generate();
    }
}
