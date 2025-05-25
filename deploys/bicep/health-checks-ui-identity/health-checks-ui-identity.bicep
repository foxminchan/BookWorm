@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource health_checks_ui_identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: take('health_checks_ui_identity-${uniqueString(resourceGroup().id)}', 128)
  location: location
}

output id string = health_checks_ui_identity.id

output clientId string = health_checks_ui_identity.properties.clientId

output principalId string = health_checks_ui_identity.properties.principalId

output principalName string = health_checks_ui_identity.name