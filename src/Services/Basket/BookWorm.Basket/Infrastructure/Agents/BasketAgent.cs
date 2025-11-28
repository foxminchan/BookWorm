using A2A;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Chassis.Security.Settings;
using BookWorm.Chassis.Utilities;
using BookWorm.Constants.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace BookWorm.Basket.Infrastructure.Agents;

internal static class BasketAgent
{
    public const string Name = Constants.Other.Agents.BasketAgent;

    public const string Description =
        "Manages shopping basket operations with approval-required changes.";

    public const string Instructions = """
        You manage shopping baskets for BookWorm customers.

        Rules:
        - Confirm book details (title, price, quantity) before adding
        - Use AddToBasket tool—requires user approval
        - Default quantity: 1
        - Provide clear summaries
        - After completion, return to RouterAgent

        Handle errors gracefully and be transparent about pricing.
        """;

    public static AgentCard AgentCard { get; } =
        new()
        {
            Name = Name,
            Description = Description,
            Version = "1.0",
            Provider = new()
            {
                Organization = nameof(BookWorm),
                Url = "https://github.com/foxminchan/BookWorm",
            },
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = new() { Streaming = true, PushNotifications = false },
            Skills =
            [
                new()
                {
                    Id = "basket_agent_add_item",
                    Tags = ["basket", "shopping", "purchase", "add"],
                    Name = "Add Item to Basket",
                    Description =
                        "Add a book to the customer's shopping basket with human approval",
                    Examples =
                    [
                        "Add this book to my basket",
                        "I'd like to purchase this title",
                        "Put that in my cart",
                    ],
                },
                new()
                {
                    Id = "basket_agent_view_basket",
                    Tags = ["basket", "shopping", "view", "summary"],
                    Name = "View Basket Contents",
                    Description = "Display current items in the customer's basket with totals",
                    Examples =
                    [
                        "What's in my basket?",
                        "Show me my cart",
                        "What have I selected so far?",
                    ],
                },
                new()
                {
                    Id = "basket_agent_update_quantity",
                    Tags = ["basket", "shopping", "update", "quantity"],
                    Name = "Update Item Quantity",
                    Description = "Change the quantity of an existing item in the basket",
                    Examples =
                    [
                        "Change the quantity to 3",
                        "I want 2 copies instead",
                        "Update that to 5 books",
                    ],
                },
                new()
                {
                    Id = "basket_agent_remove_item",
                    Tags = ["basket", "shopping", "remove", "delete"],
                    Name = "Remove Item from Basket",
                    Description = "Remove a book from the customer's basket",
                    Examples =
                    [
                        "Remove that book from my basket",
                        "I don't want that one anymore",
                        "Delete that item",
                    ],
                },
            ],
            SecuritySchemes = new()
            {
                [OAuthDefaults.DisplayName] = new OAuth2SecurityScheme(
                    new()
                    {
                        AuthorizationCode = new(
                            new(
                                HttpUtilities
                                    .AsUrlBuilder()
                                    .WithBase(
                                        ServiceDiscoveryUtilities.GetRequiredServiceEndpoint(
                                            Components.KeyCloak
                                        )
                                    )
                                    .WithPath(
                                        KeycloakEndpoints.Authorize.Replace(
                                            "{realm}",
                                            Environment.GetEnvironmentVariable(
                                                $"{IdentityOptions.ConfigurationSection}__{nameof(IdentityOptions.Realm)}"
                                            ) ?? nameof(BookWorm)
                                        )
                                    )
                                    .Build()
                            ),
                            new(
                                HttpUtilities
                                    .AsUrlBuilder()
                                    .WithBase(
                                        ServiceDiscoveryUtilities.GetRequiredServiceEndpoint(
                                            Components.KeyCloak
                                        )
                                    )
                                    .WithPath(
                                        KeycloakEndpoints.Token.Replace(
                                            "{realm}",
                                            Environment.GetEnvironmentVariable(
                                                $"{IdentityOptions.ConfigurationSection}__{nameof(IdentityOptions.Realm)}"
                                            ) ?? nameof(BookWorm)
                                        )
                                    )
                                    .Build()
                            ),
                            new Dictionary<string, string>
                            {
                                {
                                    $"{Services.Basket}_{Authorization.Actions.Read}",
                                    "Read access to basket service"
                                },
                                {
                                    $"{Services.Basket}_{Authorization.Actions.Write}",
                                    "Write access to basket service"
                                },
                            }
                        ),
                    },
                    "OAuth2 security scheme for the BookWorm API"
                ),
            },
            Security =
            [
                new()
                {
                    [$"{JwtBearerDefaults.AuthenticationScheme}"] =
                    [
                        $"{Services.Basket}_{Authorization.Actions.Read}",
                        $"{Services.Basket}_{Authorization.Actions.Write}",
                    ],
                },
            ],
            PreferredTransport = AgentTransport.JsonRpc,
        };
}
