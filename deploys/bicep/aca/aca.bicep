@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param userPrincipalId string

param tags object = { }

var resourceToken = uniqueString(resourceGroup().id)

resource aca_mi 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'mi-${resourceToken}'
  location: location
  tags: tags
}

resource aca_acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: replace('acr-${resourceToken}', '-', '')
  location: location
  sku: {
    name: 'Basic'
  }
  tags: tags
}

resource aca_acr_aca_mi_AcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aca_acr.id, aca_mi.id, subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d'))
  properties: {
    principalId: aca_mi.properties.principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalType: 'ServicePrincipal'
  }
  scope: aca_acr
}

resource aca_law 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'law-${resourceToken}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
  tags: tags
}

resource aca 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: 'cae-${resourceToken}'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: aca_law.properties.customerId
        sharedKey: aca_law.listKeys().primarySharedKey
      }
    }
    workloadProfiles: [
      {
        name: 'consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
  tags: tags
}

resource aspireDashboard 'Microsoft.App/managedEnvironments/dotNetComponents@2024-10-02-preview' = {
  name: 'aspire-dashboard'
  properties: {
    componentType: 'AspireDashboard'
  }
  parent: aca
}

resource aca_Contributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aca.id, userPrincipalId, subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b24988ac-6180-42a0-ab88-20f7382dd24c'))
  properties: {
    principalId: userPrincipalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b24988ac-6180-42a0-ab88-20f7382dd24c')
  }
  scope: aca
}

resource aca_storageVolume 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: 'vol${resourceToken}'
  kind: 'StorageV2'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    largeFileSharesState: 'Enabled'
  }
  tags: tags
}

resource storageVolumeFileService 'Microsoft.Storage/storageAccounts/fileServices@2024-01-01' = {
  name: 'default'
  parent: aca_storageVolume
}

resource shares_volumes_vectordb_0 'Microsoft.Storage/storageAccounts/fileServices/shares@2024-01-01' = {
  name: take('${toLower('vectordb')}-${toLower('bookwormapphost62fd53aa4evectordbdata')}', 60)
  properties: {
    enabledProtocols: 'SMB'
    shareQuota: 1024
  }
  parent: storageVolumeFileService
}

resource managedStorage_volumes_vectordb_0 'Microsoft.App/managedEnvironments/storages@2024-03-01' = {
  name: take('${toLower('vectordb')}-${toLower('bookwormapphost62fd53aa4evectordbdata')}', 32)
  properties: {
    azureFile: {
      accountName: aca_storageVolume.name
      accountKey: aca_storageVolume.listKeys().keys[0].value
      accessMode: 'ReadWrite'
      shareName: shares_volumes_vectordb_0.name
    }
  }
  parent: aca
}

resource shares_volumes_ollama_0 'Microsoft.Storage/storageAccounts/fileServices/shares@2024-01-01' = {
  name: take('${toLower('ollama')}-${toLower('bookwormapphost62fd53aa4eollamaollama')}', 60)
  properties: {
    enabledProtocols: 'SMB'
    shareQuota: 1024
  }
  parent: storageVolumeFileService
}

resource managedStorage_volumes_ollama_0 'Microsoft.App/managedEnvironments/storages@2024-03-01' = {
  name: take('${toLower('ollama')}-${toLower('bookwormapphost62fd53aa4eollamaollama')}', 32)
  properties: {
    azureFile: {
      accountName: aca_storageVolume.name
      accountKey: aca_storageVolume.listKeys().keys[0].value
      accessMode: 'ReadWrite'
      shareName: shares_volumes_ollama_0.name
    }
  }
  parent: aca
}

resource shares_volumes_keycloak_0 'Microsoft.Storage/storageAccounts/fileServices/shares@2024-01-01' = {
  name: take('${toLower('keycloak')}-${toLower('bookwormapphost62fd53aa4ekeycloakdata')}', 60)
  properties: {
    enabledProtocols: 'SMB'
    shareQuota: 1024
  }
  parent: storageVolumeFileService
}

resource managedStorage_volumes_keycloak_0 'Microsoft.App/managedEnvironments/storages@2024-03-01' = {
  name: take('${toLower('keycloak')}-${toLower('bookwormapphost62fd53aa4ekeycloakdata')}', 32)
  properties: {
    azureFile: {
      accountName: aca_storageVolume.name
      accountKey: aca_storageVolume.listKeys().keys[0].value
      accessMode: 'ReadWrite'
      shareName: shares_volumes_keycloak_0.name
    }
  }
  parent: aca
}

output volumes_vectordb_0 string = managedStorage_volumes_vectordb_0.name

output volumes_ollama_0 string = managedStorage_volumes_ollama_0.name

output volumes_keycloak_0 string = managedStorage_volumes_keycloak_0.name

output MANAGED_IDENTITY_NAME string = 'mi-${resourceToken}'

output MANAGED_IDENTITY_PRINCIPAL_ID string = aca_mi.properties.principalId

output AZURE_LOG_ANALYTICS_WORKSPACE_NAME string = 'law-${resourceToken}'

output AZURE_LOG_ANALYTICS_WORKSPACE_ID string = aca_law.id

output AZURE_CONTAINER_REGISTRY_NAME string = replace('acr-${resourceToken}', '-', '')

output AZURE_CONTAINER_REGISTRY_ENDPOINT string = aca_acr.properties.loginServer

output AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID string = aca_mi.id

output AZURE_CONTAINER_APPS_ENVIRONMENT_NAME string = 'cae-${resourceToken}'

output AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = aca.id

output AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = aca.properties.defaultDomain