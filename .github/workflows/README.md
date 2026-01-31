# GitHub Actions Workflows

This directory contains GitHub Actions workflows for automated CI/CD processes.

## Workflows Overview

### Code Quality & Testing
- **ci-tests.yml**: Runs automated tests for the application
- **codeql.yml**: Security scanning with CodeQL
- **ci-open-api-documentation.yml**: OpenAPI documentation validation
- **lint-documentation.yml**: Documentation linting

### Infrastructure as Code
- **bicep-lint.yml**: Validates Azure Bicep templates with deployment preflight validation
- **aws-cfn-lint.yml**: Validates AWS CloudFormation templates

### Deployment
- **deploy_staging.yml**: Deploys application to staging environment
- **aws_deploy_staging.yml**: Deploys to AWS staging environment

### Documentation
- **publish-documentation-new-version.yml**: Publishes new documentation versions
- **delete-documentation-version.yml**: Removes old documentation versions

### Other
- **issue.yml**: Automated issue management
- **awesome-ideas.yml**: Community contribution workflow
- **publish.yml**: Package publishing workflow

## GitHub Secrets Configuration

### Azure Bicep Validation (Required for bicep-lint.yml)

The Bicep validation workflow uses OpenID Connect (OIDC) with Workload Identity Federation for secure, secret-free authentication to Azure.

**Required Secrets:**

| Secret Name | Description | How to Obtain |
|-------------|-------------|---------------|
| `AZURE_CLIENT_ID` | Azure Application (Client) ID for the service principal | From Azure AD App Registration |
| `AZURE_TENANT_ID` | Azure Active Directory Tenant ID | `az account show --query tenantId -o tsv` |
| `AZURE_SUBSCRIPTION_ID` | Azure Subscription ID where validation will run | `az account show --query id -o tsv` |

**Setup Instructions:**

1. Follow the detailed setup guide in [`docs/azure-validation-setup.md`](../../docs/azure-validation-setup.md)
2. Create an Azure service principal with Workload Identity Federation configured
3. Assign minimal RBAC permissions (Reader + Deployment Validator)
4. Add the three secrets above to GitHub repository settings

**Why OIDC?**
- No passwords or client secrets stored in GitHub
- Short-lived tokens that expire automatically
- Enhanced security with Azure-recommended authentication
- Automatic credential rotation

**Security Note:** With OIDC authentication, you do **NOT** need `AZURE_CLIENT_SECRET`. The workflow uses federated credentials that GitHub exchanges for Azure access tokens.

### AWS Deployment (Required for AWS workflows)

| Secret Name | Description |
|-------------|-------------|
| `AWS_ACCESS_KEY_ID` | AWS access key for deployment |
| `AWS_SECRET_ACCESS_KEY` | AWS secret key for deployment |
| `AWS_REGION` | Target AWS region |

### Other Secrets

Additional secrets may be required for specific workflows. Check individual workflow files for their secret requirements.

## Local Testing

Some workflows can be triggered manually using `workflow_dispatch` for testing purposes. Check individual workflow files for this capability.

## Adding New Workflows

When creating new workflows:

1. Follow the naming convention: `<purpose>-<action>.yml`
2. Add clear comments explaining the workflow's purpose
3. Document required secrets in this README
4. Use appropriate triggers (push, pull_request, workflow_dispatch, etc.)
5. Set reasonable timeout limits
6. Include error handling and status reporting

## Troubleshooting

### Bicep Validation Failures

If the `bicep-lint.yml` workflow fails with authentication errors:

1. Verify secrets are configured: Settings → Secrets and variables → Actions
2. Check that `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, and `AZURE_SUBSCRIPTION_ID` are set
3. Ensure the Azure service principal has required permissions (Reader + Deployment Validator)
4. Verify federated credentials are configured for your repository
5. See detailed troubleshooting in [`docs/azure-validation-setup.md`](../../docs/azure-validation-setup.md)

### General Workflow Issues

- Check workflow run logs for specific error messages
- Verify all required secrets are configured
- Ensure branch protection rules aren't blocking workflow execution
- Check that workflow has required permissions in repository settings

## Contributing

When modifying workflows:

1. Test changes in a feature branch first
2. Use `workflow_dispatch` trigger for manual testing if possible
3. Verify all required secrets are documented
4. Update this README if adding new secrets or workflows
5. Consider security implications of any secret usage

## References

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Azure Bicep Validation Setup](../../docs/azure-validation-setup.md)
- [Workflow Syntax Reference](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions)
