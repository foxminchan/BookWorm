namespace BookWorm.Chat.Memory.Semantic;

/// <summary>
///     The book-relevant categories of preferences BookAgent is allowed to remember.
///     Anything outside these categories (for example health, religion, or politics) is never stored.
/// </summary>
internal enum PreferenceKind
{
    Genre,
    Author,
    Language,
    Format,
    Theme,
}
