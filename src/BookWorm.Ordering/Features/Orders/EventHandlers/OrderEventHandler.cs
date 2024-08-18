﻿using System.Diagnostics;
using BookWorm.Core.SeedWork;
using BookWorm.Ordering.Domain.OrderAggregate.Events;
using MediatR;

namespace BookWorm.Ordering.Features.Orders.EventHandlers;

public sealed class OrderEventHandler(IDocumentSession documentSession, ILogger<OrderEventHandler> logger)
    : INotificationHandler<OrderCreatedEvent>, INotificationHandler<OrderCompletedEvent>,
        INotificationHandler<OrderCancelledEvent>
{
    public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{Event}] - Handling {OrderId}", nameof(OrderCancelledEvent), notification.Id);
        await WriteToAggregateAsync(notification, cancellationToken);
    }

    public async Task Handle(OrderCompletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{Event}] - Handling {OrderId}", nameof(OrderCompletedEvent), notification.Id);
        await WriteToAggregateAsync(notification, cancellationToken);
    }

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{Event}] - Handling {OrderId}", nameof(OrderCreatedEvent), notification.Id);
        await WriteToAggregateAsync(notification, cancellationToken);
    }

    private async Task WriteToAggregateAsync(EventBase @event, CancellationToken cancellationToken)
    {
        using var activity = new ActivitySource(nameof(Marten))
            .StartActivity($"EventStore: {@event.GetType().Name}");

        if (activity is not null)
        {
            activity.AddTag("event.type", @event.GetType().FullName);
            activity.AddTag("event.dateoccurred", @event.DateOccurred.ToString("O"));
        }

        try
        {
            await documentSession.Events.WriteToAggregate<OrderState>(Guid.NewGuid(),
                stream => { stream.AppendOne(@event); }, cancellationToken);
        }
        catch (Exception e)
        {
            if (activity is not null)
            {
                activity.AddTag("exception.message", e.Message);
                activity.AddTag("exception.stacktrace", e.ToString());
                activity.AddTag("exception.type", e.GetType().FullName);
                activity.SetStatus(ActivityStatusCode.Error);
            }

            logger.LogError(e, "[{Event}] - Failed to write event to aggregate", @event.GetType().Name);
        }
    }
}
