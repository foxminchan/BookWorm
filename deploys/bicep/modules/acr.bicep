@description('The name of the Azure Container Registry')
param acrName string

@description('The Azure region for the ACR')
param location string

@description('Tags for the ACR resource')
param tags object = {
  environment: 'Production'
  project: 'BookWorm'
  service: 'acr'
}

resource acr 'Microsoft.ContainerRegistry/registries@2021-12-01-preview' = {
  name: acrName
  location: location
  tags: tags
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

output acrId string = acr.id
output acrLoginServer string = acr.properties.loginServer
