# Developer Guide

## Directory Structure

The code is organized into the following directory structure:

* **src**: Source code
  * **AzureIoTHub.Portal.Server.Tests**: Unit test project for the Portal
  * **AzureIoTHub.Portal**: The Portal project
    * **Client**: .NET 6 Blazor Web Assembly project that alow to visualize the IoT Hub data
    * **Server**: .NET 6 Web API project that provides the API for the Portal
    * **Shared**: Shared code between the Client and Server projects
* **templates**: contains the templates for the "deploy to Azure" button

## Overall Architecture

This schema represent the various components and how they interact to have a better understanding of the various solution elements.

![./images/overall-architecture.png](./images/overall-architecture.png)

1. The user is authenticated by the OpenID Connect server.
1. The user access to the IoT Hub Portal with the OAuth2.0 token.
1. The IoT Hub portal uses the Azure IoT Hub REST API to retrieve the data.
1. The IoT Hub portal uses the Azure Device Provisioning Service to manage IoT Edge devices.
1. The IoT Hub portal uses the Azure Storage account to store the device models configuration (Images, Commands, etc.).
1. The IoT Hub portal uses the LoRa Key Management Facade to send Cloud to Device (C2D) messages to LoRa devices.
1. The LoRa Key Management Facade uses Redis to store its cached data.
1. The LoRa Key Management Facade uses the Azure IoT Hub REST API to retrieve the LoRa device keys and send C2D messages.

> Note: For more information about the LoRa Key Management Facade, see the [Azure IoT Edge LoRaWAN Starter Kit](https://azure.github.io/iotedge-lorawan-starterkit) page.

## IoT Hub Portal Configuration

By deploying the IoT Hub Portal, the user can configure the IoT Hub and the LoRaWAN network.

Since the IoT Hub Portal is deployed as a Docker container, the application settings can be configured with environment variables.

#### Application settings

Here are different settings that the user can configure:

* **PortalName**: The name of the portal (shown in the App Bar and the Page Title).
* **OIDC__Authority**: The OpenID Connect issuer.
* **OIDC__MetadataUrl**: The OpenID Connect metadata URL (e.g. `.well-known/openid-configuration`).
* **OIDC__ClientId**: The OpenID Connect client ID for the Web UI.
* **OIDC__ApiClientId**: The OpenID Connect client ID for the API.
* **OIDC__Scope**: The OpenID Connect scope that represents the portal API.
* **IoTDPS__ServiceEndpoint**: The IoT Device Provisioning Service endpoint.
* **IoTDPS__IDScope**: The IoT Device Provisioning Service ID scope.
* **LoRaFeature__Enabled**: Whether the LoRaWAN feature is enabled or not.
* **LoRaKeyManagement__Url**: The LoRa Key Management Facade URL.
* **StorageAccount__BlobContainerName**: The name of the Azure Storage container where the device models images are stored.
* **ASPNETCORE_ENVIRONMENT**: Built-in environment variable, used to target the configuration provided by a specific environment. Two accepted values:
  * `Development`: On this environment, logs are produced up to `Debug` level.
  * `Production`: Default value if ASPNETCORE_ENVIRONMENT is not set. On this environment, logs are produced up to `Information` level.
* **Metrics__ExporterRefreshIntervalInSeconds**: (Optional, default value `30`) The refresh interval in `seconds` to collect custom metrics and expose them to the exporter endpoint.
* **Metrics__LoaderRefreshIntervalInMinutes**: (Optional, default value `10`) The refresh interval in `minutes` to calculate/refresh custom metrics values.
* **Ideas__Enabled**: (Optional, default value `false`) To enable Ideas feature when set to `true`.
* **Ideas__Url**: Url of `Awesome-Ideas`, to publish ideas submitted by users.
* **Ideas__Authentication__Header**: (Optional, default value `Ocp-Apim-Subscription-Key`) Authentication header name.
* **Ideas__Authentication__Token**: Authentication token.

#### Connection strings

Here are different connection strings that the user can configure:

* **IoTHub__ConnectionString**: The connection string to the IoT Hub.
* **IoTDPS__ConnectionString**: The connection string to the Azure IoT Device Provisioning Service.
* **StorageAccount__ConnectionString**: The connection string to the Azure Storage account.
* **LoRaKeyManagement__Code**: The LoRa Key Management Facade code.

> Note: For a production environment, an Azure Key Vault is advised to store the connection strings.

### Optional Security Settings

There are several optional security settings that the user can configure. These settings are not required for the Portal to work. By default the Portal is configured to set security levels to ``Microsoft.IdentityModel.Tokens`` defaults but the user can override these settings.

* UseSecurityHeaders
  > This boolean adds the following headers to all responses :
  > 
  > `X-Content-Type-Options: nosniff`
  > 
  > `Strict-Transport-Security: max-age=31536000; includeSubDomains` - _only applied to HTTPS responses_
  > 
  > 
  > `X-Frame-Options: Deny` - _only applied to text/html responses_
  > 
  > `X-XSS-Protection: 1; mode=block` - _only applied to text/html responses_
  > 
  > `Referrer-Policy: strict-origin-when-cross-origin `- _only applied to text/html responses_
  > 
  > `Content-Security-Policy: object-src 'none'; form-action 'self'; frame-ancestors 'none'` - _only applied to text/html responses_.
  >
  > The **default is true**.
* OIDC__ValidateIssuer
  > Validation of the issuer mitigates forwarding attacks that can occur when an IdentityProvider represents multiple tenants and signs tokens with the same keys. It is possible that a token issued for the same audience could be from a different tenant. For example an application could accept users from contoso.onmicrosoft.com but not fabrikam.onmicrosoft.com, both valid tenants. An application that accepts tokens from fabrikam could forward them to the application that accepts tokens for contoso. This boolean only applies to default issuer validation. If [IssuerValidator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.tokenvalidationparameters.issuervalidator?view=azure-dotnet#microsoft-identitymodel-tokens-tokenvalidationparameters-issuervalidator) is set, it will be called regardless of whether this property is true or false.
  >
  > The **default is true**.
* OIDC__ValidateAudience
  > Validation of the audience, mitigates forwarding attacks. For example, a site that receives a token, could not replay it to another side. A forwarded token would contain the audience of the original site. This boolean only applies to default audience validation. If [AudienceValidator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.tokenvalidationparameters.audiencevalidator?view=azure-dotnet#microsoft-identitymodel-tokens-tokenvalidationparameters-audiencevalidator) is set, it will be called regardless of whether this property is true or false.
  >
  > The **default is true**.
* OIDC__ValidateLifetime
  > This boolean only applies to default lifetime validation. If [LifetimeValidator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.tokenvalidationparameters.lifetimevalidator?view=azure-dotnet#microsoft-identitymodel-tokens-tokenvalidationparameters-lifetimevalidator) is set, it will be called regardless of whether this property is true or false.
  >
  > The **default is true**.
* OIDC__ValidateIssuerSigningKey
  > It is possible for tokens to contain the public key needed to check the signature. For example, X509Data can be hydrated into an X509Certificate, which can be used to validate the signature. In these cases it is important to validate the SigningKey that was used to validate the signature. This boolean only applies to default signing key validation. If [IssuerSigningKeyValidator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.tokenvalidationparameters.issuersigningkeyvalidator?view=azure-dotnet#microsoft-identitymodel-tokens-tokenvalidationparameters-issuersigningkeyvalidator) is set, it will be called regardless of whether this property is true or false.
  >
  > The **default is false**.
* OIDC_ValidateActor
  > If an actor token is detected, whether it should be validated.
  >
  > The **default is false**.
* OIDC_ValidateTokenReplay
  > This boolean only applies to default token replay validation. If [TokenReplayValidator](https://docs.microsoft.com/fr-fr/dotnet/api/microsoft.identitymodel.tokens.tokenvalidationparameters.tokenreplayvalidator?view=azure-dotnet#microsoft-identitymodel-tokens-tokenvalidationparameters-tokenreplayvalidator) is set, it will be called regardless of whether this property is true or false.
  >
  > The **default is false**.


## Device tags

The IoT Hub portal uses some tags to configure the devices. The tags are stored in the Azure IoT Hub in Device Twins.

* **deviceType**: The device type, can be "LoRa Device" or "null".
    > By setting the device type to "LoRa Device", the device will be configured to send LoRaWAN and receive C2D commands.
* **modelId**: The device model ID that is used to retrieve the device model configuration.

## Storage Account

The Storage Account is used to store the device models configuration. You can use the same Storage Account that is used by the LoRa Key Management Facade.
This solution will use tables and blob storage to store its data. There is no need to create the tables and containers, the application will do it for you.

#### Tables

The application uses the following tables:

* **DeviceTemplates**: The table that contains the device model configurations.
* **DeviceCommands**: The table that contains the device commands linked to the device models.

#### Blob Storage

The application uses the following blob storage:

* **device-images**: The blob storage that contains the device images.

# Working with the documentation

This documentation site is build using Github Pages.

```docs/main``` is a detached branch that is locked and only accepts PRs. On PR merge, Github Pages will automatically update the documentation website.

## How to update the documentation

1. Checkout the branch that contains the documentation:

    ```sh
    git checkout origin/docs/main
    git checkout -b docs/<your_branch_name> 
    ```

1. Update the documentation
1. Commit your changes
1. Push your changes to the branch
1. Create a PR

### Customizing

The documentation uses the Github pages theme [Leap Day](https://github.com/pages-themes/leap-day).  
You can customize the theme by editing the `_config.yml` file. You can also customize the theme by editing the `_layouts/default.html` file.

#### Stylesheet

If you'd like to add your own custom styles:

1. Update the `assets/css/style.scss` file from the documentation repository.
1. Add any custom CSS (or Sass, including imports) you'd like immediately after the `@import` line

*Note: If you'd like to change the theme's Sass variables, you must set new values before the `@import` line in your stylesheet.*

#### Layouts

If you'd like to change the theme's HTML layout:

1. Update the `/_layouts/default.html`file from the documentation repository.
1. Customize the layout as you'd like

## Problem Details

On IoT Hub Portal, we use the library [Hellang.Middleware.ProblemDetails](https://github.com/khellang/Middleware) which implements [RFC7807](https://datatracker.ietf.org/doc/html/rfc7807) to describe issues/problems that occurred on backend.

### Handle a new exception using `Problem Details`

* Create a new exception which extends [`BaseException`](https://github.com/CGI-FR/IoT-Hub-Portal/blob/main/src/AzureIoTHub.Portal/Server/Exceptions/BaseException.cs). For example see 👉 [`InternalServerErrorException`](https://github.com/CGI-FR/IoT-Hub-Portal/blob/main/src/AzureIoTHub.Portal/Server/Exceptions/InternalServerErrorException.cs)
* On [Startup](https://github.com/CGI-FR/IoT-Hub-Portal/blob/main/src/AzureIoTHub.Portal/Server/Startup.cs) class, within the instruction `services.AddProblemDetails()`:
    > Your new exception is already catched by the middleware Problem Details because its extends the exception `BaseException`.
    > If you want override the behavior of the middleware when processing your exception, you have to add a new mapping within it.

> 💡 You can also map exceptions from dotnet framework and third parties.

### Handle `Problem Details` exceptions on frontend

On frontend, http client uses a delegating handler [ProblemDetailsHandler](https://github.com/CGI-FR/IoT-Hub-Portal/blob/main/src/AzureIoTHub.Portal/Client/Handlers/ProblemDetailsHandler.cs) to:

* Execute the http request and wait the response
* If the response is not successful:
  * The body of the response is deserialized to `ProblemDetailsWithExceptionDetails`
  * An exception with type `ProblemDetailsException` (including the error response) is thrown. 

On Blazor views, http calls must be catched to capture any exceptions of type `ProblemDetailsException` to be able to execute any business code to process them.

When an http call fails, the user must be notified visually by the application: A component [Error](https://github.com/CGI-FR/IoT-Hub-Portal/blob/main/src/AzureIoTHub.Portal/Client/Shared/NavMenu.razor) has been made to respond to this use case.
Below an example on how to:

* Catch an ProblemDetailsException when making a http call
* Delegate the exception to the `Error` component, so that it can visually warn the user

```csharp
@code {
    // Inject the reference to the Error component as a cascading parameter
    [CascadingParameter]
    public Error Error {get; set;}

    private await Task GetData()
    {
        try
        {
            // Execute an http request
        }
        catch (ProblemDetailsException exception)
        {
            // Pass the ProblemDetailsException exception to Error component using its method ProcessProblemDetails()
            // The Error component will alert the user by showing a (snackbar/dialog) using the content of the exception
            Error?.ProcessProblemDetails(exception)
        }
    }
}
```
