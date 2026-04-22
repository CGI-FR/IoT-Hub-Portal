# Feature Specification: Device Import/Export

**Feature ID**: 021  
**Feature Branch**: `021-device-import-export`  
**Created**: 2026-02-03  
**Status**: Draft  
**Source**: Analysis from `specs/021-device-import-export/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - Export Device Inventory (Priority: P1)

As an IoT administrator, I need to export all registered devices to a CSV file so that I can create backups, perform analysis, or migrate devices to another system.

**Why this priority**: Export capability provides data portability and backup - essential for enterprise operations and disaster recovery.

**Independent Test**: Can be fully tested by triggering an export and verifying the CSV contains all devices with correct data.

**Acceptance Scenarios**:

1. **Given** I have device export permissions, **When** I trigger a device export, **Then** I receive a CSV file with all registered devices.
2. **Given** devices have various tags and properties, **When** I export, **Then** the CSV includes columns for each tag (TAG:*) and property (PROPERTY:*).
3. **Given** I export devices, **When** I open the CSV, **Then** mandatory columns (ID, Name, ModelId) are present along with all configured metadata.
4. **Given** the export completes, **When** I check the filename, **Then** it includes a timestamp for versioning (e.g., Devices_202602031200.csv).

---

### User Story 2 - Download Import Template (Priority: P1)

As an IoT administrator, I need to download a CSV template so that I can prepare device data in the correct format for bulk import.

**Why this priority**: Template ensures correct data format, reducing import errors and simplifying the bulk onboarding process.

**Independent Test**: Can be fully tested by downloading the template and verifying it contains the correct column headers.

**Acceptance Scenarios**:

1. **Given** I request the import template, **When** I download it, **Then** I receive a CSV with all required column headers.
2. **Given** the portal has configured tags, **When** I download the template, **Then** TAG:* columns are included for each configured tag.
3. **Given** the portal has device model properties, **When** I download the template, **Then** PROPERTY:* columns are included.
4. **Given** LoRa features are enabled, **When** I download the template, **Then** LoRaWAN-specific columns are included.

---

### User Story 3 - Import Devices from CSV (Priority: P1)

As an IoT administrator, I need to import devices from a CSV file so that I can bulk onboard new devices or update existing device configurations.

**Why this priority**: Bulk import dramatically reduces onboarding time for large deployments, which is critical for enterprise-scale operations.

**Independent Test**: Can be fully tested by preparing a CSV with valid device data, importing it, and verifying devices are created/updated.

**Acceptance Scenarios**:

1. **Given** I have a valid CSV file with device data, **When** I import it, **Then** new devices are created in the portal.
2. **Given** my CSV includes existing device IDs, **When** I import, **Then** those devices are updated with the new data.
3. **Given** my CSV includes LoRaWAN device data, **When** I import with supportLoRaFeatures tag set, **Then** devices are created as LoRaWAN devices.
4. **Given** I import a file, **When** the import completes, **Then** I receive a line-by-line result report showing success or errors.

---

### User Story 4 - Handle Import Errors Gracefully (Priority: P2)

As an IoT administrator, I need detailed error reporting during import so that I can identify and fix data issues in my CSV.

**Why this priority**: Clear error reporting enables quick resolution of data quality issues, reducing time to successful import.

**Independent Test**: Can be fully tested by importing a CSV with intentional errors and verifying each error is reported with line number and message.

**Acceptance Scenarios**:

1. **Given** my CSV has a row with a missing required field, **When** I import, **Then** the error report identifies the line number and missing field.
2. **Given** my CSV references a non-existent device model, **When** I import, **Then** an error indicates the model is not found.
3. **Given** some rows succeed and others fail, **When** I view the report, **Then** I can distinguish between successful and failed imports.
4. **Given** I receive error feedback, **When** I fix the CSV and re-import, **Then** previously failed devices are created successfully.

---

### User Story 5 - Import Device Tags and Properties (Priority: P2)

As an IoT administrator, I need to include custom tags and properties in my import so that devices are fully configured during bulk onboarding.

**Why this priority**: Complete device configuration during import eliminates the need for post-import manual configuration.

**Independent Test**: Can be fully tested by importing devices with TAG:* and PROPERTY:* columns, then verifying the values are applied.

**Acceptance Scenarios**:

1. **Given** my CSV includes TAG:location with values, **When** I import, **Then** each device has the location tag set.
2. **Given** my CSV includes PROPERTY:threshold with values, **When** I import, **Then** device properties are set according to the model schema.
3. **Given** property values don't match the expected type, **When** I import, **Then** validation errors are reported.

---

### Edge Cases

- What happens with very large CSV files (10,000+ devices)? (Import should process in reasonable time; may need progress indication)
- How are duplicate device IDs in the same CSV handled? (Later row may overwrite earlier, or second attempt updates)
- What happens if the import is interrupted? (Partial import persists; re-import needed for remaining devices)
- How are special characters in device names handled? (CSV encoding should handle UTF-8; escaping as needed)
- What happens when the cloud provider API is unavailable during import? (Import fails with appropriate error)

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST allow exporting all devices to a CSV file
- **FR-002**: Export MUST include device ID, name, and model ID as mandatory columns
- **FR-003**: Export MUST include configured tag columns with TAG: prefix
- **FR-004**: Export MUST include configured property columns with PROPERTY: prefix
- **FR-005**: Export MUST include LoRaWAN-specific columns when LoRa features are enabled
- **FR-006**: Export filename MUST include a timestamp for versioning
- **FR-007**: System MUST provide a downloadable import template
- **FR-008**: Template MUST reflect current portal tag and property configuration
- **FR-009**: System MUST allow importing devices from a CSV file
- **FR-010**: Import MUST create new devices for unknown device IDs
- **FR-011**: Import MUST update existing devices when device ID matches
- **FR-012**: Import MUST validate required fields (ID, Name, ModelId)
- **FR-013**: Import MUST validate device model existence before creating devices
- **FR-014**: Import MUST return a line-by-line result report
- **FR-015**: Import result MUST indicate success or failure with descriptive messages
- **FR-016**: Import MUST support both standard IoT devices and LoRaWAN devices

### Key Entities

- **CSV Structure**: Standard import/export format:
  - Id (device identifier)
  - Name (device display name)
  - ModelId (device model reference)
  - TAG:* columns (dynamic based on configuration)
  - PROPERTY:* columns (dynamic based on model)

- **ImportResultLine**: Import result reporting:
  - LineNumber (CSV row number)
  - DeviceId (device identifier from row)
  - Message (success or error description)
  - IsErrorMessage (boolean indicating error vs. success)

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Administrators can export 1,000 devices in under 30 seconds
- **SC-002**: Administrators can import 100 devices in under 2 minutes
- **SC-003**: Import error rate is less than 5% when using the provided template
- **SC-004**: 100% of import errors include actionable error messages with line numbers
- **SC-005**: Reduce bulk device onboarding time by 90% compared to manual entry

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/021-device-import-export/analyze.md`
- **Analyzed By**: excavator.specifier

### Code References
- AdminController: Import/export endpoints
- ExportManager: Business logic for CSV processing
- IExternalDeviceService: Device data retrieval
- DeviceService: Device creation/update operations

### Dependencies
- **Depends On**: 
  - 001-standard-device-management (device CRUD operations)
  - 002-device-model-management (model validation)
  - 004-device-tag-settings-management (tag configuration)
  - 003-device-properties-management (property configuration)
  - 008-lorawan-device-management (LoRaWAN device creation)
- **Depended By**: 
  - None (standalone admin feature)
