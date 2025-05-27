@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param aca_outputs_azure_container_apps_environment_default_domain string

param aca_outputs_azure_container_apps_environment_id string

param aca_outputs_azure_container_registry_endpoint string

param aca_outputs_azure_container_registry_managed_identity_id string

param chatting_containerimage string

param chatting_identity_outputs_id string

param chatting_containerport string

param redis_kv_outputs_name string

param signalr_outputs_hostname string

param chatting_identity_outputs_clientid string

resource redis_kv_outputs_name_kv 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: redis_kv_outputs_name
}

resource redis_kv_outputs_name_kv_connectionstrings__redis 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = {
  name: 'connectionstrings--redis'
  parent: redis_kv_outputs_name_kv
}

resource chatting 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'chatting'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'connectionstrings--redis'
          identity: chatting_identity_outputs_id
          keyVaultUrl: redis_kv_outputs_name_kv_connectionstrings__redis.properties.secretUri
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: int(chatting_containerport)
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
          image: chatting_containerimage
          name: 'chatting'
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
              value: chatting_containerport
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
              name: 'ConnectionStrings__redis'
              secretRef: 'connectionstrings--redis'
            }
            {
              name: 'ConnectionStrings__signalr'
              value: 'Endpoint=https://${signalr_outputs_hostname};AuthType=azure'
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
              name: 'services__mcptools__http__0'
              value: 'http://mcptools.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__mcptools__https__0'
              value: 'https://mcptools.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
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
              value: 'chatting'
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: chatting_identity_outputs_clientid
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
      '${chatting_identity_outputs_id}': { }
      '${aca_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}