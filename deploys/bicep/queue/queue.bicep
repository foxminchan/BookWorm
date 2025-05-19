@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param aca_outputs_azure_container_apps_environment_default_domain string

param aca_outputs_azure_container_apps_environment_id string

@secure()
param queue_password_value string

resource queue 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'queue'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'rabbitmq-default-pass'
          value: queue_password_value
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: 15672
        transport: 'http'
        additionalPortMappings: [
          {
            external: false
            targetPort: 5672
          }
        ]
      }
    }
    environmentId: aca_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: 'docker.io/library/rabbitmq:4.1-management'
          name: 'queue'
          env: [
            {
              name: 'RABBITMQ_DEFAULT_USER'
              value: 'guest'
            }
            {
              name: 'RABBITMQ_DEFAULT_PASS'
              secretRef: 'rabbitmq-default-pass'
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