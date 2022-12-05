@description('Location for the resources.')
param location string

@description('Device Provisioning Service name.')
param dpsName string

@description('IoTHub name.')
param iotHubName string

@description('IoTHub owner policy name.')
param iotHubOwnerPolicyName string

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
}

output idScope string = dps.properties.idScope
