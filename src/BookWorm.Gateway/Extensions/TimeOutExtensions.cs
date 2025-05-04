namespace BookWorm.Gateway.Extensions;

public static class TimeOutExtensions
{
    public static void AddRequestTimeouts(this IServiceCollection services)
    {
        services.AddRequestTimeouts(options =>
        {
            options.DefaultPolicy = new()
            {
                Timeout = TimeSpan.FromSeconds(30),
                TimeoutStatusCode = StatusCodes.Status408RequestTimeout,
                WriteTimeoutResponse = async context =>
                {
                    var problemDetailsFactory =
                        context.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                    var problemDetails = problemDetailsFactory.CreateProblemDetails(
                        context,
                        StatusCodes.Status408RequestTimeout,
                        "Request timed out",
                        "The request took too long to complete."
                    );

                    await context.Response.WriteAsJsonAsync(problemDetails);
                },
            };
        });
    }
}
