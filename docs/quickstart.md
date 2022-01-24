## Quick Start

This project hosts an Azure deployment template to deloy the required Azure infrastructure and get you started quickly.

> **Note:** This is a beta version of the project.

## Prerequisites

The following should be completed before proceeding with the IoT Hub Portal development or deployment in your environment.

* You must have an Azure subscription. Get an [Azure Free account](https://azure.microsoft.com/en-us/offers/ms-azr-0044p/) to get started.
* You must have configured an Azure AD B2C Tenant with applications. See [Portal AD applications configuration](./b2c-applications.md) page.
* Understandr how IoTEdge LoraWAN StarterKit work. Have a look at [https://azure.github.io/iotedge-lorawan-starterkit](https://azure.github.io/iotedge-lorawan-starterkit) to get started.

## Deployed Azure Resources

The template will deploy in your Azure subscription the Following resources:

* IoT Hub
* Azure Function and Consumption Service Plan
* Redis Cache
* Application Insights
* Log Analytics (when opted in to use Azure Monitor)
* Azure WebApp and Service Plan

## Step-By-Step instructions

1. Choose a solution prefix for your deployment.
1. Use [Portal AD applications configuration](./b2c-applications.md) page to configure your AD B2C Tenant.
    > You should have recorded the following information:
    > * Tenant name: `<your-tenant-id>`
    > * Tenant ID: `<your-tenant-id>`
    > * API Client ID: `<your-client-id>`
    > * API Client Secret: `<your-client-secret>`
    > * Client ID: `<your-client-id>`

1. Press on the button here below to start your Azure Deployment.

    [![Deploy](http://azuredeploy.net/deploybutton.png)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fmichelin%2Fi4i-iot-hub-portal%2Fmain%2Ftemplates%2Fazuredeploy.json)

1. You will get to a page asking you to fill the following fields :
    * **Resource Group**: A logical "folder" where all the template resource would be put into, just choose a meaningful name. 
    * **Location**: In which DataCenter the resources should be deployed. Make sure to choose a location where IoT Hub is available
    * **Unique Solution Prefix**: A string that would be used as prefix for all the resources name to ensure their uniqueness.
    * **B2c Directory Name**: The name of the B2C directory that will be used to authenticate the portal.
    * **Tenant Id**: the ID of the B2C tenant that will be used to authenticate the portal.
    * **Api Client Id**: the ID of the API client that will be used to authenticate the portal.
    * **Api Client Secret**: the secret of the API client that will be used to authenticate the portal.
    * **Client Id**: the ID of the web client that will be used to authenticate the portal.
    * **Edge gateway name**: the name of your LoRa Gateway node in the IoT Hub.
    * **Deploy Device**: Do you want demo end devices to be already provisioned (one using OTAA and one using ABP)? If yes set this to true, the code located in the Arduino folder would be ready to use immediately.
    * **Reset pin**:  The reset pin of your gateway (the value should be 7 for the Seed Studio LoRaWan, 25 for the IC880A)
    * **Region**:  In what region are you operating your device (currently only EU868 and US915 is supported)

    > see: [https://azure.github.io/iotedge-lorawan-starterkit/dev/quickstart/#deployed-azure-infrastructure](https://azure.github.io/iotedge-lorawan-starterkit/dev/quickstart/#deployed-azure-infrastructure) for more information about the LoRaWan IoT Hub and Azure deployment.

