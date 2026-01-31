# Azure Service Principal Setup for Bicep Template Validation

This document describes the Azure configuration required for Bicep template deployment validation in CI/CD pipelines.

## Overview

The Bicep template validation workflow uses Azure's deployment validation API to verify that templates will deploy successfully without actually creating resources. This requires authentication and appropriate permissions.

## Authentication Method: Workload Identity Federation (OIDC)

**Recommended**: Use OpenID Connect (OIDC) with Workload Identity Federation for secure, secret-free authentication between GitHub Actions and Azure.

### Benefits of OIDC Authentication

- **No credential storage**: No secrets or passwords stored in GitHub
- **Automatic rotation**: Credentials are short-lived tokens that expire automatically
- **Enhanced security**: Reduces risk of credential exposure
- **Azure recommended**: Current best practice for CI/CD authentication

### Prerequisites

1. Azure subscription with appropriate permissions
2. Azure AD tenant with permissions to create app registrations
3. GitHub repository with access to configure federated credentials

## Setup Steps

### 1. Create Azure Service Principal

```bash
# Login to Azure
az login

# Create a service principal for GitHub Actions
APP_NAME="github-bicep-validation"
SUBSCRIPTION_ID="your-subscription-id"

az ad app create --display-name "${APP_NAME}"
APP_ID=$(az ad app list --display-name "${APP_NAME}" --query "[0].appId" -o tsv)

# Create service principal
az ad sp create --id "${APP_ID}"
SP_OBJECT_ID=$(az ad sp list --display-name "${APP_NAME}" --query "[0].id" -o tsv)
```

### 2. Configure Workload Identity Federation

```bash
# Set up federated credential for GitHub Actions
GITHUB_ORG="your-github-org"
GITHUB_REPO="your-github-repo"
GITHUB_BRANCH="main"  # or your default branch

az ad app federated-credential create \
  --id "${APP_ID}" \
  --parameters '{
    "name": "github-federated-credential",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:'"${GITHUB_ORG}/${GITHUB_REPO}"':ref:refs/heads/'"${GITHUB_BRANCH}"'",
    "description": "GitHub Actions federated credential for Bicep validation",
    "audiences": ["api://AzureADTokenExchange"]
  }'

# Also create federated credential for pull requests
az ad app federated-credential create \
  --id "${APP_ID}" \
  --parameters '{
    "name": "github-pr-federated-credential",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:'"${GITHUB_ORG}/${GITHUB_REPO}"':pull_request",
    "description": "GitHub Actions federated credential for PR validation",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

### 3. Assign Minimal RBAC Permissions

The service principal needs **read-only** access plus deployment validation permissions:

```bash
# Assign Reader role at subscription level (or resource group level for stricter security)
az role assignment create \
  --assignee "${APP_ID}" \
  --role "Reader" \
  --scope "/subscriptions/${SUBSCRIPTION_ID}"

# Assign deployment validation permission
# This allows validation without actual deployment
az role assignment create \
  --assignee "${APP_ID}" \
  --role "Deployment Validator" \
  --scope "/subscriptions/${SUBSCRIPTION_ID}"
```

**Note**: If "Deployment Validator" role doesn't exist in your environment, you may need to create a custom role with the following permission:
- `Microsoft.Resources/deployments/validate/action`

#### Custom Role Definition (if needed)

```bash
# Create custom role for deployment validation only
az role definition create --role-definition '{
  "Name": "Deployment Validator",
  "Description": "Can validate ARM/Bicep deployments without deploying resources",
  "Actions": [
    "Microsoft.Resources/subscriptions/resourcegroups/read",
    "Microsoft.Resources/deployments/validate/action",
    "Microsoft.Resources/deployments/read"
  ],
  "AssignableScopes": ["/subscriptions/'"${SUBSCRIPTION_ID}"'"]
}'
```

### 4. Get Required Values

Extract these values for GitHub secrets configuration:

```bash
# Azure Tenant ID
TENANT_ID=$(az account show --query tenantId -o tsv)
echo "AZURE_TENANT_ID: ${TENANT_ID}"

# Azure Subscription ID
echo "AZURE_SUBSCRIPTION_ID: ${SUBSCRIPTION_ID}"

# Azure Client ID (Application/Service Principal ID)
echo "AZURE_CLIENT_ID: ${APP_ID}"
```

### 5. Configure GitHub Repository Secrets

Navigate to GitHub repository → Settings → Secrets and variables → Actions → New repository secret

Add the following secrets:

| Secret Name | Value | Description |
|------------|-------|-------------|
| `AZURE_CLIENT_ID` | Application (client) ID | From step 4 |
| `AZURE_TENANT_ID` | Directory (tenant) ID | From step 4 |
| `AZURE_SUBSCRIPTION_ID` | Subscription ID | From step 4 |

**Important**: With OIDC authentication, you do **NOT** need `AZURE_CLIENT_SECRET` or any password/credential secrets.

## Validation Resource Group (Optional)

For better isolation, you can create a dedicated resource group for validation:

```bash
VALIDATION_RG="rg-bicep-validation"
LOCATION="eastus"

# Create resource group
az group create --name "${VALIDATION_RG}" --location "${LOCATION}"

# Assign Reader role at resource group level
az role assignment create \
  --assignee "${APP_ID}" \
  --role "Reader" \
  --scope "/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${VALIDATION_RG}"

# Assign deployment validation permission at resource group level
az role assignment create \
  --assignee "${APP_ID}" \
  --role "Deployment Validator" \
  --scope "/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${VALIDATION_RG}"
```

## Verification

Test the configuration:

```bash
# Verify service principal exists
az ad sp show --id "${APP_ID}"

# Verify role assignments
az role assignment list --assignee "${APP_ID}" --all
```

## Security Best Practices

1. **Minimal Permissions**: Service principal has Reader + validate permissions only, cannot create/modify/delete resources
2. **Scope Limitation**: Assign permissions at resource group level instead of subscription level when possible
3. **Federated Credentials**: Use OIDC to avoid storing secrets in GitHub
4. **Audit Logs**: Monitor Azure Activity Logs for validation activity
5. **Regular Reviews**: Periodically review and rotate federated credentials

## Fallback: Service Principal with Secret

If OIDC is not available, you can use traditional service principal authentication:

```bash
# Create service principal with password
az ad sp create-for-rbac \
  --name "${APP_NAME}" \
  --role "Reader" \
  --scopes "/subscriptions/${SUBSCRIPTION_ID}" \
  --sdk-auth

# Output includes clientId, clientSecret, tenantId, subscriptionId
# Store clientSecret as AZURE_CLIENT_SECRET in GitHub secrets
```

**Warning**: This method stores a secret in GitHub and requires manual rotation. OIDC is strongly preferred.

## Troubleshooting

### Error: "Authentication failed"
- Verify `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, and `AZURE_SUBSCRIPTION_ID` are correct
- Ensure federated credentials are configured for both branch and pull_request subjects
- Check that the service principal exists: `az ad sp show --id <CLIENT_ID>`

### Error: "Authorization failed"
- Verify role assignments: `az role assignment list --assignee <CLIENT_ID> --all`
- Ensure "Reader" and "Deployment Validator" roles are assigned
- Check subscription ID matches the validation target

### Error: "The client does not have authorization to perform action"
- Service principal needs `Microsoft.Resources/deployments/validate/action` permission
- Create custom "Deployment Validator" role if it doesn't exist

## References

- [Azure Workload Identity Federation](https://learn.microsoft.com/en-us/azure/active-directory/workload-identities/workload-identity-federation)
- [GitHub Actions OIDC with Azure](https://docs.github.com/en/actions/deployment/security-hardening-your-deployments/configuring-openid-connect-in-azure)
- [Azure ARM Template Deployment Validation](https://learn.microsoft.com/en-us/azure/azure-resource-manager/templates/deployment-validation)
- [azure/login GitHub Action](https://github.com/Azure/login)

## Support

For issues or questions about Azure configuration:
- Open an issue in the repository
- Contact the DevOps team
- Review Azure documentation linked above
