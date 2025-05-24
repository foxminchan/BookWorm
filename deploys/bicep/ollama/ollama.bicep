@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param aca_outputs_azure_container_apps_environment_default_domain string

param aca_outputs_azure_container_apps_environment_id string

param aca_outputs_volumes_ollama_0 string

resource ollama 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'ollama'
  location: location
  properties: {
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: 11434
        transport: 'http'
      }
    }
    environmentId: aca_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: 'docker.io/ollama/ollama:0.6.0'
          name: 'ollama'
          env: [
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
              value: 'ollama'
            }
          ]
          volumeMounts: [
            {
              volumeName: 'v0'
              mountPath: '/root/.ollama'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
      }
      volumes: [
        {
          name: 'v0'
          storageType: 'AzureFile'
          storageName: aca_outputs_volumes_ollama_0
        }
      ]
    }
  }
}