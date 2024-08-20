namespace BookWorm.Rating.IntegrationTests;

public sealed class AutoAuthorizeMiddleware(RequestDelegate rd)
{
    public Guid UserId { get; set; } = Guid.NewGuid();

    public async Task Invoke(HttpContext httpContext)
    {
        var identity = new ClaimsIdentity("cookies");

        identity.AddClaim(new(ClaimTypes.NameIdentifier, UserId.ToString()));
        identity.AddClaim(new(ClaimTypes.Email, "example@gmail.com"));
        identity.AddClaim(new(ClaimTypes.Name, "Test User"));
        identity.AddClaim(new(ClaimTypes.Role, "Admin"));

        httpContext.User.AddIdentity(identity);

        await rd.Invoke(httpContext);
    }
}
