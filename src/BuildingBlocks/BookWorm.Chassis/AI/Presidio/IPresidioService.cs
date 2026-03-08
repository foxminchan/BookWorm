namespace BookWorm.Chassis.AI.Presidio;

public interface IPresidioService
{
    /// <summary>
    ///     Analyzes the given text for PII entities and returns the anonymized version.
    /// </summary>
    /// <param name="text">The text to analyze and anonymize.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The anonymized text with PII entities replaced.</returns>
    Task<string> AnonymizeAsync(string text, CancellationToken cancellationToken = default);
}
