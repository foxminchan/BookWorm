using System.ComponentModel.DataAnnotations;

namespace BookWorm.Ordering.Infrastructure.Idempotency;

public sealed class ClientRequest
{
    public string Id { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public DateTime Time { get; set; } = DateTime.UtcNow;
}
