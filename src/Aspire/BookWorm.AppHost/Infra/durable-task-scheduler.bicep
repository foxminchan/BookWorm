@description('Azure region for the Durable Task Scheduler resource.')
param location string = resourceGroup().location

@description('Base name of the Durable Task Scheduler resource.')
param schedulerName string

@description('Name of the Durable Task Hub.')
param taskHubName string

@allowed([
  'Consumption'
  'Dedicated'
])
@description('SKU name for the Durable Task Scheduler.')
param skuName string = 'Consumption'

resource scheduler 'Microsoft.DurableTask/schedulers@2025-11-01' = {
  name: '${schedulerName}-${uniqueString(resourceGroup().id, schedulerName)}'
  location: location
  properties: {
    sku: {
      name: skuName
    }
    ipAllowlist: [
      '0.0.0.0/0'
    ]
  }

  resource taskHub 'taskHubs@2025-11-01' = {
    name: taskHubName
  }
}

output endpoint string = scheduler.properties.endpoint
output connectionString string = 'Endpoint=${scheduler.properties.endpoint};Authentication=DefaultAzure'
output taskHubName string = scheduler::taskHub.name
