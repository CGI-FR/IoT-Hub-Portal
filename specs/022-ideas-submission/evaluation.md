# Evaluation: Ideas Submission (022)

**Specification ID**: 022  
**Feature**: Ideas Submission  
**Evaluated**: 2026-02-03  
**Evaluator**: Excavate Evaluator

---

## Summary

| Metric | Score | Weight | Weighted Score |
|--------|-------|--------|----------------|
| Correctness | 98% | 30% | 29.4% |
| Completeness | 95% | 30% | 28.5% |
| Technical Quality | 92% | 20% | 18.4% |
| Coverage | 90% | 20% | 18.0% |
| **Overall** | | | **94.3%** |

---

## Accurate Specifications

| Requirement | Spec Description | Code Evidence | Status |
|-------------|------------------|---------------|--------|
| FR-001 | Allow authenticated users to submit ideas | [IdeasController.cs#L6,21](src/IoTHub.Portal.Server/Controllers/v1.0/IdeasController.cs#L6): `[Authorize]` + `[Authorize("idea:write")]` | ✅ Verified |
| FR-002 | Idea includes title and description (body) | [IdeaRequest.cs](src/IoTHub.Portal.Shared/Models/v1.0/IdeaRequest.cs): `Title` and `Body` with `[Required]` | ✅ Verified |
| FR-003 | Support optional consent for technical details | [IdeaRequest.cs#L14](src/IoTHub.Portal.Shared/Models/v1.0/IdeaRequest.cs#L14): `ConsentToCollectTechnicalDetails` boolean | ✅ Verified |
| FR-004 | Collect browser and application version when consented | [IdeaService.cs#L38-48](src/IoTHub.Portal.Server/Services/IdeaService.cs#L38-48): Uses UAParser for browser, Assembly for version | ✅ Verified |
| FR-005 | Submit to external ideas platform API | [IdeaService.cs#L62-64](src/IoTHub.Portal.Server/Services/IdeaService.cs#L62-64): HTTP POST to "ideas" endpoint | ✅ Verified |
| FR-006 | Return tracking URL on success | [IdeaResponse.cs](src/IoTHub.Portal.Shared/Models/v1.0/IdeaResponse.cs): `Url` property returned | ✅ Verified |
| FR-007 | Validate ideas feature is enabled | [IdeaService.cs#L22-25](src/IoTHub.Portal.Server/Services/IdeaService.cs#L22-25): Checks `configHandler.IdeasEnabled` | ✅ Verified |
| FR-008 | Handle external API failures gracefully | [IdeaService.cs#L73-75](src/IoTHub.Portal.Server/Services/IdeaService.cs#L73-75): Throws `InternalServerErrorException` with reason | ✅ Verified |
| FR-009 | Ideas feature configurable via settings | [IdeaService.cs#L22](src/IoTHub.Portal.Server/Services/IdeaService.cs#L22): Uses `configHandler.IdeasEnabled` | ✅ Verified |
| FR-010 | No technical details without consent | [IdeaService.cs#L33-54](src/IoTHub.Portal.Server/Services/IdeaService.cs#L33-54): Conditional branching based on consent | ✅ Verified |

---

## Inaccuracies Found

| Issue | Spec Statement | Actual Code Behavior | Impact |
|-------|----------------|----------------------|--------|
| Error message | Spec: "appropriate message indicating the feature is unavailable" | Actual: Throws exception with message "Ideas feature is not enabled. Please check Iot Hub Portal documentation" | 🟢 Low (more specific) |
| User-agent parsing | Spec: "browser version" | Actual: Uses UAParser to get Family + Major + Minor (e.g., "Chrome12010") without separator | 🟢 Low |

---

## Key Entities Verification

| Entity | Spec Definition | Code Implementation | Match |
|--------|-----------------|---------------------|-------|
| IdeaRequest | Title, Body, ConsentToCollectTechnicalDetails | [IdeaRequest.cs](src/IoTHub.Portal.Shared/Models/v1.0/IdeaRequest.cs): All properties present with validation | ✅ Match |
| IdeaResponse | URL (tracking link) | [IdeaResponse.cs](src/IoTHub.Portal.Shared/Models/v1.0/IdeaResponse.cs): `Url` property present | ✅ Match |
| Technical Context | Application Version, Browser Version | [IdeaService.cs#L43-48](src/IoTHub.Portal.Server/Services/IdeaService.cs#L43-48): Both collected when consented | ✅ Match |

---

## Code References

| Component | File Path | Lines | Purpose |
|-----------|-----------|-------|---------|
| Controller | [IdeasController.cs](src/IoTHub.Portal.Server/Controllers/v1.0/IdeasController.cs) | 1-27 | API endpoint for idea submission |
| Service | [IdeaService.cs](src/IoTHub.Portal.Server/Services/IdeaService.cs) | 1-78 | Business logic for idea processing |
| Interface | [IIdeaService.cs](src/IoTHub.Portal.Application/Services/IIdeaService.cs) | - | Service interface |
| Request Model | [IdeaRequest.cs](src/IoTHub.Portal.Shared/Models/v1.0/IdeaRequest.cs) | 1-16 | Input DTO |
| Response Model | [IdeaResponse.cs](src/IoTHub.Portal.Shared/Models/v1.0/IdeaResponse.cs) | 1-10 | Output DTO |
| Controller Tests | [IdeasControllerTests.cs](src/IoTHub.Portal.Tests.Unit/Server/Controllers/v1.0/IdeasControllerTests.cs) | - | Unit tests |
| Service Tests | [IdeaServiceTests.cs](src/IoTHub.Portal.Tests.Unit/Server/Services/IdeaServiceTests.cs) | - | Unit tests |

---

## Test Coverage

| Area | Status | Evidence |
|------|--------|----------|
| Controller Tests | ✅ 90% | [IdeasControllerTests.cs](src/IoTHub.Portal.Tests.Unit/Server/Controllers/v1.0/IdeasControllerTests.cs): Covers submit idea flow |
| Service Tests | ✅ 95% | [IdeaServiceTests.cs](src/IoTHub.Portal.Tests.Unit/Server/Services/IdeaServiceTests.cs): Covers success, disabled feature, consent variations |
| Error Handling | ✅ 90% | Tests for disabled feature, HTTP failures |
| Consent Flow | ✅ 95% | Tests for both consent=true and consent=false |

---

## Dependencies Verification

| Dependency | Spec Statement | Code Evidence | Status |
|------------|----------------|---------------|--------|
| 023-portal-settings | IsIdeasFeatureEnabled configuration | [IdeaService.cs#L22](src/IoTHub.Portal.Server/Services/IdeaService.cs#L22): Uses `configHandler.IdeasEnabled` | ✅ Verified |
| External Platform | Ideas platform API | [Startup.cs#L303](src/IoTHub.Portal.Server/Startup.cs#L303): HTTP client configured with base URL | ✅ Verified |

---

## Recommendations

1. **Improve browser version formatting**: Currently concatenates family, major, minor without separator. Consider adding separator (e.g., "Chrome 120.1.0").

2. **Add rate limiting documentation**: Edge case mentions platform rate limiting but no handling is documented in the spec.

3. **Consider idea length validation**: Edge case mentions long descriptions may be truncated - could add client-side validation.

4. **Add logging of consent decisions**: For audit purposes, consider logging whether technical details were included.

5. **Document HTTP client configuration**: The spec mentions external platform but doesn't specify the client configuration in Startup.cs.
