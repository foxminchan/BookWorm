using Refit;

namespace BookWorm.Chassis.AI.Presidio;

public interface IPresidioAnonymizer
{
    [Post("/anonymize")]
    Task<AnonymizerResponse> AnonymizeAsync(
        [Body] AnonymizerRequest request,
        CancellationToken cancellationToken = default
    );
}
