@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource signalr 'Microsoft.SignalRService/signalR@2024-03-01' = {
  name: take('signalr-${uniqueString(resourceGroup().id)}', 63)
  location: location
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
    publicNetworkAccess: 'Enabled'
  }
  kind: 'SignalR'
  sku: {
    name: 'Premium_P1'
    capacity: 10
  }
  tags: {
    'aspire-resource-name': 'signalr'
    Projects: 'BookWorm'
  }
}

output hostName string = signalr.properties.hostName

output name string = signalr.name