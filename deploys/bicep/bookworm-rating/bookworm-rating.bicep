@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param bookworm_rating_identity_outputs_id string

param bookworm_rating_identity_outputs_clientid string

param bookworm_rating_containerport string

param cosmos_kv_outputs_name string

@secure()
param queue_password_value string

param bookworm_outputs_azure_container_apps_environment_default_domain string

param bookworm_outputs_azure_container_apps_environment_id string

param bookworm_outputs_azure_container_registry_endpoint string

param bookworm_outputs_azure_container_registry_managed_identity_id string

param bookworm_rating_containerimage string

resource cosmos_kv_outputs_name_kv 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: cosmos_kv_outputs_name
}

resource cosmos_kv_outputs_name_kv_connectionstrings__feedbacks 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = {
  name: 'connectionstrings--feedbacks'
  parent: cosmos_kv_outputs_name_kv
}

resource bookworm_rating 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'bookworm-rating'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'connectionstrings--feedbacks'
          identity: bookworm_rating_identity_outputs_id
          keyVaultUrl: cosmos_kv_outputs_name_kv_connectionstrings__feedbacks.properties.secretUri
        }
        {
          name: 'connectionstrings--queue'
          value: 'amqp://guest:${queue_password_value}@queue:5672'
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: bookworm_rating_containerport
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
          image: bookworm_rating_containerimage
          name: 'bookworm-rating'
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
              value: bookworm_rating_containerport
            }
            {
              name: 'ConnectionStrings__feedbacks'
              secretRef: 'connectionstrings--feedbacks'
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
              name: 'AZURE_CLIENT_ID'
              value: bookworm_rating_identity_outputs_clientid
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
      '${bookworm_rating_identity_outputs_id}': { }
      '${bookworm_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}