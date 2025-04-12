@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param bookworm_catalog_identity_outputs_id string

param bookworm_catalog_identity_outputs_clientid string

param bookworm_catalog_containerport string

param bookworm_outputs_azure_container_apps_environment_default_domain string

param storage_outputs_blobendpoint string

@secure()
param queue_password_value string

param postgres_kv_outputs_name string

@secure()
param vectordb_key_value string

param redis_kv_outputs_name string

param signalr_outputs_hostname string

param bookworm_outputs_azure_container_apps_environment_id string

param bookworm_outputs_azure_container_registry_endpoint string

param bookworm_outputs_azure_container_registry_managed_identity_id string

param bookworm_catalog_containerimage string

resource postgres_kv_outputs_name_kv 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: postgres_kv_outputs_name
}

resource redis_kv_outputs_name_kv 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: redis_kv_outputs_name
}

resource postgres_kv_outputs_name_kv_connectionstrings__catalogdb 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = {
  name: 'connectionstrings--catalogdb'
  parent: postgres_kv_outputs_name_kv
}

resource redis_kv_outputs_name_kv_connectionstrings__redis 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = {
  name: 'connectionstrings--redis'
  parent: redis_kv_outputs_name_kv
}

resource bookworm_catalog 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'bookworm-catalog'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'connectionstrings--queue'
          value: 'amqp://guest:${queue_password_value}@queue:5672'
        }
        {
          name: 'connectionstrings--catalogdb'
          identity: bookworm_catalog_identity_outputs_id
          keyVaultUrl: postgres_kv_outputs_name_kv_connectionstrings__catalogdb.properties.secretUri
        }
        {
          name: 'connectionstrings--vectordb'
          value: 'Endpoint=${'http://vectordb.internal.${bookworm_outputs_azure_container_apps_environment_default_domain}'};Key=${vectordb_key_value}'
        }
        {
          name: 'connectionstrings--vectordb-http'
          value: 'Endpoint=http://vectordb:6333;Key=${vectordb_key_value}'
        }
        {
          name: 'connectionstrings--redis'
          identity: bookworm_catalog_identity_outputs_id
          keyVaultUrl: redis_kv_outputs_name_kv_connectionstrings__redis.properties.secretUri
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: bookworm_catalog_containerport
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
          image: bookworm_catalog_containerimage
          name: 'bookworm-catalog'
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
              value: bookworm_catalog_containerport
            }
            {
              name: 'ConnectionStrings__embedding'
              value: 'Endpoint=http://${'ollama.internal.${bookworm_outputs_azure_container_apps_environment_default_domain}'}:80;Model=nomic-embed-text:latest'
            }
            {
              name: 'ConnectionStrings__chat'
              value: 'Endpoint=http://${'ollama.internal.${bookworm_outputs_azure_container_apps_environment_default_domain}'}:80;Model=deepseek-r1:1.5b'
            }
            {
              name: 'ConnectionStrings__blob'
              value: storage_outputs_blobendpoint
            }
            {
              name: 'ConnectionStrings__queue'
              secretRef: 'connectionstrings--queue'
            }
            {
              name: 'ConnectionStrings__catalogdb'
              secretRef: 'connectionstrings--catalogdb'
            }
            {
              name: 'ConnectionStrings__vectordb'
              secretRef: 'connectionstrings--vectordb'
            }
            {
              name: 'ConnectionStrings__vectordb_http'
              secretRef: 'connectionstrings--vectordb-http'
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
              name: 'ConnectionStrings__signalr'
              value: 'Endpoint=https://${signalr_outputs_hostname};AuthType=azure'
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: bookworm_catalog_identity_outputs_clientid
            }
          ]
        }
      ]
      scale: {
        minReplicas: 2
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${bookworm_catalog_identity_outputs_id}': { }
      '${bookworm_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}