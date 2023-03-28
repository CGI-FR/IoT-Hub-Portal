# Azure Configurations

## Overall Architecture

This schema represent the various components and how they interact to have a better understanding of the various solution elements.

![images/overall-architecture.png](images/overall-architecture.png)

1. The user is authenticated by the OpenID Connect server.
1. The user access to the IoT Hub Portal with the OAuth2.0 token.
1. The IoT Hub portal uses the Azure IoT Hub REST API to retrieve the data.
1. The IoT Hub portal uses the Azure Device Provisioning Service to manage IoT Edge devices.
1. The IoT Hub portal uses the Azure Storage account to store the device models configuration (Images, Commands, etc.).
1. The IoT Hub portal uses the LoRa Key Management Facade to send Cloud to Device (C2D) messages to LoRa devices.
1. The LoRa Key Management Facade uses Redis to store its cached data.
1. The LoRa Key Management Facade uses the Azure IoT Hub REST API to retrieve the LoRa device keys and send C2D messages.
1. The IoT Hub portal synchronizes its data with the IoT Hub to provide a consistent view of the data.

> Note: For more information about the LoRa Key Management Facade, see the [Azure IoT Edge LoRaWAN Starter Kit](https://azure.github.io/iotedge-lorawan-starterkit) page.

## Configurations/Secrets

_Secrets_ are used to fill in the login credentials to the cloud platform. You have to enter them in a `json` file to be able to connect to the IoT Hub Portal. Here is a template of a such `json` file :

```json
{
  "CloudProvider": "Azure",
  "StorageAccount:ConnectionString": "<CONNECTION_STRING_STORAGE_ACCOUNT>",
  "StorageAccount:BlobContainerName": "<BLOB_CONTAINER_NAME>",
  "OIDC:Scope": "<SCOPE>",
  "OIDC:MetadataUrl": "<METADATA_URL>",
  "OIDC:ClientId": "<CLIENT_ID>",
  "OIDC:Authority": "<AUTHORITY>",
  "OIDC:ApiClientId": "<API_CLIENT_ID>",
  "LoRaRegionRouterConfig:Url": "<LORA_WAN_ROUTER_CONFIGURATION_URL>",
  "LoRaKeyManagement:Url": "<LORA_WAN_KEY_MANAGEMENT_URL>",
  "LoRaKeyManagement:Code": "<LORA_WAN_KEY_MANAGEMENT_CODE>",
  "LoRaFeature:Enabled": "<TRUE_OR_FALSE>",
  "Kestrel:Certificates:Development:Password": "<DEV_PASSWORD>",
  "IoTHub:ConnectionString": "<IOT_HUB_CONNECTION_STRING>",
  "IoTHub:EventHub:Endpoint": "<IOT_HUB_EVENT_HUB_ENDPOINT>",
  "IoTHub:EventHub:ConsumerGroup": "<IOT_HUB_EVENT_HUB_CONSUMER_GROUP>",
  "IoTDPS:ServiceEndpoint": "<SERVICE_END_POINT>",
  "IoTDPS:LoRaEnrollmentGroup": "<LORA_WAN_ENROLLMENT_GROUP>",
  "IoTDPS:DefaultEnrollmentGroup": "<LORA_WAN_DEFAULT_ENROLLMENT_GROUP>",
  "IoTDPS:ConnectionString": "<IOT_DPS_CONNECTION_STRING>",
  "PostgreSQL:ConnectionString": "<POSTGRE_SQL_CONNECTION_STRING>"
}
```

> <u>Note:</u> You must replace all values in the brackets by your own Azure settings. If you can't find them in the Azure Portal, please contact an administrator of this project to have more information.

This `json` file must be added into your project solution. To do that, click on the `AzureIoTHub.Server` project in Visual Studio and select `Manage User Secrets` from the context menu. You can now add your secrets inside this file.

You are now ready to start your IoT Hub Portal development !
