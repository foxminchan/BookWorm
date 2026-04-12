using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.Endpoints;

public static class RegisterEndpointExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Scans the assembly that contains the specified type and registers all concrete
        ///     implementations of <see cref="IEndpoint" /> into the dependency injection container.
        /// </summary>
        /// <param name="type">
        ///     A marker type used to locate the target assembly for endpoint discovery.
        /// </param>
        public void AddEndpoints(Type type)
        {
            services.Scan(scan =>
                scan.FromAssembliesOf(type)
                    .AddClasses(
                        classes =>
                            classes
                                .AssignableTo<IEndpoint>()
                                .Where(typeInfo =>
                                    typeInfo is { IsAbstract: false, IsInterface: false }
                                ),
                        false
                    )
                    .AsImplementedInterfaces()
                    .WithTransientLifetime()
            );
        }
    }

    extension(WebApplication app)
    {
        /// <summary>
        ///     Maps all discovered <see cref="IEndpoint" /> implementations into a versioned API route group.
        /// </summary>
        /// <param name="apiVersionSet">
        ///     The API version set applied to the mapped endpoint group.
        /// </param>
        /// <param name="resourceName">
        ///     An optional resource segment appended to the base route.
        ///     When <see langword="null" /> or empty, only the versioned base path is used.
        /// </param>
        public void MapEndpoints(ApiVersionSet apiVersionSet, string? resourceName = null)
        {
            using var scope = app.Services.CreateScope();

            var endpoints = scope.ServiceProvider.GetRequiredService<IEnumerable<IEndpoint>>();

            var builder = app.MapGroup(
                    resourceName is null or { Length: 0 }
                        ? "/api/v{version:apiVersion}"
                        : $"/api/v{{version:apiVersion}}/{resourceName}"
                )
                .WithApiVersionSet(apiVersionSet);

            if (app.Environment.IsDevelopment())
            {
                builder.DisableAntiforgery();
            }

            foreach (var endpoint in endpoints)
            {
                endpoint.MapEndpoint(builder);
            }
        }
    }
}
