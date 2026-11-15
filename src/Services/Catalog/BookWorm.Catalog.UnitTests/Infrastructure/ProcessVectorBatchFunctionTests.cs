using System.Security.Claims;
using BookWorm.Catalog.Ingestion;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using MicrosoftIdentityConstants = Microsoft.Identity.Web.Constants;

namespace BookWorm.Catalog.UnitTests.Infrastructure;

public sealed class ProcessVectorBatchFunctionTests
{
    [Test]
    public async Task GivenSchedulerRequest_WhenInvoked_ThenShouldProcessOneVectorBatch()
    {
        using var cancellation = new CancellationTokenSource();
        var cancellationToken = cancellation.Token;
        var processor = new Mock<IVectorBatchProcessor>();
        processor.Setup(x => x.ProcessAsync(cancellationToken)).ReturnsAsync(7);
        var function = new ProcessVectorBatchFunction(processor.Object);
        var request = CreateRequest(CreateAuthenticatedService());

        var response = await function.ProcessVectorBatch(request, cancellationToken);

        response.Result.ShouldBeOfType<NoContent>();
        request.HttpContext.User.Identity?.IsAuthenticated.ShouldBeTrue();
        processor.Verify(x => x.ProcessAsync(cancellationToken), Times.Once);
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
                x.AuthenticateAsync(It.IsAny<HttpContext>(), MicrosoftIdentityConstants.Bearer)
            )
            .ReturnsAsync(AuthenticateResult.Fail("Invalid token"));

        var response = await function.ProcessVectorBatch(
            CreateRequest(authenticationService.Object),
            CancellationToken.None
        );

        response.Result.ShouldBeOfType<UnauthorizedHttpResult>();
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
                CreateRequest(CreateAuthenticatedService()),
                CancellationToken.None
            )
        );
    }

    [Test]
    public async Task GivenProcessingCancelled_WhenInvoked_ThenShouldPropagateCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var cancellationToken = cancellation.Token;
        var processor = new Mock<IVectorBatchProcessor>();
        processor
            .Setup(x => x.ProcessAsync(cancellationToken))
            .Returns(Task.FromCanceled<int>(cancellationToken));
        var function = new ProcessVectorBatchFunction(processor.Object);

        await Should.ThrowAsync<OperationCanceledException>(() =>
            function.ProcessVectorBatch(
                CreateRequest(CreateAuthenticatedService()),
                cancellationToken
            )
        );
    }

    private static IAuthenticationService CreateAuthenticatedService()
    {
        var authenticationService = new Mock<IAuthenticationService>();
        authenticationService
            .Setup(x =>
                x.AuthenticateAsync(It.IsAny<HttpContext>(), MicrosoftIdentityConstants.Bearer)
            )
            .ReturnsAsync(
                AuthenticateResult.Success(
                    new(
                        new(new ClaimsIdentity([], MicrosoftIdentityConstants.Bearer)),
                        MicrosoftIdentityConstants.Bearer
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
