# Deploying infrastructure

You can deploy the infrastructure using the following command using the [Deploy to Azure button](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-to-azure-button)

## Deployed Azure Resources

The template will deploy in your Azure subscription the Following resources:

* IoT Hub
* Azure Function and Consumption Service Plan (When opted for LoRaWAN Starter Kit)
* Redis Cache
* Application Insights
* Log Analytics (when opted in to use Azure Monitor)
* Azure WebApp and Service Plan
* Azure Database for PostgreSQL

## Before starting

1. Choose a solution prefix for your deployment.
1. Use [Portal AD applications configuration](https://cgi-fr.github.io/IoT-Hub-Portal/stable/b2c-applications) page to configure your AD B2C Tenant.
    > You should have recorded the following information:
    >
    > * OpenID authority: `<your-openid-authority>`
    > * OpenID metadata URL: `<your-openid-provider-metadata-url>`
    > * Client ID: `<your-client-id>`
    > * API Client ID: `<your-client-id>`

## Quick Start

### Azure

[Quick Start](azure/#quick-start) for Azure environment.

### Amazon Web Services

[Quick Start](aws/#quick-start) for AWS environment.
