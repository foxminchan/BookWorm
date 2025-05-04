namespace BookWorm.Chassis.Mapper;

/// <summary>
///     Defines a contract for mapping between source models and destination objects with automatic Dependency Injection
///     support.
/// </summary>
/// <typeparam name="TSource">The source model types to map from. Must be a reference type.</typeparam>
/// <typeparam name="TDestination">The destination object type to map to. Must be non-null.</typeparam>
/// <remarks>
///     This interface supports one-way mapping from domain/entity models to objects.
///     Implementations should be stateless and thread-safe.
/// </remarks>
public interface IMapper<in TSource, out TDestination>
    where TSource : class
    where TDestination : notnull
{
    /// <summary>
    ///     Maps a source model to a destination object.
    /// </summary>
    /// <param name="source">The source model to map from.</param>
    /// <returns>The mapped destination object.</returns>
    TDestination Map(TSource source);

    /// <summary>
    ///     Maps a collection of source models to a collection of destination objects.
    /// </summary>
    /// <param name="sources">The collection of source models to map from.</param>
    /// <returns>A read-only collection of mapped destination objects.</returns>
    IReadOnlyList<TDestination> Map(IReadOnlyList<TSource> sources);
}
