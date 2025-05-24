@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param aca_outputs_azure_container_apps_environment_default_domain string

param aca_outputs_azure_container_apps_environment_id string

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
    environmentId: aca_outputs_azure_container_apps_environment_id
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
              value: 'health-checks-ui'
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