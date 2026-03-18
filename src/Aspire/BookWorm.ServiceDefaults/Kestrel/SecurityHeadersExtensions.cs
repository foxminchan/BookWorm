namespace BookWorm.ServiceDefaults.Kestrel;

/// <summary>
///     Extension methods for adding security headers middleware.
///     Protects against XSS, clickjacking, MIME-type sniffing, and information leakage (OWASP A05/A06).
/// </summary>
public static class SecurityHeadersExtensions
{
    /// <summary>
    ///     Adds security headers middleware to the application pipeline.
    ///     Appends Content-Security-Policy, X-Content-Type-Options, X-Frame-Options,
    ///     and Referrer-Policy headers to all responses.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder with security headers middleware registered.</returns>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            // Prevents MIME-type sniffing attacks
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

            // Prevents clickjacking attacks
            context.Response.Headers.Append("X-Frame-Options", "DENY");

            // Controls how much referrer information is included with requests
            context.Response.Headers.Append(
                "Referrer-Policy",
                "strict-origin-when-cross-origin"
            );

            // Restricts resource loading to same origin; forces HTTPS upgrades
            context.Response.Headers.Append(
                "Content-Security-Policy",
                "default-src 'self'; upgrade-insecure-requests;"
            );

            await next(context);
        });
    }
}
