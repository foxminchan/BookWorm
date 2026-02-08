namespace BookWorm.Notification.Infrastructure.Render;

public interface IRenderer
{
    /// <summary>
    ///     Renders the specified embedded MJML template with the given model and returns compiled HTML.
    /// </summary>
    /// <typeparam name="T">The model type whose properties are substituted into the template.</typeparam>
    /// <param name="model">The model containing values for template variable substitution.</param>
    /// <param name="templateName">
    ///     The template name (without extension) matching the embedded resource under the Templates folder.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The compiled HTML output.</returns>
    Task<string> RenderAsync<T>(
        T model,
        string templateName,
        CancellationToken cancellationToken = default
    )
        where T : notnull;
}
