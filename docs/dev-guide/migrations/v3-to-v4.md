# Migrate from v3 to v4

To migrate from v3 to v4 `manually`, you have to add two new settings to the portal web app. These two settings are required to to pull devices telemetry from the IoT Hub:

| Name | Setting Type | Detail |
|---|---|---|
| `IoTHub__EventHub__ConsumerGroup` | Application setting | (Default value `iothub-portal`) The name of the consumer group used to to pull data from the IoT Hub |
| `IoTHub__EventHub__Endpoint` | Connection string  | The IotHub Event Hub compatible endpoint |

Below the required steps for each settings:

## `IoTHub__EventHub__ConsumerGroup`

1. Go to your IoT Hub
2. Navigate to menu Built-in endpoints
3. Create a consumer group with the name `iothub-portal`
    ![iothub-create-consumer-group.png](/images/iothub-create-consumer-group.png)
4. Back to the portal web app, add a new application setting with name `IoTHub__EventHub__ConsumerGroup` and with value `iothub-portal`

## `IoTHub__EventHub__Endpoint`

1. Go to your IoT Hub
2. Navigate to menu Built-in endpoints
3. On the section Event Hub compatible endpoint
      1. Select the shared access policy `service`
      2. Copy the value of the event Hub-compatible endpoint
      ![iothub-get-event-hub-endpoint.png](/images/iothub-get-event-hub-endpoint.png)
4. Back to the portal web app, add a new connection setting with name `IoTHub__EventHub__Endpoint` and with value the event Hub-compatible endpoint copied earlier

!!! info
    You can create your own shared access policy. But the portal needs at least the `Service Connect` permission
