@description('Environment name for deployment')
param environmentName string = 'Production${uniqueString(resourceGroup().id)}'

@description('Azure region for all resources')
param location string = resourceGroup().location

// Resource naming variables
var resourceGroupName = 'rg-${environmentName}'
var nodeResourceGroupName = 'rg-${environmentName}-mc'
var acrName = 'acr${replace(environmentName, '-', '')}'
var aksClusterName = 'aks-${environmentName}'

// Deploy AKS Cluster
module aksModule 'modules/aks.bicep' = {
  name: 'aksDeployment'
  params: {
    clusterName: aksClusterName
    location: location
    nodeResourceGroup: nodeResourceGroupName
    acrName: acrName
  }
}

// Outputs
output aksClusterName string = aksModule.outputs.clusterName
output resourceGroupName string = resourceGroupName
