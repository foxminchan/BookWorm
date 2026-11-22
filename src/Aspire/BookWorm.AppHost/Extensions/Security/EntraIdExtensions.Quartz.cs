namespace BookWorm.AppHost.Extensions.Security;

internal static partial class EntraIdExtensions
{
    extension(IResourceBuilder<QuartzDashboardResource> builder)
    {
        /// <summary>
        ///     Configures the Quartz dashboard container resource to authenticate visitors using
        ///     Microsoft Entra ID via OpenID Connect and maps the <see cref="Authorization.Roles.Admin" />
        ///     role to dashboard administrators.
        /// </summary>
        /// <param name="entraId">The Entra ID application resource builder.</param>
        /// <returns>The Quartz dashboard resource builder for method chaining.</returns>
        public IResourceBuilder<QuartzDashboardResource> WithEntraId(
            IResourceBuilder<EntraIdApplicationResource> entraId
        )
        {
            entraId.WithRedirectUris(builder.GetEndpoint(Uri.UriSchemeHttp), "/signin-oidc");

            return builder
                .WithEnvironment(context =>
                {
                    context.EnvironmentVariables.Remove(
                        "Quartz__Dashboard__Authentication__Tokens__Administrator"
                    );
                    context.EnvironmentVariables.Remove(
                        "Quartz__Dashboard__Authentication__AllowTokenInUrl"
                    );

                    context.EnvironmentVariables["Quartz__Dashboard__Authentication__Mode"] =
                        "OpenIdConnect";
                    context.EnvironmentVariables["Quartz__Dashboard__Authentication__Authority"] =
                        ReferenceExpression.Create(
                            $"{entraId.Resource.Instance.TrimEnd('/')}/{entraId.Resource.TenantIdOutputReference}/v2.0"
                        );
                    context.EnvironmentVariables["Quartz__Dashboard__Authentication__ClientId"] =
                        entraId.Resource.ClientIdOutputReference;
                    context.EnvironmentVariables["Quartz__Dashboard__Roles__Administrators__0"] =
                        Authorization.Roles.Admin;
                })
                .WithUrls(context =>
                {
                    foreach (
                        var url in context.Urls.Where(url =>
                            url.Endpoint is { EndpointName: "http", Exists: true }
                        )
                    )
                    {
                        url.Url = $"{url.Endpoint?.Url.TrimEnd('/')}/quartz";
                        url.DisplayText = "Quartz dashboard";
                    }
                });
        }
    }
}
