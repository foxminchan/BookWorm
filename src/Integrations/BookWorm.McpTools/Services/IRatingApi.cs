using BookWorm.McpTools.Models;
using BookWorm.SharedKernel.Results;
using Refit;

namespace BookWorm.McpTools.Services;

public interface IRatingApi
{
    [Get("/api/v1/feedbacks")]
    [QueryUriFormat(UriFormat.Unescaped)]
    Task<ApiResponse<PagedResult<Feedback>>> ListFeedbacksAsync(Guid bookId);
}
