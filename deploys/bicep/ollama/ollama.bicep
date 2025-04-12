@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param bookworm_outputs_volumes_ollama_0 string

param bookworm_outputs_azure_container_apps_environment_default_domain string

param bookworm_outputs_azure_container_apps_environment_id string

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
    environmentId: bookworm_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: 'docker.io/ollama/ollama:0.6.0'
          name: 'ollama'
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
          storageName: bookworm_outputs_volumes_ollama_0
        }
      ]
    }
  }
}