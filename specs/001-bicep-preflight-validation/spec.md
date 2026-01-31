# Feature Specification: Bicep Template Preflight Validation on CI

**Feature Branch**: `001-bicep-preflight-validation`  
**Created**: 2026-01-31  
**Status**: Draft  
**Input**: User description: "Add preflight validation for Bicep template deployment on CI to replace arm-ttk with azure/arm-deploy@v1 action"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Validate Bicep Deployment Before Merge (Priority: P1)

As a DevOps engineer, when I submit changes to Azure Bicep templates in a pull request, the CI pipeline validates that the deployment will succeed in Azure before the code is merged, giving me confidence that the infrastructure changes won't fail during actual deployment.

**Why this priority**: This is the core value of the feature - catching deployment failures early in the development cycle before they reach production or staging environments. It prevents broken deployments from being merged.

**Independent Test**: Can be fully tested by creating a pull request with Bicep template changes and observing the CI pipeline perform deployment validation and report success or failure status.

**Acceptance Scenarios**:

1. **Given** a pull request contains valid Bicep template changes, **When** the CI pipeline runs, **Then** the deployment validation passes and provides a success status check
2. **Given** a pull request contains Bicep templates with deployment errors (e.g., invalid resource configurations, quota limits, or API issues), **When** the CI pipeline runs, **Then** the validation fails with detailed error messages indicating what will fail during deployment
3. **Given** Bicep templates are modified in the main branch, **When** the push event triggers CI, **Then** the deployment validation runs and reports the validation status

---

### User Story 2 - Replace Syntax-Only Validation with Full Deployment Validation (Priority: P2)

As a DevOps engineer, I need the CI pipeline to perform comprehensive deployment validation beyond syntax and API version checks, so I can catch issues like resource quota limits, permission problems, or resource conflicts that would only surface during actual deployment.

**Why this priority**: This improves the quality of validation by moving from syntax-only checks to actual deployment validation, catching more types of issues. However, it's secondary to having the basic validation working.

**Independent Test**: Can be tested by creating Bicep templates that pass syntax validation but would fail deployment (e.g., requesting unavailable SKUs or exceeding quotas), and verifying the new validation catches these issues.

**Acceptance Scenarios**:

1. **Given** Bicep templates that are syntactically correct but reference unavailable Azure resources or SKUs, **When** the validation runs, **Then** it detects and reports the deployment-time issues
2. **Given** Bicep templates with valid syntax but insufficient permissions or quota violations, **When** the validation runs, **Then** it reports the specific deployment constraints that would cause failure

---

### User Story 3 - Provide Clear Feedback on Validation Results (Priority: P3)

As a developer reviewing validation results, I receive clear and actionable error messages when validation fails, including specific details about what will fail and why, so I can quickly fix the issues without trial-and-error.

**Why this priority**: While important for developer experience, this is tertiary to having working validation. Clear feedback enhances usability but the feature delivers value even with basic error reporting.

**Independent Test**: Can be tested by intentionally creating various types of errors in Bicep templates and verifying that the validation output provides specific, actionable error messages.

**Acceptance Scenarios**:

1. **Given** validation fails due to deployment issues, **When** reviewing the CI pipeline output, **Then** the error messages clearly identify which resources and configurations are problematic with specific Azure error codes and descriptions
2. **Given** multiple validation errors exist in a single pull request, **When** the validation completes, **Then** all errors are reported together rather than failing on the first error only

---

### Edge Cases

- What happens when the validation service is unavailable or times out?
- How does the system handle Bicep templates that depend on existing Azure resources that may or may not exist in the validation environment?
- What happens when Bicep templates reference secrets or parameters that shouldn't be exposed in the CI environment?
- How does the system handle very large or complex Bicep templates that may take a long time to validate?
- What happens when multiple pull requests are running validation simultaneously?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The CI pipeline MUST validate Bicep template deployments using Azure's deployment validation API before merging code changes
- **FR-002**: The validation MUST run automatically when pull requests modify files in the Bicep template directories (templates/azure/**, templates/iotedge-lorawan-starterkit/**)
- **FR-003**: The validation MUST run automatically when changes are pushed to the main branch that affect Bicep templates
- **FR-004**: The validation MUST report success or failure as a status check on pull requests, preventing merge if validation fails
- **FR-005**: The validation MUST detect deployment-time issues including but not limited to: invalid resource configurations, quota violations, permission issues, unavailable SKUs, and Azure API compatibility problems
- **FR-006**: The validation process MUST provide detailed error messages when validation fails, including specific resource names, error codes, and descriptions from Azure
- **FR-007**: The validation MUST support all Bicep templates in the repository, including main templates and module dependencies
- **FR-008**: The validation MUST handle template parameters and variables without requiring actual deployment of resources
- **FR-009**: The validation MUST complete within a reasonable timeframe (under 10 minutes for typical templates)
- **FR-010**: The validation approach MUST replace the existing arm-ttk validation while maintaining or improving validation quality
- **FR-011**: The validation MUST use appropriate Azure credentials and permissions to perform validation without exposing secrets in CI logs
- **FR-012**: The validation MUST handle both single template files and templates with module references (submodules)

### Assumptions

- **A-001**: Azure credentials with appropriate permissions for deployment validation are available in the CI environment as secrets
- **A-002**: The validation will use a "validate" mode that checks deployment feasibility without actually deploying resources
- **A-003**: Standard Azure rate limits and quotas for API calls are sufficient for CI validation needs
- **A-004**: The Bicep templates are designed to be environment-agnostic or can accept parameters for validation purposes
- **A-005**: The azure/arm-deploy@v1 GitHub Action or equivalent Azure CLI commands provide deployment validation capabilities
- **A-006**: The existing CI pipeline infrastructure (GitHub Actions runners) has sufficient permissions and configuration to interact with Azure APIs

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The validation detects 100% of deployment failures that would occur during actual Azure deployment (no false negatives for deployment-blocking issues)
- **SC-002**: The validation completes within 5 minutes for typical template changes in the repository
- **SC-003**: Developers receive actionable error messages that allow them to fix validation failures without needing to manually test deployment in Azure
- **SC-004**: Zero instances of Bicep template changes being merged that later fail during actual deployment due to issues that should have been caught by validation
- **SC-005**: The CI pipeline successfully validates at least 95% of pull requests without infrastructure failures or timeouts
- **SC-006**: Validation provides more detailed and actionable feedback compared to the previous arm-ttk approach (measured by reduction in follow-up questions or clarifications needed)
- **SC-007**: The validation process operates without exposing Azure credentials or sensitive configuration in CI logs
