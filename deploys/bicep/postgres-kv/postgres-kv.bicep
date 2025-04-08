@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource postgres_kv 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: take('postgreskv-${uniqueString(resourceGroup().id)}', 24)
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
    'aspire-resource-name': 'postgres-kv'
  }
}

output vaultUri string = postgres_kv.properties.vaultUri

output name string = postgres_kv.name