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

* **OIDC__Authority**: The OpenID Connect issuer.
* **OIDC__MetadataUrl**: The OpenID Connect metadata URL (e.g. `.well-known/openid-configuration`).
* **OIDC__ClientId**: The OpenID Connect client ID for the Web UI.
* **OIDC__ApiClientId**: The OpenID Connect client ID for the API.
* **OIDC__Scope**: The OpenID Connect scope that represents the portal API.
* **IoTDPS__ServiceEndpoint**: The IoT Device Provisioning Service endpoint.
* **LoRaFeature__Enabled**: Whether the LoRaWAN feature is enabled or not.
* **LoRaKeyManagement__Url**: The LoRa Key Management Facade URL.
* **LoRaRegionRouterConfig__Url**: The LoRa Region Router Config URL.
* **ASPNETCORE_ENVIRONMENT**: Built-in environment variable, used to target the configuration provided by a specific environment. Two accepted values:
  * `Development`: On this environement, logs are produred up to `Debug` level.
  * `Production`: Default value if ASPNETCORE_ENVIRONMENT is not set. On this environement, logs are produred up to `Information` level.

> Note: `LoRaRegionRouterConfig__Url` is the URL of the LoRa Region Router Config file repository. By default you can use 'https://raw.githubusercontent.com/Azure/iotedge-lorawan-starterkit/dev/Tools/Cli-LoRa-Device-Provisioning/DefaultRouterConfig/' which is where the Azure IoT Edge LoRaWAN project is hosted.

#### Connection strings

Here are different connection strings that the user can configure:

* **IoTHub__ConnectionString**: The connection string to the IoT Hub.
* **IoTDPS__ConnectionString**: The connection string to the Azure IoT Device Provisioning Service.
* **StorageAccount__ConnectionString**: The connection string to the Azure Storage account.
* **LoRaKeyManagement__Code**: The LoRa Key Management Facade code.

> Note: For a production environment, an Azure Key Vault is advised to store the connection strings.

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

On IoT Hub Portal, we use the library [Hellang.Middleware.ProblemDetails](https://github.com/khellang/Middleware) which implements [RFC7807](https://datatracker.ietf.org/doc/html/rfc7807) to describe issues/problems that occured on backend.

### Handle a new exception using `Problem Details`

* Create a new exception which extends [`BaseException`](https://github.com/CGI-FR/IoT-Hub-Portal/blob/main/src/AzureIoTHub.Portal/Server/Exceptions/BaseException.cs). For example see 👉 [`InternalServerErrorException`](https://github.com/CGI-FR/IoT-Hub-Portal/blob/main/src/AzureIoTHub.Portal/Server/Exceptions/InternalServerErrorException.cs)
* On [Startup](https://github.com/CGI-FR/IoT-Hub-Portal/blob/main/src/AzureIoTHub.Portal/Server/Startup.cs) class, within the instruction `services.AddProblemDetails()`:
    * Your new exception is already catched by the middleware Problem Details because its extends the exception `BaseException`.
    * It you want override the behaviour of the middleware when processing your exception, you have to add a new mapping within it.

> 💡 You can also map exceptions from dotnet framework and third parties.

### Handle `Problem Details` exceptions on frontend

On frontend, http client uses a delegating handler [ProblemDetailsHandler](https://github.com/CGI-FR/IoT-Hub-Portal/blob/main/src/AzureIoTHub.Portal/Client/Handlers/ProblemDetailsHandler.cs) to:

- Execute the http request and wait the response
- If the response is not successful:
  - The body of the response is deserialized to `ProblemDetailsWithExceptionDetails`
  - An exception with type `ProblemDetailsException` (inluding the error response) is thrown 


On blazor views, http calls must be catched to capture any exceptions of type `ProblemDetailsException` to be able to execute any business code to process them.

When an http call fails, the user must be notified visually by the application: A component [Error](https://github.com/CGI-FR/IoT-Hub-Portal/blob/main/src/AzureIoTHub.Portal/Client/Shared/NavMenu.razor) has been made to respond to this use case.
Below an example on how to:

- Catch an ProblemDetailsException when making a http call
- Delegate the exception to the `Error` component, so that it can visually warn the user

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
            // Pass the ProblemDetailsException exception to Error compoment using its method ProcessProblemDetails()
            // The Error component will alert the user by showing a (snackbar/dialog) using the content of the exception
            Error?.ProcessProblemDetails(exception)
        }
    }
}
```
