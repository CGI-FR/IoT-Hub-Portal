# Evaluation: Portal Settings (023)

**Specification ID**: 023  
**Feature**: Portal Settings  
**Evaluated**: 2026-02-03  
**Evaluator**: Excavate Evaluator

---

## Summary

| Metric | Score | Weight | Weighted Score |
|--------|-------|--------|----------------|
| Correctness | 97% | 30% | 29.1% |
| Completeness | 95% | 30% | 28.5% |
| Technical Quality | 95% | 20% | 19.0% |
| Coverage | 92% | 20% | 18.4% |
| **Overall** | | | **95.0%** |

---

## Accurate Specifications

| Requirement | Spec Description | Code Evidence | Status |
|-------------|------------------|---------------|--------|
| FR-001 | Provide OIDC configuration for authentication | [SettingsController.cs#L42-48](src/IoTHub.Portal.Server/Controllers/v1.0/SettingsController.cs#L42-48): `GetOIDCSettings()` endpoint | ✅ Verified |
| FR-002 | OIDC includes authority, metadata URL, client ID, scope | [ClientApiIndentityOptions.cs](src/IoTHub.Portal.Server/Identity/ClientApiIndentityOptions.cs): All 4 properties defined | ✅ Verified |
| FR-003 | Provide portal branding settings | [SettingsController.cs#L54-67](src/IoTHub.Portal.Server/Controllers/v1.0/SettingsController.cs#L54-67): `GetPortalSetting()` endpoint | ✅ Verified |
| FR-004 | Portal settings include portal name | [PortalSettings.cs#L23-24](src/IoTHub.Portal.Shared/Models/v1.0/PortalSettings.cs#L23-24): `PortalName` property | ✅ Verified |
| FR-005 | Include current application version | [SettingsController.cs#L62](src/IoTHub.Portal.Server/Controllers/v1.0/SettingsController.cs#L62): Uses `Assembly.GetExecutingAssembly().GetName().Version` | ✅ Verified |
| FR-006 | Include current copyright year | [SettingsController.cs#L63](src/IoTHub.Portal.Server/Controllers/v1.0/SettingsController.cs#L63): Uses `DateTime.Now.Year` | ✅ Verified |
| FR-007 | Indicate LoRa features enabled | [PortalSettings.cs#L14](src/IoTHub.Portal.Shared/Models/v1.0/PortalSettings.cs#L14): `IsLoRaSupported` property | ✅ Verified |
| FR-008 | Indicate ideas feature enabled | [PortalSettings.cs#L32](src/IoTHub.Portal.Shared/Models/v1.0/PortalSettings.cs#L32): `IsIdeasFeatureEnabled` property | ✅ Verified |
| FR-009 | Indicate cloud provider | [PortalSettings.cs#L37](src/IoTHub.Portal.Shared/Models/v1.0/PortalSettings.cs#L37): `CloudProvider` property | ✅ Verified |
| FR-010 | Settings endpoints require authorization | [SettingsController.cs#L43,55](src/IoTHub.Portal.Server/Controllers/v1.0/SettingsController.cs#L43): `[Authorize("setting:read")]` | ✅ Verified |
| FR-011 | Default portal name "Azure IoT Hub Portal" | [SettingsController.cs#L61](src/IoTHub.Portal.Server/Controllers/v1.0/SettingsController.cs#L61): `?? "Azure IoT Hub Portal"` | ✅ Verified |

---

## Inaccuracies Found

| Issue | Spec Statement | Actual Code Behavior | Impact |
|-------|----------------|----------------------|--------|
| Class attribute | Spec: controller requires authorization | Actual: [SettingsController.cs#L7](src/IoTHub.Portal.Server/Controllers/v1.0/SettingsController.cs#L7) has `[AllowAnonymous]` at class level, but methods have `[Authorize]` | 🟡 Medium (spec should clarify) |
| Authorization scope | Spec: "appropriate authorization" | Actual: Uses `setting:read` policy specifically | 🟢 Low |
| Namespace typo | N/A | [ClientApiIndentityOptions.cs](src/IoTHub.Portal.Server/Identity/ClientApiIndentityOptions.cs): Note "Indentity" spelling (not "Identity") | 🟢 Low (legacy naming) |

---

## Key Entities Verification

| Entity | Spec Definition | Code Implementation | Match |
|--------|-----------------|---------------------|-------|
| ClientApiIndentityOptions | Authority, MetadataUrl, ClientId, Scope | [ClientApiIndentityOptions.cs](src/IoTHub.Portal.Server/Identity/ClientApiIndentityOptions.cs): All properties present | ✅ Match |
| PortalSettings | PortalName, Version, CopyrightYear, IsLoRaSupported, IsIdeasFeatureEnabled, CloudProvider | [PortalSettings.cs](src/IoTHub.Portal.Shared/Models/v1.0/PortalSettings.cs): All properties present | ✅ Match |

---

## Code References

| Component | File Path | Lines | Purpose |
|-----------|-----------|-------|---------|
| Controller | [SettingsController.cs](src/IoTHub.Portal.Server/Controllers/v1.0/SettingsController.cs) | 1-68 | Settings API endpoints |
| OIDC Model | [ClientApiIndentityOptions.cs](src/IoTHub.Portal.Server/Identity/ClientApiIndentityOptions.cs) | 1-16 | OIDC configuration model |
| Portal Settings Model | [PortalSettings.cs](src/IoTHub.Portal.Shared/Models/v1.0/PortalSettings.cs) | 1-39 | Runtime settings model |
| ConfigHandler | [ConfigHandler.cs](src/IoTHub.Portal.Infrastructure/ConfigHandler.cs) | - | Configuration abstraction |
| Controller Tests | [SettingsControllerTest.cs](src/IoTHub.Portal.Tests.Unit/Server/Controllers/v1.0/SettingsControllerTest.cs) | - | Unit tests |

---

## Test Coverage

| Area | Status | Evidence |
|------|--------|----------|
| OIDC Endpoint | ✅ 90% | [SettingsControllerTest.cs](src/IoTHub.Portal.Tests.Unit/Server/Controllers/v1.0/SettingsControllerTest.cs): Tests OIDC settings retrieval |
| Portal Settings | ✅ 95% | Tests for all portal settings properties |
| Default Values | ✅ 85% | Tests for default portal name |
| Feature Flags | ✅ 90% | Tests for IsLoRaSupported, IsIdeasFeatureEnabled |

---

## Dependencies Verification

| Dependency | Spec Statement | Code Evidence | Status |
|------------|----------------|---------------|--------|
| ASP.NET Core Configuration | appsettings.json, environment variables | [SettingsController.cs#L26](src/IoTHub.Portal.Server/Controllers/v1.0/SettingsController.cs#L26): Uses `IOptions<ClientApiIndentityOptions>` | ✅ Verified |
| ConfigHandler | Centralized config access | [SettingsController.cs#L32](src/IoTHub.Portal.Server/Controllers/v1.0/SettingsController.cs#L32): Uses `ConfigHandler` for runtime settings | ✅ Verified |

---

## API Endpoints Summary

| Endpoint | Method | Path | Authorization |
|----------|--------|------|---------------|
| GET OIDC Settings | GET | `/api/settings/oidc` | `setting:read` |
| GET Portal Settings | GET | `/api/settings/portal` | `setting:read` |

---

## Recommendations

1. **Clarify class-level vs method-level authorization**: The controller has `[AllowAnonymous]` at class level but `[Authorize]` on methods - this is intentional but should be documented in the spec.

2. **Document ConfigHandler properties used**: Spec references ConfigHandler but doesn't list which properties are read (`IsLoRaEnabled`, `PortalName`, `IdeasEnabled`, `CloudProvider`).

3. **Add version nullable handling**: Code uses `Version?.ToString()` which can return null - spec should document this edge case.

4. **Consider caching documentation**: Spec mentions clients may cache but doesn't document any server-side caching strategy.

5. **Fix legacy naming**: Consider a future migration to fix "Indentity" -> "Identity" spelling in `ClientApiIndentityOptions`.
