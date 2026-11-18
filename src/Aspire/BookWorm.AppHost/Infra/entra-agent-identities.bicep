extension microsoftGraph

@description('Microsoft Entra ID tenant ID.')
param tenantId string

@description('Object ID of the parent Entra ID service principal used for federated identity trust.')
param parentServicePrincipalId string

@description('Unique suffix scoped to the environment to avoid Entra ID uniqueName collisions.')
param uniqueSuffix string

@description('Display name for the BookWorm Agent Identity Blueprint application.')
param blueprintDisplayName string = 'BookWorm Agent Identity Blueprint'

var integratedAppTag = 'WindowsAzureActiveDirectoryIntegratedApp'
var tokenExchangeAudience = 'api://AzureADTokenExchange'
var issuer = uri(environment().authentication.loginEndpoint, '${tenantId}/v2.0')

resource agentBlueprint 'Microsoft.Graph/applications@v1.0' = {
  displayName: blueprintDisplayName
  uniqueName: 'bookworm-agent-blueprint-${uniqueSuffix}'
  signInAudience: 'AzureADMyOrg'

  resource parentFic 'federatedIdentityCredentials@v1.0' = {
    name: 'bookworm-parent-blueprint-fic'
    issuer: issuer
    subject: parentServicePrincipalId
    audiences: [
      tokenExchangeAudience
    ]
    description: 'Federated identity credential trusting the BookWorm parent Entra ID service principal.'
  }
}

resource agentBlueprintSp 'Microsoft.Graph/servicePrincipals@v1.0' = {
  tags: [
    integratedAppTag
  ]
  appId: agentBlueprint.appId
}

resource chatAgentApp 'Microsoft.Graph/applications@v1.0' = {
  displayName: 'BookWorm Chat Agent'
  uniqueName: 'bookworm-chat-agent-${uniqueSuffix}'
  signInAudience: 'AzureADMyOrg'

  resource chatFic 'federatedIdentityCredentials@v1.0' = {
    name: 'bookworm-chat-agent-fic'
    issuer: issuer
    subject: parentServicePrincipalId
    audiences: [
      tokenExchangeAudience
    ]
    description: 'Federated identity credential for BookWorm Chat agent OBO token exchange.'
  }
}

resource chatAgentSp 'Microsoft.Graph/servicePrincipals@v1.0' = {
  tags: [
    integratedAppTag
  ]
  appId: chatAgentApp.appId
}

resource ratingAgentApp 'Microsoft.Graph/applications@v1.0' = {
  displayName: 'BookWorm Rating Agent'
  uniqueName: 'bookworm-rating-agent-${uniqueSuffix}'
  signInAudience: 'AzureADMyOrg'

  resource ratingFic 'federatedIdentityCredentials@v1.0' = {
    name: 'bookworm-rating-agent-fic'
    issuer: issuer
    subject: parentServicePrincipalId
    audiences: [
      tokenExchangeAudience
    ]
    description: 'Federated identity credential for BookWorm Rating agent OBO token exchange.'
  }
}

resource ratingAgentSp 'Microsoft.Graph/servicePrincipals@v1.0' = {
  tags: [
    integratedAppTag
  ]
  appId: ratingAgentApp.appId
}

output blueprintAppId string = agentBlueprint.appId
output chatAgentId string = chatAgentApp.appId
output ratingAgentId string = ratingAgentApp.appId
