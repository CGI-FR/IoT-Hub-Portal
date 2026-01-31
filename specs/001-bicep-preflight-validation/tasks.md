---
description: "Task list for Bicep Template Preflight Validation on CI feature"
---

# Tasks: Bicep Template Preflight Validation on CI

**Feature Branch**: `001-bicep-preflight-validation`  
**Input**: Design documents from `/specs/001-bicep-preflight-validation/`  
**Prerequisites**: spec.md (user stories with priorities)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- GitHub Actions workflows: `.github/workflows/`
- Bicep templates: `templates/azure/**` and `templates/iotedge-lorawan-starterkit/**`
- Test templates: `templates/azure/tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare Azure environment and credentials for validation

- [X] T001 Document Azure service principal requirements in docs/azure-validation-setup.md (OIDC-based authentication with Reader role + deployment validation permissions)
- [X] T002 Create GitHub repository secrets documentation in .github/workflows/README.md (AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID for OIDC)
- [X] T003 [P] Create test template directory structure at templates/azure/tests/ for validation test cases

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core workflow infrastructure that MUST be complete before implementing user stories

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Create workflow helper script at .github/scripts/find-bicep-templates.sh to discover all Bicep templates in repository
- [X] T005 Create workflow helper script at .github/scripts/parse-validation-errors.sh to extract and format Azure validation error messages
- [X] T006 [P] Create test parameter file template at templates/azure/tests/azuredeploy.parameters.test.json with minimal valid parameters for validation

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Validate Bicep Deployment Before Merge (Priority: P1) 🎯 MVP

**Goal**: Enable CI pipeline to validate Bicep template deployments using Azure's deployment validation API, catching deployment failures before code is merged.

**Independent Test**: Create a pull request with valid Bicep changes and observe the CI pipeline perform deployment validation with success status. Then create a PR with intentionally invalid Bicep (e.g., invalid SKU) and verify validation fails with error details.

### Implementation for User Story 1

- [X] T007 [US1] Add Azure OIDC authentication step to .github/workflows/bicep-lint.yml using azure/login@v2 action with federated credentials
- [X] T008 [US1] Add Azure CLI setup verification step in .github/workflows/bicep-lint.yml to ensure az cli is available and authenticated
- [X] T009 [US1] Update main validation job in .github/workflows/bicep-lint.yml to use azure/arm-deploy@v2 action for templates/azure/azuredeploy.bicep with deploymentMode: Validate
- [X] T010 [US1] Configure validation parameters in .github/workflows/bicep-lint.yml to pass test parameters file from templates/azure/tests/azuredeploy.parameters.test.json
- [X] T011 [US1] Add validation timeout configuration in .github/workflows/bicep-lint.yml with timeout-minutes: 10 for validation job
- [X] T012 [US1] Configure workflow to validate all main Bicep templates (portal_with_lorawan_and_starter_kit.bicep, portal_with_lorawan.bicep, portal_without_lorawan.bicep, azuredeploy.bicep) in templates/azure/
- [X] T013 [US1] Add status check reporting in .github/workflows/bicep-lint.yml to fail the workflow if any validation fails
- [ ] T014 [US1] Test validation with valid Bicep template change in a pull request to verify success path

**Checkpoint**: At this point, User Story 1 should be fully functional - basic deployment validation working for all main templates

---

## Phase 4: User Story 2 - Replace Syntax-Only Validation with Full Deployment Validation (Priority: P2)

**Goal**: Enhance validation to catch deployment-time issues beyond syntax, including resource quota limits, permission problems, unavailable SKUs, and resource conflicts.

**Independent Test**: Create test templates that pass syntax validation (`az bicep build`) but would fail deployment (e.g., requesting unavailable VM SKUs, exceeding quota limits, or referencing non-existent resources), and verify the new validation catches these issues while the old validation did not.

### Implementation for User Story 2

- [ ] T015 [P] [US2] Create test template at templates/azure/tests/invalid-sku.bicep with unavailable Azure resource SKU to verify deployment validation catches it
- [ ] T016 [P] [US2] Create test template at templates/azure/tests/quota-violation.bicep with resource configurations that would exceed typical quotas
- [ ] T017 [P] [US2] Create test template at templates/azure/tests/permission-issue.bicep requiring elevated permissions to verify validation detects permission problems
- [ ] T018 [US2] Add matrix strategy to .github/workflows/bicep-lint.yml to validate all module templates (app_service.bicep, iothub.bicep, storage.bicep, database.bicep, dps.bicep, app_insights.bicep, etc.) from templates/azure/
- [ ] T019 [US2] Configure validation scope in .github/workflows/bicep-lint.yml to use appropriate resource group scope for validation (create validation resource group if needed)
- [ ] T020 [US2] Add step in .github/workflows/bicep-lint.yml to validate module templates with their dependencies resolved
- [ ] T021 [US2] Remove old arm-ttk validation step from .github/workflows/bicep-lint.yml (line 39-40: "Lint Bicep templates and Generate ARM template")
- [ ] T022 [US2] Test with templates from templates/azure/tests/ to verify deployment validation catches quota, SKU, and permission issues that syntax validation misses

**Checkpoint**: At this point, User Stories 1 AND 2 should both work - comprehensive deployment validation replacing syntax-only validation

---

## Phase 5: User Story 3 - Provide Clear Feedback on Validation Results (Priority: P3)

**Goal**: Provide clear, actionable error messages when validation fails, including specific details about what will fail and why, enabling developers to quickly fix issues without trial-and-error.

**Independent Test**: Intentionally create various types of errors in Bicep templates (invalid resource names, wrong SKUs, missing dependencies, quota issues) and verify that the validation output provides specific, actionable error messages with Azure error codes and resource details.

### Implementation for User Story 3

- [ ] T023 [P] [US3] Create error parsing script at .github/scripts/format-validation-output.sh to extract Azure error codes, resource names, and descriptions from validation failures
- [ ] T024 [US3] Update .github/workflows/bicep-lint.yml to capture and parse validation error output using format-validation-output.sh script
- [ ] T025 [US3] Add step in .github/workflows/bicep-lint.yml to format and display validation errors in GitHub Actions annotations for easy PR review
- [ ] T026 [US3] Configure workflow to continue validation for all templates even if one fails (continue-on-error strategy) to report all errors together
- [ ] T027 [US3] Add summary step in .github/workflows/bicep-lint.yml to create job summary with all validation results (passed/failed templates with error details)
- [ ] T028 [P] [US3] Create test template at templates/azure/tests/multiple-errors.bicep with multiple intentional errors to verify all errors are reported together
- [ ] T029 [US3] Add validation result formatting to display error categories (quota, permission, SKU, API compatibility) in .github/workflows/bicep-lint.yml
- [ ] T030 [US3] Test error reporting with templates containing various error types to verify actionable error messages with specific resource names and Azure error codes

**Checkpoint**: All user stories should now be independently functional - complete deployment validation with clear, actionable feedback

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect the overall feature quality and maintainability

- [ ] T031 [P] Create documentation at docs/bicep-validation-guide.md explaining how the validation works, what it catches, and how to fix common errors
- [ ] T032 [P] Update repository README.md to document the Bicep validation requirement for pull requests
- [ ] T033 [P] Create troubleshooting guide at docs/bicep-validation-troubleshooting.md with solutions for common validation issues
- [ ] T034 Add workflow documentation comments in .github/workflows/bicep-lint.yml explaining each validation step and its purpose
- [ ] T035 [P] Create example parameter files at templates/azure/tests/examples/ showing valid parameter configurations for each template
- [ ] T036 Test edge cases: validation timeout, Azure API unavailability, concurrent PR validations, templates with external dependencies
- [ ] T037 Optimize validation performance: explore parallel validation of independent templates, caching of validation results
- [ ] T038 Security review: verify no secrets are exposed in logs, credentials are properly scoped with minimal permissions
- [ ] T039 Add workflow badge to README.md showing Bicep validation status
- [ ] T040 Final end-to-end test: Create pull request with valid and invalid templates, verify complete validation cycle and error reporting

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-5)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
  - Establishes basic deployment validation with azure/arm-deploy@v2
  - Validates main templates only
  
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Extends US1 but is independently testable
  - Adds comprehensive validation for all templates including modules
  - Creates test cases for deployment-time issues
  - Removes old arm-ttk validation
  
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Enhances US1 output but is independently testable
  - Adds error parsing and formatting
  - Improves developer experience with actionable feedback
  - Does not change validation logic from US1/US2

### Within Each User Story

- Azure authentication must be configured before validation steps (T007 before T009)
- Test templates can be created in parallel (all T015-T017 marked [P])
- Documentation tasks can run in parallel (all marked [P])
- Validation configuration tasks must be sequential (T009 → T010 → T011)
- Error parsing script (T023) must exist before using it in workflow (T024)

### Parallel Opportunities

- **Setup Phase**: T001 and T003 can run in parallel with T002
- **Foundational Phase**: T005 and T006 can run in parallel after T004 completes
- **User Story 2**: All test template creation (T015, T016, T017) can run in parallel
- **User Story 3**: Error parsing script (T023) and test template (T028) can run in parallel
- **Polish Phase**: All documentation tasks (T031, T032, T033, T035) can run in parallel

---

## Parallel Example: User Story 2

```bash
# Launch all test template creation tasks for User Story 2 together:
Task T015: "Create test template at templates/azure/tests/invalid-sku.bicep"
Task T016: "Create test template at templates/azure/tests/quota-violation.bicep"
Task T017: "Create test template at templates/azure/tests/permission-issue.bicep"

# These can all be worked on simultaneously as they create different files
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (Azure credentials documentation, test directory)
2. Complete Phase 2: Foundational (Helper scripts, parameter files) - CRITICAL
3. Complete Phase 3: User Story 1 (Basic deployment validation working)
4. **STOP and VALIDATE**: Test User Story 1 independently
   - Create PR with valid Bicep changes → validation should pass
   - Create PR with invalid Bicep → validation should fail
5. Merge and deploy if ready - **MVP delivered!**

### Incremental Delivery

1. **Phase 1+2**: Foundation → Validation infrastructure ready
2. **Phase 3 (US1)**: Basic deployment validation → Test independently → **MVP ACHIEVED**
   - Replaces `az bicep build` with `azure/arm-deploy@v2` validation
   - Validates main templates against Azure
   - Catches deployment failures before merge
3. **Phase 4 (US2)**: Comprehensive validation → Test independently → Enhanced protection
   - Validates all module templates
   - Test cases prove deployment validation catches more than syntax
   - Fully removes old arm-ttk approach
4. **Phase 5 (US3)**: Clear error feedback → Test independently → Better developer experience
   - Developers get actionable error messages
   - Multiple errors reported together
   - Categorized error types for quick fixes
5. **Phase 6**: Polish and documentation → Production-ready feature

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Basic validation)
   - Developer B: User Story 2 (Comprehensive validation + test cases)
   - Developer C: User Story 3 (Error parsing and formatting)
3. Stories complete and integrate independently
4. Team completes Polish phase together

---

## Validation Checklist

Before considering this feature complete:

- [ ] Valid Bicep changes pass validation in CI (success path)
- [ ] Invalid deployment configurations fail validation with error messages (failure path)
- [ ] All main Bicep templates are validated (azuredeploy.bicep, portal_*.bicep)
- [ ] All module templates are validated (app_service, iothub, storage, etc.)
- [ ] Validation completes within 10 minutes for typical templates
- [ ] Azure credentials are securely configured using OIDC (no secrets in logs)
- [ ] Error messages include specific resource names, error codes, and descriptions
- [ ] Multiple errors in a single PR are all reported (not just the first error)
- [ ] Validation runs on pull_request and push events for template changes
- [ ] Workflow status checks prevent merge of PRs with validation failures
- [ ] Test cases demonstrate validation catches deployment issues (quota, SKU, permissions)
- [ ] Documentation exists for setup, usage, and troubleshooting
- [ ] Old arm-ttk validation step is removed from workflow

---

## Success Metrics

After implementation, measure:

- **SC-001**: Validation catch rate - 100% of deployment failures caught before merge (no false negatives)
- **SC-002**: Validation performance - Completes within 5 minutes for typical templates
- **SC-003**: Developer feedback quality - Error messages are actionable without requiring Azure portal debugging
- **SC-004**: Zero deployment failures - No merged Bicep changes fail during actual deployment
- **SC-005**: Reliability - 95%+ of validation runs succeed without infrastructure failures
- **SC-006**: Security - No Azure credentials or secrets exposed in CI logs
- **SC-007**: Coverage - All templates validated (main + modules + submodules)

---

## Notes

- **[P] tasks**: Different files, no dependencies - can run in parallel
- **[Story] label**: Maps task to specific user story (US1, US2, US3) for traceability
- **Each user story is independently testable**: Can verify each story delivers value on its own
- **Commit after each task**: Version control for easy rollback if needed
- **Stop at checkpoints**: Validate story independently before proceeding
- **Azure OIDC preferred**: Use Workload Identity Federation, fallback to Service Principal if needed
- **Minimal permissions**: Reader role + Microsoft.Resources/deployments/validate/action
- **No actual deployment**: Validation mode only, no resources created
- **Support submodules**: Workflow includes `submodules: recursive` in checkout step (already present)
