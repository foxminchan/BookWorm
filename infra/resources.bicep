@description('The location used for all deployed resources')
param location string = resourceGroup().location
@description('Id of the user or app to assign application roles')
param principalId string = ''


@description('Tags that will be applied to all resources')
param tags object = {}

var resourceToken = uniqueString(resourceGroup().id)

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'mi-${resourceToken}'
  location: location
  tags: tags
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: replace('acr-${resourceToken}', '-', '')
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
  tags: tags
}

resource caeMiRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, managedIdentity.id, subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d'))
  scope: containerRegistry
  properties: {
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId:  subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
  }
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: 'law-${resourceToken}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
  tags: tags
}

resource storageVolume 'Microsoft.Storage/storageAccounts@2022-05-01' = {
  name: 'vol${resourceToken}'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    largeFileSharesState: 'Enabled'
  }
}

resource storageVolumeFileService 'Microsoft.Storage/storageAccounts/fileServices@2022-05-01' = {
  parent: storageVolume
  name: 'default'
}
resource volumesAccountRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageVolume.id, principalId, subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '69566ab7-960f-475b-8e7c-b3118f30c6bd'))
  scope: storageVolume
  properties: {
    principalId: principalId
    roleDefinitionId:  subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '69566ab7-960f-475b-8e7c-b3118f30c6bd')
  }
}

resource mongodbBm0FileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2022-05-01' = {
  parent: storageVolumeFileService
  name: take('${toLower('mongodb')}-${toLower('bm0')}', 32)
  properties: {
    shareQuota: 1024
    enabledProtocols: 'SMB'
  }
}
resource postgresBm0FileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2022-05-01' = {
  parent: storageVolumeFileService
  name: take('${toLower('postgres')}-${toLower('bm0')}', 32)
  properties: {
    shareQuota: 1024
    enabledProtocols: 'SMB'
  }
}
resource redisBm0FileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2022-05-01' = {
  parent: storageVolumeFileService
  name: take('${toLower('redis')}-${toLower('bm0')}', 32)
  properties: {
    shareQuota: 1024
    enabledProtocols: 'SMB'
  }
}

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2024-02-02-preview' = {
  name: 'cae-${resourceToken}'
  location: location
  properties: {
    workloadProfiles: [{
      workloadProfileType: 'Consumption'
      name: 'consumption'
    }]
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
  }
  tags: tags

  resource aspireDashboard 'dotNetComponents' = {
    name: 'aspire-dashboard'
    properties: {
      componentType: 'AspireDashboard'
    }
  }

}

resource explicitContributorUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerAppEnvironment.id, principalId, subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b24988ac-6180-42a0-ab88-20f7382dd24c'))
  scope: containerAppEnvironment
  properties: {
    principalId: principalId
    roleDefinitionId:  subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b24988ac-6180-42a0-ab88-20f7382dd24c')
  }
}

resource mongodbBm0Store 'Microsoft.App/managedEnvironments/storages@2023-05-01' = {
  parent: containerAppEnvironment
  name: take('${toLower('mongodb')}-${toLower('bm0')}', 32)
  properties: {
    azureFile: {
      shareName: '${toLower('mongodb')}-${toLower('bm0')}'
      accountName: storageVolume.name
      accountKey: storageVolume.listKeys().keys[0].value
      accessMode: 'ReadWrite'
    }
  }
}

resource postgresBm0Store 'Microsoft.App/managedEnvironments/storages@2023-05-01' = {
  parent: containerAppEnvironment
  name: take('${toLower('postgres')}-${toLower('bm0')}', 32)
  properties: {
    azureFile: {
      shareName: '${toLower('postgres')}-${toLower('bm0')}'
      accountName: storageVolume.name
      accountKey: storageVolume.listKeys().keys[0].value
      accessMode: 'ReadWrite'
    }
  }
}

resource redisBm0Store 'Microsoft.App/managedEnvironments/storages@2023-05-01' = {
  parent: containerAppEnvironment
  name: take('${toLower('redis')}-${toLower('bm0')}', 32)
  properties: {
    azureFile: {
      shareName: '${toLower('redis')}-${toLower('bm0')}'
      accountName: storageVolume.name
      accountKey: storageVolume.listKeys().keys[0].value
      accessMode: 'ReadWrite'
    }
  }
}

output MANAGED_IDENTITY_CLIENT_ID string = managedIdentity.properties.clientId
output MANAGED_IDENTITY_NAME string = managedIdentity.name
output MANAGED_IDENTITY_PRINCIPAL_ID string = managedIdentity.properties.principalId
output AZURE_LOG_ANALYTICS_WORKSPACE_NAME string = logAnalyticsWorkspace.name
output AZURE_LOG_ANALYTICS_WORKSPACE_ID string = logAnalyticsWorkspace.id
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = containerRegistry.properties.loginServer
output AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID string = managedIdentity.id
output AZURE_CONTAINER_APPS_ENVIRONMENT_NAME string = containerAppEnvironment.name
output AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = containerAppEnvironment.id
output AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = containerAppEnvironment.properties.defaultDomain
output SERVICE_MONGODB_VOLUME_BM0_NAME string = mongodbBm0Store.name
output SERVICE_MONGODB_FILE_SHARE_BM0_NAME string = mongodbBm0FileShare.name
output SERVICE_POSTGRES_VOLUME_BM0_NAME string = postgresBm0Store.name
output SERVICE_POSTGRES_FILE_SHARE_BM0_NAME string = postgresBm0FileShare.name
output SERVICE_REDIS_VOLUME_BM0_NAME string = redisBm0Store.name
output SERVICE_REDIS_FILE_SHARE_BM0_NAME string = redisBm0FileShare.name
output AZURE_VOLUMES_STORAGE_ACCOUNT string = storageVolume.name
