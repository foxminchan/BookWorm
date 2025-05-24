@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param aca_outputs_azure_container_apps_environment_default_domain string

param aca_outputs_azure_container_apps_environment_id string

resource dashboard 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'dashboard'
  location: location
  properties: {
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: 18888
        transport: 'http'
        additionalPortMappings: [
          {
            external: false
            targetPort: 18889
          }
        ]
      }
    }
    environmentId: aca_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: 'mcr.microsoft.com/dotnet/nightly/aspire-dashboard:latest'
          name: 'dashboard'
        }
      ]
      scale: {
        minReplicas: 1
      }
    }
  }
}