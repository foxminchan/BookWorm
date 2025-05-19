targetScope = 'subscription'

param resourceGroupName string

param location string

param principalId string

param postgres_username string = 'wBpFMdaVMg'

@secure()
param postgres_password string

resource rg 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: resourceGroupName
  location: location
}

module aca 'aca/aca.bicep' = {
  name: 'aca'
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

module catalog_identity 'catalog-identity/catalog-identity.bicep' = {
  name: 'catalog-identity'
  scope: rg
  params: {
    location: location
  }
}

module catalog_roles_storage 'catalog-roles-storage/catalog-roles-storage.bicep' = {
  name: 'catalog-roles-storage'
  scope: rg
  params: {
    location: location
    storage_outputs_name: storage.outputs.name
    principalId: catalog_identity.outputs.principalId
  }
}

module catalog_roles_postgres_kv 'catalog-roles-postgres-kv/catalog-roles-postgres-kv.bicep' = {
  name: 'catalog-roles-postgres-kv'
  scope: rg
  params: {
    location: location
    postgres_kv_outputs_name: postgres_kv.outputs.name
    principalId: catalog_identity.outputs.principalId
  }
}

module catalog_roles_redis_kv 'catalog-roles-redis-kv/catalog-roles-redis-kv.bicep' = {
  name: 'catalog-roles-redis-kv'
  scope: rg
  params: {
    location: location
    redis_kv_outputs_name: redis_kv.outputs.name
    principalId: catalog_identity.outputs.principalId
  }
}

module chatting_identity 'chatting-identity/chatting-identity.bicep' = {
  name: 'chatting-identity'
  scope: rg
  params: {
    location: location
  }
}

module chatting_roles_signalr 'chatting-roles-signalr/chatting-roles-signalr.bicep' = {
  name: 'chatting-roles-signalr'
  scope: rg
  params: {
    location: location
    signalr_outputs_name: signalr.outputs.name
    principalId: chatting_identity.outputs.principalId
  }
}

module chatting_roles_redis_kv 'chatting-roles-redis-kv/chatting-roles-redis-kv.bicep' = {
  name: 'chatting-roles-redis-kv'
  scope: rg
  params: {
    location: location
    redis_kv_outputs_name: redis_kv.outputs.name
    principalId: chatting_identity.outputs.principalId
  }
}

module basket_identity 'basket-identity/basket-identity.bicep' = {
  name: 'basket-identity'
  scope: rg
  params: {
    location: location
  }
}

module basket_roles_redis_kv 'basket-roles-redis-kv/basket-roles-redis-kv.bicep' = {
  name: 'basket-roles-redis-kv'
  scope: rg
  params: {
    location: location
    redis_kv_outputs_name: redis_kv.outputs.name
    principalId: basket_identity.outputs.principalId
  }
}

module notification_identity 'notification-identity/notification-identity.bicep' = {
  name: 'notification-identity'
  scope: rg
  params: {
    location: location
  }
}

module notification_roles_storage 'notification-roles-storage/notification-roles-storage.bicep' = {
  name: 'notification-roles-storage'
  scope: rg
  params: {
    location: location
    storage_outputs_name: storage.outputs.name
    principalId: notification_identity.outputs.principalId
  }
}

module ordering_identity 'ordering-identity/ordering-identity.bicep' = {
  name: 'ordering-identity'
  scope: rg
  params: {
    location: location
  }
}

module ordering_roles_postgres_kv 'ordering-roles-postgres-kv/ordering-roles-postgres-kv.bicep' = {
  name: 'ordering-roles-postgres-kv'
  scope: rg
  params: {
    location: location
    postgres_kv_outputs_name: postgres_kv.outputs.name
    principalId: ordering_identity.outputs.principalId
  }
}

module ordering_roles_redis_kv 'ordering-roles-redis-kv/ordering-roles-redis-kv.bicep' = {
  name: 'ordering-roles-redis-kv'
  scope: rg
  params: {
    location: location
    redis_kv_outputs_name: redis_kv.outputs.name
    principalId: ordering_identity.outputs.principalId
  }
}

module rating_identity 'rating-identity/rating-identity.bicep' = {
  name: 'rating-identity'
  scope: rg
  params: {
    location: location
  }
}

module rating_roles_postgres_kv 'rating-roles-postgres-kv/rating-roles-postgres-kv.bicep' = {
  name: 'rating-roles-postgres-kv'
  scope: rg
  params: {
    location: location
    postgres_kv_outputs_name: postgres_kv.outputs.name
    principalId: rating_identity.outputs.principalId
  }
}

module finance_identity 'finance-identity/finance-identity.bicep' = {
  name: 'finance-identity'
  scope: rg
  params: {
    location: location
  }
}

module finance_roles_postgres_kv 'finance-roles-postgres-kv/finance-roles-postgres-kv.bicep' = {
  name: 'finance-roles-postgres-kv'
  scope: rg
  params: {
    location: location
    postgres_kv_outputs_name: postgres_kv.outputs.name
    principalId: finance_identity.outputs.principalId
  }
}

output aca_AZURE_CONTAINER_REGISTRY_NAME string = aca.outputs.AZURE_CONTAINER_REGISTRY_NAME

output aca_AZURE_CONTAINER_REGISTRY_ENDPOINT string = aca.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT

output aca_AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID string = aca.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID

output aca_AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = aca.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN

output aca_AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = aca.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_ID

output aca_volumes_vectordb_0 string = aca.outputs.volumes_vectordb_0

output aca_volumes_ollama_0 string = aca.outputs.volumes_ollama_0

output aca_volumes_keycloak_0 string = aca.outputs.volumes_keycloak_0

output catalog_identity_id string = catalog_identity.outputs.id

output storage_blobEndpoint string = storage.outputs.blobEndpoint

output postgres_kv_name string = postgres_kv.outputs.name

output postgres_kv_vaultUri string = postgres_kv.outputs.vaultUri

output redis_kv_name string = redis_kv.outputs.name

output redis_kv_vaultUri string = redis_kv.outputs.vaultUri

output catalog_identity_clientId string = catalog_identity.outputs.clientId

output chatting_identity_id string = chatting_identity.outputs.id

output signalr_hostName string = signalr.outputs.hostName

output chatting_identity_clientId string = chatting_identity.outputs.clientId

output basket_identity_id string = basket_identity.outputs.id

output basket_identity_clientId string = basket_identity.outputs.clientId

output notification_identity_id string = notification_identity.outputs.id

output storage_tableEndpoint string = storage.outputs.tableEndpoint

output notification_identity_clientId string = notification_identity.outputs.clientId

output ordering_identity_id string = ordering_identity.outputs.id

output ordering_identity_clientId string = ordering_identity.outputs.clientId

output rating_identity_id string = rating_identity.outputs.id

output rating_identity_clientId string = rating_identity.outputs.clientId

output finance_identity_id string = finance_identity.outputs.id

output finance_identity_clientId string = finance_identity.outputs.clientId