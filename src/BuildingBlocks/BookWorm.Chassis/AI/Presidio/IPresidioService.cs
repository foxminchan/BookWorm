namespace BookWorm.Chassis.AI.Presidio;

public interface IPresidioService
{
    /// <summary>
    ///     Analyzes the given text and replaces every detected PII entity with &lt;Pii&gt;.
    /// </summary>
    /// <param name="text">The text to analyze and anonymize.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The anonymized text with PII entities replaced.</returns>
    Task<string> AnonymizeAsync(string text, CancellationToken cancellationToken = default);
}
