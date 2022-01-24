
## Azure AD B2C Tenant with applications

This solution uses Azure AD B2C to authenticate the portal.
In this page you will configure the B2C tenant and two applications (API and Web UI).

By the end, you should have recorded the following information:

    * Tenant name: `<your-tenant-id>`
    * Tenant ID: `<your-tenant-id>`
    * API Client ID: `<your-client-id>`
    * API Client Secret: `<your-client-secret>`
    * Client ID: `<your-client-id>`

## Step by Step instructions

1. Create an Azure AD B2C Tenant (see: [https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-tenant#create-an-azure-ad-b2c-tenant](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-tenant#create-an-azure-ad-b2c-tenant))
    * Record the **tenant ID** and the **tenant name**.
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
        * Select **Application permissions**.
        * Under **Application permissions**, expand **User**, then select: 
            * ``User.Invite.All``
            * ``User.ManageIdentities.All``
            * ``User.Read.All``
            * ``User.ReadWrite.All``
        * Select **Add permission**.
        * If you're prompted to select an account, select your currently signed-in administrator account, or sign in      * with an account in your Azure AD B2C tenant that's been assigned at least the Cloud application administrator       * role.
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
        * Record the **Application (client) ID** for use in your web client.
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

1. Configure the required User flow:
    1. Select **User flows**, and then select **New user flow**.
    1. Under *Select a user flow type**, select **Sign in**, then select **Create**.
    1. Enter a name for the flow **SignIn**, then select **Create**.
