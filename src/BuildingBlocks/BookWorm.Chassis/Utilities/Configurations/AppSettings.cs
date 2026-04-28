using Microsoft.OpenApi;

namespace BookWorm.Chassis.Utilities.Configurations;

public abstract class AppSettings
{
    public virtual OpenApiInfo? OpenApi { get; set; } = new();
}
