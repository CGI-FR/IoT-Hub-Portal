---
title: AzureIoTHub.Portal.Server v1.0
language_tabs:
  - shell: Shell
  - http: HTTP
  - javascript: JavaScript
  - ruby: Ruby
  - python: Python
  - php: PHP
  - java: Java
  - go: Go
toc_footers: []
includes: []
search: true
highlight_theme: darkula
headingLevel: 2

---

<!-- Generator: Widdershins v4.0.1 -->

<h1 id="azureiothub-portal-server">AzureIoTHub.Portal.Server v1.0</h1>

> Scroll down for code samples, example requests and responses. Select a language for code samples from the tabs above or the mobile navigation menu.

<h1 id="azureiothub-portal-server-commands">Commands</h1>

## post__api_Commands_{modelId}

> Code samples

```shell
# You can also use wget
curl -X POST /api/Commands/{modelId} \
  -H 'Content-Type: application/json'

```

```http
POST /api/Commands/{modelId} HTTP/1.1

Content-Type: application/json

```

```javascript
const inputBody = '{
  "name": "string",
  "frame": "string",
  "port": 1
}';
const headers = {
  'Content-Type':'application/json'
};

fetch('/api/Commands/{modelId}',
{
  method: 'POST',
  body: inputBody,
  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Content-Type' => 'application/json'
}

result = RestClient.post '/api/Commands/{modelId}',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Content-Type': 'application/json'
}

r = requests.post('/api/Commands/{modelId}', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Content-Type' => 'application/json',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('POST','/api/Commands/{modelId}', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Commands/{modelId}");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("POST");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Content-Type": []string{"application/json"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("POST", "/api/Commands/{modelId}", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

`POST /api/Commands/{modelId}`

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

```shell
# You can also use wget
curl -X DELETE /api/Commands/{modelId}/{commandId}

```

```http
DELETE /api/Commands/{modelId}/{commandId} HTTP/1.1

```

```javascript

fetch('/api/Commands/{modelId}/{commandId}',
{
  method: 'DELETE'

})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

result = RestClient.delete '/api/Commands/{modelId}/{commandId}',
  params: {
  }

p JSON.parse(result)

```

```python
import requests

r = requests.delete('/api/Commands/{modelId}/{commandId}')

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('DELETE','/api/Commands/{modelId}/{commandId}', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Commands/{modelId}/{commandId}");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("DELETE");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("DELETE", "/api/Commands/{modelId}/{commandId}", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

`DELETE /api/Commands/{modelId}/{commandId}`

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

<h1 id="azureiothub-portal-server-concentrators">Concentrators</h1>

## get__api_Concentrators

> Code samples

```shell
# You can also use wget
curl -X GET /api/Concentrators

```

```http
GET /api/Concentrators HTTP/1.1

```

```javascript

fetch('/api/Concentrators',
{
  method: 'GET'

})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

result = RestClient.get '/api/Concentrators',
  params: {
  }

p JSON.parse(result)

```

```python
import requests

r = requests.get('/api/Concentrators')

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('GET','/api/Concentrators', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Concentrators");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("GET");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("GET", "/api/Concentrators", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

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

```shell
# You can also use wget
curl -X POST /api/Concentrators \
  -H 'Content-Type: application/json'

```

```http
POST /api/Concentrators HTTP/1.1

Content-Type: application/json

```

```javascript
const inputBody = '{
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
}';
const headers = {
  'Content-Type':'application/json'
};

fetch('/api/Concentrators',
{
  method: 'POST',
  body: inputBody,
  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Content-Type' => 'application/json'
}

result = RestClient.post '/api/Concentrators',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Content-Type': 'application/json'
}

r = requests.post('/api/Concentrators', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Content-Type' => 'application/json',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('POST','/api/Concentrators', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Concentrators");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("POST");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Content-Type": []string{"application/json"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("POST", "/api/Concentrators", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

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

```shell
# You can also use wget
curl -X PUT /api/Concentrators \
  -H 'Content-Type: application/json'

```

```http
PUT /api/Concentrators HTTP/1.1

Content-Type: application/json

```

```javascript
const inputBody = '{
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
}';
const headers = {
  'Content-Type':'application/json'
};

fetch('/api/Concentrators',
{
  method: 'PUT',
  body: inputBody,
  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Content-Type' => 'application/json'
}

result = RestClient.put '/api/Concentrators',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Content-Type': 'application/json'
}

r = requests.put('/api/Concentrators', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Content-Type' => 'application/json',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('PUT','/api/Concentrators', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Concentrators");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("PUT");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Content-Type": []string{"application/json"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("PUT", "/api/Concentrators", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

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

```shell
# You can also use wget
curl -X GET /api/Concentrators/{deviceId}

```

```http
GET /api/Concentrators/{deviceId} HTTP/1.1

```

```javascript

fetch('/api/Concentrators/{deviceId}',
{
  method: 'GET'

})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

result = RestClient.get '/api/Concentrators/{deviceId}',
  params: {
  }

p JSON.parse(result)

```

```python
import requests

r = requests.get('/api/Concentrators/{deviceId}')

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('GET','/api/Concentrators/{deviceId}', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Concentrators/{deviceId}");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("GET");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("GET", "/api/Concentrators/{deviceId}", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

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

```shell
# You can also use wget
curl -X DELETE /api/Concentrators/{deviceId}

```

```http
DELETE /api/Concentrators/{deviceId} HTTP/1.1

```

```javascript

fetch('/api/Concentrators/{deviceId}',
{
  method: 'DELETE'

})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

result = RestClient.delete '/api/Concentrators/{deviceId}',
  params: {
  }

p JSON.parse(result)

```

```python
import requests

r = requests.delete('/api/Concentrators/{deviceId}')

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('DELETE','/api/Concentrators/{deviceId}', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Concentrators/{deviceId}");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("DELETE");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("DELETE", "/api/Concentrators/{deviceId}", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

`DELETE /api/Concentrators/{deviceId}`

<h3 id="delete__api_concentrators_{deviceid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|deviceId|path|string|true|none|

<h3 id="delete__api_concentrators_{deviceid}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

<h1 id="azureiothub-portal-server-configs">Configs</h1>

## get__api_Configs

> Code samples

```shell
# You can also use wget
curl -X GET /api/Configs \
  -H 'Accept: text/plain'

```

```http
GET /api/Configs HTTP/1.1

Accept: text/plain

```

```javascript

const headers = {
  'Accept':'text/plain'
};

fetch('/api/Configs',
{
  method: 'GET',

  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Accept' => 'text/plain'
}

result = RestClient.get '/api/Configs',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Accept': 'text/plain'
}

r = requests.get('/api/Configs', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Accept' => 'text/plain',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('GET','/api/Configs', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Configs");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("GET");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Accept": []string{"text/plain"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("GET", "/api/Configs", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

`GET /api/Configs`

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

```shell
# You can also use wget
curl -X GET /api/Configs/{configurationID} \
  -H 'Accept: text/plain'

```

```http
GET /api/Configs/{configurationID} HTTP/1.1

Accept: text/plain

```

```javascript

const headers = {
  'Accept':'text/plain'
};

fetch('/api/Configs/{configurationID}',
{
  method: 'GET',

  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Accept' => 'text/plain'
}

result = RestClient.get '/api/Configs/{configurationID}',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Accept': 'text/plain'
}

r = requests.get('/api/Configs/{configurationID}', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Accept' => 'text/plain',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('GET','/api/Configs/{configurationID}', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Configs/{configurationID}");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("GET");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Accept": []string{"text/plain"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("GET", "/api/Configs/{configurationID}", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

`GET /api/Configs/{configurationID}`

<h3 id="get__api_configs_{configurationid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|configurationID|path|string|true|none|

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

<h1 id="azureiothub-portal-server-devicemodels">DeviceModels</h1>

## get__api_DeviceModels

> Code samples

```shell
# You can also use wget
curl -X GET /api/DeviceModels \
  -H 'Accept: text/plain'

```

```http
GET /api/DeviceModels HTTP/1.1

Accept: text/plain

```

```javascript

const headers = {
  'Accept':'text/plain'
};

fetch('/api/DeviceModels',
{
  method: 'GET',

  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Accept' => 'text/plain'
}

result = RestClient.get '/api/DeviceModels',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Accept': 'text/plain'
}

r = requests.get('/api/DeviceModels', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Accept' => 'text/plain',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('GET','/api/DeviceModels', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/DeviceModels");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("GET");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Accept": []string{"text/plain"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("GET", "/api/DeviceModels", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

`GET /api/DeviceModels`

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

```shell
# You can also use wget
curl -X POST /api/DeviceModels \
  -H 'Content-Type: application/json'

```

```http
POST /api/DeviceModels HTTP/1.1

Content-Type: application/json

```

```javascript
const inputBody = '{
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
}';
const headers = {
  'Content-Type':'application/json'
};

fetch('/api/DeviceModels',
{
  method: 'POST',
  body: inputBody,
  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Content-Type' => 'application/json'
}

result = RestClient.post '/api/DeviceModels',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Content-Type': 'application/json'
}

r = requests.post('/api/DeviceModels', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Content-Type' => 'application/json',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('POST','/api/DeviceModels', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/DeviceModels");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("POST");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Content-Type": []string{"application/json"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("POST", "/api/DeviceModels", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

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

```shell
# You can also use wget
curl -X PUT /api/DeviceModels \
  -H 'Content-Type: application/json'

```

```http
PUT /api/DeviceModels HTTP/1.1

Content-Type: application/json

```

```javascript
const inputBody = '{
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
}';
const headers = {
  'Content-Type':'application/json'
};

fetch('/api/DeviceModels',
{
  method: 'PUT',
  body: inputBody,
  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Content-Type' => 'application/json'
}

result = RestClient.put '/api/DeviceModels',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Content-Type': 'application/json'
}

r = requests.put('/api/DeviceModels', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Content-Type' => 'application/json',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('PUT','/api/DeviceModels', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/DeviceModels");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("PUT");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Content-Type": []string{"application/json"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("PUT", "/api/DeviceModels", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

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

```shell
# You can also use wget
curl -X GET /api/DeviceModels/{modelID}

```

```http
GET /api/DeviceModels/{modelID} HTTP/1.1

```

```javascript

fetch('/api/DeviceModels/{modelID}',
{
  method: 'GET'

})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

result = RestClient.get '/api/DeviceModels/{modelID}',
  params: {
  }

p JSON.parse(result)

```

```python
import requests

r = requests.get('/api/DeviceModels/{modelID}')

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('GET','/api/DeviceModels/{modelID}', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/DeviceModels/{modelID}");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("GET");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("GET", "/api/DeviceModels/{modelID}", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

`GET /api/DeviceModels/{modelID}`

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

```shell
# You can also use wget
curl -X GET /api/DeviceModels/{modelID}/avatar \
  -H 'Accept: text/plain'

```

```http
GET /api/DeviceModels/{modelID}/avatar HTTP/1.1

Accept: text/plain

```

```javascript

const headers = {
  'Accept':'text/plain'
};

fetch('/api/DeviceModels/{modelID}/avatar',
{
  method: 'GET',

  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Accept' => 'text/plain'
}

result = RestClient.get '/api/DeviceModels/{modelID}/avatar',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Accept': 'text/plain'
}

r = requests.get('/api/DeviceModels/{modelID}/avatar', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Accept' => 'text/plain',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('GET','/api/DeviceModels/{modelID}/avatar', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/DeviceModels/{modelID}/avatar");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("GET");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Accept": []string{"text/plain"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("GET", "/api/DeviceModels/{modelID}/avatar", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

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

```shell
# You can also use wget
curl -X POST /api/DeviceModels/{modelID}/avatar \
  -H 'Content-Type: multipart/form-data' \
  -H 'Accept: text/plain'

```

```http
POST /api/DeviceModels/{modelID}/avatar HTTP/1.1

Content-Type: multipart/form-data
Accept: text/plain

```

```javascript
const inputBody = '{
  "file": "string"
}';
const headers = {
  'Content-Type':'multipart/form-data',
  'Accept':'text/plain'
};

fetch('/api/DeviceModels/{modelID}/avatar',
{
  method: 'POST',
  body: inputBody,
  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Content-Type' => 'multipart/form-data',
  'Accept' => 'text/plain'
}

result = RestClient.post '/api/DeviceModels/{modelID}/avatar',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Content-Type': 'multipart/form-data',
  'Accept': 'text/plain'
}

r = requests.post('/api/DeviceModels/{modelID}/avatar', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Content-Type' => 'multipart/form-data',
    'Accept' => 'text/plain',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('POST','/api/DeviceModels/{modelID}/avatar', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/DeviceModels/{modelID}/avatar");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("POST");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Content-Type": []string{"multipart/form-data"},
        "Accept": []string{"text/plain"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("POST", "/api/DeviceModels/{modelID}/avatar", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

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

```shell
# You can also use wget
curl -X DELETE /api/DeviceModels/{modelID}/avatar

```

```http
DELETE /api/DeviceModels/{modelID}/avatar HTTP/1.1

```

```javascript

fetch('/api/DeviceModels/{modelID}/avatar',
{
  method: 'DELETE'

})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

result = RestClient.delete '/api/DeviceModels/{modelID}/avatar',
  params: {
  }

p JSON.parse(result)

```

```python
import requests

r = requests.delete('/api/DeviceModels/{modelID}/avatar')

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('DELETE','/api/DeviceModels/{modelID}/avatar', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/DeviceModels/{modelID}/avatar");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("DELETE");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("DELETE", "/api/DeviceModels/{modelID}/avatar", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

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

```shell
# You can also use wget
curl -X DELETE /api/DeviceModels/{deviceModelID}

```

```http
DELETE /api/DeviceModels/{deviceModelID} HTTP/1.1

```

```javascript

fetch('/api/DeviceModels/{deviceModelID}',
{
  method: 'DELETE'

})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

result = RestClient.delete '/api/DeviceModels/{deviceModelID}',
  params: {
  }

p JSON.parse(result)

```

```python
import requests

r = requests.delete('/api/DeviceModels/{deviceModelID}')

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('DELETE','/api/DeviceModels/{deviceModelID}', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/DeviceModels/{deviceModelID}");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("DELETE");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("DELETE", "/api/DeviceModels/{deviceModelID}", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

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

<h1 id="azureiothub-portal-server-devices">Devices</h1>

## get__api_Devices

> Code samples

```shell
# You can also use wget
curl -X GET /api/Devices \
  -H 'Accept: text/plain'

```

```http
GET /api/Devices HTTP/1.1

Accept: text/plain

```

```javascript

const headers = {
  'Accept':'text/plain'
};

fetch('/api/Devices',
{
  method: 'GET',

  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Accept' => 'text/plain'
}

result = RestClient.get '/api/Devices',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Accept': 'text/plain'
}

r = requests.get('/api/Devices', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Accept' => 'text/plain',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('GET','/api/Devices', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Devices");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("GET");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Accept": []string{"text/plain"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("GET", "/api/Devices", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

`GET /api/Devices`

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

```shell
# You can also use wget
curl -X POST /api/Devices \
  -H 'Content-Type: application/json'

```

```http
POST /api/Devices HTTP/1.1

Content-Type: application/json

```

```javascript
const inputBody = '{
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
}';
const headers = {
  'Content-Type':'application/json'
};

fetch('/api/Devices',
{
  method: 'POST',
  body: inputBody,
  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Content-Type' => 'application/json'
}

result = RestClient.post '/api/Devices',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Content-Type': 'application/json'
}

r = requests.post('/api/Devices', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Content-Type' => 'application/json',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('POST','/api/Devices', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Devices");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("POST");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Content-Type": []string{"application/json"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("POST", "/api/Devices", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

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

```shell
# You can also use wget
curl -X PUT /api/Devices \
  -H 'Content-Type: application/json'

```

```http
PUT /api/Devices HTTP/1.1

Content-Type: application/json

```

```javascript
const inputBody = '{
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
}';
const headers = {
  'Content-Type':'application/json'
};

fetch('/api/Devices',
{
  method: 'PUT',
  body: inputBody,
  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Content-Type' => 'application/json'
}

result = RestClient.put '/api/Devices',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Content-Type': 'application/json'
}

r = requests.put('/api/Devices', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Content-Type' => 'application/json',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('PUT','/api/Devices', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Devices");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("PUT");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Content-Type": []string{"application/json"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("PUT", "/api/Devices", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

`PUT /api/Devices`

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
|body|body|[DeviceDetails](#schemadevicedetails)|false|none|

<h3 id="put__api_devices-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## get__api_Devices_{deviceID}

> Code samples

```shell
# You can also use wget
curl -X GET /api/Devices/{deviceID} \
  -H 'Accept: text/plain'

```

```http
GET /api/Devices/{deviceID} HTTP/1.1

Accept: text/plain

```

```javascript

const headers = {
  'Accept':'text/plain'
};

fetch('/api/Devices/{deviceID}',
{
  method: 'GET',

  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Accept' => 'text/plain'
}

result = RestClient.get '/api/Devices/{deviceID}',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Accept': 'text/plain'
}

r = requests.get('/api/Devices/{deviceID}', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Accept' => 'text/plain',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('GET','/api/Devices/{deviceID}', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Devices/{deviceID}");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("GET");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Accept": []string{"text/plain"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("GET", "/api/Devices/{deviceID}", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

`GET /api/Devices/{deviceID}`

<h3 id="get__api_devices_{deviceid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|deviceID|path|string|true|none|

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

```shell
# You can also use wget
curl -X DELETE /api/Devices/{deviceID}

```

```http
DELETE /api/Devices/{deviceID} HTTP/1.1

```

```javascript

fetch('/api/Devices/{deviceID}',
{
  method: 'DELETE'

})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

result = RestClient.delete '/api/Devices/{deviceID}',
  params: {
  }

p JSON.parse(result)

```

```python
import requests

r = requests.delete('/api/Devices/{deviceID}')

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('DELETE','/api/Devices/{deviceID}', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Devices/{deviceID}");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("DELETE");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("DELETE", "/api/Devices/{deviceID}", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

`DELETE /api/Devices/{deviceID}`

<h3 id="delete__api_devices_{deviceid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|deviceID|path|string|true|none|

<h3 id="delete__api_devices_{deviceid}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## post__api_Devices_{deviceId}_{commandId}

> Code samples

```shell
# You can also use wget
curl -X POST /api/Devices/{deviceId}/{commandId}

```

```http
POST /api/Devices/{deviceId}/{commandId} HTTP/1.1

```

```javascript

fetch('/api/Devices/{deviceId}/{commandId}',
{
  method: 'POST'

})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

result = RestClient.post '/api/Devices/{deviceId}/{commandId}',
  params: {
  }

p JSON.parse(result)

```

```python
import requests

r = requests.post('/api/Devices/{deviceId}/{commandId}')

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('POST','/api/Devices/{deviceId}/{commandId}', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Devices/{deviceId}/{commandId}");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("POST");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("POST", "/api/Devices/{deviceId}/{commandId}", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

`POST /api/Devices/{deviceId}/{commandId}`

<h3 id="post__api_devices_{deviceid}_{commandid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|deviceId|path|string|true|none|
|commandId|path|string|true|none|

<h3 id="post__api_devices_{deviceid}_{commandid}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

<h1 id="azureiothub-portal-server-gateways">Gateways</h1>

## get__api_Gateways

> Code samples

```shell
# You can also use wget
curl -X GET /api/Gateways \
  -H 'Accept: text/plain'

```

```http
GET /api/Gateways HTTP/1.1

Accept: text/plain

```

```javascript

const headers = {
  'Accept':'text/plain'
};

fetch('/api/Gateways',
{
  method: 'GET',

  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Accept' => 'text/plain'
}

result = RestClient.get '/api/Gateways',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Accept': 'text/plain'
}

r = requests.get('/api/Gateways', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Accept' => 'text/plain',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('GET','/api/Gateways', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Gateways");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("GET");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Accept": []string{"text/plain"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("GET", "/api/Gateways", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

`GET /api/Gateways`

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

```shell
# You can also use wget
curl -X POST /api/Gateways \
  -H 'Content-Type: application/json'

```

```http
POST /api/Gateways HTTP/1.1

Content-Type: application/json

```

```javascript
const inputBody = '{
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
}';
const headers = {
  'Content-Type':'application/json'
};

fetch('/api/Gateways',
{
  method: 'POST',
  body: inputBody,
  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Content-Type' => 'application/json'
}

result = RestClient.post '/api/Gateways',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Content-Type': 'application/json'
}

r = requests.post('/api/Gateways', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Content-Type' => 'application/json',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('POST','/api/Gateways', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Gateways");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("POST");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Content-Type": []string{"application/json"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("POST", "/api/Gateways", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

`POST /api/Gateways`

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
|body|body|[Gateway](#schemagateway)|false|none|

<h3 id="post__api_gateways-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## get__api_Gateways_{deviceId}

> Code samples

```shell
# You can also use wget
curl -X GET /api/Gateways/{deviceId} \
  -H 'Accept: text/plain'

```

```http
GET /api/Gateways/{deviceId} HTTP/1.1

Accept: text/plain

```

```javascript

const headers = {
  'Accept':'text/plain'
};

fetch('/api/Gateways/{deviceId}',
{
  method: 'GET',

  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Accept' => 'text/plain'
}

result = RestClient.get '/api/Gateways/{deviceId}',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Accept': 'text/plain'
}

r = requests.get('/api/Gateways/{deviceId}', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Accept' => 'text/plain',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('GET','/api/Gateways/{deviceId}', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Gateways/{deviceId}");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("GET");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Accept": []string{"text/plain"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("GET", "/api/Gateways/{deviceId}", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

`GET /api/Gateways/{deviceId}`

<h3 id="get__api_gateways_{deviceid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|deviceId|path|string|true|none|

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

```shell
# You can also use wget
curl -X DELETE /api/Gateways/{deviceId}

```

```http
DELETE /api/Gateways/{deviceId} HTTP/1.1

```

```javascript

fetch('/api/Gateways/{deviceId}',
{
  method: 'DELETE'

})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

result = RestClient.delete '/api/Gateways/{deviceId}',
  params: {
  }

p JSON.parse(result)

```

```python
import requests

r = requests.delete('/api/Gateways/{deviceId}')

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('DELETE','/api/Gateways/{deviceId}', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Gateways/{deviceId}");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("DELETE");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("DELETE", "/api/Gateways/{deviceId}", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

`DELETE /api/Gateways/{deviceId}`

<h3 id="delete__api_gateways_{deviceid}-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|deviceId|path|string|true|none|

<h3 id="delete__api_gateways_{deviceid}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## get__api_Gateways_{deviceId}_{deviceType}_ConnectionString

> Code samples

```shell
# You can also use wget
curl -X GET /api/Gateways/{deviceId}/{deviceType}/ConnectionString

```

```http
GET /api/Gateways/{deviceId}/{deviceType}/ConnectionString HTTP/1.1

```

```javascript

fetch('/api/Gateways/{deviceId}/{deviceType}/ConnectionString',
{
  method: 'GET'

})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

result = RestClient.get '/api/Gateways/{deviceId}/{deviceType}/ConnectionString',
  params: {
  }

p JSON.parse(result)

```

```python
import requests

r = requests.get('/api/Gateways/{deviceId}/{deviceType}/ConnectionString')

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('GET','/api/Gateways/{deviceId}/{deviceType}/ConnectionString', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Gateways/{deviceId}/{deviceType}/ConnectionString");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("GET");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("GET", "/api/Gateways/{deviceId}/{deviceType}/ConnectionString", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

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

```shell
# You can also use wget
curl -X PUT /api/Gateways/{gateway} \
  -H 'Content-Type: application/json'

```

```http
PUT /api/Gateways/{gateway} HTTP/1.1

Content-Type: application/json

```

```javascript
const inputBody = '{
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
}';
const headers = {
  'Content-Type':'application/json'
};

fetch('/api/Gateways/{gateway}',
{
  method: 'PUT',
  body: inputBody,
  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Content-Type' => 'application/json'
}

result = RestClient.put '/api/Gateways/{gateway}',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Content-Type': 'application/json'
}

r = requests.put('/api/Gateways/{gateway}', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Content-Type' => 'application/json',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('PUT','/api/Gateways/{gateway}', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Gateways/{gateway}");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("PUT");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Content-Type": []string{"application/json"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("PUT", "/api/Gateways/{gateway}", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

`PUT /api/Gateways/{gateway}`

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
|body|body|[Gateway](#schemagateway)|false|none|

<h3 id="put__api_gateways_{gateway}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

<aside class="success">
This operation does not require authentication
</aside>

## post__api_Gateways_{deviceId}_{moduleId}_{methodName}

> Code samples

```shell
# You can also use wget
curl -X POST /api/Gateways/{deviceId}/{moduleId}/{methodName} \
  -H 'Content-Type: application/json' \
  -H 'Accept: text/plain'

```

```http
POST /api/Gateways/{deviceId}/{moduleId}/{methodName} HTTP/1.1

Content-Type: application/json
Accept: text/plain

```

```javascript
const inputBody = '{
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
}';
const headers = {
  'Content-Type':'application/json',
  'Accept':'text/plain'
};

fetch('/api/Gateways/{deviceId}/{moduleId}/{methodName}',
{
  method: 'POST',
  body: inputBody,
  headers: headers
})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

headers = {
  'Content-Type' => 'application/json',
  'Accept' => 'text/plain'
}

result = RestClient.post '/api/Gateways/{deviceId}/{moduleId}/{methodName}',
  params: {
  }, headers: headers

p JSON.parse(result)

```

```python
import requests
headers = {
  'Content-Type': 'application/json',
  'Accept': 'text/plain'
}

r = requests.post('/api/Gateways/{deviceId}/{moduleId}/{methodName}', headers = headers)

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$headers = array(
    'Content-Type' => 'application/json',
    'Accept' => 'text/plain',
);

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('POST','/api/Gateways/{deviceId}/{moduleId}/{methodName}', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/api/Gateways/{deviceId}/{moduleId}/{methodName}");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("POST");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    headers := map[string][]string{
        "Content-Type": []string{"application/json"},
        "Accept": []string{"text/plain"},
    }

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("POST", "/api/Gateways/{deviceId}/{moduleId}/{methodName}", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

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

<h1 id="azureiothub-portal-server-oidcsettings">OIDCSettings</h1>

## get__OIDCSettings

> Code samples

```shell
# You can also use wget
curl -X GET /OIDCSettings

```

```http
GET /OIDCSettings HTTP/1.1

```

```javascript

fetch('/OIDCSettings',
{
  method: 'GET'

})
.then(function(res) {
    return res.json();
}).then(function(body) {
    console.log(body);
});

```

```ruby
require 'rest-client'
require 'json'

result = RestClient.get '/OIDCSettings',
  params: {
  }

p JSON.parse(result)

```

```python
import requests

r = requests.get('/OIDCSettings')

print(r.json())

```

```php
<?php

require 'vendor/autoload.php';

$client = new \GuzzleHttp\Client();

// Define array of request body.
$request_body = array();

try {
    $response = $client->request('GET','/OIDCSettings', array(
        'headers' => $headers,
        'json' => $request_body,
       )
    );
    print_r($response->getBody()->getContents());
 }
 catch (\GuzzleHttp\Exception\BadResponseException $e) {
    // handle exception or api errors.
    print_r($e->getMessage());
 }

 // ...

```

```java
URL obj = new URL("/OIDCSettings");
HttpURLConnection con = (HttpURLConnection) obj.openConnection();
con.setRequestMethod("GET");
int responseCode = con.getResponseCode();
BufferedReader in = new BufferedReader(
    new InputStreamReader(con.getInputStream()));
String inputLine;
StringBuffer response = new StringBuffer();
while ((inputLine = in.readLine()) != null) {
    response.append(inputLine);
}
in.close();
System.out.println(response.toString());

```

```go
package main

import (
       "bytes"
       "net/http"
)

func main() {

    data := bytes.NewBuffer([]byte{jsonReq})
    req, err := http.NewRequest("GET", "/OIDCSettings", data)
    req.Header = headers

    client := &http.Client{}
    resp, err := client.Do(req)
    // ...
}

```

`GET /OIDCSettings`

<h3 id="get__oidcsettings-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|None|

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

