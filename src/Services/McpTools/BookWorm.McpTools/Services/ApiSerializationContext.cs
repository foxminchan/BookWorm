using System.Text.Json.Serialization;
using BookWorm.McpTools.Models;
using BookWorm.SharedKernel.Results;

namespace BookWorm.McpTools.Services;

[JsonSerializable(typeof(PagedResult<Book>))]
[JsonSerializable(typeof(Book))]
[JsonSerializable(typeof(List<Category>))]
[JsonSerializable(typeof(List<Author>))]
[JsonSerializable(typeof(List<Feedback>))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class ApiSerializationContext : JsonSerializerContext;
