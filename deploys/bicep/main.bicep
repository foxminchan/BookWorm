targetScope = 'subscription'

param environmentName string

param location string

param principalId string

param postgres_username string = 'pwgDQpwYuw'

@secure()
param postgres_password string

var tags = {
  'aspire-env-name': environmentName
}

resource rg 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

module postgres 'postgres/postgres.bicep' = {
  name: 'postgres'
  scope: rg
  params: {
    location: location
    administratorLogin: postgres_username
    administratorLoginPassword: postgres_password
    keyVaultName: postgres_kv.outputs.name
  }
}

module redis 'redis/redis.bicep' = {
  name: 'redis'
  scope: rg
  params: {
    location: location
    keyVaultName: redis_kv.outputs.name
  }
}

module cosmos 'cosmos/cosmos.bicep' = {
  name: 'cosmos'
  scope: rg
  params: {
    location: location
    keyVaultName: cosmos_kv.outputs.name
  }
}

module storage 'storage/storage.bicep' = {
  name: 'storage'
  scope: rg
  params: {
    location: location
  }
}

module signalr 'signalr/signalr.bicep' = {
  name: 'signalr'
  scope: rg
  params: {
    location: location
  }
}

module cosmos_kv 'cosmos-kv/cosmos-kv.bicep' = {
  name: 'cosmos-kv'
  scope: rg
  params: {
    location: location
  }
}

module postgres_kv 'postgres-kv/postgres-kv.bicep' = {
  name: 'postgres-kv'
  scope: rg
  params: {
    location: location
  }
}

module redis_kv 'redis-kv/redis-kv.bicep' = {
  name: 'redis-kv'
  scope: rg
  params: {
    location: location
  }
}

module storage_roles 'storage-roles/storage-roles.bicep' = {
  name: 'storage-roles'
  scope: rg
  params: {
    location: location
    storage_outputs_name: storage.outputs.name
    principalType: ''
    principalId: ''
  }
}

module signalr_roles 'signalr-roles/signalr-roles.bicep' = {
  name: 'signalr-roles'
  scope: rg
  params: {
    location: location
    signalr_outputs_name: signalr.outputs.name
    principalType: ''
    principalId: ''
  }
}

module cosmos_kv_roles 'cosmos-kv-roles/cosmos-kv-roles.bicep' = {
  name: 'cosmos-kv-roles'
  scope: rg
  params: {
    location: location
    cosmos_kv_outputs_name: cosmos_kv.outputs.name
    principalType: ''
    principalId: ''
  }
}

module postgres_kv_roles 'postgres-kv-roles/postgres-kv-roles.bicep' = {
  name: 'postgres-kv-roles'
  scope: rg
  params: {
    location: location
    postgres_kv_outputs_name: postgres_kv.outputs.name
    principalType: ''
    principalId: ''
  }
}

module redis_kv_roles 'redis-kv-roles/redis-kv-roles.bicep' = {
  name: 'redis-kv-roles'
  scope: rg
  params: {
    location: location
    redis_kv_outputs_name: redis_kv.outputs.name
    principalType: ''
    principalId: ''
  }
}