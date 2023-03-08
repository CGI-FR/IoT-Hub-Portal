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

@description('The name of the Edge gateway')
param edgeGatewayName string

@description('Provision a final LoRa device in the IoT hub in addition to the gateway')
param deployDevice bool

@description('Provide the reset pin value of your gateway. Please refer to the doc if you are unfamiliar with the value')
param resetPin int

@description('In what region is your gateway deployed?')
@allowed(['EU863', 'US902', 'AS923-1', 'AS923-2', 'AS923-3', 'CN470RP1', 'CN470RP2', 'AU915'])
param region string = 'EU863'

@description('[In Mbps] Custom SPI speed for your gateway, currently only supported for ARM gateways')
@allowed([8,2])
param spiSpeed int = 8

@description('SPI Dev version for x86 based gateway')
@allowed([0,1,2])
param spiDev int = 0

@description('PostgreSQL Server Name')
param pgsqlServerName string

@description('IotHub Name')
param iotHubName string

@description('DPS Name')
param dpsName string

@description('Site Name')
param siteName string

@description('Service Plan Name')
param servicePlanName string

@description('Storage Account Name')
param storageAccountName string

@description('IotHub Owner Policy Name')
param iotHubOwnerPolicyName string

@description('Provisioning Service Owner Policy Name')
param provisioningserviceownerPolicyName string

@description('Device Image Container Name')
param deviceImageContainerName string

@description('IAM Scope Name')
param iamScopeName string

@description('Storage Account Id')
param storageAccountId string

@description('App Insight Name')
param appInsightName string

var functionAppName = '${uniqueSolutionPrefix}function'
var functionAppDefaultHost = '${resourceId('Microsoft.Web/sites', functionAppName)}/host/default/'
var ioTHubEventHubConsumerGroupName = 'iothub-portal'

module starterKitDeployment '../iotedge-lorawan-starterkit/TemplateBicep/main.bicep' = {
  name: 'lorawan-starter-kit'
  params: {
    location: location
    uniqueSolutionPrefix: uniqueSolutionPrefix
    edgeGatewayName: edgeGatewayName
    deployDevice: deployDevice
    resetPin: resetPin
    region: region
    spiSpeed: spiSpeed
    spiDev: spiDev
  }
}

resource iotHub 'Microsoft.Devices/IotHubs@2021-07-02' existing = {
  name: iotHubName
}

module ioTHubEventHubConsumerGroup './iothub_eventhub_consumer_group.bicep' = {
  name: 'ioTHubEventHubConsumerGroup'
  params: {
    iotHubName: iotHubName
    ioTHubEventHubConsumerGroupName: ioTHubEventHubConsumerGroupName
  }
}

module storageAccountName_default_deviceImageContainer './blob_container.bicep' = {
  name: 'storageAccountName_default_deviceImageContainer'
  params: {
    storageAccountName: storageAccountName
    deviceImageContainerName: deviceImageContainerName
  }
}

module dps './dps.bicep' = {
  name: 'dps'
  params: {
    location: location
    dpsName: dpsName
    iotHubName: iotHubName
    iotHubOwnerPolicyName: iotHubOwnerPolicyName
  }
  dependsOn: [
    iotHub
  ]
}

module pgsqlServer './database.bicep' = {
  name: 'pgsqlServer'
  params: {
    location: location
    pgsqlServerName: pgsqlServerName
    pgsqlAdminLogin: pgsqlAdminLogin
    pgsqlAdminPassword:pgsqlAdminPassword
  }
}

module servicePlan './app_service_plan.bicep' = {
  name: 'servicePlan'
  params: {
    location: location
    servicePlanName: servicePlanName
  }
}

module site './app_service.bicep' = {
  name: 'site'
  params: {
    location: location
    siteName: siteName
    appServicePlanId: servicePlan.outputs.id
    iotHubName: iotHubName
    iotHubOwnerPolicyName: iotHubOwnerPolicyName
    iotHubEventHubEndpoint: iotHub.properties.eventHubEndpoints.events.endpoint
    ioTHubEventHubConsumerGroupName: ioTHubEventHubConsumerGroupName
    dpsName: dpsName
    dpsOwnerPolicyName: provisioningserviceownerPolicyName
    dpsIdScope: dps.outputs.idScope
    storageAccountName: storageAccountName
    storageAccountId: storageAccountId
    pgsqlServerName: pgsqlServerName
    pgsqlAdminLogin: pgsqlAdminLogin
    pgsqlAdminPassword: pgsqlAdminPassword
    appInsightsName: appInsightName
    isLoRaFeatureEnabled: true
    openIdAuthority: openIdAuthority
    openIdMetadataURL: openIdMetadataURL
    openIdClientId: clientId
    openIdApiClientId: apiClientId
    openIdScopeName: iamScopeName
    functionAppName: functionAppName
    functionAppDefaultHost: functionAppDefaultHost
    ideasEnabled: ideasEnabled
    ideasUrl: ideasUrl
    ideasAuthenticationHeader: ideasAuthenticationHeader
    ideasAuthenticationToken: ideasAuthenticationToken
  }
  dependsOn: [
    pgsqlServer
  ]
}
