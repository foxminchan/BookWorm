using BookWorm.Constants.Core;
using BookWorm.SharedKernel.Helpers;
using Grpc.Core;

namespace BookWorm.Basket.UnitTests.Grpc.Context;

public sealed class TestServerCallContext(
    Metadata? requestHeaders = null,
    CancellationToken cancellationToken = default,
    DateTime? deadline = null,
    Metadata? responseTrailers = null,
    WriteOptions? writeOptions = null,
    AuthContext? authContext = null
) : ServerCallContext
{
    private readonly Dictionary<object, object> _userState = [];
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

    protected override IDictionary<object, object> UserStateCore => _userState;

    internal void SetUserState(object key, object value)
    {
        _userState[key] = value;
    }

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

    public static TestServerCallContext Create(
        Metadata? requestHeaders = null,
        CancellationToken ct = default
    )
    {
        return new([], ct);
    }
}
