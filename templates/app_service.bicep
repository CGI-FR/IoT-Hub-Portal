@description('Location for the resources.')
param location string

@description('App Service name.')
param siteName string

@description('App Service Plan id.')
param appServicePlanId string

@description('IoTHub name.')
param iotHubName string

@description('IoTHub owner policy name.')
param iotHubOwnerPolicyName string

@description('IoTHub EnventHub Endpoint.')
param iotHubEventHubEndpoint string = ''

@description('IoTHub EventHub Consumer Group name.')
param ioTHubEventHubConsumerGroupName string = ''

@description('DPS name.')
param dpsName string

@description('DPS Policy name.')
param dpsOwnerPolicyName string

@description('DPS Id Scope.')
param dpsIdScope string

@description('Storage Account name.')
param storageAccountName string

@description('Storage Account id.')
param storageAccountId string

@description('AppInsights name.')
param appInsightsName string

@description('PostgreSQL server name')
param pgsqlServerName string

@description('PostgreSQL user')
param pgsqlAdminLogin string

@description('PostgreSQL password')
@secure()
param pgsqlAdminPassword string

@description('The Open ID Authority')
param openIdAuthority string

@description('The Open ID metadata Url from the Identity provider')
param openIdMetadataURL string

@description('The Open ID client ID for the B2C tenant')
param openIdClientId string

@description('The Open ID API client ID for the B2C tenant')
param openIdApiClientId string

@description('The Open ID Scope name')
param openIdScopeName string

@description('The Function App name')
param functionAppName string = ''

@description('The Function App Default Host')
param functionAppDefaultHost string = ''

@description('Enable LoRaWAN feature?')
param isLoRaFeatureEnabled bool

@description('To enable Awesome-Ideas feature when set to true')
param ideasEnabled bool

@description('Url of Awesome-Ideas, to publish ideas submitted by users. Required when ideasEnabled is true')
param ideasUrl string = ''

@description('Authentication header to interact with Awesome-Ideas. Required when ideasEnabled is true')
param ideasAuthenticationHeader string = ''

@description('Authentication token to interact with Awesome-Ideas. Required when ideasEnabled is true')
param ideasAuthenticationToken string = ''

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
    serverFarmId: appServicePlanId
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
          name: 'IoTHub__EventHub__Endpoint'
          type: 'Custom'
          connectionString: isLoRaFeatureEnabled ? 'Endpoint=${iotHubEventHubEndpoint};SharedAccessKeyName=service;SharedAccessKey=${listKeys(resourceId('Microsoft.Devices/IotHubs/IotHubKeys', iotHubName, 'service'), '2021-07-02').primaryKey};EntityPath=${iotHubName}' : ''
        }
        {
          name: 'IoTDPS__ConnectionString'
          type: 'Custom'
          connectionString: 'HostName=${dpsName}.azure-devices-provisioning.net;SharedAccessKeyName=${dpsOwnerPolicyName};SharedAccessKey=${listKeys(resourceId('Microsoft.Devices/provisioningServices/keys', dpsName, dpsOwnerPolicyName), '2021-10-15').primaryKey}'
        }
        {
          name: 'StorageAccount__ConnectionString'
          type: 'Custom'
          connectionString: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${listKeys(storageAccountId, '2015-05-01-preview').key1}'
        }
        {
          name: 'LoRaKeyManagement__Code'
          type: 'Custom'
          connectionString: isLoRaFeatureEnabled ? listkeys(functionAppDefaultHost, '2021-02-01').masterKey : ''
        }
        {
          name: 'PostgreSQL__ConnectionString'
          type: 'Custom'
          connectionString: 'Server=${pgsqlServerName}.postgres.database.azure.com;Database=${siteName};Port=5432;User Id=${pgsqlAdminLogin}@${pgsqlServerName};Password=${pgsqlAdminPassword};Pooling=true;Connection Lifetime=0;Command Timeout=0;Ssl Mode=VerifyFull;'
        }
      ]
      appSettings: [
        {
          name: 'IoTHub__EventHub__ConsumerGroup'
          value: isLoRaFeatureEnabled ? ioTHubEventHubConsumerGroupName : ''
        }
        {
          name: 'IoTDPS__ServiceEndpoint'
          value: '${dpsName}.azure-devices-provisioning.net'
        }
        {
          name: 'IoTDPS__IDScope'
          value: dpsIdScope
        }
        {
          name: 'OIDC__ApiClientId'
          value: openIdApiClientId
        }
        {
          name: 'OIDC__ClientId'
          value: openIdClientId
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
          value: openIdScopeName
        }
        {
          name: 'LoRaFeature__Enabled'
          value: '${isLoRaFeatureEnabled}'
        }
        {
          name: 'LoRaKeyManagement__Url'
          value: isLoRaFeatureEnabled ? 'https://${functionAppName}.azurewebsites.net' : ''
        }
        {
          name: 'LoRaRegionRouterConfig__Url'
          value: isLoRaFeatureEnabled ? 'https://raw.githubusercontent.com/Azure/iotedge-lorawan-starterkit/dev/Tools/Cli-LoRa-Device-Provisioning/DefaultRouterConfig/' : ''
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value:  reference(resourceId('Microsoft.Insights/components', appInsightsName), '2020-02-02', 'Full').properties.InstrumentationKey
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
}
