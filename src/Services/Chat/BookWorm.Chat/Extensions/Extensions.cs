using System.Security.Claims;
using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Governance;
using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Chat.Agents.Basket;
using BookWorm.Chat.Agents.BookSearch;
using BookWorm.Chat.Agents.CustomerSupport;
using BookWorm.Chat.Agents.LanguageTranslation;
using BookWorm.Chat.Agents.Routing;
using BookWorm.Chat.Agents.SentimentAnalysis;
using BookWorm.Chat.Agents.Summarization;
using BookWorm.Chat.Configurations;
using BookWorm.Chat.Orchestration;
using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using BookWorm.ServiceDefaults.Cors;
using Microsoft.AspNetCore.Authorization;

namespace BookWorm.Chat.Extensions;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddApplicationServices()
        {
            var services = builder.Services;

            builder.AddDefaultCors();

            builder.AddAppSettings<ChatAppSettings>();

            builder.AddDefaultAuthentication().WithKeycloakClaimsTransformation();

            services
                .AddAuthorizationBuilder()
                .AddPolicy(
                    Authorization.Policies.Admin,
                    policy =>
                    {
                        policy
                            .RequireAuthenticatedUser()
                            .RequireRole(Authorization.Roles.Admin)
                            .RequireScope(
                                $"{Services.Chatting}_{Authorization.Actions.Read}",
                                $"{Services.Chatting}_{Authorization.Actions.Write}"
                            );
                    }
                )
                .SetDefaultPolicy(
                    new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .RequireRole(Authorization.Roles.User)
                        .RequireScope(
                            $"{Services.Chatting}_{Authorization.Actions.Read}",
                            $"{Services.Chatting}_{Authorization.Actions.Write}"
                        )
                        .Build()
                );

            // Add exception handlers
            services.AddNotFoundExceptionHandler();
            services.AddGlobalExceptionHandler();
            services.AddProblemDetails();

            builder.AddRateLimiting();

            services.AddVersioning();
            services.AddEndpoints(typeof(IChatApiMarker));
            services.AddDefaultOpenApi(options =>
                options.AddDocumentTransformer<OpenApiInfoDefinitionsTransformer<ChatAppSettings>>()
            );

            // Configure ClaimsPrincipal
            services.AddTransient(s =>
                s.GetRequiredService<IHttpContextAccessor>().HttpContext?.User
                ?? new ClaimsPrincipal()
            );

            // AI infrastructure (chat client, telemetry, protocols)
            builder.AddAIInfrastructure();

            // Agent governance (policy enforcement, identity, rings, injection detection)
            builder.AddAgentGovernance("Policies/chat-agents.yaml");

            // Agent discovery client for locating
            services.AddAgentDiscoveryClient(
                HttpUtilities
                    .AsUrlBuilder()
                    .WithScheme(Http.Schemes.HttpOrHttps)
                    .WithHost(Services.Rating)
                    .Build()
            );

            // Register each agent as a self-contained vertical slice
            builder.AddBookAgent();
            builder.AddBasketAgent();
            builder.AddLanguageAgent();
            builder.AddSentimentAgent();
            builder.AddSummarizeAgent();
            builder.AddQAAgent();
            builder.AddRouterAgent();

            // Compose the multi-agent workflow
            builder.AddChatWorkflow();

            services.AddKeycloakTokenIntrospection();
        }
    }
}
