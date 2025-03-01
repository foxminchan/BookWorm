namespace BookWorm.SharedKernel.SeedWork;

/// <summary>
///     Provides a way to map from model to DTO with injected dependencies.
/// </summary>
public interface IMapper<in TModel, out TDto>
    where TModel : class
    where TDto : notnull
{
    /// <summary>
    ///     Maps the model to a DTO.
    /// </summary>
    /// <param name="model">The model to map.</param>
    /// <returns>The DTO.</returns>
    TDto MapToDto(TModel model);

    /// <summary>
    ///     Maps a list of models to a list of DTOs.
    /// </summary>
    /// <param name="models">The list of models to map.</param>
    /// <returns>The list of DTOs.</returns>
    IReadOnlyList<TDto> MapToDtos(IReadOnlyList<TModel> models);
}
