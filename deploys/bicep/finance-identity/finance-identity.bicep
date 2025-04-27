@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource finance_identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: take('finance_identity-${uniqueString(resourceGroup().id)}', 128)
  location: location
}

output id string = finance_identity.id

output clientId string = finance_identity.properties.clientId

output principalId string = finance_identity.properties.principalId

output principalName string = finance_identity.name