@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource ordering_identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: take('ordering_identity-${uniqueString(resourceGroup().id)}', 128)
  location: location
}

output id string = ordering_identity.id

output clientId string = ordering_identity.properties.clientId

output principalId string = ordering_identity.properties.principalId

output principalName string = ordering_identity.name