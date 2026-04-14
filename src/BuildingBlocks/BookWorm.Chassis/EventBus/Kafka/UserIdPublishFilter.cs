using System.Security.Claims;
using MassTransit;
using Microsoft.AspNetCore.Http;

namespace BookWorm.Chassis.EventBus.Kafka;

internal sealed class UserIdPublishFilter<T>(IHttpContextAccessor contextAccessor)
    : IFilter<PublishContext<T>>
    where T : class
{
    public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        if (typeof(IntegrationEvent).IsAssignableFrom(typeof(T)))
        {
            var userId = contextAccessor.HttpContext?.User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!string.IsNullOrEmpty(userId))
            {
                context.Headers.Set(EventBusHeaders.UserId, userId);
            }
        }

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("userIdPublish");
    }
}
