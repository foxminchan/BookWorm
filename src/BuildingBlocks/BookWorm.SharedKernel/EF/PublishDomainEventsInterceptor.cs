using BookWorm.SharedKernel.Mediator;
using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BookWorm.SharedKernel.EF;

public sealed class PublishDomainEventsInterceptor(IPublisher publisher) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is not null)
        {
            await publisher.DispatchDomainEventsAsync(eventData.Context);
        }

        return result;
    }
}
