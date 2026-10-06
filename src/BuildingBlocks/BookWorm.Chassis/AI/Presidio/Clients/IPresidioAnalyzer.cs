using Refit;

namespace BookWorm.Chassis.AI.Presidio.Clients;

public interface IPresidioAnalyzer
{
    [Post("/analyze")]
    Task<AnalyzerResponse[]> AnalyzeAsync(
        [Body] AnalyzerRequest request,
        CancellationToken cancellationToken = default
    );
}
