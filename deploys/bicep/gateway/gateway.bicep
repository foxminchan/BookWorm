@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param aca_outputs_azure_container_apps_environment_default_domain string

param aca_outputs_azure_container_apps_environment_id string

resource gateway 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'gateway'
  location: location
  properties: {
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: 5000
        transport: 'http'
      }
    }
    environmentId: aca_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: 'mcr.microsoft.com/dotnet/nightly/yarp:latest'
          name: 'gateway'
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
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
              name: 'services__chatting__http__0'
              value: 'http://chatting.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__chatting__https__0'
              value: 'https://chatting.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__ordering__http__0'
              value: 'http://ordering.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__ordering__https__0'
              value: 'https://ordering.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__rating__http__0'
              value: 'http://rating.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__rating__https__0'
              value: 'https://rating.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
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
              name: 'services__finance__http__0'
              value: 'http://finance.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__finance__https__0'
              value: 'https://finance.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__keycloak__http__0'
              value: 'http://keycloak.internal.${aca_outputs_azure_container_apps_environment_default_domain}'
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
}