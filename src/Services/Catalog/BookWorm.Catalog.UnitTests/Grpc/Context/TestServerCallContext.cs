using BookWorm.Constants.Core;
using BookWorm.SharedKernel.Helpers;
using Grpc.Core;

namespace BookWorm.Catalog.UnitTests.Grpc.Context;

public sealed class TestServerCallContext(
    Metadata? requestHeaders = null,
    DateTime? deadline = null,
    Metadata? responseTrailers = null,
    WriteOptions? writeOptions = null,
    AuthContext? authContext = null,
    CancellationToken cancellationToken = default
) : ServerCallContext
{
    protected override string MethodCore => "test";

    protected override string HostCore => Network.Localhost;

    protected override string PeerCore => Network.LoopbackIpV4;

    protected override DateTime DeadlineCore { get; } =
        deadline ?? DateTimeHelper.UtcNow().AddMinutes(5);

    protected override Metadata RequestHeadersCore { get; } = requestHeaders ?? [];

    protected override CancellationToken CancellationTokenCore => cancellationToken;

    protected override Metadata ResponseTrailersCore { get; } = responseTrailers ?? [];

    protected override Status StatusCore { get; set; }

    protected override WriteOptions? WriteOptionsCore
    {
        get => writeOptions;
        set => _ = value;
    }

    protected override AuthContext AuthContextCore { get; } =
        authContext ?? new AuthContext("anonymous", []);

    protected override ContextPropagationToken CreatePropagationTokenCore(
        ContextPropagationOptions? options
    )
    {
        throw new NotImplementedException();
    }

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
    {
        return Task.CompletedTask;
    }
}
