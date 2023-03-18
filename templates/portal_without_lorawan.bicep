@description('Location for the resources.')
param location string

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
