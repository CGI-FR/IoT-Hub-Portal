@description('Location for the resources.')
param location string

@description('Prefix used for resource names. Should be unique as this will also be used for domain names.')
param uniqueSolutionPrefix string

@description('PostgreSQL user')
param pgsqlAdminLogin string = concat(uniqueString(resourceGroup().id, newGuid()))

@description('PostgreSQL password')
@secure()
param pgsqlAdminPassword string = '${uniqueString(resourceGroup().id, newGuid())}x!'

@description('The Open ID Authority')
param openIdAuthority string

@description('The Open ID metadata Url from the Identity provider')
param openIdMetadataURL string

@description('The client ID for the B2C tenant')
param clientId string

@description('The API client ID for the B2C tenant')
param apiClientId string

@description('To enable Awesome-Ideas feature when set to true')
param ideasEnabled bool = false

@description('Url of Awesome-Ideas, to publish ideas submitted by users. Required when ideasEnabled is true')
param ideasUrl string = ''

@description('Authentication header to interact with Awesome-Ideas. Required when ideasEnabled is true')
param ideasAuthenticationHeader string = 'Ocp-Apim-Subscription-Key'

@description('Authentication token to interact with Awesome-Ideas. Required when ideasEnabled is true')
param ideasAuthenticationToken string = ''

var pgsqlServerName = '${uniqueSolutionPrefix}pgsql'
var iotHubName = '${uniqueSolutionPrefix}hub'
var dpsName = '${uniqueSolutionPrefix}dps'
var siteName = '${uniqueSolutionPrefix}portal'
var servicePlanName = '${uniqueSolutionPrefix}asp'
var storageAccountName = '${uniqueSolutionPrefix}storage'
var iotHubOwnerPolicyName = 'iothubowner'
var provisioningserviceownerPolicyName = 'provisioningserviceowner'
var deviceImageContainerName = 'device-images'
var iamScopeName = 'API.Access'
var storageAccountId = '${resourceGroup().id}/providers/Microsoft.Storage/storageAccounts/${storageAccountName}'
var appInsightName = '${uniqueSolutionPrefix}insight'

resource iotHub 'Microsoft.Devices/IotHubs@2021-07-02' = {
  sku: {
    name: 'S1'
    capacity: 1
  }
  name: iotHubName
  location: location
  properties: {
  }
  dependsOn: []
}

resource dps 'Microsoft.Devices/provisioningServices@2021-10-15' = {
  name: dpsName
  location: location
  sku: {
    name: 'S1'
    capacity: 1
  }
  properties: {
    state: 'Active'
    iotHubs: [
      {
        connectionString: 'HostName=${iotHubName}.azure-devices.net;SharedAccessKeyName=${iotHubOwnerPolicyName};SharedAccessKey=${listKeys(resourceId('Microsoft.Devices/IotHubs/IotHubKeys', iotHubName, iotHubOwnerPolicyName), '2021-07-02').primaryKey}'
        location: location
      }
    ]
    allocationPolicy: 'Hashed'
  }
  dependsOn: [
    iotHub
  ]
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-09-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  dependsOn: []
}

resource storageAccountName_default_deviceImageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-05-01' = {
  name: '${storageAccountName}/default/${deviceImageContainerName}'
  dependsOn: [
    storageAccount
  ]
}

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

resource appInsight 'Microsoft.Insights/components@2020-02-02' = {
  kind: 'web'
  name: appInsightName
  location: location
  properties: {
    Application_Type: 'web'
  }
  dependsOn: []
}

resource servicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: servicePlanName
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
    size: 'B1'
    family: 'B'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    perSiteScaling: false
    elasticScaleEnabled: false
    maximumElasticWorkerCount: 1
    isSpot: false
    reserved: true
    isXenon: false
    hyperV: false
    targetWorkerCount: 0
    targetWorkerSizeId: 0
    zoneRedundant: false
  }
}

resource site 'Microsoft.Web/sites@2021-03-01' = {
  name: siteName
  location: location
  kind: 'app,linux,container'
  properties: {
    enabled: true
    hostNameSslStates: [
      {
        name: '${siteName}.azurewebsites.net'
        sslState: 'Disabled'
        hostType: 'Standard'
      }
      {
        name: '${siteName}.scm.azurewebsites.net'
        sslState: 'Disabled'
        hostType: 'Repository'
      }
    ]
    serverFarmId: servicePlan.id
    reserved: true
    siteConfig: {
      numberOfWorkers: 1
      linuxFxVersion: 'DOCKER|ghcr.io/cgi-fr/iothub-portal:latest'
      connectionStrings: [
        {
          name: 'IoTHub__ConnectionString'
          type: 'Custom'
          connectionString: 'HostName=${iotHubName}.azure-devices.net;SharedAccessKeyName=${iotHubOwnerPolicyName};SharedAccessKey=${listKeys(resourceId('Microsoft.Devices/iotHubs/iotHubKeys', iotHubName, iotHubOwnerPolicyName), '2021-07-02').primaryKey}'
        }
        {
          name: 'IoTDPS__ConnectionString'
          type: 'Custom'
          connectionString: 'HostName=${dpsName}.azure-devices-provisioning.net;SharedAccessKeyName=${provisioningserviceownerPolicyName};SharedAccessKey=${listKeys(resourceId('Microsoft.Devices/provisioningServices/keys', dpsName, provisioningserviceownerPolicyName), '2021-10-15').primaryKey}'
        }
        {
          name: 'StorageAccount__ConnectionString'
          type: 'Custom'
          connectionString: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${listKeys(storageAccountId, '2015-05-01-preview').key1}'
        }
        {
          name: 'PostgreSQL__ConnectionString'
          type: 'Custom'
          connectionString: 'Server=${pgsqlServerName}.postgres.database.azure.com;Database=${siteName};Port=5432;User Id=${pgsqlAdminLogin}@${pgsqlServerName};Password=${pgsqlAdminPassword};Pooling=true;Connection Lifetime=0;Command Timeout=0;Ssl Mode=VerifyFull;'
        }
      ]
      appSettings: [
        {
          name: 'IoTDPS__ServiceEndpoint'
          value: '${dpsName}.azure-devices-provisioning.net'
        }
        {
          name: 'IoTDPS__IDScope'
          value: dps.properties.idScope
        }
        {
          name: 'OIDC__ApiClientId'
          value: apiClientId
        }
        {
          name: 'OIDC__ClientId'
          value: clientId
        }
        {
          name: 'OIDC__Authority'
          value: openIdAuthority
        }
        {
          name: 'OIDC__MetadataUrl'
          value: openIdMetadataURL
        }
        {
          name: 'OIDC__Scope'
          value: iamScopeName
        }
        {
          name: 'LoRaFeature__Enabled'
          value: 'false'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: reference(appInsight.id, '2020-02-02', 'Full').properties.InstrumentationKey
        }
        {
          name: 'Ideas__Enabled'
          value: '${ideasEnabled}'
        }
        {
          name: 'Ideas__Url'
          value: ideasUrl
        }
        {
          name: 'Ideas__Authentication__Header'
          value: ideasAuthenticationHeader
        }
        {
          name: 'Ideas__Authentication__Token'
          value: ideasAuthenticationToken
        }
      ]
    }
    httpsOnly: true
  }
  dependsOn: [
    storageAccount
    pgsqlServer
  ]
}
