namespace BookWorm.Chassis.Mapper;

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
