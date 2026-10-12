using System.Security.Claims;
using BookWorm.Catalog.Ingestion;
using BookWorm.Constants.Aspire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Catalog.UnitTests.Infrastructure;

public sealed class ProcessVectorBatchFunctionTests
{
    [Test]
    public async Task GivenSchedulerRequest_WhenInvoked_ThenShouldProcessOneVectorBatch()
    {
        using var cancellation = new CancellationTokenSource();
        var processor = new Mock<IVectorBatchProcessor>();
        processor.Setup(x => x.ProcessAsync(cancellation.Token)).ReturnsAsync(7);
        var function = new ProcessVectorBatchFunction(processor.Object);

        var response = await function.ProcessVectorBatch(
            CreateRequest(CreateSchedulerAuthentication()),
            cancellation.Token
        );

        response.ShouldBeOfType<NoContent>();
        processor.Verify(x => x.ProcessAsync(cancellation.Token), Times.Once);
        processor.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GivenUnauthenticatedRequest_WhenInvoked_ThenShouldReturnUnauthorized()
    {
        var processor = new Mock<IVectorBatchProcessor>();
        var function = new ProcessVectorBatchFunction(processor.Object);
        var authenticationService = new Mock<IAuthenticationService>();
        authenticationService
            .Setup(x =>
                x.AuthenticateAsync(It.IsAny<HttpContext>(), JwtBearerDefaults.AuthenticationScheme)
            )
            .ReturnsAsync(AuthenticateResult.Fail("Invalid token"));

        var response = await function.ProcessVectorBatch(
            CreateRequest(authenticationService.Object),
            CancellationToken.None
        );

        response.ShouldBeOfType<UnauthorizedHttpResult>();
        processor.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GivenDifferentClientRequest_WhenInvoked_ThenShouldReturnForbidden()
    {
        var processor = new Mock<IVectorBatchProcessor>();
        var function = new ProcessVectorBatchFunction(processor.Object);
        var authenticationService = new Mock<IAuthenticationService>();
        authenticationService
            .Setup(x =>
                x.AuthenticateAsync(It.IsAny<HttpContext>(), JwtBearerDefaults.AuthenticationScheme)
            )
            .ReturnsAsync(
                AuthenticateResult.Success(
                    new(
                        new(new ClaimsIdentity([new("azp", "other-client")], "Bearer")),
                        JwtBearerDefaults.AuthenticationScheme
                    )
                )
            );

        var response = await function.ProcessVectorBatch(
            CreateRequest(authenticationService.Object),
            CancellationToken.None
        );

        response.ShouldBeOfType<ForbidHttpResult>();
        processor.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GivenProcessingFails_WhenInvoked_ThenShouldSurfaceFailure()
    {
        var processor = new Mock<IVectorBatchProcessor>();
        processor
            .Setup(x => x.ProcessAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Embedding service unavailable"));
        var function = new ProcessVectorBatchFunction(processor.Object);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            function.ProcessVectorBatch(
                CreateRequest(CreateSchedulerAuthentication()),
                CancellationToken.None
            )
        );
    }

    [Test]
    public async Task GivenProcessingCancelled_WhenInvoked_ThenShouldPropagateCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var processor = new Mock<IVectorBatchProcessor>();
        processor
            .Setup(x => x.ProcessAsync(cancellation.Token))
            .Returns(Task.FromCanceled<int>(cancellation.Token));
        var function = new ProcessVectorBatchFunction(processor.Object);

        await Should.ThrowAsync<OperationCanceledException>(() =>
            function.ProcessVectorBatch(
                CreateRequest(CreateSchedulerAuthentication()),
                cancellation.Token
            )
        );
    }

    private static IAuthenticationService CreateSchedulerAuthentication()
    {
        var authenticationService = new Mock<IAuthenticationService>();
        authenticationService
            .Setup(x =>
                x.AuthenticateAsync(It.IsAny<HttpContext>(), JwtBearerDefaults.AuthenticationScheme)
            )
            .ReturnsAsync(
                AuthenticateResult.Success(
                    new(
                        new(new ClaimsIdentity([new("azp", Services.Scheduler)], "Bearer")),
                        JwtBearerDefaults.AuthenticationScheme
                    )
                )
            );

        return authenticationService.Object;
    }

    private static HttpRequest CreateRequest(IAuthenticationService authenticationService)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddSingleton(authenticationService)
                .BuildServiceProvider(),
        };
        return context.Request;
    }
}
