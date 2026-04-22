# Bicep Template Preflight Validation - MVP Implementation Notes

**Date**: 2026-01-31  
**Status**: MVP Complete - Phases 1-3 Implemented  
**Commit**: 987d408

## Implementation Summary

Successfully implemented the MVP scope of the Bicep Template Preflight Validation feature, covering Phases 1-3 (Setup, Foundational, and User Story 1). The implementation enables deployment validation of Bicep templates in CI using Azure's deployment validation API without actually deploying resources.

## What Was Implemented

### Phase 1: Setup Infrastructure (3 tasks)
- ✅ **Azure Service Principal Documentation**: Comprehensive guide for setting up OIDC-based authentication with step-by-step instructions
- ✅ **GitHub Workflows README**: Complete documentation of required secrets and workflow configuration
- ✅ **Test Template Directory**: Created `templates/azure/tests/` directory structure for validation test cases

### Phase 2: Foundational Prerequisites (3 tasks)
- ✅ **find-bicep-templates.sh**: Helper script to discover all Bicep templates in the repository
- ✅ **parse-validation-errors.sh**: Helper script to parse and format Azure validation error messages
- ✅ **Test Parameter File**: Created `azuredeploy.parameters.test.json` with minimal valid configuration for validation

### Phase 3: User Story 1 - Basic Deployment Validation (7 tasks)
- ✅ **Azure OIDC Authentication**: Integrated `azure/login@v2` with Workload Identity Federation
- ✅ **Azure CLI Verification**: Added authentication verification step
- ✅ **Deployment Validation**: Configured `azure/arm-deploy@v2` with Validate mode
- ✅ **Parameter Configuration**: Pass test parameters from `azuredeploy.parameters.test.json`
- ✅ **Timeout Configuration**: Set 10-minute timeout for validation job
- ✅ **Multi-Template Validation**: Validates all 4 main Bicep templates
- ✅ **Status Check Reporting**: Fail workflow if any validation fails
- ⏳ **Testing**: Requires Azure credentials to be configured (T014)

## Key Features

### 🔐 Security
- **OIDC Authentication**: No client secrets stored in GitHub
- **Minimal Permissions**: Documented Reader + Deployment Validator roles
- **No Secrets in Logs**: Secure credential handling
- **Federated Credentials**: Azure-recommended authentication method

### ✅ Validation Capabilities
- **Deployment Mode**: Uses Azure ARM deployment validation API
- **No Resource Creation**: Validation-only mode (no actual deployment)
- **Multi-Template Support**: Validates 4 main Bicep templates
- **Resource Group Isolation**: Creates temporary RG per run, auto-cleanup

### ⚡ Performance & Reliability
- **10-Minute Timeout**: Prevents hung validations
- **Status Reporting**: Clear pass/fail indicators for each template
- **Workflow Failure**: Prevents merge if validation fails
- **Automatic Cleanup**: Deletes validation resource groups with `--no-wait`

## Validated Templates

1. `templates/azure/azuredeploy.bicep` - Main deployment template
2. `templates/azure/portal_with_lorawan_and_starter_kit.bicep` - Full LoRaWAN deployment
3. `templates/azure/portal_with_lorawan.bicep` - LoRaWAN without starter kit
4. `templates/azure/portal_without_lorawan.bicep` - IoT Portal without LoRaWAN

## Files Created/Modified

### Documentation (353 lines)
- `docs/azure-validation-setup.md` - Azure setup guide (234 lines)
- `.github/workflows/README.md` - Workflow documentation (119 lines)

### Scripts (245 lines)
- `.github/scripts/find-bicep-templates.sh` - Template discovery (83 lines)
- `.github/scripts/parse-validation-errors.sh` - Error parsing (162 lines)

### Configuration (219 lines)
- `.github/workflows/bicep-lint.yml` - Updated workflow (159 lines)
- `templates/azure/tests/azuredeploy.parameters.test.json` - Test parameters (60 lines)

### Tracking
- `specs/001-bicep-preflight-validation/tasks.md` - Updated task status (13 tasks completed)

**Total**: 793 insertions, 17 deletions across 7 files

## Requirements Satisfied

### Fully Implemented
- ✅ **FR-001**: Validate using Azure deployment validation API
- ✅ **FR-002**: Auto-validate on PR template changes
- ✅ **FR-003**: Auto-validate on main branch pushes
- ✅ **FR-004**: Report as status check, prevent merge on failure
- ✅ **FR-009**: Complete within 10 minutes
- ✅ **FR-011**: Use OIDC credentials without exposing secrets
- ✅ **FR-012**: Handle templates with module references (submodules: recursive)

### Partially Implemented (Requires Azure Setup)
- 🔄 **FR-005**: Detect deployment-time issues (ready, needs Azure testing)
- 🔄 **FR-006**: Provide detailed error messages (error parsing script ready)
- 🔄 **FR-007**: Support all templates (main templates done, modules in Phase 4)
- 🔄 **FR-008**: Handle parameters without deployment (configured, needs testing)
- 🔄 **FR-010**: Replace arm-ttk validation (old step still present, removal in Phase 4)

## What's NOT Yet Implemented

### Phase 4: User Story 2 - Comprehensive Validation (8 tasks)
- Module template validation (app_service.bicep, iothub.bicep, storage.bicep, etc.)
- Test templates for quota/SKU/permission errors
- Matrix strategy for parallel module validation
- Removal of old `az bicep build` validation step

### Phase 5: User Story 3 - Enhanced Error Feedback (8 tasks)
- Integration of `parse-validation-errors.sh` into workflow
- GitHub Actions annotations for PR review
- Error categorization (quota, permission, SKU, API compatibility)
- Multiple error reporting (continue-on-error strategy)
- Job summary with validation results

### Phase 6: Polish & Documentation (10 tasks)
- Bicep validation guide (usage, common errors)
- Troubleshooting guide
- Example parameter files
- Edge case testing (timeouts, API unavailability, concurrent PRs)
- Performance optimization
- Security review
- Workflow badge
- End-to-end testing

## Next Steps

### Immediate: Complete MVP Testing (T014)

**Prerequisites for testing**:
1. Set up Azure Service Principal with OIDC (follow `docs/azure-validation-setup.md`)
2. Configure GitHub repository secrets:
   - `AZURE_CLIENT_ID`
   - `AZURE_TENANT_ID`
   - `AZURE_SUBSCRIPTION_ID`
3. Assign RBAC permissions to service principal:
   - Reader role at subscription or RG level
   - Deployment Validator role (or custom role with `Microsoft.Resources/deployments/validate/action`)
4. Create a test PR modifying a Bicep template
5. Verify workflow runs and reports validation status

**Expected outcome**: Workflow runs successfully, validates templates, reports pass/fail status

### Short Term: Phase 4 (User Story 2)
- Expand validation to all module templates
- Create test cases proving deployment validation catches issues syntax checks miss
- Remove old `az bicep build` step from workflow
- **Deliverable**: Comprehensive validation coverage for all templates

### Medium Term: Phase 5 (User Story 3)
- Integrate error parsing for actionable feedback
- Add GitHub Actions annotations for inline PR comments
- Improve developer experience with clear error categorization
- **Deliverable**: Developers can quickly identify and fix validation failures

### Long Term: Phase 6 (Polish)
- Complete documentation suite
- Edge case testing
- Performance optimization
- Security audit
- **Deliverable**: Production-ready, well-documented feature

## Testing Strategy

### Manual Testing Required
1. **Success Path**: Create PR with valid Bicep changes, verify validation passes
2. **Failure Path**: Create PR with invalid Bicep (e.g., wrong SKU), verify validation fails with error message
3. **Timeout**: Test with complex template approaching 10-minute limit
4. **Concurrent PRs**: Multiple PRs running validation simultaneously
5. **Azure API Issues**: Simulate Azure service degradation/unavailability

### Automated Testing (Future)
- Unit tests for helper scripts (`find-bicep-templates.sh`, `parse-validation-errors.sh`)
- Integration tests with mock Azure responses
- End-to-end tests in staging environment

## Known Limitations

1. **Requires Azure Credentials**: Cannot test validation without Azure subscription and configured secrets
2. **Module Templates Not Yet Validated**: Only main 4 templates validated in MVP
3. **Old Validation Still Present**: `az bicep build` step not yet removed (Phase 4)
4. **Basic Error Reporting**: Error parsing scripts created but not yet integrated (Phase 5)
5. **No Test Cases for Errors**: Test templates for quota/SKU/permission issues not yet created (Phase 4)

## Success Metrics to Validate

Once Azure is configured and testing is complete:

- **SC-001**: 100% deployment failure detection rate (no false negatives)
- **SC-002**: Validation completes within 5 minutes for typical templates
- **SC-003**: Error messages are actionable without Azure portal debugging
- **SC-004**: Zero merged Bicep changes fail during actual deployment
- **SC-005**: 95%+ validation runs succeed without infrastructure failures
- **SC-006**: No Azure credentials exposed in CI logs
- **SC-007**: All templates validated (main + modules + submodules) - *partial in MVP*

## Technical Decisions

### OIDC vs Service Principal with Secret
**Decision**: Use OIDC (Workload Identity Federation)  
**Rationale**:
- No secrets stored in GitHub (better security)
- Azure recommended best practice
- Automatic credential rotation
- No manual secret management
- Future-proof authentication method

### Resource Group Scope vs Subscription Scope
**Decision**: Use resource group scope with temporary RG per run  
**Rationale**:
- Templates use `resourceGroup()` function
- Better isolation between validation runs
- Easier cleanup with `az group delete`
- Reduces subscription-level permissions needed
- Unique RG per run ID prevents conflicts

### azure/arm-deploy@v2 vs Azure CLI
**Decision**: Use `azure/arm-deploy@v2` GitHub Action  
**Rationale**:
- Higher-level abstraction with better error handling
- Built-in support for Validate deployment mode
- Consistent with GitHub Actions ecosystem
- Easier to configure than raw CLI commands
- Automatic handling of deployment artifacts

### Validate Mode vs Actual Deployment
**Decision**: Use Validate mode only (no resource creation)  
**Rationale**:
- No Azure costs for validation runs
- Faster validation (no resource provisioning)
- No cleanup of deployed resources needed
- Meets requirement of checking deployment feasibility
- Reduces Azure API rate limit impact

## Risks & Mitigations

### Risk: Azure API Rate Limits
**Mitigation**: Validation uses read-only operations, validate action is lightweight, 10-minute timeout prevents resource exhaustion

### Risk: False Positives (Valid Template Reported as Invalid)
**Mitigation**: Use realistic test parameters, validate against actual Azure subscription, comprehensive testing in Phase 6

### Risk: False Negatives (Invalid Template Reported as Valid)
**Mitigation**: Phase 4 includes test cases with intentional errors to verify detection capability

### Risk: Validation Timeout
**Mitigation**: 10-minute timeout configured, Phase 6 includes performance optimization, consider parallel validation for large template sets

### Risk: Concurrent PR Conflicts
**Mitigation**: Unique resource group per run ID, isolated validation environments, no shared state between runs

## Lessons Learned

1. **Start with MVP**: Phases 1-3 provide immediate value without implementing all user stories
2. **OIDC Setup is Critical**: Comprehensive documentation essential for adoption
3. **Resource Group Isolation**: Prevents conflicts and simplifies cleanup
4. **Helper Scripts Early**: Created error parsing scripts in Phase 2, ready for Phase 5 integration
5. **Test Parameters Matter**: Realistic test parameters essential for meaningful validation

## References

- Feature Spec: `specs/001-bicep-preflight-validation/spec.md`
- Task Breakdown: `specs/001-bicep-preflight-validation/tasks.md`
- Azure Setup Guide: `docs/azure-validation-setup.md`
- Workflow Documentation: `.github/workflows/README.md`
- Workflow Implementation: `.github/workflows/bicep-lint.yml`

## Change Log

### 2026-01-31 - MVP Implementation (Phases 1-3)
- Created Azure service principal setup documentation
- Created GitHub workflows README with secrets configuration
- Created helper scripts for template discovery and error parsing
- Created test parameter file for validation
- Updated `bicep-lint.yml` workflow with OIDC authentication and deployment validation
- Configured validation for 4 main Bicep templates
- Added 10-minute timeout and status check reporting
- Committed changes (commit 987d408)

---

**Status**: MVP Complete - Ready for Azure Configuration and Testing  
**Next Action**: Configure Azure credentials and test workflow (T014)  
**Blocked By**: Azure subscription access and service principal setup
