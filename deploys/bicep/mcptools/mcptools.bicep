@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param aca_outputs_azure_container_apps_environment_default_domain string

param aca_outputs_azure_container_apps_environment_id string

param aca_outputs_azure_container_registry_endpoint string

param aca_outputs_azure_container_registry_managed_identity_id string

param mcptools_containerimage string

param mcptools_containerport string

@secure()
param vectordb_key_value string

resource mcptools 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'mcptools'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'connectionstrings--vectordb'
          value: 'Endpoint=${'http://vectordb.internal.${aca_outputs_azure_container_apps_environment_default_domain}'};Key=${vectordb_key_value}'
        }
        {
          name: 'connectionstrings--vectordb-http'
          value: 'Endpoint=http://vectordb:6333;Key=${vectordb_key_value}'
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: int(mcptools_containerport)
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
          image: mcptools_containerimage
          name: 'mcptools'
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
              value: mcptools_containerport
            }
            {
              name: 'ConnectionStrings__embedding'
              value: 'Endpoint=http://${'ollama.internal.${aca_outputs_azure_container_apps_environment_default_domain}'}:80;Model=nomic-embed-text:latest'
            }
            {
              name: 'ConnectionStrings__chat'
              value: 'Endpoint=http://${'ollama.internal.${aca_outputs_azure_container_apps_environment_default_domain}'}:80;Model=deepseek-r1:1.5b'
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
              name: 'services__catalog__http__0'
              value: 'http://catalog.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__catalog__https__0'
              value: 'https://catalog.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
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
      '${aca_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}