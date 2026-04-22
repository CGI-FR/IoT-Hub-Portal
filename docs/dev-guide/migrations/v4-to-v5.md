# Migrate from v4 to v5

In this v5, the major change is the integration of AWS in the portal.
Some changes have also been made at the portal web app settings.

## AWS

Starting from version 5, the portal now supports `AWS` integration.  
To learn how to deploy `AWS services` using the portal, please refer to the [Quick Start](/aws/#quick-start) for AWS documentation. It provides step-by-step instructions on setting up and deploying AWS resources using the portal's interface.

## Azure

To migrate from v4 to v5 `manually`, you have to add `CloudProvider` with `Azure` as default value.  
You have to add also `Azure__` prefix in all setting to the portal web app.  

| Name | Setting Type | Detail |
|---|---|---|
| `CloudProvider` | Application setting | (Possible value `Azure`) The name of the CLoud Provider to run in the portal |
| `Azure__LoRaRegionRouterConfig__Url` | Application setting  | The Url for LoRa Region Router Configuration |
| `Azure__LoRaKeyManagement__Url` | Application setting  | The Url for LoRa Key Management |
| `Azure__LoRaKeyManagement__Code` | Application setting  | The Code for LoRa Key Management |
| `Azure__LoRaFeature__Enabled` | Application setting  | To enable or disable LoRa Feature |
| `Azure__IoTHub__ConnectionString` | Connection string  | The IotHub Connection String |
| `Azure__IoTHub__EventHub__Endpoint` | Connection string  | The IotHub Event Hub compatible endpoint |
| `Azure__IoTHub__EventHub__ConsumerGroup` | Application setting  | (Default value `iothub-portal`) The name of the consumer group used to to pull data from the IoT Hub |
| `Azure__IoTDPS__ServiceEndpoint` | Application setting  | The IotDPS Service Endpoint |
| `Azure__IoTDP__LoRaEnrollmentGroup` | Application setting  | The name of the IotDPS LoRa Enrollment group |
| `Azure__IoTDPS__DefaultEnrollmentGroup` | Application setting  | The name of the default IotDPS Enrollment group |
| `Azure__IoTDPS__ConnectionString` | Connection string  | The IotDPS Connection String |
| `Azure__IoTDPS__IDScope` | Application setting  | The IotDPS Scope ID |
| `Azure__StorageAccount__ConnectionString` | Connection string  | The Storage Account Connection String |
