# Bicep Template Test Resources

This directory contains test resources for Bicep template validation in CI.

## Files

### `azuredeploy.parameters.test.json`

**Purpose**: Parameter file template used for deployment validation in CI pipeline.

**Usage**: This file provides the minimum required parameters for validating Bicep templates using Azure's deployment validation API. The validation runs in `Validate` mode only - **no actual resources are deployed**.

**Important**: This is a **TEMPLATE FILE**. The values with `REPLACE_WITH_*` prefixes are placeholders that must be replaced with actual values before the validation can run successfully.

### Parameter Replacement Guide

Before the CI workflow can validate templates, the following placeholders must be replaced:

| Placeholder | Description | How to Obtain |
|-------------|-------------|---------------|
| `REPLACE_WITH_SECURE_PASSWORD_Min12Chars!` | PostgreSQL admin password | Generate a secure password (min 12 characters, mix of uppercase, lowercase, numbers, special chars) |
| `REPLACE_WITH_TENANT_ID` | Azure AD Tenant ID | `az account show --query tenantId -o tsv` |
| `REPLACE_WITH_CLIENT_ID` | Azure AD B2C Client ID | From Azure AD B2C Application Registration |
| `REPLACE_WITH_API_CLIENT_ID` | Azure AD B2C API Client ID | From Azure AD B2C API Application Registration |

### Security Notes

⚠️ **DO NOT commit actual credentials or production values to this file**

- This file is checked into source control and should only contain placeholder values
- For local testing, create a copy with actual values (e.g., `azuredeploy.parameters.local.json`) and add it to `.gitignore`
- For CI/CD, consider using Azure Key Vault references or GitHub Secrets for sensitive parameters
- The placeholder password is intentionally obvious to prevent accidental usage

### Alternative: Using Key Vault References

For production deployments, replace sensitive parameters with Key Vault references:

```json
{
  "pgsqlAdminPassword": {
    "reference": {
      "keyVault": {
        "id": "/subscriptions/{subscription-id}/resourceGroups/{rg-name}/providers/Microsoft.KeyVault/vaults/{vault-name}"
      },
      "secretName": "postgresql-admin-password"
    }
  }
}
```

### Example: Creating a Local Parameter File

```bash
# Copy template to local file
cp azuredeploy.parameters.test.json azuredeploy.parameters.local.json

# Edit local file with actual values
# Then add to .gitignore
echo "templates/azure/tests/*.local.json" >> .gitignore
```

### Validation Mode Note

The CI pipeline uses Azure's deployment validation API with `deploymentMode: Validate`. This mode:
- ✅ Checks if the deployment would succeed
- ✅ Validates resource configurations against Azure APIs
- ✅ Detects quota limits, SKU availability, permission issues
- ❌ Does NOT deploy any actual resources
- ❌ Does NOT incur Azure costs

Therefore, placeholder values are acceptable for validation as long as they pass basic format validation (e.g., GUIDs for client IDs, URLs for endpoints).

## Future Test Cases

This directory will be expanded with additional test cases in future phases:

### Phase 4: Deployment-Time Error Tests
- `invalid-sku.bicep` - Test template with unavailable Azure resource SKU
- `quota-violation.bicep` - Test template exceeding typical quotas
- `permission-issue.bicep` - Test template requiring elevated permissions

### Phase 6: Example Parameter Files
- `examples/basic-deployment.json` - Minimal working configuration
- `examples/with-lorawan.json` - LoRaWAN enabled configuration
- `examples/without-lorawan.json` - IoT Hub only configuration

## References

- Azure Deployment Validation: https://learn.microsoft.com/en-us/azure/azure-resource-manager/templates/deployment-validation
- Parameter Files: https://learn.microsoft.com/en-us/azure/azure-resource-manager/templates/parameter-files
- Key Vault References: https://learn.microsoft.com/en-us/azure/azure-resource-manager/templates/key-vault-parameter

## Support

For questions about parameter configuration:
- Check the main Bicep templates for parameter descriptions
- Review the Azure validation setup guide: `docs/azure-validation-setup.md`
- See the workflow documentation: `.github/workflows/README.md`
