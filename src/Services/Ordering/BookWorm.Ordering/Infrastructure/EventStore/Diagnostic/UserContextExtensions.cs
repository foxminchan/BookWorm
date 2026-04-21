namespace BookWorm.Ordering.Infrastructure.EventStore.Diagnostic;

internal static class DocumentSessionUserContextExtensions
{
    extension(IDocumentSession documentSession)
    {
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
