namespace BookWorm.StoreFront.Components.Layout;

public sealed partial class Footer
{
    private sealed record SocialMediaLink(string Url, string IconPath, string Platform);

    private SocialMediaLink[] _socialMediaLinks = [];

    protected override void OnInitialized()
    {
        _socialMediaLinks =
        [
            new("https://facebook.com", "/icons/facebook.svg", "Facebook"),
            new("https://instagram.com", "/icons/instagram.svg", "Instagram"),
            new("https://linkedin.com", "/icons/linkedin.svg", "LinkedIn"),
        ];
    }
}
