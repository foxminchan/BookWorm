@description('Environment name for deployment')
param environmentName string = 'Production${uniqueString(resourceGroup().id)}'

@description('Azure region for all resources')
param location string = resourceGroup().location

// Resource naming variables
var resourceGroupName = 'rg-${environmentName}'
var nodeResourceGroupName = 'rg-${environmentName}-mc'
var acrName = 'acr${replace(environmentName, '-', '')}'
var aksClusterName = 'aks-${environmentName}'

// Deploy Azure Container Registry
module acrModule 'modules/acr.bicep' = {
  name: 'acrDeployment'
  params: {
    acrName: acrName
    location: location
  }
}

// Deploy AKS Cluster
module aksModule 'modules/aks.bicep' = {
  name: 'aksDeployment'
  params: {
    clusterName: aksClusterName
    location: location
    nodeResourceGroup: nodeResourceGroupName
    acrName: acrName
  }
  dependsOn: [
    acrModule
  ]
}

// Outputs
output acrLoginServer string = acrModule.outputs.acrLoginServer
output aksClusterName string = aksModule.outputs.clusterName
output resourceGroupName string = resourceGroupName
