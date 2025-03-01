using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Ordering.Infrastructure.Headers;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public sealed class FromIdempotencyHeaderAttribute : FromHeaderAttribute
{
    public new string Name => Restful.RequestIdHeader;
}
