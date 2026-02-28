using System.Diagnostics.CodeAnalysis;
using BookWorm.Chassis.Utilities.Configurations;
using Microsoft.OpenApi;

namespace BookWorm.McpTools.Configurations;

[ExcludeFromCodeCoverage]
public sealed class McpToolsAppSettings : AppSettings
{
    public override OpenApiInfo? OpenApi { get; set; } =
        new()
        {
            Title = "MCP Tools Server",
            Summary = "A service providing various tools and utilities for the MCP ecosystem",
            Description =
                "Provides tools for managing and interacting with BookWorm catalog data, including book search and catalog operations",
            Contact = new()
            {
                Name = "Nhan Nguyen",
                Email = "nguyenxuannhan407@gmail.com",
                Url = new("https://github.com/foxminchan"),
            },
            License = new() { Name = "MIT", Url = new("https://opensource.org/licenses/MIT") },
        };
}
