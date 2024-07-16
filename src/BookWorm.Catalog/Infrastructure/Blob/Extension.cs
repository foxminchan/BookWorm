using Azure;
using BookWorm.Shared.Validator;
using Microsoft.Extensions.Options;
using Polly;

namespace BookWorm.Catalog.Infrastructure.Blob;

public static class Extension
{
    public static IHostApplicationBuilder AddStorage(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOptionsWithValidateOnStart<AzuriteOptions>()
            .Bind(builder.Configuration.GetSection(nameof(AzuriteOptions)))
            .ValidateFluentValidation();

        builder.Services.AddSingleton(options => options.GetRequiredService<IOptions<AzuriteOptions>>().Value);

        builder.Services.AddResiliencePipeline(nameof(Blob), resiliencePipelineBuilder => resiliencePipelineBuilder
            .AddRetry(new()
            {
                ShouldHandle = new PredicateBuilder().Handle<RequestFailedException>(),
                Delay = TimeSpan.FromSeconds(2),
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Constant
            })
            .AddTimeout(TimeSpan.FromSeconds(10)));

        builder.Services.AddSingleton<IAzuriteService, AzuriteService>();

        return builder;
    }
}
