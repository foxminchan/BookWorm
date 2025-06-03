@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param aca_outputs_azure_container_apps_environment_default_domain string

param aca_outputs_azure_container_apps_environment_id string

param aca_outputs_azure_container_registry_endpoint string

param aca_outputs_azure_container_registry_managed_identity_id string

param ordering_containerimage string

param ordering_identity_outputs_id string

param ordering_containerport string

param postgres_kv_outputs_name string

@secure()
param queue_password_value string

param redis_kv_outputs_name string

param ordering_identity_outputs_clientid string

resource postgres_kv_outputs_name_kv 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: postgres_kv_outputs_name
}

resource postgres_kv_outputs_name_kv_connectionstrings__orderingdb 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = {
  name: 'connectionstrings--orderingdb'
  parent: postgres_kv_outputs_name_kv
}

resource redis_kv_outputs_name_kv 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: redis_kv_outputs_name
}

resource redis_kv_outputs_name_kv_connectionstrings__redis 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = {
  name: 'connectionstrings--redis'
  parent: redis_kv_outputs_name_kv
}

resource ordering 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'ordering'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'connectionstrings--orderingdb'
          identity: ordering_identity_outputs_id
          keyVaultUrl: postgres_kv_outputs_name_kv_connectionstrings__orderingdb.properties.secretUri
        }
        {
          name: 'connectionstrings--queue'
          value: 'amqp://guest:${queue_password_value}@queue:5672'
        }
        {
          name: 'connectionstrings--redis'
          identity: ordering_identity_outputs_id
          keyVaultUrl: redis_kv_outputs_name_kv_connectionstrings__redis.properties.secretUri
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: int(ordering_containerport)
        transport: 'http'
      }
      registries: [
        {
          server: aca_outputs_azure_container_registry_endpoint
          identity: aca_outputs_azure_container_registry_managed_identity_id
        }
      ]
    }
    environmentId: aca_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: ordering_containerimage
          name: 'ordering'
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
              value: ordering_containerport
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
              name: 'ConnectionStrings__redis'
              secretRef: 'connectionstrings--redis'
            }
            {
              name: 'services__keycloak__http__0'
              value: 'http://keycloak.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__keycloak__management__0'
              value: 'http://keycloak:9000'
            }
            {
              name: 'services__catalog__http__0'
              value: 'http://catalog.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__catalog__https__0'
              value: 'https://catalog.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__basket__http__0'
              value: 'http://basket.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__basket__https__0'
              value: 'https://basket.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'OTEL_EXPORTER_OTLP_ENDPOINT'
              value: 'http://dashboard:18889'
            }
            {
              name: 'OTEL_EXPORTER_OTLP_PROTOCOL'
              value: 'grpc'
            }
            {
              name: 'OTEL_SERVICE_NAME'
              value: 'ordering'
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: ordering_identity_outputs_clientid
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
      '${ordering_identity_outputs_id}': { }
      '${aca_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}