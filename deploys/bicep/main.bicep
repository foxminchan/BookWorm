targetScope = 'subscription'

param environmentName string

param location string

param principalId string

param postgres_username string = 'NNaaSGCrJD'

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

module bookworm 'bookworm/bookworm.bicep' = {
  name: 'bookworm'
  scope: rg
  params: {
    location: location
    userPrincipalId: principalId
  }
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

module postgres_kv 'postgres-kv/postgres-kv.bicep' = {
  name: 'postgres-kv'
  scope: rg
  params: {
    location: location
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

module redis_kv 'redis-kv/redis-kv.bicep' = {
  name: 'redis-kv'
  scope: rg
  params: {
    location: location
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

module cosmos_kv 'cosmos-kv/cosmos-kv.bicep' = {
  name: 'cosmos-kv'
  scope: rg
  params: {
    location: location
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

module bookworm_catalog_identity 'bookworm-catalog-identity/bookworm-catalog-identity.bicep' = {
  name: 'bookworm-catalog-identity'
  scope: rg
  params: {
    location: location
  }
}

module bookworm_catalog_roles_storage 'bookworm-catalog-roles-storage/bookworm-catalog-roles-storage.bicep' = {
  name: 'bookworm-catalog-roles-storage'
  scope: rg
  params: {
    location: location
    storage_outputs_name: storage.outputs.name
    principalId: bookworm_catalog_identity.outputs.principalId
  }
}

module bookworm_catalog_roles_signalr 'bookworm-catalog-roles-signalr/bookworm-catalog-roles-signalr.bicep' = {
  name: 'bookworm-catalog-roles-signalr'
  scope: rg
  params: {
    location: location
    signalr_outputs_name: signalr.outputs.name
    principalId: bookworm_catalog_identity.outputs.principalId
  }
}

module bookworm_catalog_roles_postgres_kv 'bookworm-catalog-roles-postgres-kv/bookworm-catalog-roles-postgres-kv.bicep' = {
  name: 'bookworm-catalog-roles-postgres-kv'
  scope: rg
  params: {
    location: location
    postgres_kv_outputs_name: postgres_kv.outputs.name
    principalId: bookworm_catalog_identity.outputs.principalId
  }
}

module bookworm_catalog_roles_redis_kv 'bookworm-catalog-roles-redis-kv/bookworm-catalog-roles-redis-kv.bicep' = {
  name: 'bookworm-catalog-roles-redis-kv'
  scope: rg
  params: {
    location: location
    redis_kv_outputs_name: redis_kv.outputs.name
    principalId: bookworm_catalog_identity.outputs.principalId
  }
}

module bookworm_basket_identity 'bookworm-basket-identity/bookworm-basket-identity.bicep' = {
  name: 'bookworm-basket-identity'
  scope: rg
  params: {
    location: location
  }
}

module bookworm_basket_roles_redis_kv 'bookworm-basket-roles-redis-kv/bookworm-basket-roles-redis-kv.bicep' = {
  name: 'bookworm-basket-roles-redis-kv'
  scope: rg
  params: {
    location: location
    redis_kv_outputs_name: redis_kv.outputs.name
    principalId: bookworm_basket_identity.outputs.principalId
  }
}

module bookworm_ordering_identity 'bookworm-ordering-identity/bookworm-ordering-identity.bicep' = {
  name: 'bookworm-ordering-identity'
  scope: rg
  params: {
    location: location
  }
}

module bookworm_ordering_roles_postgres_kv 'bookworm-ordering-roles-postgres-kv/bookworm-ordering-roles-postgres-kv.bicep' = {
  name: 'bookworm-ordering-roles-postgres-kv'
  scope: rg
  params: {
    location: location
    postgres_kv_outputs_name: postgres_kv.outputs.name
    principalId: bookworm_ordering_identity.outputs.principalId
  }
}

module bookworm_ordering_roles_redis_kv 'bookworm-ordering-roles-redis-kv/bookworm-ordering-roles-redis-kv.bicep' = {
  name: 'bookworm-ordering-roles-redis-kv'
  scope: rg
  params: {
    location: location
    redis_kv_outputs_name: redis_kv.outputs.name
    principalId: bookworm_ordering_identity.outputs.principalId
  }
}

module bookworm_rating_identity 'bookworm-rating-identity/bookworm-rating-identity.bicep' = {
  name: 'bookworm-rating-identity'
  scope: rg
  params: {
    location: location
  }
}

module bookworm_rating_roles_cosmos_kv 'bookworm-rating-roles-cosmos-kv/bookworm-rating-roles-cosmos-kv.bicep' = {
  name: 'bookworm-rating-roles-cosmos-kv'
  scope: rg
  params: {
    location: location
    cosmos_kv_outputs_name: cosmos_kv.outputs.name
    principalId: bookworm_rating_identity.outputs.principalId
  }
}

module bookworm_finance_identity 'bookworm-finance-identity/bookworm-finance-identity.bicep' = {
  name: 'bookworm-finance-identity'
  scope: rg
  params: {
    location: location
  }
}

module bookworm_finance_roles_postgres_kv 'bookworm-finance-roles-postgres-kv/bookworm-finance-roles-postgres-kv.bicep' = {
  name: 'bookworm-finance-roles-postgres-kv'
  scope: rg
  params: {
    location: location
    postgres_kv_outputs_name: postgres_kv.outputs.name
    principalId: bookworm_finance_identity.outputs.principalId
  }
}

output bookworm_volumes_vectordb_0 string = bookworm.outputs.volumes_vectordb_0

output bookworm_AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = bookworm.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN

output bookworm_AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = bookworm.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_ID

output bookworm_volumes_keycloak_0 string = bookworm.outputs.volumes_keycloak_0

output bookworm_catalog_identity_id string = bookworm_catalog_identity.outputs.id

output bookworm_catalog_identity_clientId string = bookworm_catalog_identity.outputs.clientId

output storage_blobEndpoint string = storage.outputs.blobEndpoint

output postgres_kv_name string = postgres_kv.outputs.name

output postgres_kv_vaultUri string = postgres_kv.outputs.vaultUri

output redis_kv_name string = redis_kv.outputs.name

output redis_kv_vaultUri string = redis_kv.outputs.vaultUri

output signalr_hostName string = signalr.outputs.hostName

output bookworm_AZURE_CONTAINER_REGISTRY_ENDPOINT string = bookworm.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT

output bookworm_AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID string = bookworm.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID

output bookworm_volumes_ollama_0 string = bookworm.outputs.volumes_ollama_0

output bookworm_basket_identity_id string = bookworm_basket_identity.outputs.id

output bookworm_basket_identity_clientId string = bookworm_basket_identity.outputs.clientId

output bookworm_ordering_identity_id string = bookworm_ordering_identity.outputs.id

output bookworm_ordering_identity_clientId string = bookworm_ordering_identity.outputs.clientId

output bookworm_rating_identity_id string = bookworm_rating_identity.outputs.id

output bookworm_rating_identity_clientId string = bookworm_rating_identity.outputs.clientId

output cosmos_kv_name string = cosmos_kv.outputs.name

output cosmos_kv_vaultUri string = cosmos_kv.outputs.vaultUri

output bookworm_finance_identity_id string = bookworm_finance_identity.outputs.id

output bookworm_finance_identity_clientId string = bookworm_finance_identity.outputs.clientId