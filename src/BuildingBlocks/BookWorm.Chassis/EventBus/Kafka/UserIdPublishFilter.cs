using System.Security.Claims;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.EventBus.Kafka;

/// <summary>
///     A MassTransit publish pipeline filter that automatically propagates the authenticated user's
///     identifier as a message header for all <see cref="IntegrationEvent" /> messages.
/// </summary>
/// <remarks>
///     This filter extracts the <see cref="ClaimTypes.NameIdentifier" /> claim from the current
///     <see cref="HttpContext" /> and writes it to the outbound message header keyed by
///     <see cref="EventBusHeaders.UserId" />. If no HTTP context is available (e.g. background
///     workers, saga state machines), the header is simply omitted and no error is thrown.
/// </remarks>
internal sealed class UserIdPublishFilter<T>(IServiceProvider serviceProvider)
    : IFilter<PublishContext<T>>
    where T : class
{
    /// <inheritdoc />
    public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        if (typeof(IntegrationEvent).IsAssignableFrom(typeof(T)))
        {
            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            var userId = httpContextAccessor?.HttpContext?.User?.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!string.IsNullOrEmpty(userId))
            {
                context.Headers.Set(EventBusHeaders.UserId, userId);
            }
        }

        await next.Send(context);
    }

    /// <inheritdoc />
    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("userIdPublish");
    }
}
