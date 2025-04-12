@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param bookworm_notification_containerport string

@secure()
param api_key_value string

@secure()
param sender_email_value string

@secure()
param sender_name_value string

@secure()
param queue_password_value string

param bookworm_outputs_azure_container_apps_environment_default_domain string

param bookworm_outputs_azure_container_apps_environment_id string

param bookworm_outputs_azure_container_registry_endpoint string

param bookworm_outputs_azure_container_registry_managed_identity_id string

param bookworm_notification_containerimage string

resource bookworm_notification 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'bookworm-notification'
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
        targetPort: bookworm_notification_containerport
        transport: 'http'
      }
      registries: [
        {
          server: bookworm_outputs_azure_container_registry_endpoint
          identity: bookworm_outputs_azure_container_registry_managed_identity_id
        }
      ]
    }
    environmentId: bookworm_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: bookworm_notification_containerimage
          name: 'bookworm-notification'
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
              value: bookworm_notification_containerport
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
      '${bookworm_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}