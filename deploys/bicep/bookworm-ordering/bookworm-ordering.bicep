@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param bookworm_ordering_identity_outputs_id string

param bookworm_ordering_identity_outputs_clientid string

param bookworm_ordering_containerport string

param postgres_kv_outputs_name string

@secure()
param queue_password_value string

param bookworm_outputs_azure_container_apps_environment_default_domain string

param redis_kv_outputs_name string

param bookworm_outputs_azure_container_apps_environment_id string

param bookworm_outputs_azure_container_registry_endpoint string

param bookworm_outputs_azure_container_registry_managed_identity_id string

param bookworm_ordering_containerimage string

resource postgres_kv_outputs_name_kv 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: postgres_kv_outputs_name
}

resource redis_kv_outputs_name_kv 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: redis_kv_outputs_name
}

resource postgres_kv_outputs_name_kv_connectionstrings__orderingdb 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = {
  name: 'connectionstrings--orderingdb'
  parent: postgres_kv_outputs_name_kv
}

resource redis_kv_outputs_name_kv_connectionstrings__redis 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = {
  name: 'connectionstrings--redis'
  parent: redis_kv_outputs_name_kv
}

resource bookworm_ordering 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'bookworm-ordering'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'connectionstrings--orderingdb'
          identity: bookworm_ordering_identity_outputs_id
          keyVaultUrl: postgres_kv_outputs_name_kv_connectionstrings__orderingdb.properties.secretUri
        }
        {
          name: 'connectionstrings--queue'
          value: 'amqp://guest:${queue_password_value}@queue:5672'
        }
        {
          name: 'connectionstrings--redis'
          identity: bookworm_ordering_identity_outputs_id
          keyVaultUrl: redis_kv_outputs_name_kv_connectionstrings__redis.properties.secretUri
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: bookworm_ordering_containerport
        transport: 'http'
      }
      registries: [
        {
          server: bookworm_outputs_azure_container_registry_endpoint
          identity: bookworm_outputs_azure_container_registry_managed_identity_id
        }
      ]
    }
    environmentId: bookworm_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: bookworm_ordering_containerimage
          name: 'bookworm-ordering'
          env: [
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EXCEPTION_LOG_ATTRIBUTES'
              value: 'true'
            }
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EVENT_LOG_ATTRIBUTES'
              value: 'true'
            }
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY'
              value: 'in_memory'
            }
            {
              name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
              value: 'true'
            }
            {
              name: 'HTTP_PORTS'
              value: bookworm_ordering_containerport
            }
            {
              name: 'ConnectionStrings__orderingdb'
              secretRef: 'connectionstrings--orderingdb'
            }
            {
              name: 'ConnectionStrings__queue'
              secretRef: 'connectionstrings--queue'
            }
            {
              name: 'services__keycloak__http__0'
              value: 'http://keycloak.internal.${bookworm_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__keycloak__management__0'
              value: 'http://keycloak:9000'
            }
            {
              name: 'ConnectionStrings__redis'
              secretRef: 'connectionstrings--redis'
            }
            {
              name: 'services__bookworm-catalog__http__0'
              value: 'http://bookworm-catalog.internal.${bookworm_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__bookworm-catalog__https__0'
              value: 'https://bookworm-catalog.internal.${bookworm_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__bookworm-basket__http__0'
              value: 'http://bookworm-basket.internal.${bookworm_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__bookworm-basket__https__0'
              value: 'https://bookworm-basket.internal.${bookworm_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: bookworm_ordering_identity_outputs_clientid
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${bookworm_ordering_identity_outputs_id}': { }
      '${bookworm_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}