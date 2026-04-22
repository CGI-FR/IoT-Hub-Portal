# Feature Specification: Device Tag Settings Management

**Feature ID**: 004  
**Feature Branch**: `004-device-tag-settings-management`  
**Created**: 2026-01-30  
**Status**: Draft  
**Source**: Analysis from `specs/004-device-tag-settings-management/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - View All Device Tag Definitions (Priority: P1)

As an IoT administrator, I need to view all configured device tags with their properties (label, required, searchable) so that I understand what metadata fields are available for device organization.

**Why this priority**: Core functionality for understanding the device tagging system. Without this, users cannot see what tags exist or how they're configured.

**Independent Test**: Can be fully tested by navigating to the device tag settings page and viewing the list of all tag definitions in a table. Delivers immediate value by providing tag visibility.

**Acceptance Scenarios**:

1. **Given** I am an authorized user with device-tag:read permission, **When** I navigate to the device tag settings page, **Then** I see a table displaying all device tags with columns for Name, Label, Required, and Searchable
2. **Given** I am viewing the device tag list, **When** no tags have been configured, **Then** I see an empty table with a message indicating no tags exist
3. **Given** I am viewing the device tag list, **When** tags exist, **Then** each tag shows its name (alphanumeric identifier), label (display name), required flag (checkbox), and searchable flag (checkbox)
4. **Given** I am a user without device-tag:read permission, **When** I attempt to access the device tag settings page, **Then** I am denied access

---

### User Story 2 - Create New Device Tag Definitions (Priority: P1)

As an IoT administrator, I need to create new device tag definitions so that I can add custom metadata fields to devices for organization and filtering.

**Why this priority**: Essential for adding new categorization dimensions. Without this, the tag system cannot be extended to meet new organizational needs.

**Independent Test**: Can be tested by clicking "Add Tag," entering name and label, setting required and searchable flags, and successfully creating a tag. Delivers value by enabling tag taxonomy expansion.

**Acceptance Scenarios**:

1. **Given** I have device-tag:write permission, **When** I click the "Add Tag" button, **Then** a new editable row appears in the table
2. **Given** I am creating a new tag, **When** I enter a tag name, **Then** the name must be alphanumeric only (regex validation: `^[a-zA-Z0-9]*$`)
3. **Given** I am creating a new tag, **When** I enter a tag label, **Then** the label is required and can contain any text
4. **Given** I am creating a new tag, **When** I check the "Required" checkbox, **Then** the tag will be mandatory during device creation
5. **Given** I am creating a new tag, **When** I check the "Searchable" checkbox, **Then** the tag will appear as a filter option in the device list
6. **Given** I have filled in valid tag information, **When** I click Save on the row, **Then** the tag is created in the database
7. **Given** I attempt to create a tag with a duplicate name, **When** I save, **Then** I see a validation error preventing the duplicate
8. **Given** I attempt to create a tag with an invalid name (contains special characters or spaces), **When** I save, **Then** I see a validation error
9. **Given** I create a required tag, **When** users later create devices, **Then** they must provide a value for this tag

---

### User Story 3 - Update Existing Device Tag Definitions (Priority: P2)

As an IoT administrator, I need to update device tag properties (label, required, searchable) so that I can refine tag behavior as organizational needs evolve.

**Why this priority**: Important for maintaining accurate tag configurations but not required for initial setup. Enables tag refinement without recreating tags.

**Independent Test**: Can be tested by selecting an existing tag, modifying its label or flags, saving changes, and verifying updates are persisted. Delivers value by allowing tag evolution.

**Acceptance Scenarios**:

1. **Given** I have device-tag:write permission and am viewing the tags list, **When** I click Edit on a tag row, **Then** the label, required, and searchable fields become editable
2. **Given** I am editing a tag, **When** I change the label, **Then** the new label is validated as required
3. **Given** I am editing a tag, **When** I toggle the Required flag, **Then** future device creation will respect the updated requirement
4. **Given** I am editing a tag, **When** I toggle the Searchable flag, **Then** the tag will appear or disappear from device list filters accordingly
5. **Given** I am editing a tag, **When** the tag name field is displayed, **Then** it is read-only (names cannot be changed after creation)
6. **Given** I save tag updates, **When** the save succeeds, **Then** the row returns to read-only mode with updated values
7. **Given** I edit a tag and save invalid data, **When** validation fails, **Then** I see error messages and the row remains in edit mode

---

### User Story 4 - Delete Device Tag Definitions (Priority: P2)

As an IoT administrator, I need to delete device tag definitions that are no longer needed so that I can maintain a clean tag taxonomy.

**Why this priority**: Important for housekeeping but not critical for operations. Most valuable when the tag system has evolved and obsolete tags need removal.

**Independent Test**: Can be tested by selecting a tag not in use by devices, clicking delete, confirming the action, and verifying removal. Delivers value through tag library maintenance.

**Acceptance Scenarios**:

1. **Given** I have device-tag:write permission, **When** I click the Delete button on a tag row, **Then** I see a confirmation dialog
2. **Given** I see the delete confirmation dialog, **When** I confirm deletion, **Then** the tag definition is removed from the database
3. **Given** I delete a tag, **When** devices have values for this tag, **Then** the existing tag values on devices are also deleted
4. **Given** I delete a required tag, **When** users later create devices, **Then** they no longer need to provide a value for this tag
5. **Given** I delete a searchable tag, **When** users view device filters, **Then** the tag no longer appears in filter options
6. **Given** I delete a tag, **When** the deletion completes, **Then** the tag is removed from the table immediately

---

### User Story 5 - Configure Required Tags for Device Creation (Priority: P2)

As an IoT administrator, I need to mark tags as required so that device creation enforces consistent metadata across the device fleet.

**Why this priority**: Important for data quality and consistency but not essential for basic operations. Most valuable in regulated environments requiring complete device metadata.

**Independent Test**: Can be tested by marking a tag as required, attempting to create a device without that tag, and verifying validation prevents submission. Delivers value through enforced metadata standards.

**Acceptance Scenarios**:

1. **Given** I have configured a tag as required, **When** a user creates a new device, **Then** the tag appears in the device creation form with a required field indicator
2. **Given** a required tag exists, **When** a user attempts to create a device without providing that tag value, **Then** validation prevents submission with an error message
3. **Given** I change a tag from required to optional, **When** users create devices, **Then** the tag becomes optional in the form
4. **Given** multiple required tags exist, **When** a user creates a device, **Then** all required tags must have values before submission succeeds

---

### User Story 6 - Configure Searchable Tags for Device Filtering (Priority: P2)

As an IoT administrator, I need to mark tags as searchable so that users can filter the device list by specific tag values for quick device discovery.

**Why this priority**: Important for usability and device discoverability but not essential for basic operations. Most valuable for large device fleets requiring advanced filtering.

**Independent Test**: Can be tested by marking a tag as searchable, navigating to the device list, and verifying the tag appears as a filter option. Delivers value through enhanced device search capabilities.

**Acceptance Scenarios**:

1. **Given** I have configured a tag as searchable, **When** a user views the device list page, **Then** the tag appears in the search/filter panel
2. **Given** a searchable tag exists, **When** a user selects a tag value in the filter, **Then** the device list filters to show only devices with that tag value
3. **Given** I change a tag from searchable to non-searchable, **When** users view the device list, **Then** the tag is removed from the filter panel
4. **Given** multiple searchable tags exist, **When** a user applies multiple tag filters, **Then** devices must match all selected tag values (AND logic)

---

### Edge Cases

- **What happens when deleting a tag that is currently in use by many devices?**  
  The system allows deletion and removes all associated tag values from devices. Users should see a confirmation warning about the number of affected devices.

- **How does the system handle duplicate tag names entered in different case (e.g., "Location" vs "location")?**  
  The alphanumeric regex validator is case-sensitive, so "Location" and "location" are treated as different tags. Organizations should establish naming conventions.

- **What happens when a tag is marked as required after devices already exist without that tag?**  
  Existing devices are not affected (no retroactive validation). Only new devices and device updates will require the tag value.

- **How does the system behave when a user edits a tag and another user simultaneously deletes it?**  
  The edit or delete operation may fail with a concurrency error. The UI should refresh to show current state.

- **What happens to device twin tags in Azure IoT Hub when a tag is deleted from the portal?**  
  The portal removes device tag values from its database, but the corresponding tags in Azure IoT Hub device twins may remain unless explicitly synchronized.

- **How does the system handle very long tag names or labels?**  
  The system should enforce reasonable length limits (e.g., 50 characters for names, 100 for labels) to prevent UI display issues.

- **What happens when searching devices by a tag that has been deleted?**  
  The tag no longer appears in filters. Any existing search links or bookmarks using that tag would fail or return empty results.

- **How does the system handle special characters in tag labels (e.g., emojis, non-Latin characters)?**  
  Labels accept any text, so special characters are allowed. The UI should display them correctly with proper encoding.

- **What happens when a searchable tag has thousands of unique values across devices?**  
  The filter UI may become unwieldy. The system should consider implementing autocomplete or limiting displayed values for usability.

- **How does validation behave when a user adds multiple new tags and one fails validation?**  
  Each tag row is independently validated. Failed validations prevent that specific row from saving, but other rows can save successfully.

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST provide a list of all device tag definitions showing name, label, required flag, and searchable flag
- **FR-002**: System MUST enable creation of new device tags with fields: name (alphanumeric only), label (required), required flag, and searchable flag
- **FR-003**: System MUST validate tag names match the regex pattern: `^[a-zA-Z0-9]*$` (alphanumeric only, no spaces or special characters)
- **FR-004**: System MUST validate tag labels are provided (required field)
- **FR-005**: System MUST prevent creation of duplicate tag names
- **FR-006**: System MUST allow updating of tag label, required flag, and searchable flag for existing tags
- **FR-007**: System MUST prevent editing of tag names after creation (name is immutable)
- **FR-008**: System MUST allow deletion of device tag definitions with confirmation
- **FR-009**: System MUST delete all associated device tag values when a tag definition is deleted
- **FR-010**: System MUST expose required tags in device creation and editing forms
- **FR-011**: System MUST enforce validation requiring values for required tags during device creation
- **FR-012**: System MUST display searchable tags in device list filter panels
- **FR-013**: System MUST filter device lists by searchable tag values when filters are applied
- **FR-014**: System MUST support multiple searchable tag filters with AND logic (devices must match all selected tags)
- **FR-015**: System MUST persist device tag definitions in the portal database
- **FR-016**: System MUST persist device-specific tag values in the portal database associated with device records
- **FR-017**: System MUST provide API endpoints for retrieving all tags, all tag names, and all searchable tag names
- **FR-018**: System MUST enforce authorization policies: device-tag:read for viewing, device-tag:write for modifications
- **FR-019**: System MUST support inline editing of tags in the UI table with row-level save/cancel
- **FR-020**: System MUST refresh the tag list after create, update, or delete operations

### Key Entities

- **DeviceTag**: Tag definition entity with properties: ID (name), Label (display name), Required (boolean flag), and Searchable (boolean flag)
- **DeviceTagValue**: Device-specific tag value entity with properties: ID, Name (tag name), and Value (tag value for specific device)
- **DeviceTagDto**: Data transfer object for tag definitions with validation annotations including name regex and required label

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Users can view the complete list of device tags within 1 second
- **SC-002**: Users can create a new device tag in under 30 seconds including validation
- **SC-003**: Tag updates save within 2 seconds with immediate UI feedback
- **SC-004**: Tag deletions complete within 3 seconds including cleanup of associated device tag values
- **SC-005**: 100% of invalid tag names (with special characters or spaces) are rejected during creation
- **SC-006**: Required tags enforce 100% compliance during device creation (no devices created without required tag values)
- **SC-007**: Searchable tags appear in device list filters within 1 second of being marked searchable
- **SC-008**: Device list filtering by searchable tags returns results in under 2 seconds for fleets up to 10,000 devices
- **SC-009**: Users can configure a complete tag taxonomy (10+ tags) in under 10 minutes
- **SC-010**: 98% of tag operations succeed without errors under normal database availability

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/004-device-tag-settings-management/analyze.md`
- **Analyzed By**: excavator.analyze
- **Analysis Date**: 2026-01-30

### Code References

**Controllers**:
- `src/IoTHub.Portal.Server/Controllers/v1.0/DeviceTagSettingsController.cs` (Lines 1-89)

**Business Logic**:
- `src/IoTHub.Portal.Application/Services/IDeviceTagService.cs` (Lines 1-20)
- `src/IoTHub.Portal.Infrastructure/Services/DeviceTagService.cs` (Lines 1-114)

**Data Access**:
- `src/IoTHub.Portal.Domain/Repositories/IDeviceTagRepository.cs` (Lines 1-9)
- `src/IoTHub.Portal.Domain/Repositories/IDeviceTagValueRepository.cs` (Lines 1-9)
- `src/IoTHub.Portal.Domain/Entities/DeviceTag.cs` (Lines 1-16)
- `src/IoTHub.Portal.Domain/Entities/DeviceTagValue.cs` (Lines 1-12)

**UI Components**:
- `src/IoTHub.Portal.Client/Pages/Settings/DeviceTagsPage.razor` (Lines 1-188)

### Dependencies

**Depends On**:
- None - This is a foundational configuration feature

**Depended On By**:
- Standard Device Management (001) - Uses tags for device metadata and filtering
- Edge Device Management (006) - Uses tags for edge device metadata and filtering
- LoRaWAN Device Management (008) - Uses tags for LoRaWAN device metadata
- Device Configurations Management (005) - Uses tags for targeting device configurations
