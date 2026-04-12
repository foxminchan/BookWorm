namespace BookWorm.Chassis.Utilities.Guards;

public static class GuardAgainstAuthenticationExtensions
{
    extension(Guard guard)
    {
        /// <summary>
        ///     Validates that the provided user ID is not null, empty, or whitespace, indicating the user is authenticated.
        /// </summary>
        /// <param name="userId">The user ID to validate.</param>
        /// <returns>The validated user ID if it passes the authentication check.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user ID is null, empty, or whitespace.</exception>
        public string NotAuthenticated(string? userId)
        {
            return string.IsNullOrWhiteSpace(userId)
                ? throw new UnauthorizedAccessException("User is not authenticated.")
                : userId;
        }
    }
}
