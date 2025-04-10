@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource cosmos_kv 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: take('cosmoskv-${uniqueString(resourceGroup().id)}', 24)
  location: location
  properties: {
    tenantId: tenant().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: true
  }
  tags: {
    'aspire-resource-name': 'cosmos-kv'
  }
}

output vaultUri string = cosmos_kv.properties.vaultUri

output name string = cosmos_kv.name