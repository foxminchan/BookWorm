@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param bookworm_outputs_volumes_keycloak_0 string

@secure()
param keycloak_password_value string

param bookworm_outputs_azure_container_apps_environment_default_domain string

param bookworm_outputs_azure_container_apps_environment_id string

resource keycloak 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'keycloak'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'kc-bootstrap-admin-password'
          value: keycloak_password_value
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: 8080
        transport: 'http'
        additionalPortMappings: [
          {
            external: false
            targetPort: 9000
          }
        ]
      }
    }
    environmentId: bookworm_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: 'quay.io/keycloak/keycloak:26.1'
          name: 'keycloak'
          args: [
            'start'
            '--import-realm'
          ]
          env: [
            {
              name: 'KC_BOOTSTRAP_ADMIN_USERNAME'
              value: 'admin'
            }
            {
              name: 'KC_BOOTSTRAP_ADMIN_PASSWORD'
              secretRef: 'kc-bootstrap-admin-password'
            }
            {
              name: 'KC_HEALTH_ENABLED'
              value: 'true'
            }
          ]
          volumeMounts: [
            {
              volumeName: 'v0'
              mountPath: '/opt/keycloak/data'
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
          storageName: bookworm_outputs_volumes_keycloak_0
        }
      ]
    }
  }
}