using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace BookWorm.Finance.Saga;

[OptionsValidator]
[ExcludeFromCodeCoverage]
public sealed partial class OrderStateMachineSettings : IValidateOptions<OrderStateMachineSettings>
{
    internal const string ConfigurationSection = "OrderStateMachine";

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Max attempts must be a positive integer.")]
    public int MaxAttempts { get; set; }

    [Required]
    public TimeSpan MaxRetryTimeout { get; set; }
}
