using BookWorm.Identity.Options;

namespace BookWorm.Identity;

public class AppSettings
{
    public ServiceOptions Services { get; set; } = new();
}
