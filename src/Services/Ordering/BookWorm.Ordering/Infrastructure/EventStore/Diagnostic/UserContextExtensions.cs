using BookWorm.Chassis.EventBus;

namespace BookWorm.Ordering.Infrastructure.EventStore.Diagnostic;

internal static class DocumentSessionUserContextExtensions
{
    extension(IDocumentSession documentSession)
    {
        /// <summary>
        ///     Propagates the originating user identifier from the supplied
        ///     <see cref="ClaimsPrincipal" /> into the Marten document session headers
        ///     so that it is persisted alongside the events in the event store.
        /// </summary>
        /// <remarks>
        ///     The header is later read by the Marten subscription (e.g.
        ///     <c>MartenEventPublisher</c>) and forwarded onto the integration event
        ///     publish context, ensuring the user context survives the asynchronous
        ///     hop between the request scope and the daemon scope where
        ///     <c>HttpContext</c> is unavailable.
        /// </remarks>
        public void PropagateUserId(ClaimsPrincipal? claimsPrincipal)
        {
            var userId = claimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            documentSession.SetHeader(EventBusHeaders.UserId, userId);
        }
    }
}
