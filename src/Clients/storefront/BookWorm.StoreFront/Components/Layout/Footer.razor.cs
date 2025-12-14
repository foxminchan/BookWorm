using Microsoft.AspNetCore.Components;

namespace BookWorm.StoreFront.Components.Layout;

public sealed partial class Footer : ComponentBase
{
    private sealed record SocialMediaLink(string Url, string IconPath, string Platform);

    private readonly SocialMediaLink[] _socialMediaLinks =
    [
        new("https://facebook.com", "/icons/facebook.svg", "Facebook"),
        new("https://instagram.com", "/icons/instagram.svg", "Instagram"),
        new("https://linkedin.com", "/icons/linkedin.svg", "LinkedIn"),
    ];
}
