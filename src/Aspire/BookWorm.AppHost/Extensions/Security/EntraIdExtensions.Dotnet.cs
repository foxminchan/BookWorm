namespace BookWorm.AppHost.Extensions.Security;

internal static partial class EntraIdExtensions
{
    extension<TResource>(IResourceBuilder<TResource> builder)
        where TResource : IDotnetProgramResource, IResourceWithEnvironment
    {
        /// <summary>
        ///     Exposes read/write scopes for the .NET resource on the Microsoft Entra ID application registration
        ///     and injects its <c>AzureAd</c> configuration.
        /// </summary>
        /// <param name="entraId">The Entra ID application resource builder.</param>
        /// <returns>The resource builder for method chaining.</returns>
        public IResourceBuilder<TResource> WithEntraId(
            IResourceBuilder<EntraIdApplicationResource> entraId
        )
        {
            var clientId = builder.Resource.Name;
            var clientName = Services.ToClientName(clientId, "API");

            return builder.WithReference(
                entraId
                    .WithScope(
                        $"{clientId}_{Authorization.Actions.Read}",
                        $"{nameof(Authorization.Actions.Read)} for {clientName}"
                    )
                    .WithScope(
                        $"{clientId}_{Authorization.Actions.Write}",
                        $"{nameof(Authorization.Actions.Write)} for {clientName}"
                    )
            );
        }

        /// <summary>
        ///     Injects <c>AgentIdentities</c> configuration outputs from the Bicep-provisioned
        ///     Entra Agent Identities resource into the .NET resource.
        /// </summary>
        /// <param name="agentIdentities">The Bicep agent identities resource builder, or <see langword="null" /> in run mode.</param>
        /// <param name="defaultAgentOutputName">The Bicep output name for <c>AgentIdentities:DefaultAgentId</c>.</param>
        /// <param name="agentOverrides">Optional per-service agent ID overrides mapped to Bicep output names.</param>
        /// <returns>The resource builder for method chaining.</returns>
        public IResourceBuilder<TResource> WithEntraAgentIdentity(
            IResourceBuilder<AzureBicepResource>? agentIdentities,
            string defaultAgentOutputName,
            params (string ServiceKey, string OutputName)[] agentOverrides
        )
        {
            if (agentIdentities is null)
            {
                return builder;
            }

            builder.WithEnvironment(
                "AgentIdentities__DefaultAgentId",
                agentIdentities.GetOutput(defaultAgentOutputName)
            );

            foreach (var (serviceKey, outputName) in agentOverrides)
            {
                builder.WithEnvironment(
                    $"AgentIdentities__Agents__{serviceKey}",
                    agentIdentities.GetOutput(outputName)
                );
            }

            return builder;
        }
    }
}
