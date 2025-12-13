using Microsoft.FluentUI.AspNetCore.Components;

namespace BookWorm.StoreFront.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddRazorComponents().AddInteractiveServerComponents();
        services.AddFluentUIComponents();
    }
}
