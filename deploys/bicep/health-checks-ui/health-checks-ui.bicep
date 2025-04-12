@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param bookworm_outputs_azure_container_apps_environment_default_domain string

param bookworm_outputs_azure_container_apps_environment_id string

resource health_checks_ui 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'health-checks-ui'
  location: location
  properties: {
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 80
        transport: 'http'
      }
    }
    environmentId: bookworm_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: 'docker.io/xabarilcoding/healthchecksui:5.0.0'
          name: 'health-checks-ui'
          env: [
            {
              name: 'ui_path'
              value: '/'
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