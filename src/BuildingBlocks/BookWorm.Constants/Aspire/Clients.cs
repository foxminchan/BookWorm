namespace BookWorm.Constants.Aspire;

public static class Clients
{
    private const string Prefix = "@bookworm";
    public const string StoreFront = "storefront";
    public const string StoreFrontTurboApp = $"{Prefix}/{StoreFront}";
    public const string Commerce = "commerce";
    public const string CommerceTurboApp = $"{Prefix}/{Commerce}";
    public const string BackOffice = "backoffice";
    public const string BackOfficeTurboApp = $"{Prefix}/{BackOffice}";
}
