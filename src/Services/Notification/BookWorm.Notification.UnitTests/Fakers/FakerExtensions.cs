using BookWorm.Contracts;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Domain.Settings;

namespace BookWorm.Notification.UnitTests.Fakers;

public static class FakerExtensions
{
    public static EmailOptions WithInvalidEmail(this Faker<EmailOptions> faker)
    {
        return faker.RuleFor(x => x.From, "invalid-email").Generate();
    }

    public static EmailOptions WithEmptyEmail(this Faker<EmailOptions> faker)
    {
        return faker.RuleFor(x => x.From, string.Empty).Generate();
    }

    public static SendGridOptions WithInvalidSenderEmail(this Faker<SendGridOptions> faker)
    {
        return faker.RuleFor(x => x.SenderEmail, "invalid-email").Generate();
    }

    public static SendGridOptions WithEmptyApiKey(this Faker<SendGridOptions> faker)
    {
        return faker.RuleFor(x => x.ApiKey, string.Empty).Generate();
    }

    public static SendGridOptions WithEmptySenderName(this Faker<SendGridOptions> faker)
    {
        return faker.RuleFor(x => x.SenderName, string.Empty).Generate();
    }

    public static JobOptions WithInvalidCronExpressions(this Faker<JobOptions> faker)
    {
        return faker
            .RuleFor(x => x.CleanUpSentEmailCronExpression, "invalid-cron")
            .RuleFor(x => x.ResendErrorEmailCronExpression, "invalid-cron")
            .Generate();
    }

    public static Outbox AsSent(this Faker<Outbox> faker)
    {
        var outbox = faker.Generate();
        outbox.MarkAsSent();
        return outbox;
    }

    public static Order WithStatus(this Faker<Order> faker, Status status)
    {
        return faker.RuleFor(x => x.Status, status).Generate();
    }

    public static PlaceOrderCommand WithNullEmail(this Faker<PlaceOrderCommand> faker)
    {
        return new Faker<PlaceOrderCommand>()
            .CustomInstantiator(f =>
                new(
                    f.Random.Guid(), // BasketId
                    f.Person.FullName,
                    null!, // Email
                    f.Random.Guid(), // OrderId
                    f.Random.Decimal(10, 1000)
                )
            )
            .Generate();
    }

    public static PlaceOrderCommand WithEmptyEmailAddress(this Faker<PlaceOrderCommand> faker)
    {
        return new Faker<PlaceOrderCommand>()
            .CustomInstantiator(f =>
                new(
                    f.Random.Guid(), // BasketId
                    f.Person.FullName,
                    string.Empty, // Email
                    f.Random.Guid(), // OrderId
                    f.Random.Decimal(10, 1000)
                )
            )
            .Generate();
    }
}
