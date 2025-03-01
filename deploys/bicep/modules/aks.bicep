@description('The name of the AKS cluster')
param clusterName string

@description('The Azure region for the cluster')
param location string

@description('The resource group name for the AKS node pools')
param nodeResourceGroup string

@description('The name of the ACR to attach to the AKS cluster')
param acrName string

@description('The VM size for the default node pool')
param nodeVmSize string = 'Standard_B2s'

@description('Tags for the AKS cluster resource')
param tags object = {
  environment: 'dev'
  application: 'BookWorm'
  service: 'aks'
}

resource acr 'Microsoft.ContainerRegistry/registries@2021-12-01-preview' existing = {
  name: acrName
}

resource aksCluster 'Microsoft.ContainerService/managedClusters@2022-05-02-preview' = {
  name: clusterName
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    nodeResourceGroup: nodeResourceGroup
    dnsPrefix: clusterName
    networkProfile: {
      networkPlugin: 'azure'
    }
    agentPoolProfiles: [
      {
        name: 'agentpool'
        count: 1
        vmSize: nodeVmSize
        mode: 'System'
      }
    ]
  }
}

// Grant AKS access to ACR
resource aksAcrRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aksCluster.id, acr.id, 'acrpull')
  scope: acr
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '7f951dda-4ed3-4680-a7ca-43fe172d538d'
    ) // AcrPull role
    principalId: aksCluster.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

output clusterName string = aksCluster.name
output controlPlaneFQDN string = aksCluster.properties.fqdn
output kubeletIdentity string = aksCluster.properties.identityProfile.kubeletidentity.objectId
