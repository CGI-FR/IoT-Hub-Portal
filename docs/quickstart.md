## Quick Start

This project hosts an Azure deployment template to deloy the required Azure infrastructure and get you started quickly.

> **Note:** This is a beta version of the project.

## Prerequisites

The following should be completed before proceeding with the IoT Hub Portal development or deployment in your environment.

* You must have an Azure subscription. Get an [Azure Free account](https://azure.microsoft.com/en-us/offers/ms-azr-0044p/) to get started.
* You must have configured an Azure AD B2C Tenant with applications. See [Portal AD applications configuration]() page.
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

1. Create an Azure AD B2C Tenant.
1. Configure the requiered AD Applications.
1. Press on the button here below to start your Azure Deployment.

    <a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fmichelin%2Fi4i-iot-hub-portal%2Fmain%2Ftemplates%2Fazuredeploy.json" target="_blank">
        <img src="http://azuredeploy.net/deploybutton.png"/>
    </a>
