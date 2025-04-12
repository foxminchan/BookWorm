@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource bookworm_finance_identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: take('bookworm_finance_identity-${uniqueString(resourceGroup().id)}', 128)
  location: location
}

output id string = bookworm_finance_identity.id

output clientId string = bookworm_finance_identity.properties.clientId

output principalId string = bookworm_finance_identity.properties.principalId

output principalName string = bookworm_finance_identity.name