namespace BookWorm.AppHost.Extensions.Dashboard;

internal static class ResourceGroups
{
    public const string Edge = "edge";
    public const string Services = "services";
    public const string Functions = "functions";
    public const string Data = "data";
    public const string Messaging = "messaging";
    public const string AI = "ai";
    public const string Security = "security";
    public const string Tooling = "tooling";
}

internal static class ResourceDisplayOrder
{
    public const int Edge = 100;
    public const int Services = 50;
    public const int Functions = 40;
    public const int Infrastructure = 0;
    public const int Tooling = -50;
}

internal static class DashboardExtensions
{
    extension<T>(IResourceBuilder<T> builder)
        where T : IResource
    {
        /// <summary>
        ///     Tags the resource with a logical group and a relative display order so the redesigned
        ///     Aspire 17 dashboard can group, filter, and sort resources consistently.
        /// </summary>
        /// <param name="group">The logical group tag (see <see cref="ResourceGroups" />).</param>
        /// <param name="displayOrder">The relative sort key (see <see cref="ResourceDisplayOrder" />).</param>
        /// <returns>The resource builder for method chaining.</returns>
        public IResourceBuilder<T> InGroup(
            string group,
            int displayOrder = ResourceDisplayOrder.Infrastructure
        )
        {
            return builder.ApplicationBuilder.ExecutionContext.IsPublishMode
                ? builder
                : builder.WithTags(group).WithDisplayOrder(displayOrder);
        }
    }
}
