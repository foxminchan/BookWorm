using BookWorm.Chassis.Security.TokenExchange;
using A2AClientFactory = BookWorm.Chassis.AI.Agents.A2AClientFactory;

namespace BookWorm.Chat.Agents.Rating;

internal static class RatingAgentRegistration
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddRatingAgent()
        {
            var services = builder.Services;

            services.AddHttpContextAccessor();
            services
                .AddHttpClient(Constants.Other.Agents.RatingAgent, client =>
                {
                    client.BaseAddress = new(
                        ServiceDiscoveryUtilities.GetRequiredServiceEndpoint(Services.Rating)
                    );
                    client.Timeout = TimeSpan.FromMinutes(2);
                })
                .AddAuthTokenExchange(Services.Rating);

            builder.AddAIAgent(Constants.Other.Agents.RatingAgent, (sp, _) =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(Constants.Other.Agents.RatingAgent);

                return A2AClientFactory.CreateA2AAgentClient(
                    Services.Rating,
                    Constants.Other.Agents.RatingAgent,
                    httpClient
                );
            });
        }
    }
}
