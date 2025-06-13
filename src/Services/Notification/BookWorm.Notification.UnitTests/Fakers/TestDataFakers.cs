using BookWorm.Contracts;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Domain.Settings;

namespace BookWorm.Notification.UnitTests.Fakers;

public static class TestDataFakers
{
    public static readonly Faker<EmailOptions> EmailOptions = new Faker<EmailOptions>()
        .RuleFor(x => x.Name, _ => "BookWorm")
        .RuleFor(x => x.From, f => f.Internet.Email());

    public static readonly Faker<SendGridOptions> SendGridOptions = new Faker<SendGridOptions>()
        .RuleFor(x => x.ApiKey, f => f.Random.AlphaNumeric(50))
        .RuleFor(x => x.SenderEmail, f => f.Internet.Email())
        .RuleFor(x => x.SenderName, f => f.Person.FullName);

    public static readonly Faker<JobOptions> JobOptions = new Faker<JobOptions>()
        .RuleFor(x => x.CleanUpSentEmailCronExpression, _ => "0 0 * * *")
        .RuleFor(x => x.ResendErrorEmailCronExpression, _ => "0 12 * * *");

    public static readonly Faker<Outbox> Outbox = new Faker<Outbox>().CustomInstantiator(f =>
        new(f.Person.FullName, f.Internet.Email(), f.Lorem.Sentence(), f.Lorem.Paragraphs(2))
    );

    public static readonly Faker<Order> Order = new Faker<Order>().CustomInstantiator(f =>
        new(f.Random.Guid(), f.Person.FullName, f.Random.Decimal(10, 1000), f.PickRandom<Status>())
    );

    public static readonly Faker<PlaceOrderCommand> PlaceOrderCommand =
        new Faker<PlaceOrderCommand>().CustomInstantiator(f =>
            new(
                f.Random.Guid(), // BasketId
                f.Person.FullName,
                f.Internet.Email(),
                f.Random.Guid(), // OrderId
                f.Random.Decimal(10, 1000)
            )
        );

    public static readonly Faker<CompleteOrderCommand> CompleteOrderCommand =
        new Faker<CompleteOrderCommand>().CustomInstantiator(f =>
            new(
                f.Random.Guid(), // OrderId
                f.Person.FullName,
                f.Internet.Email(),
                f.Random.Decimal(10, 1000)
            )
        );

    public static readonly Faker<CancelOrderCommand> CancelOrderCommand =
        new Faker<CancelOrderCommand>().CustomInstantiator(f =>
            new(
                f.Random.Guid(), // OrderId
                f.Person.FullName,
                f.Internet.Email(),
                f.Random.Decimal(10, 1000)
            )
        );
}
