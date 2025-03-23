@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param principalId string

resource cosmos_roleDefinition 'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions@2024-08-15' existing = {
  parent: cosmos
  name: '00000000-0000-0000-0000-000000000002'
}

resource cosmos 'Microsoft.DocumentDB/databaseAccounts@2024-08-15' = {
  name: take('cosmos-${uniqueString(resourceGroup().id)}', 44)
  location: location
  kind: 'GlobalDocumentDB'
  tags: {
    'aspire-resource-name': 'cosmos'
    Environment: 'Production'
    Project: 'BookWorm'
  }
  properties: {
    locations: [
      {
        locationName: location
        failoverPriority: 0
      }
    ]
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    databaseAccountOfferType: 'Standard'
    disableLocalAuth: true
  }
}

resource ratingdb 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-08-15' = {
  parent: cosmos
  name: 'ratingdb'
  location: location
  properties: {
    resource: {
      id: 'ratingdb'
    }
  }
}

resource Feedbacks 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-08-15' = {
  parent: ratingdb
  name: 'Feedbacks'
  location: location
  properties: {
    resource: {
      id: 'Feedbacks'
      partitionKey: {
        paths: [
          '/id'
        ]
      }
    }
  }
}

resource cosmos_roleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-08-15' = {
  parent: cosmos
  name: guid(principalId, cosmos_roleDefinition.id, cosmos.id)
  properties: {
    principalId: principalId
    roleDefinitionId: cosmos_roleDefinition.id
    scope: cosmos.id
  }
}

output connectionString string = cosmos.properties.documentEndpoint
