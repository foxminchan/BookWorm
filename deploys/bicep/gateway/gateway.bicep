@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param gateway_containerport string

param bookworm_aca_outputs_azure_container_apps_environment_default_domain string

param bookworm_aca_outputs_azure_container_apps_environment_id string

param bookworm_aca_outputs_azure_container_registry_endpoint string

param bookworm_aca_outputs_azure_container_registry_managed_identity_id string

param gateway_containerimage string

resource gateway 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'gateway'
  location: location
  properties: {
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: gateway_containerport
        transport: 'http'
      }
      registries: [
        {
          server: bookworm_aca_outputs_azure_container_registry_endpoint
          identity: bookworm_aca_outputs_azure_container_registry_managed_identity_id
        }
      ]
    }
    environmentId: bookworm_aca_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: gateway_containerimage
          name: 'gateway'
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
              value: gateway_containerport
            }
            {
              name: 'services__catalog__http__0'
              value: 'http://catalog.internal.${bookworm_aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__catalog__https__0'
              value: 'https://catalog.internal.${bookworm_aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__ordering__http__0'
              value: 'http://ordering.internal.${bookworm_aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__ordering__https__0'
              value: 'https://ordering.internal.${bookworm_aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__rating__http__0'
              value: 'http://rating.internal.${bookworm_aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__rating__https__0'
              value: 'https://rating.internal.${bookworm_aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__basket__http__0'
              value: 'http://basket.internal.${bookworm_aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__basket__https__0'
              value: 'https://basket.internal.${bookworm_aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__finance__http__0'
              value: 'http://finance.internal.${bookworm_aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__finance__https__0'
              value: 'https://finance.internal.${bookworm_aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__keycloak__http__0'
              value: 'http://keycloak.internal.${bookworm_aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__keycloak__management__0'
              value: 'http://keycloak:9000'
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
      '${bookworm_aca_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}