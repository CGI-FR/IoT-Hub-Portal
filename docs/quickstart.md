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

1. Choose a solution prefix for your deployment
1. Create an Azure AD B2C Tenant (see: [https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-tenant#create-an-azure-ad-b2c-tenant](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-tenant#create-an-azure-ad-b2c-tenant))
1. Configure the requiered AD Applications.
    1. Create the **IoT Hub Portal API** Application:
        * Select **App registrations**, and then select **New registration**.
        * Enter a Name for the application. For example, **IoT Hub Portal**.
        * Under **Redirect URI**, select **Web**, and then enter an expected endpoint for your portal (ex: _https://**tenantName**.b2clogin.com/**tenantName**.onmicrosoft.com/oauth2/authresp_)
        * Select **Register**.
        * Record the **Application (client) ID** for use in your web API's code.
        * Under **Manage**, select **Certificates & Secrets**.
        * Under **Client secret**, select **New client secret**. 
        * Enter a name for the secret.
        * Record the **Client secret** for use in your web API's code.
        * Under **Manage**, select **API permissions**.
        * Under **Configured permissions**, select **Add a permission**.
        * Select the **Microsoft APIs** tab.
        * Under **Commonly used Microsoft APIs**, select **Microsoft Graph**.
        * Select **Delegated permissions**.
        * Under **Delegated permissions**, select: 
            * ``openid``
            * ``offline_access``
        * Select **Add permission**.
        * Under **Configured permissions**, select **Add a permission**.
        * Select the **Microsoft APIs** tab.
        * Under **Commonly used Microsoft APIs**, select **Microsoft Graph**.
        * Select **Application permissions**.
        * Under **Application permissions**, expand **User**, then select: 
            * ``User.Invite.All``
            * ``User.ManageIdentities.All``
            * ``User.Read.All``
            * ``User.ReadWrite.All``
        * Select **Add permission**.
        * Under **Manage**, select **Expose an API**.
        * Next to **Application ID URI**, select the **Set** link.
        * Under **Scopes defined by this API**, select **Add a scope**.
        * Enter the following values to create a scope that defines read access to the API, then select **Add scope**:
            * **Scope name**: ``API.Access`` (this is the name of the scope that will be used in the template)
            * **Admin consent display name**: ``Access to the Portal API``
            * **Admin consent description**: ``Allows the application to get access to the Portal API``

    1. Create the **IoT Hub Portal Client** Application:
        * Select **App registrations**, and then select **New registration**.
        * Enter a Name for the application. For example, **IoT Hub Portal Client**.
        * Under **Redirect URI**, select **Web**, and then enter an expected endpoint for your portal (ex: _https://**solutionPrefix**portal.azurewebsites.net/authentication/login-callback_)
        * Select **Register**.
        * Select **App registrations**, and then select the web application that should have access to the API.
        * Under **Manage**, select **API permissions**.
        * Under **Configured permissions**, select **Add a permission**.
        * Select the **My APIs** tab.
        * Select the API to which the web application should be granted access.
        * Under **Permission**, expand **API**, and then select the scope that you defined earlier.
        * Select **Add permissions**.
        * Select **Grant admin consent for (your tenant name)**.
        * If you're prompted to select an account, select your currently signed-in administrator account, or sign in      * with an account in your Azure AD B2C tenant that's been assigned at least the Cloud application administrator       * role.
        * Select **Yes**.
        * Select **Refresh**, and then verify that "Granted for ..." appears under **Status** for both scopes.

1. Press on the button here below to start your Azure Deployment.

    <a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fmichelin%2Fi4i-iot-hub-portal%2Fmain%2Ftemplates%2Fazuredeploy.json" target="_blank">
        <img src="http://azuredeploy.net/deploybutton.png"/>
    </a>


