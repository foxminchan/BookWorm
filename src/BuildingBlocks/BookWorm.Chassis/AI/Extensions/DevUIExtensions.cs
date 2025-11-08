using Microsoft.Agents.AI.DevUI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.AI.Extensions;

public static class DevUIExtensions
{
    public static void UseDevUI(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapDevUI();
        }
    }
}
