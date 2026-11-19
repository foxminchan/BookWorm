using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace BookWorm.Finance.Features.Orders.Orchestration;

[OptionsValidator]
[ExcludeFromCodeCoverage]
public sealed partial class OrderStateMachineSettings : IValidateOptions<OrderStateMachineSettings>
{
    internal const string ConfigurationSection = "OrderStateMachine";

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Max attempts must be a positive integer.")]
    public int MaxAttempts { get; init; }

    [Required]
    public TimeSpan MaxRetryTimeout { get; init; }
}
