using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Ordering.Infrastructure.Headers;

public sealed class FromIdempotencyHeader : FromHeaderAttribute
{
    public new string Name => Restful.RequestIdHeader;
}
