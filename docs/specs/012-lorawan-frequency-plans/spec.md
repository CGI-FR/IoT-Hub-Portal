# Feature Specification: LoRaWAN Frequency Plans

**Feature ID**: 012  
**Feature Branch**: `012-lorawan-frequency-plans`  
**Created**: 2026-02-03  
**Status**: Draft  
**Source**: Analysis from `specs/012-lorawan-frequency-plans/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - Select Frequency Plan for Concentrator (Priority: P1)

As an IoT administrator, I need to select the appropriate frequency plan when configuring a LoRaWAN concentrator so that the gateway operates on the correct radio frequencies for my geographical region and complies with local regulations.

**Why this priority**: Frequency plan selection is mandatory for concentrator configuration - without it, the gateway cannot communicate with devices. Incorrect frequency selection can cause regulatory violations.

**Independent Test**: Can be fully tested by creating or editing a concentrator, viewing the frequency plan dropdown, selecting a region, and verifying the selection is saved.

**Acceptance Scenarios**:

1. **Given** I am configuring a new concentrator, **When** I access the region/frequency plan field, **Then** I see a dropdown with all supported frequency plans organized by region.
2. **Given** I am in Europe, **When** I select "Europe 863-870 MHz", **Then** the frequency plan is applied to the concentrator configuration.
3. **Given** I am in the United States, **When** I view frequency options, **Then** I see multiple FSB (Frequency Sub-Band) options to choose from.
4. **Given** a concentrator already has a frequency plan, **When** I view its configuration, **Then** the current frequency plan is pre-selected.

---

### User Story 2 - View Available Frequency Plans (Priority: P2)

As an IoT administrator, I need to view all supported frequency plans so that I can plan my LoRaWAN network deployment across multiple regions.

**Why this priority**: Understanding available options helps in network planning, but the primary use case is configuration during concentrator setup.

**Independent Test**: Can be fully tested by accessing the frequency plans endpoint and verifying the complete list is returned.

**Acceptance Scenarios**:

1. **Given** LoRa features are enabled in the portal, **When** I request the frequency plans list, **Then** I receive all 22 supported frequency plans.
2. **Given** I am reviewing frequency plans, **When** I examine the list, **Then** each plan shows a human-readable name and technical identifier.
3. **Given** LoRa features are disabled, **When** I attempt to access frequency plans, **Then** the feature is unavailable with an appropriate message.

---

### Edge Cases

- What happens if a frequency plan is selected but later becomes deprecated? (Existing configurations remain valid; new configurations should warn users)
- How are regional variations handled (e.g., AS923 groups for different Asian countries)? (Multiple sub-plans provided for regional specifics)
- What happens when creating a concentrator in a region without a matching frequency plan? (User must select closest appropriate plan)

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST provide a predefined list of supported LoRaWAN frequency plans
- **FR-002**: System MUST include frequency plans for major global regions:
  - Europe (EU 863-870 MHz)
  - United States (US 902-928 MHz with FSB options)
  - Asia (AS 923-925 MHz with regional groups)
  - Australia (AU 915-928 MHz with FSB options)
  - China (CN 470-510 MHz with sub-band options)
- **FR-003**: Each frequency plan MUST have a unique identifier and human-readable name
- **FR-004**: System MUST display frequency plans in alphabetical order by name
- **FR-005**: Frequency plan selection MUST be required for concentrator configuration
- **FR-006**: System MUST validate that LoRa features are enabled before providing frequency plan data
- **FR-007**: System MUST persist the selected frequency plan with the concentrator configuration

### Key Entities

- **FrequencyPlan**: Read-only reference data containing:
  - FrequencyPlanID (unique technical identifier, e.g., "EU_863_870", "US_902_928_FSB_1")
  - Name (human-readable description, e.g., "Europe 863-870 MHz", "United States 902-928 MHz, FSB 1")

### Supported Frequency Plans

| Region | Plans Available |
|--------|----------------|
| Europe | EU_863_870 |
| United States | US_902_928_FSB_1 through FSB_8 |
| Asia | AS_923_925_1 through AS_923_925_4 |
| Australia | AU_915_928_FSB_1 through FSB_8 |
| China | CN_470_510_FSB_1 through FSB_11 |

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Administrators can identify and select the correct frequency plan within 30 seconds
- **SC-002**: 100% of concentrators have a valid frequency plan configured before activation
- **SC-003**: Zero regulatory compliance issues due to incorrect frequency configuration (when correct plan is selected)
- **SC-004**: Portal supports LoRaWAN deployments across 5+ major geographical regions

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/012-lorawan-frequency-plans/analyze.md`
- **Analyzed By**: excavator.specifier

### Code References
- LoRaWANFrequencyPlansController: Frequency plan retrieval
- FrequencyPlan DTO: Data structure for frequency plan information
- LoRaFeatureActiveFilter: Feature gate for LoRa functionality

### Dependencies
- **Depends On**: 
  - Portal configuration (LoRa features must be enabled)
- **Depended By**: 
  - 010-lorawan-concentrator-management (concentrators require frequency plan selection)
