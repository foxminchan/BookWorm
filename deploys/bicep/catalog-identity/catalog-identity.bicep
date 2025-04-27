@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource catalog_identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: take('catalog_identity-${uniqueString(resourceGroup().id)}', 128)
  location: location
}

output id string = catalog_identity.id

output clientId string = catalog_identity.properties.clientId

output principalId string = catalog_identity.properties.principalId

output principalName string = catalog_identity.name