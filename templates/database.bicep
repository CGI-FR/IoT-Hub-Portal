@description('Location for the resources.')
param location string

@description('PostgreSQL server name')
param pgsqlServerName string

@description('PostgreSQL user')
param pgsqlAdminLogin string

@description('PostgreSQL password')
@secure()
param pgsqlAdminPassword string

resource pgsqlServer 'Microsoft.DBforPostgreSQL/servers@2017-12-01' = {
  name: pgsqlServerName
  location: location
  sku: {
    name: 'B_Gen5_2'
    tier: 'Basic'
    capacity: 2
    size: '5120'
    family: 'Gen5'
  }
  properties: {
    createMode: 'Default'
    version: '11'
    administratorLogin: pgsqlAdminLogin
    administratorLoginPassword: pgsqlAdminPassword
    storageProfile: {
      storageMB: 5120
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
  }
}
