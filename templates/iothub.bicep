@description('Location for the resources.')
param location string

@description('IoTHub name.')
param iotHubName string

resource iotHub 'Microsoft.Devices/IotHubs@2021-07-02' = {
  sku: {
    name: 'S1'
    capacity: 1
  }
  name: iotHubName
  location: location
  properties: {
  }
}
