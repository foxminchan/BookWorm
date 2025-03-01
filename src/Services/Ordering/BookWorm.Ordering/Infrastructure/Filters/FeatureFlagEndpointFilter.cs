namespace BookWorm.Ordering.Infrastructure.Filters;

public sealed class FeatureFlagEndpointFilter(string endpoint) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var featureFlag = context.HttpContext.RequestServices.GetRequiredService<IFeatureManager>();

        if (await featureFlag.IsEnabledAsync(endpoint))
        {
            return await next(context);
        }

        return TypedResults.NotFound();
    }
}

public static class FeatureFlagEndpointFilterExtensions
{
    public static TBuilder WithFeatureFlag<TBuilder>(this TBuilder builder, string endpoint)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilter(new FeatureFlagEndpointFilter(endpoint));
        return builder;
    }
}
