using BookWorm.Chassis.Endpoints;
using BookWorm.Ordering.Features.Orders.Stream;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;

namespace BookWorm.Ordering.UnitTests.Features.Orders.Stream;

public sealed class OrderStreamEndpointTests
{
    private readonly OrderStreamEndpoint _endpoint = new();

    [Test]
    public void GivenOrderStreamEndpoint_WhenCheckingImplementation_ThenShouldImplementIEndpointInterface()
    {
        // Arrange & Act & Assert
        _endpoint.ShouldNotBeNull();
        _endpoint.ShouldBeAssignableTo<IEndpoint>();
    }

    [Test]
    public void GivenOrderStreamHub_WhenCheckingHubType_ThenShouldInheritFromHub()
    {
        // Arrange & Act
        var hubType = typeof(OrderStreamHub);

        // Assert
        hubType.ShouldNotBeNull();
        hubType.IsSealed.ShouldBeTrue();
        hubType.BaseType.ShouldBe(typeof(Hub));
    }

    [Test]
    public void GivenTwoOrderStreamEndpointInstances_WhenComparing_ThenShouldBeDifferentInstances()
    {
        // Arrange
        var endpoint1 = new OrderStreamEndpoint();
        var endpoint2 = new OrderStreamEndpoint();

        // Act & Assert
        endpoint1.ShouldNotBeSameAs(endpoint2);
        endpoint1.ShouldNotBe(endpoint2);
    }

    [Test]
    public void GivenOrderStreamEndpoint_WhenCreatingInstance_ThenShouldNotThrowException()
    {
        // Act & Assert
        Should.NotThrow(() => new OrderStreamEndpoint());
    }

    [Test]
    public void GivenNullRouteBuilder_WhenMappingEndpoint_ThenShouldThrowNullReferenceException()
    {
        // Act & Assert
        Should.Throw<NullReferenceException>(() => _endpoint.MapEndpoint(null!));
    }

    [Test]
    public void GivenOrderStreamHub_WhenCheckingType_ThenShouldHaveCorrectCharacteristics()
    {
        // Arrange & Act
        var hubType = typeof(OrderStreamHub);

        // Assert
        hubType.IsClass.ShouldBeTrue();
        hubType.IsAbstract.ShouldBeFalse();
        hubType.IsSealed.ShouldBeTrue();
        hubType.IsPublic.ShouldBeTrue();
    }

    [Test]
    public void GivenOrderStreamEndpoint_WhenCheckingType_ThenShouldHaveCorrectCharacteristics()
    {
        // Arrange & Act
        var endpointType = typeof(OrderStreamEndpoint);

        // Assert
        endpointType.IsClass.ShouldBeTrue();
        endpointType.IsAbstract.ShouldBeFalse();
        endpointType.IsSealed.ShouldBeTrue();
        endpointType.IsPublic.ShouldBeTrue();
    }

    [Test]
    public void GivenOrderStreamEndpoint_WhenCheckingMethods_ThenShouldHaveMapEndpointMethod()
    {
        // Arrange & Act
        var endpointType = typeof(OrderStreamEndpoint);
        var mapEndpointMethod = endpointType.GetMethod(nameof(IEndpoint.MapEndpoint));

        // Assert
        mapEndpointMethod.ShouldNotBeNull();
        mapEndpointMethod.IsPublic.ShouldBeTrue();
        mapEndpointMethod.ReturnType.ShouldBe(typeof(void));

        var parameters = mapEndpointMethod.GetParameters();
        parameters.Length.ShouldBe(1);
        parameters[0].ParameterType.ShouldBe(typeof(IEndpointRouteBuilder));
        parameters[0].Name.ShouldBe("app");
    }

    [Test]
    public void GivenOrderStreamEndpoint_WhenCheckingNamespace_ThenShouldBeInCorrectNamespace()
    {
        // Arrange & Act
        var endpointType = typeof(OrderStreamEndpoint);

        // Assert
        endpointType.Namespace.ShouldBe("BookWorm.Ordering.Features.Orders.Stream");
    }

    [Test]
    public void GivenOrderStreamHub_WhenCheckingNamespace_ThenShouldBeInCorrectNamespace()
    {
        // Arrange & Act
        var hubType = typeof(OrderStreamHub);

        // Assert
        hubType.Namespace.ShouldBe("BookWorm.Ordering.Features.Orders.Stream");
    }

    [Test]
    public void GivenOrderStreamHub_WhenCheckingConstructors_ThenShouldHaveParameterlessConstructor()
    {
        // Arrange & Act
        var hubType = typeof(OrderStreamHub);
        var constructors = hubType.GetConstructors();
        var parameterlessConstructor = constructors.FirstOrDefault(c =>
            c.GetParameters().Length == 0
        );

        // Assert
        parameterlessConstructor.ShouldNotBeNull();
        parameterlessConstructor.IsPublic.ShouldBeTrue();
    }

    [Test]
    public void GivenOrderStreamEndpoint_WhenCheckingConstructors_ThenShouldHaveParameterlessConstructor()
    {
        // Arrange & Act
        var endpointType = typeof(OrderStreamEndpoint);
        var constructors = endpointType.GetConstructors();
        var parameterlessConstructor = constructors.FirstOrDefault(c =>
            c.GetParameters().Length == 0
        );

        // Assert
        parameterlessConstructor.ShouldNotBeNull();
        parameterlessConstructor.IsPublic.ShouldBeTrue();
    }

    [Test]
    public void GivenValidMapEndpointCall_WhenCheckingMethodSignature_ThenShouldMatchInterfaceContract()
    {
        // Arrange
        var endpointType = typeof(OrderStreamEndpoint);
        var interfaceType = typeof(IEndpoint);
        var interfaceMethod = interfaceType.GetMethod(nameof(IEndpoint.MapEndpoint));
        var implementationMethod = endpointType.GetMethod(nameof(IEndpoint.MapEndpoint));

        // Assert
        interfaceMethod.ShouldNotBeNull();
        implementationMethod.ShouldNotBeNull();

        // Check return types match
        implementationMethod.ReturnType.ShouldBe(interfaceMethod.ReturnType);

        // Check parameter types match
        var interfaceParams = interfaceMethod.GetParameters();
        var implementationParams = implementationMethod.GetParameters();

        interfaceParams.Length.ShouldBe(implementationParams.Length);
        for (int i = 0; i < interfaceParams.Length; i++)
        {
            implementationParams[i].ParameterType.ShouldBe(interfaceParams[i].ParameterType);
            implementationParams[i].Name.ShouldBe(interfaceParams[i].Name);
        }
    }
}
