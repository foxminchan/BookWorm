@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param aca_outputs_azure_container_apps_environment_default_domain string

param aca_outputs_azure_container_apps_environment_id string

param aca_outputs_azure_container_registry_endpoint string

param aca_outputs_azure_container_registry_managed_identity_id string

param notification_containerimage string

param notification_identity_outputs_id string

param notification_containerport string

@secure()
param api_key_value string

@secure()
param sender_email_value string

@secure()
param sender_name_value string

@secure()
param queue_password_value string

param storage_outputs_tableendpoint string

param notification_identity_outputs_clientid string

resource notification 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'notification'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'sendgrid--apikey'
          value: api_key_value
        }
        {
          name: 'sendgrid--senderemail'
          value: sender_email_value
        }
        {
          name: 'sendgrid--sendername'
          value: sender_name_value
        }
        {
          name: 'connectionstrings--queue'
          value: 'amqp://guest:${queue_password_value}@queue:5672'
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: int(notification_containerport)
        transport: 'http'
      }
      registries: [
        {
          server: aca_outputs_azure_container_registry_endpoint
          identity: aca_outputs_azure_container_registry_managed_identity_id
        }
      ]
    }
    environmentId: aca_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: notification_containerimage
          name: 'notification'
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
              value: notification_containerport
            }
            {
              name: 'SendGrid__ApiKey'
              secretRef: 'sendgrid--apikey'
            }
            {
              name: 'SendGrid__SenderEmail'
              secretRef: 'sendgrid--senderemail'
            }
            {
              name: 'SendGrid__SenderName'
              secretRef: 'sendgrid--sendername'
            }
            {
              name: 'ConnectionStrings__queue'
              secretRef: 'connectionstrings--queue'
            }
            {
              name: 'ConnectionStrings__table'
              value: storage_outputs_tableendpoint
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: notification_identity_outputs_clientid
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
      '${notification_identity_outputs_id}': { }
      '${aca_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}