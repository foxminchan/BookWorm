@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param aca_outputs_azure_container_apps_environment_default_domain string

param aca_outputs_azure_container_apps_environment_id string

param aca_outputs_azure_container_registry_endpoint string

param aca_outputs_azure_container_registry_managed_identity_id string

param finance_containerimage string

param finance_identity_outputs_id string

param finance_containerport string

param postgres_kv_outputs_name string

@secure()
param queue_password_value string

param finance_identity_outputs_clientid string

resource postgres_kv_outputs_name_kv 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: postgres_kv_outputs_name
}

resource postgres_kv_outputs_name_kv_connectionstrings__financedb 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = {
  name: 'connectionstrings--financedb'
  parent: postgres_kv_outputs_name_kv
}

resource finance 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'finance'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'connectionstrings--financedb'
          identity: finance_identity_outputs_id
          keyVaultUrl: postgres_kv_outputs_name_kv_connectionstrings__financedb.properties.secretUri
        }
        {
          name: 'connectionstrings--queue'
          value: 'amqp://guest:${queue_password_value}@queue:5672'
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: int(finance_containerport)
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
          image: finance_containerimage
          name: 'finance'
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
              value: finance_containerport
            }
            {
              name: 'ConnectionStrings__financedb'
              secretRef: 'connectionstrings--financedb'
            }
            {
              name: 'ConnectionStrings__queue'
              secretRef: 'connectionstrings--queue'
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: finance_identity_outputs_clientid
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
      '${finance_identity_outputs_id}': { }
      '${aca_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}