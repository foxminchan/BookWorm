@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource bookworm_notification_identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: take('bookworm_notification_identity-${uniqueString(resourceGroup().id)}', 128)
  location: location
}

output id string = bookworm_notification_identity.id

output clientId string = bookworm_notification_identity.properties.clientId

output principalId string = bookworm_notification_identity.properties.principalId

output principalName string = bookworm_notification_identity.name