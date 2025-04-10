@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource redis_kv 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: take('rediskv-${uniqueString(resourceGroup().id)}', 24)
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
    'aspire-resource-name': 'redis-kv'
  }
}

output vaultUri string = redis_kv.properties.vaultUri

output name string = redis_kv.name