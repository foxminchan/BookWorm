@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource basket_identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: take('basket_identity-${uniqueString(resourceGroup().id)}', 128)
  location: location
}

output id string = basket_identity.id

output clientId string = basket_identity.properties.clientId

output principalId string = basket_identity.properties.principalId

output principalName string = basket_identity.name