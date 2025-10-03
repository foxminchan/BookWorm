using BookWorm.Constants.Aspire;
using BookWorm.Scheduler.Infrastructure;
using BookWorm.Scheduler.Migrator;
using BookWorm.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<DataMigratorWorker>();

builder
    .Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(DataMigratorWorker.ActivitySourceName));

builder.Services.AddDbContextPool<SchedulerDbContext>(options =>
    options
        .UseNpgsql(
            builder.Configuration.GetConnectionString(Components.Database.Scheduler),
            sqlOptions =>
                sqlOptions.MigrationsAssembly(typeof(SchedulerDbContext).Assembly.FullName)
        )
        .UseSnakeCaseNamingConvention()
        // Issue: https://github.com/dotnet/efcore/issues/35285
        .ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning)
        )
);

builder.EnrichAzureNpgsqlDbContext<SchedulerDbContext>();

var app = builder.Build();

app.Run();
