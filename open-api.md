---
title: Azure IoT Hub Portal API v1.0
language_tabs: []
toc_footers: []
includes: []
search: true
highlight_theme: darkula
headingLevel: 2

---

<!-- Generator: Widdershins v4.0.1 -->

<h1 id="azure-iot-hub-portal-api">Azure IoT Hub Portal API v1.0</h1>

> Scroll down for example requests and responses.

<h1 id="azure-iot-hub-portal-api-commands">Commands</h1>

## post__api_Commands_{modelId}

> Code samples

`POST /api/Commands/{modelId}`

*Add a command to an Azure DataTable.*

> Body parameter

```json
{
  "name": "string",
  "frame": "string",
  "port": 1
}
```

<h3 id="post__api_commands_{modelid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|modelId|path|string|true|none|
|body|body|[DeviceModelCommand](#schemadevicemodelcommand)|false|none|

<h3 id="post__api_commands_{modelid}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## delete__api_Commands_{modelId}_{commandId}

> Code samples

`DELETE /api/Commands/{modelId}/{commandId}`

*Delete a command from an Azure DataTable.*

<h3 id="delete__api_commands_{modelid}_{commandid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|modelId|path|string|true|none|
|commandId|path|string|true|none|

<h3 id="delete__api_commands_{modelid}_{commandid}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

<h1 id="azure-iot-hub-portal-api-concentrators">Concentrators</h1>

## get__api_Concentrators

> Code samples

`GET /api/Concentrators`

<h3 id="get__api_concentrators-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## post__api_Concentrators

> Code samples

`POST /api/Concentrators`

> Body parameter

```json
{
  "deviceId": "string",
  "deviceName": "string",
  "loraRegion": "string",
  "deviceType": "string",
  "clientCertificateThumbprint": "string",
  "isConnected": true,
  "isEnabled": true,
  "alreadyLoggedInOnce": true,
  "routerConfig": {
    "netID": [
      0
    ],
    "joinEui": [
      [
        "string"
      ]
    ],
    "region": "string",
    "hwspec": "string",
    "freq_range": [
      0
    ],
    "dRs": [
      [
        0
      ]
    ],
    "sx1301_conf": [
      {
        "property1": {
          "enable": true,
          "freq": 0,
          "radio": 0,
          "if": 0,
          "bandwidth": 0,
          "spread_factor": 0
        },
        "property2": {
          "enable": true,
          "freq": 0,
          "radio": 0,
          "if": 0,
          "bandwidth": 0,
          "spread_factor": 0
        }
      }
    ],
    "nocca": true,
    "nodc": true,
    "nodwell": true
  }
}
```

<h3 id="post__api_concentrators-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|body|body|[Concentrator](#schemaconcentrator)|false|none|

<h3 id="post__api_concentrators-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## put__api_Concentrators

> Code samples

`PUT /api/Concentrators`

> Body parameter

```json
{
  "deviceId": "string",
  "deviceName": "string",
  "loraRegion": "string",
  "deviceType": "string",
  "clientCertificateThumbprint": "string",
  "isConnected": true,
  "isEnabled": true,
  "alreadyLoggedInOnce": true,
  "routerConfig": {
    "netID": [
      0
    ],
    "joinEui": [
      [
        "string"
      ]
    ],
    "region": "string",
    "hwspec": "string",
    "freq_range": [
      0
    ],
    "dRs": [
      [
        0
      ]
    ],
    "sx1301_conf": [
      {
        "property1": {
          "enable": true,
          "freq": 0,
          "radio": 0,
          "if": 0,
          "bandwidth": 0,
          "spread_factor": 0
        },
        "property2": {
          "enable": true,
          "freq": 0,
          "radio": 0,
          "if": 0,
          "bandwidth": 0,
          "spread_factor": 0
        }
      }
    ],
    "nocca": true,
    "nodc": true,
    "nodwell": true
  }
}
```

<h3 id="put__api_concentrators-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|body|body|[Concentrator](#schemaconcentrator)|false|none|

<h3 id="put__api_concentrators-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## get__api_Concentrators_{deviceId}

> Code samples

`GET /api/Concentrators/{deviceId}`

<h3 id="get__api_concentrators_{deviceid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|deviceId|path|string|true|none|

<h3 id="get__api_concentrators_{deviceid}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## delete__api_Concentrators_{deviceId}

> Code samples

`DELETE /api/Concentrators/{deviceId}`

*this function delete a device.*

<h3 id="delete__api_concentrators_{deviceid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|deviceId|path|string|true|the device id.|

<h3 id="delete__api_concentrators_{deviceid}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

<h1 id="azure-iot-hub-portal-api-configs">Configs</h1>

## get__api_Configs

> Code samples

`GET /api/Configs`

*Gets a list of deployments as ConfigListItem from Azure IoT Hub.*

> Example responses

> 200 Response

```
[{"configurationID":"string","conditions":"string","metricsTargeted":0,"metricsApplied":0,"metricsSuccess":0,"metricsFailure":0,"priority":0,"creationDate":"2019-08-24T14:15:22Z","modules":[{"moduleName":"string","version":"string","status":"string","environmentVariables":{"property1":"string","property2":"string"},"moduleIdentityTwinSettings":{"property1":"string","property2":"string"}}]}]
```

```json
[
  {
    "configurationID": "string",
    "conditions": "string",
    "metricsTargeted": 0,
    "metricsApplied": 0,
    "metricsSuccess": 0,
    "metricsFailure": 0,
    "priority": 0,
    "creationDate": "2019-08-24T14:15:22Z",
    "modules": [
      {
        "moduleName": "string",
        "version": "string",
        "status": "string",
        "environmentVariables": {
          "property1": "string",
          "property2": "string"
        },
        "moduleIdentityTwinSettings": {
          "property1": "string",
          "property2": "string"
        }
      }
    ]
  }
]
```

<h3 id="get__api_configs-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|Inline|

<h3 id="get__api_configs-responseschema">Response Schema</h3>

Status Code **200**

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|*anonymous*|[[ConfigListItem](#schemaconfiglistitem)]|false|none|none|
|» configurationID|string¦null|false|none|none|
|» conditions|string¦null|false|none|none|
|» metricsTargeted|integer(int64)|false|none|none|
|» metricsApplied|integer(int64)|false|none|none|
|» metricsSuccess|integer(int64)|false|none|none|
|» metricsFailure|integer(int64)|false|none|none|
|» priority|integer(int32)|false|none|none|
|» creationDate|string(date-time)|false|none|none|
|» modules|[[GatewayModule](#schemagatewaymodule)]¦null|false|none|none|
|»» moduleName|string¦null|false|none|none|
|»» version|string¦null|false|none|none|
|»» status|string¦null|false|none|none|
|»» environmentVariables|object¦null|false|none|none|
|»»» **additionalProperties**|string|false|none|none|
|»» moduleIdentityTwinSettings|object¦null|false|none|none|
|»»» **additionalProperties**|string|false|none|none|

<aside class="success">
This operation does not require authentication
</aside>

## get__api_Configs_{configurationID}

> Code samples

`GET /api/Configs/{configurationID}`

*Retrieve a specific deployment and its modules from the IoT Hub.
Converts it to a ConfigListItem.*

<h3 id="get__api_configs_{configurationid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|configurationID|path|string|true|ID of the deployment to retrieve.|

> Example responses

> 200 Response

```
{"configurationID":"string","conditions":"string","metricsTargeted":0,"metricsApplied":0,"metricsSuccess":0,"metricsFailure":0,"priority":0,"creationDate":"2019-08-24T14:15:22Z","modules":[{"moduleName":"string","version":"string","status":"string","environmentVariables":{"property1":"string","property2":"string"},"moduleIdentityTwinSettings":{"property1":"string","property2":"string"}}]}
```

```json
{
  "configurationID": "string",
  "conditions": "string",
  "metricsTargeted": 0,
  "metricsApplied": 0,
  "metricsSuccess": 0,
  "metricsFailure": 0,
  "priority": 0,
  "creationDate": "2019-08-24T14:15:22Z",
  "modules": [
    {
      "moduleName": "string",
      "version": "string",
      "status": "string",
      "environmentVariables": {
        "property1": "string",
        "property2": "string"
      },
      "moduleIdentityTwinSettings": {
        "property1": "string",
        "property2": "string"
      }
    }
  ]
}
```

<h3 id="get__api_configs_{configurationid}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|[ConfigListItem](#schemaconfiglistitem)|

<aside class="success">
This operation does not require authentication
</aside>

<h1 id="azure-iot-hub-portal-api-devicemodels">DeviceModels</h1>

## get__api_DeviceModels

> Code samples

`GET /api/DeviceModels`

*Gets a list of device models from an Azure DataTable.*

> Example responses

> 200 Response

```
[{"modelId":"string","imageUrl":"string","name":"string","description":"string","appEUI":"string","sensorDecoderURL":"string","commands":[{"name":"string","frame":"string","port":1}]}]
```

```json
[
  {
    "modelId": "string",
    "imageUrl": "string",
    "name": "string",
    "description": "string",
    "appEUI": "string",
    "sensorDecoderURL": "string",
    "commands": [
      {
        "name": "string",
        "frame": "string",
        "port": 1
      }
    ]
  }
]
```

<h3 id="get__api_devicemodels-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|Inline|

<h3 id="get__api_devicemodels-responseschema">Response Schema</h3>

Status Code **200**

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|*anonymous*|[[DeviceModel](#schemadevicemodel)]|false|none|none|
|» modelId|string¦null|false|none|none|
|» imageUrl|string¦null|false|none|none|
|» name|string|true|none|none|
|» description|string¦null|false|none|none|
|» appEUI|string|true|none|none|
|» sensorDecoderURL|string¦null|false|none|none|
|» commands|[[DeviceModelCommand](#schemadevicemodelcommand)]¦null|false|none|none|
|»» name|string|true|none|none|
|»» frame|string|true|none|none|
|»» port|integer(int32)|true|none|none|

<aside class="success">
This operation does not require authentication
</aside>

## post__api_DeviceModels

> Code samples

`POST /api/DeviceModels`

> Body parameter

```json
{
  "modelId": "string",
  "imageUrl": "string",
  "name": "string",
  "description": "string",
  "appEUI": "string",
  "sensorDecoderURL": "string",
  "commands": [
    {
      "name": "string",
      "frame": "string",
      "port": 1
    }
  ]
}
```

<h3 id="post__api_devicemodels-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|body|body|[DeviceModel](#schemadevicemodel)|false|none|

<h3 id="post__api_devicemodels-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## put__api_DeviceModels

> Code samples

`PUT /api/DeviceModels`

> Body parameter

```json
{
  "modelId": "string",
  "imageUrl": "string",
  "name": "string",
  "description": "string",
  "appEUI": "string",
  "sensorDecoderURL": "string",
  "commands": [
    {
      "name": "string",
      "frame": "string",
      "port": 1
    }
  ]
}
```

<h3 id="put__api_devicemodels-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|body|body|[DeviceModel](#schemadevicemodel)|false|none|

<h3 id="put__api_devicemodels-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## get__api_DeviceModels_{modelID}

> Code samples

`GET /api/DeviceModels/{modelID}`

*Get a specific device model from an Azure DataTable.*

<h3 id="get__api_devicemodels_{modelid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|modelID|path|string|true|none|

<h3 id="get__api_devicemodels_{modelid}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## get__api_DeviceModels_{modelID}_avatar

> Code samples

`GET /api/DeviceModels/{modelID}/avatar`

<h3 id="get__api_devicemodels_{modelid}_avatar-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|modelID|path|string|true|none|

> Example responses

> 200 Response

```
"string"
```

```json
"string"
```

<h3 id="get__api_devicemodels_{modelid}_avatar-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|string|

<aside class="success">
This operation does not require authentication
</aside>

## post__api_DeviceModels_{modelID}_avatar

> Code samples

`POST /api/DeviceModels/{modelID}/avatar`

> Body parameter

```yaml
file: string

```

<h3 id="post__api_devicemodels_{modelid}_avatar-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|modelID|path|string|true|none|
|body|body|object|false|none|
|» file|body|string(binary)|false|none|

> Example responses

> 200 Response

```
"string"
```

```json
"string"
```

<h3 id="post__api_devicemodels_{modelid}_avatar-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|string|

<aside class="success">
This operation does not require authentication
</aside>

## delete__api_DeviceModels_{modelID}_avatar

> Code samples

`DELETE /api/DeviceModels/{modelID}/avatar`

<h3 id="delete__api_devicemodels_{modelid}_avatar-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|modelID|path|string|true|none|

<h3 id="delete__api_devicemodels_{modelid}_avatar-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## delete__api_DeviceModels_{deviceModelID}

> Code samples

`DELETE /api/DeviceModels/{deviceModelID}`

<h3 id="delete__api_devicemodels_{devicemodelid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|deviceModelID|path|string|true|none|

<h3 id="delete__api_devicemodels_{devicemodelid}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

<h1 id="azure-iot-hub-portal-api-devices">Devices</h1>

## get__api_Devices

> Code samples

`GET /api/Devices`

*Gets a list of devices as DeviceListItem from Azure IoT Hub.
Fields that do not appear in the device list are not defined here.*

> Example responses

> 200 Response

```
[{"deviceID":"string","deviceName":"string","imageUrl":"string","isConnected":true,"isEnabled":true,"statusUpdatedTime":"2019-08-24T14:15:22Z","appEUI":"string","appKey":"string","locationCode":"string"}]
```

```json
[
  {
    "deviceID": "string",
    "deviceName": "string",
    "imageUrl": "string",
    "isConnected": true,
    "isEnabled": true,
    "statusUpdatedTime": "2019-08-24T14:15:22Z",
    "appEUI": "string",
    "appKey": "string",
    "locationCode": "string"
  }
]
```

<h3 id="get__api_devices-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|Inline|

<h3 id="get__api_devices-responseschema">Response Schema</h3>

Status Code **200**

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|*anonymous*|[[DeviceListItem](#schemadevicelistitem)]|false|none|none|
|» deviceID|string¦null|false|none|none|
|» deviceName|string¦null|false|none|none|
|» imageUrl|string¦null|false|none|none|
|» isConnected|boolean|false|none|none|
|» isEnabled|boolean|false|none|none|
|» statusUpdatedTime|string(date-time)|false|none|none|
|» appEUI|string¦null|false|none|none|
|» appKey|string¦null|false|none|none|
|» locationCode|string¦null|false|none|none|

<aside class="success">
This operation does not require authentication
</aside>

## post__api_Devices

> Code samples

`POST /api/Devices`

> Body parameter

```json
{
  "deviceID": "string",
  "deviceName": "string",
  "imageUrl": "string",
  "isConnected": true,
  "isEnabled": true,
  "statusUpdatedTime": "2019-08-24T14:15:22Z",
  "appEUI": "string",
  "appKey": "string",
  "locationCode": "string",
  "assetId": "string",
  "deviceType": "string",
  "modelId": "string",
  "modelName": "string",
  "sensorDecoder": "string",
  "alreadyLoggedInOnce": true,
  "commands": [
    {
      "commandId": "string",
      "frame": "string"
    }
  ]
}
```

<h3 id="post__api_devices-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|body|body|[DeviceDetails](#schemadevicedetails)|false|none|

<h3 id="post__api_devices-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## put__api_Devices

> Code samples

`PUT /api/Devices`

*this function update the twin and the device.*

> Body parameter

```json
{
  "deviceID": "string",
  "deviceName": "string",
  "imageUrl": "string",
  "isConnected": true,
  "isEnabled": true,
  "statusUpdatedTime": "2019-08-24T14:15:22Z",
  "appEUI": "string",
  "appKey": "string",
  "locationCode": "string",
  "assetId": "string",
  "deviceType": "string",
  "modelId": "string",
  "modelName": "string",
  "sensorDecoder": "string",
  "alreadyLoggedInOnce": true,
  "commands": [
    {
      "commandId": "string",
      "frame": "string"
    }
  ]
}
```

<h3 id="put__api_devices-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|body|body|[DeviceDetails](#schemadevicedetails)|false|the device object.|

<h3 id="put__api_devices-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## get__api_Devices_{deviceID}

> Code samples

`GET /api/Devices/{deviceID}`

*Retrieve a specific device and from the IoT Hub.
Converts it to a DeviceListItem.*

<h3 id="get__api_devices_{deviceid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|deviceID|path|string|true|ID of the device to retrieve.|

> Example responses

> 200 Response

```
{"deviceID":"string","deviceName":"string","imageUrl":"string","isConnected":true,"isEnabled":true,"statusUpdatedTime":"2019-08-24T14:15:22Z","appEUI":"string","appKey":"string","locationCode":"string","assetId":"string","deviceType":"string","modelId":"string","modelName":"string","sensorDecoder":"string","alreadyLoggedInOnce":true,"commands":[{"commandId":"string","frame":"string"}]}
```

```json
{
  "deviceID": "string",
  "deviceName": "string",
  "imageUrl": "string",
  "isConnected": true,
  "isEnabled": true,
  "statusUpdatedTime": "2019-08-24T14:15:22Z",
  "appEUI": "string",
  "appKey": "string",
  "locationCode": "string",
  "assetId": "string",
  "deviceType": "string",
  "modelId": "string",
  "modelName": "string",
  "sensorDecoder": "string",
  "alreadyLoggedInOnce": true,
  "commands": [
    {
      "commandId": "string",
      "frame": "string"
    }
  ]
}
```

<h3 id="get__api_devices_{deviceid}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|[DeviceDetails](#schemadevicedetails)|

<aside class="success">
This operation does not require authentication
</aside>

## delete__api_Devices_{deviceID}

> Code samples

`DELETE /api/Devices/{deviceID}`

*this function delete a device.*

<h3 id="delete__api_devices_{deviceid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|deviceID|path|string|true|the device id.|

<h3 id="delete__api_devices_{deviceid}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## post__api_Devices_{deviceId}_{commandId}

> Code samples

`POST /api/Devices/{deviceId}/{commandId}`

*Permit to execute cloud to device message.*

<h3 id="post__api_devices_{deviceid}_{commandid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|deviceId|path|string|true|id of the device.|
|commandId|path|string|true|the command who contain the name and the trame.|

<h3 id="post__api_devices_{deviceid}_{commandid}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

<h1 id="azure-iot-hub-portal-api-gateways">Gateways</h1>

## get__api_Gateways

> Code samples

`GET /api/Gateways`

*Fonction permettant de récupèrer la liste des appareils Edge .
Après avoir éxecuté la query du registryManager on récupère le resultat
sous la forme d'une liste de Twin.*

> Example responses

> 200 Response

```
[{"deviceId":"string","status":"string","type":"string","nbDevices":0}]
```

```json
[
  {
    "deviceId": "string",
    "status": "string",
    "type": "string",
    "nbDevices": 0
  }
]
```

<h3 id="get__api_gateways-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|Inline|

<h3 id="get__api_gateways-responseschema">Response Schema</h3>

Status Code **200**

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|*anonymous*|[[GatewayListItem](#schemagatewaylistitem)]|false|none|none|
|» deviceId|string¦null|false|none|none|
|» status|string¦null|false|none|none|
|» type|string¦null|false|none|none|
|» nbDevices|integer(int32)|false|none|none|

<aside class="success">
This operation does not require authentication
</aside>

## post__api_Gateways

> Code samples

`POST /api/Gateways`

*this function create a device with the twin information.*

> Body parameter

```json
{
  "deviceId": "string",
  "symmetricKey": "string",
  "connectionState": "string",
  "scope": "string",
  "endPoint": "string",
  "type": "string",
  "status": "string",
  "runtimeResponse": "string",
  "nbDevices": 0,
  "nbModules": 0,
  "environment": "string",
  "lastDeployment": {
    "name": "string",
    "dateCreation": "2019-08-24T14:15:22Z",
    "status": "string"
  },
  "modules": [
    {
      "moduleName": "string",
      "version": "string",
      "status": "string",
      "environmentVariables": {
        "property1": "string",
        "property2": "string"
      },
      "moduleIdentityTwinSettings": {
        "property1": "string",
        "property2": "string"
      }
    }
  ]
}
```

<h3 id="post__api_gateways-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|body|body|[Gateway](#schemagateway)|false|the gateway object.|

<h3 id="post__api_gateways-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## get__api_Gateways_{deviceId}

> Code samples

`GET /api/Gateways/{deviceId}`

*This function return all the information we want of
a device.*

<h3 id="get__api_gateways_{deviceid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|deviceId|path|string|true|the device id.|

> Example responses

> 200 Response

```
{"deviceId":"string","symmetricKey":"string","connectionState":"string","scope":"string","endPoint":"string","type":"string","status":"string","runtimeResponse":"string","nbDevices":0,"nbModules":0,"environment":"string","lastDeployment":{"name":"string","dateCreation":"2019-08-24T14:15:22Z","status":"string"},"modules":[{"moduleName":"string","version":"string","status":"string","environmentVariables":{"property1":"string","property2":"string"},"moduleIdentityTwinSettings":{"property1":"string","property2":"string"}}]}
```

```json
{
  "deviceId": "string",
  "symmetricKey": "string",
  "connectionState": "string",
  "scope": "string",
  "endPoint": "string",
  "type": "string",
  "status": "string",
  "runtimeResponse": "string",
  "nbDevices": 0,
  "nbModules": 0,
  "environment": "string",
  "lastDeployment": {
    "name": "string",
    "dateCreation": "2019-08-24T14:15:22Z",
    "status": "string"
  },
  "modules": [
    {
      "moduleName": "string",
      "version": "string",
      "status": "string",
      "environmentVariables": {
        "property1": "string",
        "property2": "string"
      },
      "moduleIdentityTwinSettings": {
        "property1": "string",
        "property2": "string"
      }
    }
  ]
}
```

<h3 id="get__api_gateways_{deviceid}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|[Gateway](#schemagateway)|

<aside class="success">
This operation does not require authentication
</aside>

## delete__api_Gateways_{deviceId}

> Code samples

`DELETE /api/Gateways/{deviceId}`

*this function delete a device.*

<h3 id="delete__api_gateways_{deviceid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|deviceId|path|string|true|the device id to delete.|

<h3 id="delete__api_gateways_{deviceid}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## get__api_Gateways_{deviceId}_{deviceType}_ConnectionString

> Code samples

`GET /api/Gateways/{deviceId}/{deviceType}/ConnectionString`

<h3 id="get__api_gateways_{deviceid}_{devicetype}_connectionstring-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|deviceId|path|string|true|none|
|deviceType|path|string|true|none|

<h3 id="get__api_gateways_{deviceid}_{devicetype}_connectionstring-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## put__api_Gateways_{gateway}

> Code samples

`PUT /api/Gateways/{gateway}`

*This function update the properties of a device.*

> Body parameter

```json
{
  "deviceId": "string",
  "symmetricKey": "string",
  "connectionState": "string",
  "scope": "string",
  "endPoint": "string",
  "type": "string",
  "status": "string",
  "runtimeResponse": "string",
  "nbDevices": 0,
  "nbModules": 0,
  "environment": "string",
  "lastDeployment": {
    "name": "string",
    "dateCreation": "2019-08-24T14:15:22Z",
    "status": "string"
  },
  "modules": [
    {
      "moduleName": "string",
      "version": "string",
      "status": "string",
      "environmentVariables": {
        "property1": "string",
        "property2": "string"
      },
      "moduleIdentityTwinSettings": {
        "property1": "string",
        "property2": "string"
      }
    }
  ]
}
```

<h3 id="put__api_gateways_{gateway}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|gateway|path|string|true|none|
|body|body|[Gateway](#schemagateway)|false|a gateways object.|

<h3 id="put__api_gateways_{gateway}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## post__api_Gateways_{deviceId}_{moduleId}_{methodName}

> Code samples

`POST /api/Gateways/{deviceId}/{moduleId}/{methodName}`

> Body parameter

```json
{
  "moduleName": "string",
  "version": "string",
  "status": "string",
  "environmentVariables": {
    "property1": "string",
    "property2": "string"
  },
  "moduleIdentityTwinSettings": {
    "property1": "string",
    "property2": "string"
  }
}
```

<h3 id="post__api_gateways_{deviceid}_{moduleid}_{methodname}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|deviceId|path|string|true|none|
|methodName|path|string|true|none|
|moduleId|path|string|true|none|
|body|body|[GatewayModule](#schemagatewaymodule)|false|none|

> Example responses

> 200 Response

```
{"payload":"string","status":0}
```

```json
{
  "payload": "string",
  "status": 0
}
```

<h3 id="post__api_gateways_{deviceid}_{moduleid}_{methodname}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|[C2Dresult](#schemac2dresult)|

<aside class="success">
This operation does not require authentication
</aside>

<h1 id="azure-iot-hub-portal-api-oidcsettings">OIDCSettings</h1>

## GET ODICSettings

<a id="opIdGET ODICSettings"></a>

> Code samples

`GET /OIDCSettings`

*Get the Open ID Settings.*

<h3 id="get-odicsettings-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Returns the OIDC settings.|None|

<aside class="success">
This operation does not require authentication
</aside>

# Schemas

<h2 id="tocS_C2Dresult">C2Dresult</h2>
<!-- backwards compatibility -->
<a id="schemac2dresult"></a>
<a id="schema_C2Dresult"></a>
<a id="tocSc2dresult"></a>
<a id="tocsc2dresult"></a>

```json
{
  "payload": "string",
  "status": 0
}

```

### Properties

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|payload|string¦null|false|none|none|
|status|integer(int32)|false|none|none|

<h2 id="tocS_Channel">Channel</h2>
<!-- backwards compatibility -->
<a id="schemachannel"></a>
<a id="schema_Channel"></a>
<a id="tocSchannel"></a>
<a id="tocschannel"></a>

```json
{
  "enable": true,
  "freq": 0,
  "radio": 0,
  "if": 0,
  "bandwidth": 0,
  "spread_factor": 0
}

```

### Properties

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|enable|boolean¦null|false|none|none|
|freq|integer(int32)|false|none|none|
|radio|integer(int32)|false|none|none|
|if|integer(int32)|false|none|none|
|bandwidth|integer(int32)|false|none|none|
|spread_factor|integer(int32)|false|none|none|

<h2 id="tocS_Command">Command</h2>
<!-- backwards compatibility -->
<a id="schemacommand"></a>
<a id="schema_Command"></a>
<a id="tocScommand"></a>
<a id="tocscommand"></a>

```json
{
  "commandId": "string",
  "frame": "string"
}

```

### Properties

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|commandId|string¦null|false|none|none|
|frame|string¦null|false|none|none|

<h2 id="tocS_Concentrator">Concentrator</h2>
<!-- backwards compatibility -->
<a id="schemaconcentrator"></a>
<a id="schema_Concentrator"></a>
<a id="tocSconcentrator"></a>
<a id="tocsconcentrator"></a>

```json
{
  "deviceId": "string",
  "deviceName": "string",
  "loraRegion": "string",
  "deviceType": "string",
  "clientCertificateThumbprint": "string",
  "isConnected": true,
  "isEnabled": true,
  "alreadyLoggedInOnce": true,
  "routerConfig": {
    "netID": [
      0
    ],
    "joinEui": [
      [
        "string"
      ]
    ],
    "region": "string",
    "hwspec": "string",
    "freq_range": [
      0
    ],
    "dRs": [
      [
        0
      ]
    ],
    "sx1301_conf": [
      {
        "property1": {
          "enable": true,
          "freq": 0,
          "radio": 0,
          "if": 0,
          "bandwidth": 0,
          "spread_factor": 0
        },
        "property2": {
          "enable": true,
          "freq": 0,
          "radio": 0,
          "if": 0,
          "bandwidth": 0,
          "spread_factor": 0
        }
      }
    ],
    "nocca": true,
    "nodc": true,
    "nodwell": true
  }
}

```

### Properties

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|deviceId|string|true|none|none|
|deviceName|string|true|none|none|
|loraRegion|string|true|none|none|
|deviceType|string¦null|false|none|none|
|clientCertificateThumbprint|string¦null|false|none|none|
|isConnected|boolean|false|none|none|
|isEnabled|boolean|false|none|none|
|alreadyLoggedInOnce|boolean|false|none|none|
|routerConfig|[RouterConfig](#schemarouterconfig)|false|none|none|

<h2 id="tocS_ConfigItem">ConfigItem</h2>
<!-- backwards compatibility -->
<a id="schemaconfigitem"></a>
<a id="schema_ConfigItem"></a>
<a id="tocSconfigitem"></a>
<a id="tocsconfigitem"></a>

```json
{
  "name": "string",
  "dateCreation": "2019-08-24T14:15:22Z",
  "status": "string"
}

```

### Properties

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|name|string¦null|false|none|none|
|dateCreation|string(date-time)|false|none|none|
|status|string¦null|false|none|none|

<h2 id="tocS_ConfigListItem">ConfigListItem</h2>
<!-- backwards compatibility -->
<a id="schemaconfiglistitem"></a>
<a id="schema_ConfigListItem"></a>
<a id="tocSconfiglistitem"></a>
<a id="tocsconfiglistitem"></a>

```json
{
  "configurationID": "string",
  "conditions": "string",
  "metricsTargeted": 0,
  "metricsApplied": 0,
  "metricsSuccess": 0,
  "metricsFailure": 0,
  "priority": 0,
  "creationDate": "2019-08-24T14:15:22Z",
  "modules": [
    {
      "moduleName": "string",
      "version": "string",
      "status": "string",
      "environmentVariables": {
        "property1": "string",
        "property2": "string"
      },
      "moduleIdentityTwinSettings": {
        "property1": "string",
        "property2": "string"
      }
    }
  ]
}

```

### Properties

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|configurationID|string¦null|false|none|none|
|conditions|string¦null|false|none|none|
|metricsTargeted|integer(int64)|false|none|none|
|metricsApplied|integer(int64)|false|none|none|
|metricsSuccess|integer(int64)|false|none|none|
|metricsFailure|integer(int64)|false|none|none|
|priority|integer(int32)|false|none|none|
|creationDate|string(date-time)|false|none|none|
|modules|[[GatewayModule](#schemagatewaymodule)]¦null|false|none|none|

<h2 id="tocS_DeviceDetails">DeviceDetails</h2>
<!-- backwards compatibility -->
<a id="schemadevicedetails"></a>
<a id="schema_DeviceDetails"></a>
<a id="tocSdevicedetails"></a>
<a id="tocsdevicedetails"></a>

```json
{
  "deviceID": "string",
  "deviceName": "string",
  "imageUrl": "string",
  "isConnected": true,
  "isEnabled": true,
  "statusUpdatedTime": "2019-08-24T14:15:22Z",
  "appEUI": "string",
  "appKey": "string",
  "locationCode": "string",
  "assetId": "string",
  "deviceType": "string",
  "modelId": "string",
  "modelName": "string",
  "sensorDecoder": "string",
  "alreadyLoggedInOnce": true,
  "commands": [
    {
      "commandId": "string",
      "frame": "string"
    }
  ]
}

```

### Properties

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|deviceID|string|true|none|none|
|deviceName|string|true|none|none|
|imageUrl|string¦null|false|none|none|
|isConnected|boolean|false|none|none|
|isEnabled|boolean|false|none|none|
|statusUpdatedTime|string(date-time)|false|none|none|
|appEUI|string|true|none|none|
|appKey|string¦null|false|none|none|
|locationCode|string¦null|false|none|none|
|assetId|string¦null|false|none|none|
|deviceType|string¦null|false|none|none|
|modelId|string¦null|false|none|none|
|modelName|string|true|none|none|
|sensorDecoder|string¦null|false|none|none|
|alreadyLoggedInOnce|boolean|false|none|none|
|commands|[[Command](#schemacommand)]¦null|false|none|none|

<h2 id="tocS_DeviceListItem">DeviceListItem</h2>
<!-- backwards compatibility -->
<a id="schemadevicelistitem"></a>
<a id="schema_DeviceListItem"></a>
<a id="tocSdevicelistitem"></a>
<a id="tocsdevicelistitem"></a>

```json
{
  "deviceID": "string",
  "deviceName": "string",
  "imageUrl": "string",
  "isConnected": true,
  "isEnabled": true,
  "statusUpdatedTime": "2019-08-24T14:15:22Z",
  "appEUI": "string",
  "appKey": "string",
  "locationCode": "string"
}

```

### Properties

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|deviceID|string¦null|false|none|none|
|deviceName|string¦null|false|none|none|
|imageUrl|string¦null|false|none|none|
|isConnected|boolean|false|none|none|
|isEnabled|boolean|false|none|none|
|statusUpdatedTime|string(date-time)|false|none|none|
|appEUI|string¦null|false|none|none|
|appKey|string¦null|false|none|none|
|locationCode|string¦null|false|none|none|

<h2 id="tocS_DeviceModel">DeviceModel</h2>
<!-- backwards compatibility -->
<a id="schemadevicemodel"></a>
<a id="schema_DeviceModel"></a>
<a id="tocSdevicemodel"></a>
<a id="tocsdevicemodel"></a>

```json
{
  "modelId": "string",
  "imageUrl": "string",
  "name": "string",
  "description": "string",
  "appEUI": "string",
  "sensorDecoderURL": "string",
  "commands": [
    {
      "name": "string",
      "frame": "string",
      "port": 1
    }
  ]
}

```

### Properties

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|modelId|string¦null|false|none|none|
|imageUrl|string¦null|false|none|none|
|name|string|true|none|none|
|description|string¦null|false|none|none|
|appEUI|string|true|none|none|
|sensorDecoderURL|string¦null|false|none|none|
|commands|[[DeviceModelCommand](#schemadevicemodelcommand)]¦null|false|none|none|

<h2 id="tocS_DeviceModelCommand">DeviceModelCommand</h2>
<!-- backwards compatibility -->
<a id="schemadevicemodelcommand"></a>
<a id="schema_DeviceModelCommand"></a>
<a id="tocSdevicemodelcommand"></a>
<a id="tocsdevicemodelcommand"></a>

```json
{
  "name": "string",
  "frame": "string",
  "port": 1
}

```

### Properties

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|name|string|true|none|none|
|frame|string|true|none|none|
|port|integer(int32)|true|none|none|

<h2 id="tocS_Gateway">Gateway</h2>
<!-- backwards compatibility -->
<a id="schemagateway"></a>
<a id="schema_Gateway"></a>
<a id="tocSgateway"></a>
<a id="tocsgateway"></a>

```json
{
  "deviceId": "string",
  "symmetricKey": "string",
  "connectionState": "string",
  "scope": "string",
  "endPoint": "string",
  "type": "string",
  "status": "string",
  "runtimeResponse": "string",
  "nbDevices": 0,
  "nbModules": 0,
  "environment": "string",
  "lastDeployment": {
    "name": "string",
    "dateCreation": "2019-08-24T14:15:22Z",
    "status": "string"
  },
  "modules": [
    {
      "moduleName": "string",
      "version": "string",
      "status": "string",
      "environmentVariables": {
        "property1": "string",
        "property2": "string"
      },
      "moduleIdentityTwinSettings": {
        "property1": "string",
        "property2": "string"
      }
    }
  ]
}

```

### Properties

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|deviceId|string|true|none|none|
|symmetricKey|string¦null|false|none|none|
|connectionState|string¦null|false|none|none|
|scope|string¦null|false|none|none|
|endPoint|string¦null|false|none|none|
|type|string|true|none|none|
|status|string¦null|false|none|none|
|runtimeResponse|string¦null|false|none|none|
|nbDevices|integer(int32)|false|none|none|
|nbModules|integer(int32)|false|none|none|
|environment|string¦null|false|none|none|
|lastDeployment|[ConfigItem](#schemaconfigitem)|false|none|none|
|modules|[[GatewayModule](#schemagatewaymodule)]¦null|false|none|none|

<h2 id="tocS_GatewayListItem">GatewayListItem</h2>
<!-- backwards compatibility -->
<a id="schemagatewaylistitem"></a>
<a id="schema_GatewayListItem"></a>
<a id="tocSgatewaylistitem"></a>
<a id="tocsgatewaylistitem"></a>

```json
{
  "deviceId": "string",
  "status": "string",
  "type": "string",
  "nbDevices": 0
}

```

### Properties

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|deviceId|string¦null|false|none|none|
|status|string¦null|false|none|none|
|type|string¦null|false|none|none|
|nbDevices|integer(int32)|false|none|none|

<h2 id="tocS_GatewayModule">GatewayModule</h2>
<!-- backwards compatibility -->
<a id="schemagatewaymodule"></a>
<a id="schema_GatewayModule"></a>
<a id="tocSgatewaymodule"></a>
<a id="tocsgatewaymodule"></a>

```json
{
  "moduleName": "string",
  "version": "string",
  "status": "string",
  "environmentVariables": {
    "property1": "string",
    "property2": "string"
  },
  "moduleIdentityTwinSettings": {
    "property1": "string",
    "property2": "string"
  }
}

```

### Properties

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|moduleName|string¦null|false|none|none|
|version|string¦null|false|none|none|
|status|string¦null|false|none|none|
|environmentVariables|object¦null|false|none|none|
|» **additionalProperties**|string|false|none|none|
|moduleIdentityTwinSettings|object¦null|false|none|none|
|» **additionalProperties**|string|false|none|none|

<h2 id="tocS_RouterConfig">RouterConfig</h2>
<!-- backwards compatibility -->
<a id="schemarouterconfig"></a>
<a id="schema_RouterConfig"></a>
<a id="tocSrouterconfig"></a>
<a id="tocsrouterconfig"></a>

```json
{
  "netID": [
    0
  ],
  "joinEui": [
    [
      "string"
    ]
  ],
  "region": "string",
  "hwspec": "string",
  "freq_range": [
    0
  ],
  "dRs": [
    [
      0
    ]
  ],
  "sx1301_conf": [
    {
      "property1": {
        "enable": true,
        "freq": 0,
        "radio": 0,
        "if": 0,
        "bandwidth": 0,
        "spread_factor": 0
      },
      "property2": {
        "enable": true,
        "freq": 0,
        "radio": 0,
        "if": 0,
        "bandwidth": 0,
        "spread_factor": 0
      }
    }
  ],
  "nocca": true,
  "nodc": true,
  "nodwell": true
}

```

### Properties

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|netID|[integer]¦null|false|none|none|
|joinEui|[array]¦null|false|none|none|
|region|string¦null|false|none|none|
|hwspec|string¦null|false|none|none|
|freq_range|[integer]¦null|false|none|none|
|dRs|[array]¦null|false|none|none|
|sx1301_conf|[object]¦null|false|none|none|
|» **additionalProperties**|[Channel](#schemachannel)|false|none|none|
|nocca|boolean|false|none|none|
|nodc|boolean|false|none|none|
|nodwell|boolean|false|none|none|

