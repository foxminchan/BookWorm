using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.AI.Search;

public static class Extensions
{
    extension(IServiceCollection services)
    {
        public void AddHybridSearch()
        {
            services.AddScoped<ISearch, HybridSearch>();
        }
    }
}
