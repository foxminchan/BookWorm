@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param aca_outputs_azure_container_apps_environment_default_domain string

param aca_outputs_azure_container_apps_environment_id string

param health_checks_ui_identity_outputs_id string

param postgres_kv_outputs_name string

param health_checks_ui_identity_outputs_clientid string

resource postgres_kv_outputs_name_kv 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: postgres_kv_outputs_name
}

resource postgres_kv_outputs_name_kv_connectionstrings__healthdb 'Microsoft.KeyVault/vaults/secrets@2023-07-01' existing = {
  name: 'connectionstrings--healthdb'
  parent: postgres_kv_outputs_name_kv
}

resource health_checks_ui 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'health-checks-ui'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'storage-connection'
          identity: health_checks_ui_identity_outputs_id
          keyVaultUrl: postgres_kv_outputs_name_kv_connectionstrings__healthdb.properties.secretUri
        }
      ]
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
              name: 'storage_provider'
              value: 'PostgreSql'
            }
            {
              name: 'storage_connection'
              secretRef: 'storage-connection'
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
            {
              name: 'AZURE_CLIENT_ID'
              value: health_checks_ui_identity_outputs_clientid
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
      '${health_checks_ui_identity_outputs_id}': { }
    }
  }
}