@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource bookworm_basket_identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: take('bookworm_basket_identity-${uniqueString(resourceGroup().id)}', 128)
  location: location
}

output id string = bookworm_basket_identity.id

output clientId string = bookworm_basket_identity.properties.clientId

output principalId string = bookworm_basket_identity.properties.principalId

output principalName string = bookworm_basket_identity.name