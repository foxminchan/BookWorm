@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param userPrincipalId string

param tags object = { }

resource bookworm_mi 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: take('bookworm_mi-${uniqueString(resourceGroup().id)}', 128)
  location: location
  tags: tags
}

resource bookworm_acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: take('bookwormacr${uniqueString(resourceGroup().id)}', 50)
  location: location
  sku: {
    name: 'Basic'
  }
  tags: tags
}

resource bookworm_acr_bookworm_mi_AcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(bookworm_acr.id, bookworm_mi.id, subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d'))
  properties: {
    principalId: bookworm_mi.properties.principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalType: 'ServicePrincipal'
  }
  scope: bookworm_acr
}

resource bookworm_law 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: take('bookwormlaw-${uniqueString(resourceGroup().id)}', 63)
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
  tags: tags
}

resource bookworm 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: take('bookworm${uniqueString(resourceGroup().id)}', 24)
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: bookworm_law.properties.customerId
        sharedKey: bookworm_law.listKeys().primarySharedKey
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
  parent: bookworm
}

resource bookworm_Contributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(bookworm.id, userPrincipalId, subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b24988ac-6180-42a0-ab88-20f7382dd24c'))
  properties: {
    principalId: userPrincipalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b24988ac-6180-42a0-ab88-20f7382dd24c')
  }
  scope: bookworm
}

resource bookworm_storageVolume 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: take('bookwormstoragevolume${uniqueString(resourceGroup().id)}', 24)
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
  parent: bookworm_storageVolume
}

resource shares_volumes_vectordb_0 'Microsoft.Storage/storageAccounts/fileServices/shares@2024-01-01' = {
  name: take('sharesvolumesvectordb0-${uniqueString(resourceGroup().id)}', 63)
  properties: {
    enabledProtocols: 'SMB'
    shareQuota: 1024
  }
  parent: storageVolumeFileService
}

resource managedStorage_volumes_vectordb_0 'Microsoft.App/managedEnvironments/storages@2024-03-01' = {
  name: take('managedstoragevolumesvectordb${uniqueString(resourceGroup().id)}', 24)
  properties: {
    azureFile: {
      accountName: bookworm_storageVolume.name
      accountKey: bookworm_storageVolume.listKeys().keys[0].value
      accessMode: 'ReadWrite'
      shareName: shares_volumes_vectordb_0.name
    }
  }
  parent: bookworm
}

resource shares_volumes_keycloak_0 'Microsoft.Storage/storageAccounts/fileServices/shares@2024-01-01' = {
  name: take('sharesvolumeskeycloak0-${uniqueString(resourceGroup().id)}', 63)
  properties: {
    enabledProtocols: 'SMB'
    shareQuota: 1024
  }
  parent: storageVolumeFileService
}

resource managedStorage_volumes_keycloak_0 'Microsoft.App/managedEnvironments/storages@2024-03-01' = {
  name: take('managedstoragevolumeskeycloak${uniqueString(resourceGroup().id)}', 24)
  properties: {
    azureFile: {
      accountName: bookworm_storageVolume.name
      accountKey: bookworm_storageVolume.listKeys().keys[0].value
      accessMode: 'ReadWrite'
      shareName: shares_volumes_keycloak_0.name
    }
  }
  parent: bookworm
}

resource shares_volumes_ollama_0 'Microsoft.Storage/storageAccounts/fileServices/shares@2024-01-01' = {
  name: take('sharesvolumesollama0-${uniqueString(resourceGroup().id)}', 63)
  properties: {
    enabledProtocols: 'SMB'
    shareQuota: 1024
  }
  parent: storageVolumeFileService
}

resource managedStorage_volumes_ollama_0 'Microsoft.App/managedEnvironments/storages@2024-03-01' = {
  name: take('managedstoragevolumesollama${uniqueString(resourceGroup().id)}', 24)
  properties: {
    azureFile: {
      accountName: bookworm_storageVolume.name
      accountKey: bookworm_storageVolume.listKeys().keys[0].value
      accessMode: 'ReadWrite'
      shareName: shares_volumes_ollama_0.name
    }
  }
  parent: bookworm
}

output volumes_vectordb_0 string = managedStorage_volumes_vectordb_0.name

output volumes_keycloak_0 string = managedStorage_volumes_keycloak_0.name

output volumes_ollama_0 string = managedStorage_volumes_ollama_0.name

output MANAGED_IDENTITY_NAME string = bookworm_mi.name

output MANAGED_IDENTITY_PRINCIPAL_ID string = bookworm_mi.properties.principalId

output AZURE_LOG_ANALYTICS_WORKSPACE_NAME string = bookworm_law.name

output AZURE_LOG_ANALYTICS_WORKSPACE_ID string = bookworm_law.id

output AZURE_CONTAINER_REGISTRY_NAME string = bookworm_acr.name

output AZURE_CONTAINER_REGISTRY_ENDPOINT string = bookworm_acr.properties.loginServer

output AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID string = bookworm_mi.id

output AZURE_CONTAINER_APPS_ENVIRONMENT_NAME string = bookworm.name

output AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = bookworm.id

output AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = bookworm.properties.defaultDomain