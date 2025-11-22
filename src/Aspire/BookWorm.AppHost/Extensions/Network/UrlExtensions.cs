namespace BookWorm.AppHost.Extensions.Network;

public static class UrlExtensions
{
    extension<T>(IResourceBuilder<T> builder)
        where T : IResource
    {
        public IResourceBuilder<T> WithFriendlyUrls(
            string? displayText = null,
            string? endpointName = null,
            string? path = null
        )
        {
            if (builder.ApplicationBuilder.ExecutionContext.IsPublishMode)
            {
                return builder;
            }

            if (builder is IResourceBuilder<ProjectResource> projectBuilder)
            {
                projectBuilder.WithHttpHealthCheck(Http.Endpoints.HealthEndpointPath);
            }

            return builder.WithUrls(c =>
            {
                List<string?> endpointNames = [endpointName, Http.Schemes.Https, Http.Schemes.Http];

                var endpoint = endpointNames
                    .Where(name => name is not null)
                    .Select(name => c.GetEndpoint(name!))
                    .FirstOrDefault(e => e?.Exists ?? false);

                if (endpoint is null)
                {
                    return;
                }

                displayText ??= builder.Resource.Name;

                foreach (var url in c.Urls)
                {
                    url.DisplayLocation = UrlDisplayLocation.DetailsOnly;
                }

                c.Urls.Add(
                    new()
                    {
                        Endpoint = endpoint,
                        DisplayText = displayText,
                        DisplayLocation = UrlDisplayLocation.SummaryAndDetails,
                        Url = path ?? "/",
                    }
                );
            });
        }
    }
}
