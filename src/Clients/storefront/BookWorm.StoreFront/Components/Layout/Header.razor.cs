namespace BookWorm.StoreFront.Components.Layout;

public sealed partial class Header
{
    private sealed record NavigationItem(string Href, string IconPath, string AriaLabel);

    private NavigationItem[] _navigationItems = [];

    protected override void OnInitialized()
    {
        _navigationItems =
        [
            new("/", "/icons/home.svg", "Home"),
            new("catalog", "/icons/books.svg", "Books"),
            new("basket", "/icons/cart.svg", "Shopping cart"),
            new("account", "/icons/account.svg", "Account"),
        ];
    }
}
