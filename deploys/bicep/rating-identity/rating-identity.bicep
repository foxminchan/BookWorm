@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource rating_identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: take('rating_identity-${uniqueString(resourceGroup().id)}', 128)
  location: location
}

output id string = rating_identity.id

output clientId string = rating_identity.properties.clientId

output principalId string = rating_identity.properties.principalId

output principalName string = rating_identity.name