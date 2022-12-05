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

module iotHub './iothub.bicep' = {
  name: 'iotHub'
  params: {
    location: location
    iotHubName: iotHubName
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

module storageAccount './storage.bicep' = {
  name: 'storageAccount'
  params: {
    location: location
    storageAccountName: storageAccountName
  }
}

module storageAccountName_default_deviceImageContainer './blob_container.bicep' = {
  name: 'storageAccountName_default_deviceImageContainer'
  params: {
    storageAccountName: storageAccountName
    deviceImageContainerName: deviceImageContainerName
  }
  dependsOn: [
    storageAccount
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

module appInsights './app_insights.bicep' = {
  name: 'appInsights'
  params: {
    location: location
    appInsighstName: appInsightName
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
    dpsName: dpsName
    dpsOwnerPolicyName: provisioningserviceownerPolicyName
    dpsIdScope: dps.outputs.idScope
    storageAccountName: storageAccountName
    storageAccountId: storageAccountId
    pgsqlServerName: pgsqlServerName
    pgsqlAdminLogin: pgsqlAdminLogin
    pgsqlAdminPassword: pgsqlAdminPassword
    appInsightsName: appInsightName
    isLoRaFeatureEnabled: false
    openIdAuthority: openIdAuthority
    openIdMetadataURL: openIdMetadataURL
    openIdClientId: clientId
    openIdApiClientId: apiClientId
    openIdScopeName: iamScopeName
    ideasEnabled: ideasEnabled
    ideasUrl: ideasUrl
    ideasAuthenticationHeader: ideasAuthenticationHeader
    ideasAuthenticationToken: ideasAuthenticationToken
  }
  dependsOn: [
    storageAccount
    pgsqlServer
  ]
}
