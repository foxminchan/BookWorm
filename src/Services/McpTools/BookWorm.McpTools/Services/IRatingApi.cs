using BookWorm.McpTools.Models;
using Refit;

namespace BookWorm.McpTools.Services;

public interface IRatingApi
{
    [Get("/api/v1/feedbacks")]
    Task<ApiResponse<List<Feedback>>> ListFeedbacksAsync(Guid bookId);
}
