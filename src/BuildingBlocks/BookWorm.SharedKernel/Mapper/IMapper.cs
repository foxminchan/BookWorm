namespace BookWorm.SharedKernel.Mapper;

/// <summary>
///     Defines a contract for mapping between source models and destination objects with automatic Dependency Injection
///     support.
/// </summary>
/// <typeparam name="TSource">The source model types to map from. Must be a reference type.</typeparam>
/// <typeparam name="TDestination">The destination object type to map to. Must be non-null.</typeparam>
/// <remarks>
///     This interface supports one-way mapping from domain/entity models to DTOs or view models.
///     Implementations should be stateless and thread-safe.
/// </remarks>
public interface IMapper<in TSource, out TDestination>
    where TSource : class
    where TDestination : notnull
{
    /// <summary>
    ///     Maps a source model to a destination DTO.
    /// </summary>
    /// <param name="model">The source model to map from.</param>
    /// <returns>The mapped destination DTO.</returns>
    TDestination MapToDto(TSource model);

    /// <summary>
    ///     Maps a collection of source models to a collection of destination DTOs.
    /// </summary>
    /// <param name="models">The collection of source models to map from.</param>
    /// <returns>A read-only collection of mapped destination DTOs.</returns>
    IReadOnlyList<TDestination> MapToDtos(IReadOnlyList<TSource> models);
}
