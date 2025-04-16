namespace BookWorm.Notification.Infrastructure.Render;

public interface IRenderer
{
    string Render<T>(T model, string template)
        where T : notnull;
}
