namespace BookWorm.ServiceDefaults.ApiSpecification;

public sealed class Document
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AuthorName { get; set; } = "Nhan Nguyen";
    public string AuthorEmail { get; set; } = "nguyenxuannhan407@gmail.com";
    public Uri AuthorUrl { get; set; } = new("https://github.com/foxminchan");
    public string LicenseName { get; set; } = "MIT";
    public Uri LicenseUrl { get; set; } = new("https://opensource.org/licenses/MIT");
}
