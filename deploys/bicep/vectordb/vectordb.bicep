@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param aca_outputs_azure_container_apps_environment_default_domain string

param aca_outputs_azure_container_apps_environment_id string

@secure()
param vectordb_key_value string

param aca_outputs_volumes_vectordb_0 string

resource vectordb 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'vectordb'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'qdrant--service--api-key'
          value: vectordb_key_value
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: 6334
        transport: 'http2'
        additionalPortMappings: [
          {
            external: false
            targetPort: 6333
          }
        ]
      }
    }
    environmentId: aca_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: 'docker.io/qdrant/qdrant:v1.13.6'
          name: 'vectordb'
          env: [
            {
              name: 'QDRANT__SERVICE__API_KEY'
              secretRef: 'qdrant--service--api-key'
            }
            {
              name: 'QDRANT__SERVICE__ENABLE_STATIC_CONTENT'
              value: '0'
            }
          ]
          volumeMounts: [
            {
              volumeName: 'v0'
              mountPath: '/qdrant/storage'
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
          storageName: aca_outputs_volumes_vectordb_0
        }
      ]
    }
  }
}