@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource chatting_identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: take('chatting_identity-${uniqueString(resourceGroup().id)}', 128)
  location: location
}

output id string = chatting_identity.id

output clientId string = chatting_identity.properties.clientId

output principalId string = chatting_identity.properties.principalId

output principalName string = chatting_identity.name