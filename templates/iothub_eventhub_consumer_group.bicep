@description('IoTHub name.')
param iotHubName string

@description('IoTHub EventHub Consumer Group name.')
param ioTHubEventHubConsumerGroupName string

resource ioTHubEventHubConsumerGroup 'Microsoft.Devices/IotHubs/eventHubEndpoints/ConsumerGroups@2021-07-02' = {
  name: '${iotHubName}/events/${ioTHubEventHubConsumerGroupName}'
  properties: {
    name: ioTHubEventHubConsumerGroupName
  }
}
