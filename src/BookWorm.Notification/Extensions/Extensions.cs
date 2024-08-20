using BookWorm.Constants;
using BookWorm.Notification.OpenTelemetry;

namespace BookWorm.Notification.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.AddRabbitMqEventBus(typeof(Program));

        var emailConn = new UriBuilder(builder.Configuration.GetConnectionString(ServiceName.Mail) ??
                                       throw new InvalidOperationException());

        var defaultFromEmail = builder.Configuration["Smtp:Email"];

        builder.Services.AddFluentEmail(defaultFromEmail, nameof(BookWorm))
            .AddSmtpSender(emailConn.Host, emailConn.Port, emailConn.UserName, emailConn.Password);

        builder.Services.AddResiliencePipeline(nameof(Email), resiliencePipelineBuilder => resiliencePipelineBuilder
            .AddRetry(new()
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                Delay = TimeSpan.FromSeconds(2),
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Constant
            })
            .AddTimeout(TimeSpan.FromSeconds(10)));

        builder.Services.AddOpenTelemetry()
            .WithMetrics(t => t
                .AddMeter(SmtpTelemetry.ActivityName)
                .AddMeter(MartenTelemetry.ActivityName))
            .WithTracing(t => t
                .AddSource(SmtpTelemetry.ActivityName)
                .AddSource(MartenTelemetry.ActivityName));

        builder.Services.AddMarten(_ =>
        {
            var options = new StoreOptions();

            var dbConn = builder.Configuration.GetConnectionString(ServiceName.Database.Notification);
            Guard.Against.NullOrEmpty(dbConn);
            options.Connection(dbConn);

            options.GeneratedCodeMode = TypeLoadMode.Auto;
            options.AutoCreateSchemaObjects = AutoCreate.All;

            options.Schema.For<EmailOutbox>().Identity(e => e.Id);
            options.RegisterDocumentType<EmailOutbox>();
        }).UseLightweightSessions();

        builder.Services.AddScoped<ISmtpService, SmtpService>();

        builder.Services.Decorate<ISmtpService, SmtpOutboxDecorator>();
    }
}
