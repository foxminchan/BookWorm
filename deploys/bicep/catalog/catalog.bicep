@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param aca_outputs_azure_container_apps_environment_default_domain string

param aca_outputs_azure_container_apps_environment_id string

param aca_outputs_azure_container_registry_endpoint string

param aca_outputs_azure_container_registry_managed_identity_id string

param catalog_containerimage string

param catalog_identity_outputs_id string

param catalog_containerport string

param storage_outputs_blobendpoint string

@secure()
param queue_password_value string

param postgres_kv_outputs_name string

@secure()
param vectordb_key_value string

param redis_kv_outputs_name string

param catalog_identity_outputs_clientid string

resource postgres_kv_outputs_name_kv 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: postgres_kv_outputs_name
}

resource postgres_kv_outputs_name_kv_connectionstrings__catalogdb 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = {
  name: 'connectionstrings--catalogdb'
  parent: postgres_kv_outputs_name_kv
}

resource redis_kv_outputs_name_kv 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: redis_kv_outputs_name
}

resource redis_kv_outputs_name_kv_connectionstrings__redis 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = {
  name: 'connectionstrings--redis'
  parent: redis_kv_outputs_name_kv
}

resource catalog 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'catalog'
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
          identity: catalog_identity_outputs_id
          keyVaultUrl: postgres_kv_outputs_name_kv_connectionstrings__catalogdb.properties.secretUri
        }
        {
          name: 'connectionstrings--vectordb'
          value: 'Endpoint=${'http://vectordb.internal.${aca_outputs_azure_container_apps_environment_default_domain}'};Key=${vectordb_key_value}'
        }
        {
          name: 'connectionstrings--vectordb-http'
          value: 'Endpoint=http://vectordb:6333;Key=${vectordb_key_value}'
        }
        {
          name: 'connectionstrings--redis'
          identity: catalog_identity_outputs_id
          keyVaultUrl: redis_kv_outputs_name_kv_connectionstrings__redis.properties.secretUri
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: int(catalog_containerport)
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
          image: catalog_containerimage
          name: 'catalog'
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
              value: catalog_containerport
            }
            {
              name: 'ConnectionStrings__embedding'
              value: 'Endpoint=http://${'ollama.internal.${aca_outputs_azure_container_apps_environment_default_domain}'}:80;Model=nomic-embed-text:latest'
            }
            {
              name: 'ConnectionStrings__chat'
              value: 'Endpoint=http://${'ollama.internal.${aca_outputs_azure_container_apps_environment_default_domain}'}:80;Model=gemma3:4b'
            }
            {
              name: 'ConnectionStrings__catalog-blob'
              value: 'Endpoint="${storage_outputs_blobendpoint}";ContainerName=catalog-blob;'
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
              value: 'http://keycloak.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
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
              name: 'OTEL_EXPORTER_OTLP_ENDPOINT'
              value: 'http://dashboard:18889'
            }
            {
              name: 'OTEL_EXPORTER_OTLP_PROTOCOL'
              value: 'grpc'
            }
            {
              name: 'OTEL_SERVICE_NAME'
              value: 'catalog'
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: catalog_identity_outputs_clientid
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
      '${catalog_identity_outputs_id}': { }
      '${aca_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}