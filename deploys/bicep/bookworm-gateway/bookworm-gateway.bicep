@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param bookworm_gateway_containerport string

param bookworm_outputs_azure_container_apps_environment_default_domain string

param bookworm_outputs_azure_container_apps_environment_id string

param bookworm_outputs_azure_container_registry_endpoint string

param bookworm_outputs_azure_container_registry_managed_identity_id string

param bookworm_gateway_containerimage string

resource bookworm_gateway 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'bookworm-gateway'
  location: location
  properties: {
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: bookworm_gateway_containerport
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
          image: bookworm_gateway_containerimage
          name: 'bookworm-gateway'
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
              value: bookworm_gateway_containerport
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
              name: 'services__bookworm-ordering__http__0'
              value: 'http://bookworm-ordering.internal.${bookworm_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__bookworm-ordering__https__0'
              value: 'https://bookworm-ordering.internal.${bookworm_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__bookworm-rating__http__0'
              value: 'http://bookworm-rating.internal.${bookworm_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__bookworm-rating__https__0'
              value: 'https://bookworm-rating.internal.${bookworm_outputs_azure_container_apps_environment_default_domain}'
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
              name: 'services__bookworm-finance__http__0'
              value: 'http://bookworm-finance.internal.${bookworm_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__bookworm-finance__https__0'
              value: 'https://bookworm-finance.internal.${bookworm_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__keycloak__http__0'
              value: 'http://keycloak.internal.${bookworm_outputs_azure_container_apps_environment_default_domain}'
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
      '${bookworm_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}