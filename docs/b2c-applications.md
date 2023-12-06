# Azure AD B2C Tenant with applications

This solution uses Azure AD B2C to authenticate the portal.
In this page you will configure the B2C tenant and two applications (API and Web UI).

By the end, you should have recorded the following information:

    * OpenID authority: `<your-openid-authority>`
    * OpenID metadata URL: `<your-openid-provider-metadata-url>`
    * Client ID: `<your-client-id>`
    * API Client ID: `<your-client-id>`

## Step by Step instructions

1. Create an Azure AD B2C Tenant (see: [https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-tenant#create-an-azure-ad-b2c-tenant](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-tenant#create-an-azure-ad-b2c-tenant))
    * Record the **tenant ID** and the **tenant name**.

1. After creating your Azure AD B2C Tenant and registering your applications, you need to set up OpenID Connect to secure your applications. Here’s how to find your OpenID authority and OpenID metadata URL:

    1. **Determine your OpenID Authority**:
    - Your OpenID Authority is the issuer URL of your Azure AD B2C Tenant. It typically follows the format: `https://<tenant-name>.b2clogin.com/<tenant-name>.onmicrosoft.com/v2.0/`.
    - Replace `<tenant-name>` with your actual tenant name.

    2. **Find your OpenID Metadata URL**:
    - The OpenID Metadata URL for Azure AD B2C tenants is usually in the format: `https://<tenant-name>.b2clogin.com/<tenant-name>.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=<policy-name>`.
    - Replace `<tenant-name>` and `<policy-name>` with your actual tenant name and the policy name you are using (like B2C_1_SignUpSignIn).
    3. Make sure to record the OpenID authority and OpenID metadata URL for future configuration steps.
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
        * If you're prompted to select an account, select your currently signed-in administrator account, or sign in **with an account in your Azure AD B2C tenant that's been assigned at least the Cloud application administrator** role.
        * Under **Manage**, select **Expose an API**.
        * Next to **Application ID URI**, select the **Set** link.
        * Under **Scopes defined by this API**, select **Add a scope**.
        * Enter the following values to create a scope that defines read access to the API, then select **Add scope**:
            * **Scope name**: ``API.Access`` (this is the name of the scope that will be used in the template)
            * **Admin consent display name**: ``Access to the Portal API``
            * **Admin consent description**: ``Allows the application to get access to the Portal API``

    2. Create the **IoT Hub Portal Client** Application:
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
        * If you're prompted to select an account, select your currently signed-in administrator account, or sign in **with an account in your Azure AD B2C tenant that's been assigned at least the Cloud application administrator** role.
        * Select **Yes**.
        * Select **Refresh**, and then verify that "Granted for ..." appears under **Status** for both scopes.

1. Configure the required User flow:
    1. Select **User flows**, and then select **New user flow**.
    2. Under **Select a user flow type**, select **Sign in**, then select **Create**.
    3. Enter a name for the flow **SignIn**, then select **Create**.
