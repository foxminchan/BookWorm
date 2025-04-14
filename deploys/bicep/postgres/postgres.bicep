@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param administratorLogin string

@secure()
param administratorLoginPassword string

param keyVaultName string

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: take('postgres-${uniqueString(resourceGroup().id)}', 63)
  location: location
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    authConfig: {
      activeDirectoryAuth: 'Disabled'
      passwordAuth: 'Enabled'
    }
    availabilityZone: '1'
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    storage: {
      storageSizeGB: 32
    }
    version: '16'
  }
  sku: {
    tier: 'GeneralPurpose'
  }
  tags: {
    'aspire-resource-name': 'postgres'
    Environment: 'Development'
    Projects: 'BookWorm'
  }
}

resource postgreSqlFirewallRule_AllowAllAzureIps 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = {
  name: 'AllowAllAzureIps'
  properties: {
    endIpAddress: '0.0.0.0'
    startIpAddress: '0.0.0.0'
  }
  parent: postgres
}

resource catalogdb 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  name: 'catalogdb'
  parent: postgres
}

resource orderingdb 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  name: 'orderingdb'
  parent: postgres
}

resource financedb 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  name: 'financedb'
  parent: postgres
}

resource ratingdb 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  name: 'ratingdb'
  parent: postgres
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource connectionString 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: 'connectionstrings--postgres'
  properties: {
    value: 'Host=${postgres.properties.fullyQualifiedDomainName};Username=${administratorLogin};Password=${administratorLoginPassword}'
  }
  parent: keyVault
}

resource catalogdb_connectionString 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: 'connectionstrings--catalogdb'
  properties: {
    value: 'Host=${postgres.properties.fullyQualifiedDomainName};Username=${administratorLogin};Password=${administratorLoginPassword};Database=catalogdb'
  }
  parent: keyVault
}

resource orderingdb_connectionString 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: 'connectionstrings--orderingdb'
  properties: {
    value: 'Host=${postgres.properties.fullyQualifiedDomainName};Username=${administratorLogin};Password=${administratorLoginPassword};Database=orderingdb'
  }
  parent: keyVault
}

resource financedb_connectionString 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: 'connectionstrings--financedb'
  properties: {
    value: 'Host=${postgres.properties.fullyQualifiedDomainName};Username=${administratorLogin};Password=${administratorLoginPassword};Database=financedb'
  }
  parent: keyVault
}

resource ratingdb_connectionString 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: 'connectionstrings--ratingdb'
  properties: {
    value: 'Host=${postgres.properties.fullyQualifiedDomainName};Username=${administratorLogin};Password=${administratorLoginPassword};Database=ratingdb'
  }
  parent: keyVault
}

output name string = postgres.name