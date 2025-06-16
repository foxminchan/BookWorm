using BookWorm.Contracts;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Infrastructure.Senders.MailKit;

namespace BookWorm.Notification.UnitTests.Fakers;

public static class TestDataFakers
{
    public static readonly Faker<MailKitSettings> EmailOptions = new Faker<MailKitSettings>()
        .RuleFor(x => x.Name, _ => nameof(BookWorm))
        .RuleFor(x => x.From, f => f.Internet.Email());

    public static readonly Faker<Outbox> Outbox = new Faker<Outbox>().CustomInstantiator(f =>
        new(f.Person.FullName, f.Internet.Email(), f.Lorem.Sentence(), f.Lorem.Paragraphs(2))
    );

    public static readonly Faker<PlaceOrderCommand> PlaceOrderCommand =
        new Faker<PlaceOrderCommand>().CustomInstantiator(f =>
            new(
                f.Random.Guid(),
                f.Person.FullName,
                f.Internet.Email(),
                f.Random.Guid(),
                f.Random.Decimal(10, 1000)
            )
        );
}
