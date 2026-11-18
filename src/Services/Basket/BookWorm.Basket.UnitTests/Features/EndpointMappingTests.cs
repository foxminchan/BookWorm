using BookWorm.Basket.Features.Create;
using BookWorm.Chassis.Endpoints;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Basket.UnitTests.Features;

public sealed class EndpointMappingTests
{
    [Test]
    public void GivenBasketEndpoints_WhenMappingEndpoints_ThenShouldRegisterRouteEndpoints()
    {
        // Arrange
        var builder = WebApplication.CreateSlimBuilder();
        builder.Services.AddApiVersioning();
        builder.Services.AddSingleton(Mock.Of<ISender>());
        var app = builder.Build();
        var group = app.NewVersionedApi().MapGroup("/api/v{version:apiVersion}");
        var endpointTypes = typeof(CreateBasketEndpoint)
            .Assembly.GetTypes()
            .Where(t =>
                t is { IsAbstract: false, IsInterface: false }
                && typeof(IEndpoint).IsAssignableFrom(t)
            )
            .ToList();

        // Act
        foreach (
            var endpoint in endpointTypes.Select(type => (IEndpoint)Activator.CreateInstance(type)!)
        )
        {
            endpoint.MapEndpoint(group);
        }

        // Assert
        var endpoints = ((IEndpointRouteBuilder)app)
            .DataSources.SelectMany(ds => ds.Endpoints)
            .ToList();
        endpoints.Count.ShouldBe(endpointTypes.Count);
    }
}
