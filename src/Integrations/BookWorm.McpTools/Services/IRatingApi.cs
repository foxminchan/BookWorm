using BookWorm.McpTools.Models;
using BookWorm.McpTools.Tools;
using BookWorm.SharedKernel.Results;
using Refit;

namespace BookWorm.McpTools.Services;

public interface IRatingApi
{
    [Get("/api/v1/feedbacks")]
    Task<PagedResult<Rating>> ListRatingsAsync(FeedbackQueryParams queryParams);
}
