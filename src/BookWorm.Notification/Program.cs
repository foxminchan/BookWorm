using Ardalis.GuardClauses;
using BookWorm.Notification.Infrastructure;
using BookWorm.ServiceDefaults;
using BookWorm.Shared.Bus;
using BookWorm.Shared.Exceptions;
using FluentEmail.Core;
using Marten;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.AddRabbitMqEventBus(typeof(Program));

var emailConn = new UriBuilder(builder.Configuration.GetConnectionString("mailserver") ??
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

var dbConn = builder.Configuration.GetConnectionString("notificationdb");

Guard.Against.NullOrEmpty(dbConn);

builder.Services.AddMarten(dbConn).UseLightweightSessions();

builder.Services.AddScoped<ISmtpService, SmtpService>();

builder.Services.Decorate<ISmtpService, SmtpOutboxDecorator>();

var app = builder.Build();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.Run();
