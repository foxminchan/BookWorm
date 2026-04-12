using Microsoft.Agents.AI.DevUI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.AI.Extensions;

public static class DevUIExtensions
{
    extension(WebApplication app)
    {
        /// <summary>
        ///     Maps OpenAI response and conversation endpoints, and maps the Dev UI endpoint only in development environments.
        /// </summary>
        /// <remarks>
        ///     This method is intended for local development support. In non-development environments,
        ///     only the OpenAI-related endpoints are registered.
        /// </remarks>
        public void UseDevUI()
        {
            app.MapOpenAIResponses();
            app.MapOpenAIConversations();

            if (app.Environment.IsDevelopment())
            {
                app.MapDevUI();
            }
        }
    }
}
