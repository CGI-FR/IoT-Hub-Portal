# Feature Specification: Portal Settings

**Feature ID**: 023  
**Feature Branch**: `023-portal-settings`  
**Created**: 2026-02-03  
**Status**: Draft  
**Source**: Analysis from `specs/023-portal-settings/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - Initialize Portal Client with OIDC Settings (Priority: P1)

As a portal client application, I need to retrieve OIDC authentication configuration so that I can properly authenticate users against the identity provider.

**Why this priority**: OIDC configuration is critical for user authentication - without it, users cannot log in to the portal.

**Independent Test**: Can be fully tested by requesting OIDC settings and verifying all required authentication parameters are returned.

**Acceptance Scenarios**:

1. **Given** a client application is initializing, **When** it requests OIDC settings, **Then** it receives the authority URL, metadata URL, client ID, and scope.
2. **Given** OIDC settings are retrieved, **When** the client uses them, **Then** it can successfully redirect users to the identity provider for authentication.
3. **Given** OIDC configuration is missing, **When** settings are requested, **Then** an appropriate error response is returned.

---

### User Story 2 - Display Portal Branding (Priority: P1)

As a portal user, I need to see correct branding information so that I know which portal instance I'm using and can reference version information for support.

**Why this priority**: Branding and version information are essential for user orientation and support troubleshooting.

**Independent Test**: Can be fully tested by requesting portal settings and verifying branding information matches configuration.

**Acceptance Scenarios**:

1. **Given** I am viewing the portal, **When** settings are loaded, **Then** I see the configured portal name.
2. **Given** the portal is configured with a custom name, **When** I view the portal, **Then** the custom name is displayed instead of the default.
3. **Given** I need support, **When** I view portal settings, **Then** I can see the version number to report.
4. **Given** it's a new year, **When** I view the portal, **Then** the copyright year reflects the current year.

---

### User Story 3 - Adapt UI Based on Feature Flags (Priority: P1)

As a portal client application, I need to know which features are enabled so that I can show or hide feature-specific UI elements appropriately.

**Why this priority**: Feature flag discovery prevents UI errors when accessing disabled features and improves user experience.

**Independent Test**: Can be fully tested by checking settings with various feature configurations and verifying correct flag values.

**Acceptance Scenarios**:

1. **Given** LoRa features are enabled, **When** I check portal settings, **Then** IsLoRaSupported is true.
2. **Given** LoRa features are disabled, **When** I check portal settings, **Then** IsLoRaSupported is false.
3. **Given** ideas feature is enabled, **When** I check portal settings, **Then** IsIdeasFeatureEnabled is true.
4. **Given** the portal is hosted on Azure, **When** I check settings, **Then** CloudProvider indicates "Azure".

---

### User Story 4 - Identify Cloud Provider (Priority: P2)

As a portal client application, I need to know which cloud provider the portal is connected to so that I can display provider-specific information and options.

**Why this priority**: Cloud provider awareness enables the UI to adapt to provider-specific capabilities, though less critical than authentication.

**Independent Test**: Can be fully tested by checking settings on different cloud deployments and verifying correct provider identification.

**Acceptance Scenarios**:

1. **Given** the portal is connected to Azure IoT Hub, **When** I check settings, **Then** CloudProvider is "Azure".
2. **Given** the portal is connected to AWS IoT Core, **When** I check settings, **Then** CloudProvider is "AWS".
3. **Given** I know the cloud provider, **When** I view relevant UI, **Then** provider-appropriate terminology and options are shown.

---

### Edge Cases

- What happens if configuration is incomplete? (System should use sensible defaults; e.g., "Azure IoT Hub Portal" for missing name)
- How are settings cached by clients? (Clients may cache; settings don't change frequently)
- What happens when assembly version information is unavailable? (Version may be null or "Unknown")
- How do settings behave during a version upgrade? (Version reflects the running assembly)

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST provide OIDC configuration for client authentication setup
- **FR-002**: OIDC settings MUST include authority URL, metadata URL, client ID, and scope
- **FR-003**: System MUST provide portal branding settings
- **FR-004**: Portal settings MUST include portal name with a default value
- **FR-005**: Portal settings MUST include current application version
- **FR-006**: Portal settings MUST include current copyright year
- **FR-007**: Portal settings MUST indicate whether LoRa features are enabled
- **FR-008**: Portal settings MUST indicate whether ideas feature is enabled
- **FR-009**: Portal settings MUST indicate the cloud provider (Azure, AWS)
- **FR-010**: Settings endpoints MUST require appropriate authorization
- **FR-011**: Portal name MUST default to "Azure IoT Hub Portal" if not configured

### Key Entities

- **ClientApiIndentityOptions**: OIDC configuration containing:
  - Authority (identity provider URL)
  - MetadataUrl (OIDC discovery endpoint)
  - ClientId (application client ID)
  - Scope (requested OAuth scopes)

- **PortalSettings**: Runtime configuration containing:
  - PortalName (display name with default)
  - Version (application version from assembly)
  - CopyrightYear (current year)
  - IsLoRaSupported (LoRa feature flag)
  - IsIdeasFeatureEnabled (ideas feature flag)
  - CloudProvider (Azure, AWS, etc.)

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Settings endpoints respond in under 100 milliseconds
- **SC-002**: 100% of client applications successfully retrieve OIDC configuration on first attempt
- **SC-003**: Portal branding is correctly displayed in all portal views
- **SC-004**: Feature flags correctly hide/show features with zero UI errors for disabled features
- **SC-005**: Support teams can identify portal version in under 10 seconds

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/023-portal-settings/analyze.md`
- **Analyzed By**: excavator.specifier

### Code References
- SettingsController: Settings retrieval endpoints
- ClientApiIndentityOptions: OIDC configuration model
- PortalSettings: Portal configuration model
- ConfigHandler: Centralized configuration access

### Dependencies
- **Depends On**: 
  - ASP.NET Core configuration system (appsettings.json, environment variables)
  - Azure AD / OIDC identity provider configuration
- **Depended By**: 
  - Portal client application (uses OIDC settings for authentication)
  - 022-ideas-submission (checks IsIdeasFeatureEnabled)
  - LoRaWAN features (check IsLoRaSupported)
