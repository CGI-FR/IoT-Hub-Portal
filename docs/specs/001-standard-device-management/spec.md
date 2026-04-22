# Feature Specification: Standard Device Management

**Feature ID**: 001  
**Feature Branch**: `001-standard-device-management`  
**Created**: 2026-01-30  
**Status**: Draft  
**Source**: Analysis from `specs/001-standard-device-management/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - View and Search Device Inventory (Priority: P1)

As an IoT administrator, I need to view a comprehensive list of all devices in the system with search and filtering capabilities so that I can quickly locate specific devices for monitoring or troubleshooting.

**Why this priority**: Core functionality for device visibility and management. Without this, users cannot access or manage their device fleet.

**Independent Test**: Can be fully tested by navigating to the device list page, performing searches by device name/ID, and applying filters by status, state, model, tags, or labels. Delivers immediate value by providing device visibility.

**Acceptance Scenarios**:

1. **Given** I am an authorized user with device:read permission, **When** I navigate to the device list page, **Then** I see a paginated list of devices showing device ID, name, model, connection status, enabled state, and labels
2. **Given** I am viewing the device list, **When** I enter a device name or ID in the search field, **Then** the list filters to show only matching devices
3. **Given** I am viewing the device list, **When** I select a device model filter, **Then** only devices of that model are displayed
4. **Given** I am viewing the device list, **When** I select multiple labels, **Then** only devices with those labels are displayed
5. **Given** I am viewing the device list, **When** I select connection state filter (connected/disconnected), **Then** only devices in that state are displayed
6. **Given** I am viewing the device list, **When** I select status filter (enabled/disabled), **Then** only devices with that status are displayed
7. **Given** I am viewing the device list with more than 10 devices, **When** I navigate to the next page, **Then** I see the next set of results with consistent filtering applied
8. **Given** I am viewing the device list, **When** I click on a column header, **Then** the list sorts by that column in ascending or descending order
9. **Given** I am a user without device:read permission, **When** I attempt to access the device list page, **Then** I am denied access

---

### User Story 2 - View Device Details and Properties (Priority: P1)

As an IoT administrator, I need to view detailed information about a specific device including its configuration, properties, and connection credentials so that I can understand device status and configuration.

**Why this priority**: Essential for device troubleshooting and configuration verification. Users need to access device details to diagnose issues and verify settings.

**Independent Test**: Can be tested by selecting a device from the list and viewing its detail page with tabs for general information, properties, tags, and labels. Delivers value by providing complete device context.

**Acceptance Scenarios**:

1. **Given** I have device:read permission, **When** I click on a device in the list, **Then** I see the device detail page showing device ID, name, model, image, connection status, enabled state, last activity time, and status update time
2. **Given** I am viewing a device detail page, **When** I navigate to the Properties tab, **Then** I see all device properties with their current values retrieved from the device twin
3. **Given** I am viewing device properties, **When** a property is marked as writable, **Then** I see it as an editable field
4. **Given** I am viewing device properties, **When** a property is marked as read-only, **Then** I see it as a display-only field
5. **Given** I am viewing a device detail page, **When** I click to view credentials, **Then** I see the device enrollment credentials required for connection
6. **Given** I am viewing a device detail page, **When** the device has tags, **Then** I see all tag names and values
7. **Given** I am viewing a device detail page, **When** the device has labels, **Then** I see all assigned labels
8. **Given** I am viewing a device detail page, **When** the device has a layer assignment, **Then** I see the layer information

---

### User Story 3 - Create New Devices (Priority: P1)

As an IoT administrator, I need to create new devices in the system by selecting a device model and providing required information so that new devices can be registered and managed.

**Why this priority**: Critical for device onboarding. Without this capability, no new devices can be added to the system.

**Independent Test**: Can be tested by clicking "Create Device," selecting a model, filling in required fields, and successfully creating a device. Delivers value by enabling device fleet expansion.

**Acceptance Scenarios**:

1. **Given** I have device:write permission, **When** I click the "Create Device" button, **Then** I see a device creation form
2. **Given** I am on the device creation form, **When** I select a device model, **Then** the form displays all properties defined by that model with appropriate input controls
3. **Given** I am filling out the device creation form, **When** I provide a device ID, device name, and select a model, **Then** the form validates the required fields
4. **Given** I am filling out the device creation form, **When** I enter an invalid device ID format (not alphanumeric, exceeds 128 characters), **Then** I see a validation error
5. **Given** I have completed the device creation form with valid data, **When** I click Save, **Then** the device is created in both the portal database and the cloud IoT service (Azure IoT Hub or AWS IoT Core)
6. **Given** I have created a device, **When** I return to the device list, **Then** I see the newly created device in the list
7. **Given** I am creating a device, **When** I set writable properties, **Then** those properties are written to the device twin desired properties
8. **Given** I am creating a device, **When** I assign tags, **Then** those tags are saved with the device and synchronized to the IoT service
9. **Given** I am creating a device, **When** I assign labels, **Then** those labels are associated with the device
10. **Given** the device creation fails in the cloud IoT service, **When** I attempt to save, **Then** I see an error message and the device is not created in the portal database

---

### User Story 4 - Update Device Configuration (Priority: P2)

As an IoT administrator, I need to update device properties, tags, and labels so that I can modify device configuration and categorization as requirements change.

**Why this priority**: Important for ongoing device management but not required for initial deployment. Enables configuration changes without recreating devices.

**Independent Test**: Can be tested by opening an existing device, modifying properties or tags, saving changes, and verifying updates in both the portal and device twin. Delivers value by enabling device reconfiguration.

**Acceptance Scenarios**:

1. **Given** I have device:write permission and am viewing a device, **When** I click Edit, **Then** I can modify device name, enabled state, properties, tags, and labels
2. **Given** I am editing a device, **When** I change a writable property value, **Then** the new value is validated according to the property type (integer, double, boolean, string, etc.)
3. **Given** I am editing a device, **When** I save changes with valid data, **Then** the device is updated in both the portal database and the device twin in the cloud IoT service
4. **Given** I am editing device properties, **When** I update a desired property, **Then** the change is written to the device twin and eventually synchronized to the physical device
5. **Given** I am editing a device, **When** I change tag values, **Then** the tags are updated in the portal and synchronized to the IoT service
6. **Given** I am editing a device, **When** I add or remove labels, **Then** the label associations are updated in the database
7. **Given** I am editing a device, **When** I change the device model, **Then** the property schema updates to match the new model
8. **Given** the device update fails in the cloud IoT service, **When** I attempt to save, **Then** I see an error message and changes are not persisted

---

### User Story 5 - Delete Devices (Priority: P2)

As an IoT administrator, I need to delete devices from the system so that decommissioned or incorrectly configured devices can be removed.

**Why this priority**: Important for device lifecycle management but not required for initial setup. Enables cleanup and decommissioning.

**Independent Test**: Can be tested by selecting a device, clicking delete, confirming the action, and verifying removal from both the portal and cloud IoT service. Delivers value by enabling fleet cleanup.

**Acceptance Scenarios**:

1. **Given** I have device:write permission, **When** I click the delete button on a device in the list, **Then** I see a confirmation dialog warning about irreversibility
2. **Given** I see the delete confirmation dialog, **When** I confirm deletion, **Then** the device is removed from both the portal database and the cloud IoT service (Azure IoT Hub or AWS IoT Core)
3. **Given** I have deleted a device, **When** I return to the device list, **Then** the device no longer appears
4. **Given** I attempt to delete a device, **When** the deletion fails in the cloud IoT service, **Then** I see an error message and the device remains in the portal
5. **Given** I attempt to delete a device, **When** I am not authorized (missing device:write permission), **Then** I do not see the delete button
6. **Given** I delete a device, **When** the device has associated tags, labels, and properties, **Then** all related data is properly cleaned up

---

### User Story 6 - Import and Export Device Lists (Priority: P3)

As an IoT administrator, I need to import devices from CSV/Excel files and export device lists so that I can manage devices in bulk and integrate with external systems.

**Why this priority**: Enhances operational efficiency for large-scale deployments but not required for basic operations. Most valuable for users managing hundreds or thousands of devices.

**Independent Test**: Can be tested by downloading a template file, filling it with device data, importing it, and verifying devices are created. Also by exporting the current device list and verifying the file contains correct data. Delivers value through bulk operations.

**Acceptance Scenarios**:

1. **Given** I have device:import permission, **When** I click the "Import Devices" button, **Then** I can download a template file and upload a populated CSV/Excel file
2. **Given** I upload a valid device import file, **When** the import processes, **Then** all devices in the file are created in the system with a progress indicator
3. **Given** I upload a device import file with errors, **When** the import processes, **Then** I see a report of errors with line numbers and reasons
4. **Given** I have device:export permission, **When** I click the "Export Devices" button, **Then** I receive a file containing all devices matching current filters
5. **Given** I export devices, **When** I open the export file, **Then** I see device ID, name, model, status, connection state, tags, labels, and layer information
6. **Given** I export devices with filters applied, **When** the export completes, **Then** only devices matching the filters are included in the export

---

### User Story 7 - Manage Device Labels (Priority: P3)

As an IoT administrator, I need to assign labels to devices for categorization and filtering so that I can organize devices logically across different dimensions (location, department, environment, etc.).

**Why this priority**: Useful for organization and filtering but not essential for basic device operations. Most valuable for large deployments requiring multi-dimensional categorization.

**Independent Test**: Can be tested by creating or selecting labels, assigning them to devices, and using label filters in the device list. Delivers value through improved device organization.

**Acceptance Scenarios**:

1. **Given** I am creating or editing a device, **When** I access the labels section, **Then** I see a list of available labels to assign
2. **Given** I am managing device labels, **When** I select one or more labels, **Then** those labels are associated with the device
3. **Given** I am managing device labels, **When** I remove a label, **Then** the association is removed but the label remains available for other devices
4. **Given** I am on the device list page, **When** I filter by one or more labels, **Then** only devices with all selected labels are displayed
5. **Given** I am viewing the device list, **When** I look at a device row, **Then** I see all labels assigned to that device
6. **Given** I request available labels, **When** the system responds, **Then** I see only labels that are currently in use by at least one device

---

### Edge Cases

- **What happens when a device is created in the portal but fails to create in Azure IoT Hub or AWS IoT Core?**  
  The system should rollback the database transaction and display an error message to the user, preventing orphaned records.

- **How does the system handle concurrent updates to the same device?**  
  The system uses version tracking (Version property) to detect concurrent modifications. Last write wins, but users should see an error if the device was modified by another user.

- **What happens when searching with special characters or SQL injection attempts?**  
  Input is validated and parameterized queries are used to prevent SQL injection. Special characters in search terms are escaped properly.

- **How does the system behave when Azure IoT Hub or AWS IoT Core is unavailable?**  
  Device CRUD operations fail gracefully with error messages. The portal does not cache device twin data, so properties cannot be viewed or edited during cloud service outages.

- **What happens when filtering by a device model that is later deleted?**  
  The system prevents deletion of device models that are in use by devices. If somehow orphaned, the device list would show devices with invalid model references, requiring data cleanup.

- **How does the system handle devices with very long names or IDs?**  
  Device IDs are limited to 128 characters and validated. Device names have validation constraints. UI truncates long values with tooltips showing full text.

- **What happens when importing a file with duplicate device IDs?**  
  The import process validates for duplicates and reports errors. Existing devices are not overwritten unless explicitly configured.

- **How does pagination behave when devices are deleted while browsing?**  
  Server-side pagination may result in page shifts. Users may see inconsistent results if devices are added/removed during pagination. Refresh is recommended.

- **What happens when a device's connection state changes during viewing?**  
  The UI shows the state at the time of page load. Real-time updates require manual refresh or future SignalR integration.

- **How does the system handle device property values that don't match the expected type?**  
  Type validation occurs on the client and server. Invalid values are rejected with error messages. The device twin may contain non-conforming data from device-side updates.

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST provide a paginated list of all devices with device ID, name, model, image, connection status, enabled state, labels, and last activity time
- **FR-002**: System MUST support full-text search of devices by device ID or device name with case-insensitive matching
- **FR-003**: System MUST allow filtering devices by connection state (connected/disconnected)
- **FR-004**: System MUST allow filtering devices by status (enabled/disabled)
- **FR-005**: System MUST allow filtering devices by device model with single or multiple model selection
- **FR-006**: System MUST allow filtering devices by custom tags where tag names are defined in tag settings
- **FR-007**: System MUST allow filtering devices by labels with multiple label selection
- **FR-008**: System MUST support sorting device lists by multiple columns with ascending and descending order
- **FR-009**: System MUST enable creation of new devices with required fields: device ID, device name, and device model
- **FR-010**: System MUST validate device ID format (alphanumeric with special characters, maximum 128 characters)
- **FR-011**: System MUST create devices in both the portal database and the cloud IoT service (Azure IoT Hub or AWS IoT Core) atomically
- **FR-012**: System MUST retrieve device details including device twin properties from the cloud IoT service
- **FR-013**: System MUST display device properties with distinction between writable (desired) and read-only (reported) properties
- **FR-014**: System MUST allow updating device name, enabled state, properties, tags, and labels for existing devices
- **FR-015**: System MUST synchronize device updates to the cloud IoT service device twin
- **FR-016**: System MUST allow deletion of devices with confirmation dialogs
- **FR-017**: System MUST delete devices from both the portal database and the cloud IoT service
- **FR-018**: System MUST provide device enrollment credentials for device connection configuration
- **FR-019**: System MUST support import of devices from CSV/Excel files with validation and error reporting
- **FR-020**: System MUST support export of device lists to CSV/Excel files with current filters applied
- **FR-021**: System MUST provide a template file for device import with correct column structure
- **FR-022**: System MUST allow assignment and removal of labels to devices for categorization
- **FR-023**: System MUST retrieve available labels that are currently in use by at least one device
- **FR-024**: System MUST display device images from the associated device model
- **FR-025**: System MUST enforce authorization policies: device:read for viewing, device:write for modifications, device:import for imports, device:export for exports
- **FR-026**: System MUST handle both Azure IoT Hub and AWS IoT Core as cloud providers based on configuration
- **FR-027**: System MUST provide pagination controls with configurable page size (default 10 items)
- **FR-028**: System MUST persist device tags in both the portal database and cloud IoT service tags

### Key Entities

- **Device**: Represents an IoT device with properties including ID, name, model reference, connection status, enabled state, status update time, last activity time, version, tags, labels, and optional layer assignment
- **DeviceDetails**: Complete device information DTO including all properties, tags, labels, and model reference used for API communication
- **DeviceListItem**: Lightweight device summary for list display including essential fields and image URL
- **DevicePropertyValue**: Property name, display name, type, value, writability, and order used for device twin property management
- **DeviceCredentials**: Enrollment credentials including authentication method and keys/certificates for device connection
- **Label**: Categorization label that can be assigned to devices and device models
- **DeviceTagValue**: Custom metadata key-value pair associated with a device
- **DeviceModel**: Reference to the device model template defining properties and behavior

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Users can view a complete list of devices within 3 seconds for fleets of up to 1,000 devices with pagination
- **SC-002**: Users can search and filter device lists with results appearing in under 2 seconds
- **SC-003**: Device creation completes in under 10 seconds including cloud IoT service provisioning
- **SC-004**: Device property updates synchronize to device twins within 5 seconds under normal conditions
- **SC-005**: Users can import 100 devices in under 60 seconds with validation and error reporting
- **SC-006**: 95% of device searches return relevant results on the first attempt without refinement
- **SC-007**: Device deletion completes in under 5 seconds including cloud IoT service cleanup
- **SC-008**: System handles concurrent access by 50 users viewing and modifying different devices without degradation
- **SC-009**: 98% of device CRUD operations succeed without errors under normal cloud service availability
- **SC-010**: Users with appropriate permissions can complete device onboarding (create device with properties) in under 3 minutes

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/001-standard-device-management/analyze.md`
- **Analyzed By**: excavator.analyze
- **Analysis Date**: 2026-01-30

### Code References

**Controllers**:
- `src/IoTHub.Portal.Server/Controllers/v1.0/DevicesController.cs` (Lines 1-137)
- `src/IoTHub.Portal.Server/Controllers/v1.0/DevicesControllerBase.cs` (Lines 1-185)

**Business Logic**:
- `src/IoTHub.Portal.Application/Services/IDeviceService.cs` (Lines 1-39)
- `src/IoTHub.Portal.Infrastructure/Services/DeviceService.cs` (Lines 1-134)
- `src/IoTHub.Portal.Infrastructure/Services/DeviceServiceBase.cs` (Lines 1-220+)
- `src/IoTHub.Portal.Server/Services/DevicePropertyService.cs` (Lines 1-129)

**Data Access**:
- `src/IoTHub.Portal.Domain/Repositories/IDeviceRepository.cs` (Lines 1-9)
- `src/IoTHub.Portal.Domain/Entities/Device.cs` (Lines 1-63)

**UI Components**:
- `src/IoTHub.Portal.Client/Pages/Devices/DeviceListPage.razor` (Lines 1-480)
- `src/IoTHub.Portal.Client/Pages/Devices/DeviceDetailPage.razor` (Lines 1-41)
- `src/IoTHub.Portal.Client/Pages/Devices/CreateDevicePage.razor` (Lines 1-13)
- `src/IoTHub.Portal.Client/Components/Devices/EditDevice.razor` (Lines 1-150+)

### Dependencies

**Depends On**:
- Device Model Management (002) - Devices must be associated with a model; model defines properties and behavior
- Device Tag Settings Management (004) - Custom searchable metadata fields for devices
- Layer Management (019) - Optional hierarchical organization of devices
- Azure IoT Hub / AWS IoT Core - External cloud IoT device management service

**Depended On By**:
- Edge Device Management (006) - Shares base functionality through DevicesControllerBase
- LoRaWAN Device Management (008) - Shares base functionality through DevicesControllerBase
- Device Import Export (021) - Bulk operations depend on device CRUD functionality
- Dashboard Metrics (020) - Device counts and status aggregation
