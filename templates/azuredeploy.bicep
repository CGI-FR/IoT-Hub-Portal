@description('The Azure resources location')
param location string = resourceGroup().location

@description('Prefix used for resource names. Should be unique as this will also be used for domain names.')
param uniqueSolutionPrefix string

@description('PostgreSQL user')
param pgsqlAdminLogin string

@description('PostgreSQL password')
@secure()
param pgsqlAdminPassword string

@description('The Open ID Authority')
param openIdAuthority string

@description('The Open ID metadata Url from the Identity provider')
param openIdMetadataURL string

@description('The client ID for the B2C tenant')
param clientId string

@description('The API client ID for the B2C tenant')
param apiClientId string

@description('The name of the Edge gateway')
param edgeGatewayName string = 'TestLoRaWANGateway'

@description('Provision a final LoRa device in the IoT hub in addition to the gateway')
param deployDevice bool = true

@description('Provide the reset pin value of your gateway. Please refer to the doc if you are unfamiliar with the value')
param resetPin int = 2

@description('In what region is your gateway deployed?')
@allowed([
    'AS923-1'
    'AS923-2' 
    'AS923-3' 
    'AU915' 
    'CN470RP1' 
    'CN470RP2' 
    'EU863' 
    'US902'
])
param region string = 'EU863'

@description('[In Mbps] Custom SPI speed for your gateway, currently only supported for ARM gateways')
@allowed([
  8
  2
])
param spiSpeed int = 8

@description('SPI Dev version for x86 based gateway')
@allowed([
  1
  2
])
param spiDev int = 2

@description('Enable LoRaWAN feature?')
param isLoRaFeatureEnabled bool = true

@description('To enable Awesome-Ideas feature when set to true')
param ideasEnabled bool = false

@description('Url of Awesome-Ideas, to publish ideas submitted by users. Required when ideasEnabled is true')
param ideasUrl string = ''

@description('Authentication header to interact with Awesome-Ideas. Required when ideasEnabled is true')
param ideasAuthenticationHeader string = 'Ocp-Apim-Subscription-Key'

@description('Authentication token to interact with Awesome-Ideas. Required when ideasEnabled is true')
param ideasAuthenticationToken string = ''

var starterKitDeploymentName = 'lorawan-starter-kit'
var portalWithLoRaDeploymentName = 'iothub-portal-with-lora'
var portalWithoutLoRaDeploymentName = 'iothub-portal-without-lora'

module starterKitDeployment '../iotedge-lorawan-starterkit/TemplateBicep/main.bicep' = if (isLoRaFeatureEnabled) {
  name: starterKitDeploymentName
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

module portalWithLoRaDeployment './portalDeployWithLoRa.bicep' = if (isLoRaFeatureEnabled) {
  name: portalWithLoRaDeploymentName
  params: {
    location: location
    uniqueSolutionPrefix: uniqueSolutionPrefix
    pgsqlAdminLogin: pgsqlAdminLogin
    pgsqlAdminPassword: pgsqlAdminPassword
    openIdAuthority: openIdAuthority
    openIdMetadataURL: openIdMetadataURL
    apiClientId: apiClientId
    clientId: clientId
    ideasEnabled: ideasEnabled
    ideasUrl: ideasUrl
    ideasAuthenticationHeader: ideasAuthenticationHeader
    ideasAuthenticationToken: ideasAuthenticationToken
  }
  dependsOn: [
    starterKitDeployment
  ]
}

module portalWithoutLoRaDeployment './portalDeployWithoutLoRa.bicep' = if (!isLoRaFeatureEnabled) {
  name: portalWithoutLoRaDeploymentName
  params: {
    location: location
    uniqueSolutionPrefix: uniqueSolutionPrefix
    pgsqlAdminLogin: pgsqlAdminLogin
    pgsqlAdminPassword: pgsqlAdminPassword
    openIdAuthority: openIdAuthority
    openIdMetadataURL: openIdMetadataURL
    apiClientId: apiClientId
    clientId: clientId
    ideasEnabled: ideasEnabled
    ideasUrl: ideasUrl
    ideasAuthenticationHeader: ideasAuthenticationHeader
    ideasAuthenticationToken: ideasAuthenticationToken
  }
}