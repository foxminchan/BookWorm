@description('The name of the AKS cluster')
param clusterName string

@description('The Azure region for the cluster')
param location string

@description('The resource group name for the AKS node pools')
param nodeResourceGroup string

@description('The VM size for the default node pool')
param nodeVmSize string = 'Standard_B2s'

@description('Tags for the AKS cluster resource')
param tags object = {
  environment: 'dev'
  application: 'BookWorm'
  service: 'aks'
}

resource aksCluster 'Microsoft.ContainerService/managedClusters@2022-05-02-preview' = {
  name: clusterName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  tags: tags
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

output clusterName string = aksCluster.name
output controlPlaneFQDN string = aksCluster.properties.fqdn
output kubeletIdentity string = aksCluster.properties.identityProfile.kubeletidentity.objectId
