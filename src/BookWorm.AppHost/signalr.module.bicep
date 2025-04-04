@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param principalType string

param principalId string

resource signalr 'Microsoft.SignalRService/signalR@2024-03-01' = {
  name: take('signalr-${uniqueString(resourceGroup().id)}', 63)
  location: location
  sku: {
    name: 'Free_F1'
    capacity: 1
  }
  kind: 'SignalR'
  tags: {
    'aspire-resource-name': 'signalr'
    Environment: 'Production'
    Project: 'BookWorm'
  }
  properties: {
    cors: {
      allowedOrigins: [
        '*'
      ]
    }
    features: [
      {
        flag: 'ServiceMode'
        value: 'Default'
      }
    ]
  }
}

resource signalr_SignalRAppServer 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: signalr
  name: guid(
    signalr.id,
    principalId,
    subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '420fcaa2-552c-430f-98ca-3264be4806c7')
  )
  properties: {
    principalId: principalId
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '420fcaa2-552c-430f-98ca-3264be4806c7'
    )
    principalType: principalType
  }
}

output hostName string = signalr.properties.hostName
