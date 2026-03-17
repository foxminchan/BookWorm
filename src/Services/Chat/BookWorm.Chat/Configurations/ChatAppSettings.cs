using System.Diagnostics.CodeAnalysis;
using BookWorm.Chassis.Utilities.Configurations;
using Microsoft.OpenApi;

namespace BookWorm.Chat.Configurations;

[ExcludeFromCodeCoverage]
internal sealed class ChatAppSettings : AppSettings
{
    public override OpenApiInfo? OpenApi { get; set; } =
        new()
        {
            Title = "Chat Service API",
            Summary = "A service responsible for managing the chat functionality",
            Description =
                "Manages the chat functionality for the BookWorm platform, providing conversational interactions about books and reading recommendations",
            Contact = new()
            {
                Name = "BookWorm Engineering",
                Email = "engineering@bookworm.com",
                Url = new("https://github.com/foxminchan"),
            },
            License = new() { Name = "MIT", Url = new("https://opensource.org/licenses/MIT") },
        };
}
