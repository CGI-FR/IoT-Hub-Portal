# Evaluation Report: Bicep Template Preflight Validation on CI

**Evaluated**: 2026-02-03
**Spec Version**: 2026-01-31 (Draft)
**Evaluator**: Excavator Evaluator Agent

---

## Summary

| Metric | Score | Grade | Weight |
|--------|-------|-------|--------|
| Correctness | 95/100 | A | 30% |
| Completeness | 90/100 | A | 30% |
| Technical Quality | 88/100 | B | 20% |
| Coverage | 85/100 | B | 20% |
| **Overall** | **90/100** | **A** | **100%** |

**Grade Scale**: A (90-100), B (70-89), C (50-69), D (30-49), F (0-29)

---

## Correctness Analysis

### ✅ Accurate Specifications

| Spec Element | Code Location | Verification |
|--------------|---------------|--------------|
| FR-001 - Validate Bicep using Azure deployment validation API | `bicep-lint.yml#L74-82` | Uses `azure/arm-deploy@v2` with `deploymentMode: Validate` |
| FR-002 - Run on PR with templates/azure/** paths | `bicep-lint.yml#L4-9` | Correct path filters configured |
| FR-003 - Run on push to main branch | `bicep-lint.yml#L10-14` | Push trigger with same path filters |
| FR-004 - Report success/failure as status check | `bicep-lint.yml#L139-158` | Validation status check step with exit 1 on failure |
| FR-007 - Support all Bicep templates | `bicep-lint.yml#L74-123` | Validates 4 main templates |
| FR-008 - Handle template parameters | `bicep-lint.yml#L81` | Uses test parameters file |
| FR-009 - Complete within 10 minutes | `bicep-lint.yml#L19` | `timeout-minutes: 10` configured |
| FR-010 - Replace arm-ttk | Workflow file name | Workflow named "Validate Bicep templates" |
| FR-011 - Use Azure credentials securely | `bicep-lint.yml#L45-51` | OIDC authentication via secrets |
| FR-012 - Handle module references | `bicep-lint.yml#L40` | `submodules: recursive` checkout |
| A-001 - Azure credentials available | `bicep-lint.yml#L47-49` | Uses AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID |
| A-002 - Validate mode | `bicep-lint.yml#L82` | `deploymentMode: Validate` |
| User Story 1 - Validate before merge | Workflow configuration | Runs on pull_request event |
| User Story 3 - Clear feedback | `bicep-lint.yml#L139-158` | Status output with icons |

### ⚠️ Inaccuracies Found

| Spec Element | Issue | Code Reality | Severity |
|--------------|-------|--------------|----------|
| FR-005 - Detect quota violations | Not fully verifiable | Validate mode may not catch all quota issues | Low |
| FR-006 - Detailed error messages | Partial | Azure API errors shown but no custom formatting | Low |
| A-005 - Uses arm-deploy@v1 | Version mismatch | Actually uses `azure/arm-deploy@v2` (better) | Low (improvement) |

### Correctness Score Breakdown

- User Stories: 3/3 correct
- Functional Requirements: 11/12 correct (1 minor limitation)
- Assumptions: 6/6 correct
- Triggers/Paths: 4/4 correct

---

## Completeness Analysis

### ✅ Well-Documented Areas

| Aspect | Coverage | Notes |
|--------|----------|-------|
| User Stories | Complete | 3 prioritized stories with acceptance criteria |
| Functional Requirements | Complete | 12 detailed requirements |
| Assumptions | Complete | 6 clear assumptions documented |
| Success Criteria | Complete | 7 measurable outcomes |
| Edge Cases | Documented | 5 edge cases identified |

### 🔴 Missing Documentation

| Missing Aspect | Code Location | Impact |
|----------------|---------------|--------|
| Resource group creation/cleanup | `bicep-lint.yml#L63-71` | Low - Not mentioned in spec but implemented |
| LoRaWAN starter kit submodule paths | `bicep-lint.yml#L8` | Low - Only templates/azure/ explicitly mentioned |
| Azure CLI version verification | `bicep-lint.yml#L54-61` | Low - Verification step not in spec |
| PR labeling step | `bicep-lint.yml#L26-36` | Low - Adds arm-templates label, not in spec |
| Specific templates validated | `bicep-lint.yml#L74-123` | Medium - Spec says "all" but 4 specific ones validated |

### Completeness Score Breakdown

- Workflow Steps: 8/10 documented
- Templates Covered: 4/4 (main templates)
- Security Configuration: 4/4 documented
- Error Handling: 3/4 documented
- Configuration: 5/5 documented

---

## Technical Quality Analysis

### Testability Assessment

| Criterion | Status | Notes |
|-----------|--------|-------|
| Acceptance criteria are specific | ✅ | Given/When/Then format consistently used |
| Scenarios are measurable | ✅ | Clear pass/fail outcomes |
| Test data requirements clear | ⚠️ | Test parameters file exists but not detailed in spec |
| Success/failure conditions unambiguous | ✅ | Clear success criteria defined |

### Traceability Assessment

| Criterion | Status | Notes |
|-----------|--------|-------|
| Requirements map to code | ✅ | All FRs map to workflow steps |
| No orphan requirements | ✅ | All requirements implemented |
| No orphan code | ⚠️ | PR labeling, resource group cleanup not in spec |
| Dependencies documented | ✅ | Azure secrets, OIDC mentioned |

### Consistency Assessment

| Criterion | Status | Notes |
|-----------|--------|-------|
| Terminology matches codebase | ✅ | "Bicep templates", "deployment validation" consistent |
| File paths match implementation | ✅ | templates/azure/** matches |
| Action versions accurate | ⚠️ | Spec says v1, code uses v2 |

### Currency Assessment

| Criterion | Status | Notes |
|-----------|--------|-------|
| Reflects latest code changes | ✅ | Spec dated 2026-01-31, workflow appears current |
| No deprecated references | ⚠️ | Spec references arm-ttk (correctly as being replaced) |
| Version info accurate | ⚠️ | Action version outdated in spec |

### Technical Quality Score Breakdown

- Testability: 7/8 checks passed
- Traceability: 7/8 checks passed
- Consistency: 5/6 checks passed
- Currency: 5/6 checks passed

---

## Coverage Analysis

### Security Coverage

| Aspect | Documented | Code Location | Gap |
|--------|------------|---------------|-----|
| Authentication | ✅ | `bicep-lint.yml#L45-51` | OIDC with workload identity |
| Credential Handling | ✅ | SC-007 | Secrets not exposed in logs |
| Permissions | ✅ | `bicep-lint.yml#L20-23` | id-token, contents, pull-requests |
| Secret Management | ✅ | A-001 | Azure credentials as secrets |

### Error Handling Coverage

| Aspect | Documented | Code Location | Gap |
|--------|------------|---------------|-----|
| Failure Scenarios | ✅ | Edge Cases section | 5 scenarios documented |
| Error Reporting | ✅ | FR-006 | Detailed error messages required |
| Cleanup on Failure | ⚠️ | `bicep-lint.yml#L127-133` | `if: always()` cleanup, not in spec |
| Multiple Error Reporting | ✅ | User Story 3, Scenario 2 | All errors reported together |

### Performance Coverage

| Aspect | Documented | Code Location | Gap |
|--------|------------|---------------|-----|
| Timeout | ✅ | FR-009, SC-002 | 10 min max, 5 min target |
| Concurrent Execution | ⚠️ | Edge Cases | Question asked but not answered |
| Rate Limiting | ✅ | A-003 | Azure API limits acknowledged |

### Integration Coverage

| Aspect | Documented | Code Location | Gap |
|--------|------------|---------------|-----|
| GitHub Actions | ✅ | A-006 | Runner infrastructure mentioned |
| Azure APIs | ✅ | FR-001, A-005 | Deployment validation API |
| OIDC Federation | ✅ | `bicep-lint.yml#L43` | Workload Identity Federation |

### Configuration Coverage

| Aspect | Documented | Code Location | Gap |
|--------|------------|---------------|-----|
| Azure Secrets | ✅ | A-001 | CLIENT_ID, TENANT_ID, SUBSCRIPTION_ID |
| Path Filters | ✅ | FR-002, FR-003 | templates/azure/** |
| Test Parameters | ⚠️ | `bicep-lint.yml#L81` | File exists but not specified in spec |

### Coverage Score Breakdown

- Security: 4/4 aspects documented
- Error Handling: 3/4 aspects documented
- Performance: 3/3 aspects documented
- Integration: 4/4 aspects documented
- Configuration: 4/5 aspects documented

---

## Recommendations

### Critical (Must Fix)
None - Spec is well-aligned with implementation.

### High Priority
1. **Update action version reference**: Change A-005 from arm-deploy@v1 to arm-deploy@v2

### Medium Priority
1. **Document resource group lifecycle**: Add FR for temporary resource group creation and cleanup
2. **List specific templates validated**: Document the 4 main templates being validated
3. **Document test parameters file**: Add reference to `azuredeploy.parameters.test.json`

### Low Priority
1. **Answer edge case questions**: Provide answers to the 5 edge case questions
2. **Document PR labeling behavior**: Add note about automatic arm-templates label
3. **Add implementation notes**: Document the actual workflow structure

---

## Detailed Findings

### Finding 1: Action Version Discrepancy

**Observation**: Spec references v1, implementation uses v2
**Spec Says**: A-005 "azure/arm-deploy@v1 GitHub Action"
**Code Shows**: `azure/arm-deploy@v2` used in all validation steps
**File**: `.github/workflows/bicep-lint.yml` (Lines 74, 86, 100, 114)
**Recommendation**: Update spec assumption to reference v2 (improvement, not regression)

### Finding 2: Resource Group Lifecycle Not Documented

**Observation**: Implementation creates and cleans up a temporary resource group
**Spec Says**: No mention of resource group management
**Code Shows**: Creates `rg-bicep-validation-{run_id}` and deletes after validation
**File**: `.github/workflows/bicep-lint.yml` (Lines 63-71, 127-133)
**Recommendation**: Add functional requirement for resource group lifecycle

### Finding 3: Specific Templates Not Listed

**Observation**: Spec says "all Bicep templates" but 4 specific ones are validated
**Spec Says**: FR-007 "support all Bicep templates in the repository"
**Code Shows**: Only validates: azuredeploy.bicep, portal_with_lorawan_and_starter_kit.bicep, portal_with_lorawan.bicep, portal_without_lorawan.bicep
**File**: `.github/workflows/bicep-lint.yml` (Lines 74-123)
**Recommendation**: Either list specific templates or add dynamic template discovery

### Finding 4: Test Parameters File Undocumented

**Observation**: Validation uses a test parameters file
**Spec Says**: FR-008 "handle template parameters" (generic)
**Code Shows**: Uses `./templates/azure/tests/azuredeploy.parameters.test.json`
**File**: `.github/workflows/bicep-lint.yml` (Line 81)
**Recommendation**: Document the test parameters file and its purpose

---

## Code References

| File | Lines | Purpose |
|------|-------|---------|
| `.github/workflows/bicep-lint.yml` | 1-159 | Main CI workflow for Bicep validation |
| `templates/azure/tests/azuredeploy.parameters.test.json` | - | Test parameters for validation |
| `templates/azure/azuredeploy.bicep` | - | Main deployment template |
| `templates/azure/portal_with_lorawan.bicep` | - | Portal with LoRaWAN template |
| `templates/azure/portal_with_lorawan_and_starter_kit.bicep` | - | Full LoRaWAN template |
| `templates/azure/portal_without_lorawan.bicep` | - | Portal without LoRaWAN template |
