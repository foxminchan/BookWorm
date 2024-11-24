using Ardalis.Result;

namespace BookWorm.Core.SeedWork;

public sealed record PagedItems<T>(PagedInfo PagedInfo, List<T> Data);
