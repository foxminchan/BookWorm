namespace BookWorm.StoreFront.Components.Components.Home;

public sealed partial class SocialMedia
{
    private sealed record SocialMediaPost(string Emoji, string Label);

    private List<SocialMediaPost> _posts = [];

    protected override void OnInitialized()
    {
        _posts =
        [
            new("📖", "Open Book"),
            new("📚", "Stack of Books"),
            new("📘", "Blue Book"),
            new("📕", "Red Book"),
        ];
    }
}
