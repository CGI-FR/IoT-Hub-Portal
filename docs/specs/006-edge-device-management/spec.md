# Feature Specification: Edge Device Management

**Feature ID**: 006  
**Feature Branch**: `006-edge-device-management`  
**Created**: January 30, 2025  
**Status**: Draft  
**Source**: Analysis from `specs/006-edge-device-management/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - Create and Provision Edge Gateway Device (Priority: P1)

An IoT administrator needs to register a new edge gateway device that will act as a transparent gateway for downstream IoT devices, enabling local data processing and offline capabilities at remote locations.

**Why this priority**: Core foundational capability required for any edge deployment. Without the ability to create edge devices, no other edge functionality can be utilized. This represents the entry point for edge device lifecycle management.

**Independent Test**: Can be fully tested by creating a new edge device through the portal, verifying cloud provider registration, and confirming the device appears in the device list with correct metadata.

**Acceptance Scenarios**:

1. **Given** an authenticated administrator with edge device write permission, **When** they select a device model, provide a device name, assign required tags, and save the edge device, **Then** the system creates the device in both the cloud provider and local database, generates authentication credentials, and displays the new device in the device list.

2. **Given** a user attempting to create an edge device without required tags, **When** they attempt to save, **Then** the system displays validation errors indicating which required fields are missing and prevents device creation until corrected.

3. **Given** a user creating an edge device on Azure IoT Hub, **When** they complete the device creation process, **Then** the system provisions the device with symmetric key authentication and enables the device by default.

4. **Given** a user creating an edge device on AWS IoT Greengrass, **When** they complete the device creation process, **Then** the system registers a Greengrass core device with certificate-based authentication configuration.

---

### User Story 2 - Monitor Edge Device Health and Connectivity (Priority: P1)

Operations teams need to monitor the health status, connectivity state, and runtime status of deployed edge devices to ensure gateway operations remain functional and downstream devices maintain cloud connectivity.

**Why this priority**: Critical for operational reliability. Edge devices are often deployed in remote locations where direct physical access is limited. Real-time health monitoring enables proactive issue detection and resolution before downstream device connectivity is impacted.

**Independent Test**: Can be tested by viewing the device list with connection status indicators, filtering by connection state, and viewing detailed device information showing runtime health status.

**Acceptance Scenarios**:

1. **Given** multiple edge devices with varying connection states, **When** an operator views the edge device list, **Then** the system displays each device with visual connection status indicators (connected/disconnected) and the count of connected downstream devices.

2. **Given** an operator filtering the device list, **When** they select "Connected" status filter, **Then** the system displays only devices currently connected to the cloud platform.

3. **Given** an operator viewing device details, **When** the device page loads, **Then** the system displays runtime health status, count of deployed modules, count of connected leaf devices, and last deployment information.

4. **Given** an edge device that becomes disconnected, **When** an operator refreshes the device list, **Then** the system reflects the updated connection status with appropriate visual indicators.

---

### User Story 3 - Search and Filter Edge Devices (Priority: P1)

System administrators managing large edge device deployments need to quickly locate specific devices or groups of devices based on name, model, connection status, or organizational labels.

**Why this priority**: Essential for scalability. As edge deployments grow from dozens to hundreds or thousands of devices, the ability to efficiently search and filter becomes critical for operational efficiency and reduces time to locate devices requiring attention.

**Independent Test**: Can be tested by applying various search terms and filter combinations in the device list, verifying server-side filtering returns correct results, and confirming pagination works with filtered data.

**Acceptance Scenarios**:

1. **Given** a deployment with hundreds of edge devices, **When** an administrator enters a partial device name or ID in the search field, **Then** the system returns only devices matching the search text with pagination.

2. **Given** an administrator needing to find all edge devices of a specific model, **When** they select a model from the filter dropdown, **Then** the system displays only devices using that device model.

3. **Given** devices organized with labels for departments or locations, **When** an administrator selects multiple labels from the filter, **Then** the system displays devices having any of the selected labels.

4. **Given** multiple filters applied simultaneously, **When** an administrator combines search text, status filter, model filter, and label filters, **Then** the system applies all filters with AND logic and returns matching devices.

5. **Given** a large filtered result set, **When** an administrator navigates through pages, **Then** the system maintains filter state across page navigation and displays accurate page counts.

---

### User Story 4 - Update Edge Device Configuration (Priority: P2)

Device administrators need to update edge device metadata, tags, and labels after initial provisioning to reflect organizational changes, update device categorization, or correct device information.

**Why this priority**: Important for maintaining accurate device inventory as organizational needs evolve. While not as critical as initial provisioning, the ability to update device configuration prevents the need to delete and recreate devices when changes are required.

**Independent Test**: Can be tested by modifying device name, tags, and labels on the device details page, saving changes, and verifying updates are reflected in both the portal and cloud provider.

**Acceptance Scenarios**:

1. **Given** an edge device with existing configuration, **When** an administrator changes the device name and saves, **Then** the system updates the device name in both the local database and cloud provider device twin.

2. **Given** an administrator updating device tags, **When** they modify tag values and save, **Then** the system synchronizes the updated tags to the cloud provider and validates required tags are present.

3. **Given** an administrator managing device labels, **When** they add or remove labels and save, **Then** the system updates the device's label associations and the device appears in appropriate filtered lists.

4. **Given** an administrator attempting to save with missing required tags, **When** they attempt to save, **Then** the system displays validation errors and prevents the update until required tags are provided.

---

### User Story 5 - Retrieve Device Enrollment Credentials (Priority: P2)

Field technicians provisioning physical edge devices need to retrieve device authentication credentials and enrollment scripts to configure the IoT Edge runtime on the device hardware.

**Why this priority**: Critical for device deployment workflow but occurs after initial portal registration. Enables physical device commissioning and connects hardware to the cloud identity created in the portal.

**Independent Test**: Can be tested by selecting an edge device, requesting enrollment credentials, verifying symmetric keys are displayed, and generating time-limited enrollment script URLs.

**Acceptance Scenarios**:

1. **Given** a registered edge device, **When** an administrator views device details and requests credentials, **Then** the system displays the device's primary and secondary symmetric keys for manual configuration.

2. **Given** an administrator preparing automated device provisioning, **When** they request an enrollment script for a specific template (bash/PowerShell), **Then** the system generates a secure time-limited URL valid for 15 minutes.

3. **Given** a field technician with the enrollment script URL, **When** they download the script within the valid time window, **Then** the system returns a platform-specific script containing the device connection string and IoT Edge runtime configuration commands.

4. **Given** an enrollment script URL that has expired, **When** someone attempts to download the script, **Then** the system returns an error indicating the URL has expired and requires regeneration.

---

### User Story 6 - Execute Commands on Edge Modules (Priority: P2)

Operations teams need to remotely execute commands on edge modules (such as restarting modules or triggering custom business logic) without physical access to edge devices, enabling remote troubleshooting and management.

**Why this priority**: Important for operational efficiency and reduces mean time to resolution for edge device issues. Enables remote management capabilities that would otherwise require costly site visits or device reboots.

**Independent Test**: Can be tested on Azure IoT Hub deployments by selecting a connected edge device with running modules, executing a module restart command, and verifying command execution results.

**Acceptance Scenarios**:

1. **Given** a connected edge device on Azure IoT Hub with running modules, **When** an operator executes the RestartModule command on a specific module, **Then** the system invokes the direct method and returns the command execution status and response payload.

2. **Given** a module with custom commands defined in the device model, **When** an operator executes a custom command, **Then** the system invokes the command and displays the execution result.

3. **Given** a disconnected edge device, **When** an operator attempts to execute a module command, **Then** the system disables command buttons and prevents command execution.

4. **Given** a user without edge-device:execute permission, **When** they view device details, **Then** the system hides or disables module command buttons.

5. **Given** an edge device on AWS IoT Greengrass, **When** an operator views device details, **Then** the system hides module command features as they are not supported on AWS.

---

### User Story 7 - View Edge Module Logs (Priority: P3)

Support engineers troubleshooting edge device issues need to retrieve real-time logs from edge modules to diagnose runtime errors, connectivity problems, or business logic failures.

**Why this priority**: Valuable for troubleshooting but not required for core operational workflows. Provides diagnostic capabilities that enhance supportability but can be supplemented with alternative logging approaches.

**Independent Test**: Can be tested on Azure IoT Hub deployments by selecting a running module on a connected edge device, requesting logs, and verifying recent log entries are displayed in a dialog.

**Acceptance Scenarios**:

1. **Given** a connected edge device on Azure IoT Hub with running modules, **When** a support engineer requests logs for a specific module, **Then** the system retrieves and displays recent log entries from the module via IoT Hub APIs.

2. **Given** log entries retrieved from a module, **When** displayed to the user, **Then** the system presents logs in a readable dialog format with timestamp and log level information.

3. **Given** an edge device on AWS IoT Greengrass, **When** a user views device details, **Then** the system does not display log retrieval options as the feature is not supported on AWS.

---

### User Story 8 - Delete Edge Devices (Priority: P3)

System administrators decommissioning edge devices need to remove devices from both the portal and cloud provider when devices are retired, replaced, or no longer in service.

**Why this priority**: Important for maintaining clean device inventory but occurs infrequently compared to create/read/update operations. Proper cleanup prevents billing for unused devices and maintains accurate inventory counts.

**Independent Test**: Can be tested by selecting an edge device, confirming deletion, and verifying the device is removed from both the portal database and cloud provider.

**Acceptance Scenarios**:

1. **Given** an edge device in the portal, **When** an administrator with write permission deletes the device and confirms the action, **Then** the system removes the device from the cloud provider and deletes the database entry with cascade deletion of associated tags and labels.

2. **Given** an administrator without write permission, **When** they view the device list, **Then** the system does not display delete action buttons.

3. **Given** an edge device selected for deletion, **When** the administrator confirms deletion, **Then** the system displays a success notification and removes the device from the device list.

---

### User Story 9 - Duplicate Edge Device Configuration (Priority: P4)

Device administrators provisioning multiple similar edge devices need to duplicate an existing device's configuration (model, tags, labels) to accelerate deployment of standardized device configurations.

**Why this priority**: Nice-to-have efficiency feature that reduces repetitive data entry but is not required for core functionality. Provides time savings for specific deployment scenarios with similar device configurations.

**Independent Test**: Can be tested by viewing an existing edge device, selecting "Save and Duplicate", verifying the new device page is populated with copied configuration, and completing device creation with a new device ID.

**Acceptance Scenarios**:

1. **Given** an existing edge device with configured tags and labels, **When** an administrator selects "Save and Duplicate" from the device details page, **Then** the system navigates to the create device page with model, tags, and labels pre-populated from the source device.

2. **Given** a duplicated device configuration, **When** the administrator provides a new device ID and saves, **Then** the system creates a new independent edge device with the copied configuration.

---

### User Story 10 - View Gateway Connected Leaf Devices (Priority: P4)

Network architects need to understand which downstream devices are using each edge device as a gateway to assess gateway capacity utilization and plan for gateway scaling.

**Why this priority**: Useful for capacity planning and architecture optimization but not required for basic edge device operation. Provides visibility into gateway utilization patterns for advanced deployments.

**Independent Test**: Can be tested by viewing an edge device that has downstream devices connected, verifying the count of connected leaf devices is displayed in both the list view and detail view.

**Acceptance Scenarios**:

1. **Given** an edge device acting as a transparent gateway for downstream devices, **When** an administrator views the device list, **Then** the system displays the count of connected leaf devices for each edge device.

2. **Given** an edge device with connected downstream devices, **When** an administrator views device details, **Then** the system displays the current count of leaf devices using this edge device as their gateway.

---

### Edge Cases

- **Cloud Provider Synchronization Failure**: What happens when the edge device is created in the cloud provider but local database creation fails? System should attempt rollback of cloud provider device or log for manual cleanup.

- **Device Already Exists**: What happens when attempting to create an edge device with a device ID that already exists in the cloud provider? System should return validation error indicating device ID conflict.

- **Expired Enrollment URLs**: What happens when a technician attempts to download an enrollment script after the 15-minute validity period? System should return clear error message and provide ability to regenerate URL.

- **Module Command Timeout**: What happens when executing a module command that doesn't respond within timeout period? System should return timeout error and allow retry.

- **Disconnected Device Updates**: What happens when updating configuration on a disconnected edge device? System should allow updates to device twin/thing shadow which will sync when device reconnects.

- **Concurrent Device Updates**: What happens when multiple administrators update the same edge device simultaneously? System should use version tracking or optimistic concurrency to detect conflicts.

- **Label Filtering with No Results**: What happens when label filters are applied that match no devices? System should display empty state message indicating no devices match the selected filters.

- **Large Deployment Pagination**: What happens when an organization has thousands of edge devices? System should maintain responsive pagination and provide efficient search capabilities without loading all devices.

- **Module Logs Unavailable**: What happens when requesting logs from a module that has no recent log output? System should return empty result set with appropriate message.

- **Required Tag Value Changes**: What happens when a required tag's definition changes after devices are created? System should enforce new validation on updates while allowing existing devices to retain old values until edited.

- **Cloud Provider Deletion Failure**: What happens when local device deletion succeeds but cloud provider deletion fails? System should log error for manual remediation and potentially queue for retry.

- **Cross-Provider Device Migration**: What happens when attempting to migrate an edge device from Azure to AWS or vice versa? System does not support migration; devices must be recreated on the target platform.

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST allow administrators with edge-device:write permission to create new edge devices by selecting a device model, providing device name, and assigning required tags.

- **FR-002**: System MUST provision edge devices in the configured cloud provider (Azure IoT Hub or AWS IoT Greengrass) with appropriate authentication mechanisms (symmetric keys for Azure, certificates for AWS).

- **FR-003**: System MUST display paginated list of edge devices with server-side filtering by search text, connection status, device model, and organizational labels.

- **FR-004**: System MUST show real-time connection state (connected/disconnected) for each edge device with visual status indicators.

- **FR-005**: System MUST display count of connected downstream leaf devices and deployed modules for each edge device.

- **FR-006**: System MUST allow administrators with edge-device:write permission to update edge device name, tags, and labels with synchronization to cloud provider.

- **FR-007**: System MUST validate that all required tags are provided before allowing edge device creation or updates.

- **FR-008**: System MUST allow administrators with edge-device:write permission to delete edge devices with removal from both local database and cloud provider.

- **FR-009**: System MUST display detailed edge device information including runtime health status, module list, deployment information, and device metadata.

- **FR-010**: System MUST provide device enrollment credentials (symmetric keys or certificate information) for manual device configuration.

- **FR-011**: System MUST generate time-limited secure enrollment script URLs valid for 15 minutes using data protection encryption.

- **FR-012**: System MUST provide platform-specific enrollment scripts (bash/PowerShell templates) containing device connection strings and IoT Edge runtime configuration.

- **FR-013**: System MUST allow administrators with edge-device:execute permission to execute commands on edge modules (Azure IoT Hub only).

- **FR-014**: System MUST retrieve and display real-time logs from edge modules for troubleshooting (Azure IoT Hub only).

- **FR-015**: System MUST support multi-select label filtering with AND logic when combined with other filters.

- **FR-016**: System MUST display available labels for filtering based on labels currently assigned to edge devices or device models.

- **FR-017**: System MUST maintain device model associations with display of model images in device lists.

- **FR-018**: System MUST support device duplication to accelerate provisioning of devices with similar configurations.

- **FR-019**: System MUST disable module command execution when edge devices are disconnected.

- **FR-020**: System MUST enforce permission-based authorization for read, write, and execute operations on edge devices.

- **FR-021**: System MUST persist edge device entities with relationships to device models, tags, and labels.

- **FR-022**: System MUST synchronize edge device connection state and runtime status through periodic background jobs.

- **FR-023**: System MUST support sorting edge device lists by device ID, connection status, or number of connected devices.

- **FR-024**: System MUST provide distinct module management capabilities for Azure IoT Hub (commands and logs) versus AWS IoT Greengrass (limited functionality).

- **FR-025**: System MUST validate device IDs according to cloud provider requirements (Azure: case-sensitive alphanumeric; AWS: thing name conventions).

### Key Entities

- **Edge Device**: Represents an IoT Edge gateway device with properties including device ID, device name, connection state, runtime status, enabled status, device scope, count of connected leaf devices, count of deployed modules, and associations to device model, tags, and labels.

- **Edge Device Model**: Defines the capabilities, available modules, and visual representation for a category of edge devices. Each edge device references exactly one edge device model.

- **Device Tag Value**: Custom metadata key-value pairs associated with edge devices, synchronized to cloud provider device twin or thing shadow. Tags can be marked as required and are validated during device creation and updates.

- **Label**: Organizational categorization tags that can be assigned to edge devices or inherited from device models. Used for filtering and grouping devices. Stored locally and not synchronized to cloud provider.

- **Edge Module**: Containerized workload deployed to an edge device. Modules have runtime status, support command execution (Azure only), and provide log output (Azure only).

- **Enrollment Credentials**: Authentication credentials (symmetric keys for Azure, certificate paths for AWS) used to configure physical edge devices to authenticate with cloud provider.

- **Module Command**: Direct method that can be invoked on edge modules for remote management operations such as RestartModule or custom business logic commands.

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Administrators can create and provision a new edge device in under 3 minutes including model selection, name entry, and required tag assignment.

- **SC-002**: System displays paginated edge device lists with applied filters (search, status, model, labels) with page load times under 2 seconds for deployments with up to 10,000 edge devices.

- **SC-003**: Device connection status indicators reflect actual cloud provider connectivity state with accuracy of 95% or higher within 1 minute of state change.

- **SC-004**: Administrators can locate a specific edge device using search functionality in under 30 seconds in deployments with hundreds of devices.

- **SC-005**: Module command execution (restart, custom commands) completes successfully with response status in under 10 seconds for connected devices.

- **SC-006**: Enrollment script generation and download completes in under 5 seconds, enabling field technicians to quickly provision physical devices.

- **SC-007**: System maintains responsive user interface performance with pagination limiting result sets to configurable page sizes (default 10 items).

- **SC-008**: Device configuration updates (name, tags, labels) synchronize to cloud provider within 5 seconds and reflect in UI within 10 seconds.

- **SC-009**: Edge device deletion removes device from both portal and cloud provider with 100% success rate or provides clear error messages for manual remediation.

- **SC-010**: 90% of edge device troubleshooting scenarios can be diagnosed using portal features (status indicators, module logs, command execution) without requiring direct device access.

- **SC-011**: Background synchronization jobs update device connection states, module counts, and leaf device counts with maximum staleness of 5 minutes.

- **SC-012**: System prevents unauthorized edge device operations with 100% enforcement of permission-based authorization (read, write, execute).

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/006-edge-device-management/analyze.md`
- **Analyzed By**: excavator.specifier
- **Analysis Date**: January 30, 2025

### Code References

**Controllers**:
- `src/IoTHub.Portal.Server/Controllers/v1.0/EdgeDevicesController.cs` - REST API endpoints for edge device operations

**Business Logic**:
- `src/IoTHub.Portal.Application/Services/IEdgeDevicesService.cs` - Service interface for edge device operations
- `src/IoTHub.Portal.Infrastructure/Services/EdgeDevicesServiceBase.cs` - Shared base implementation for cloud providers
- `src/IoTHub.Portal.Server/Services/AzureEdgeDevicesService.cs` - Azure IoT Hub specific implementation
- `src/IoTHub.Portal.Infrastructure/Services/AWS/AWSEdgeDevicesService.cs` - AWS IoT Greengrass specific implementation

**Data Access**:
- `src/IoTHub.Portal.Domain/Repositories/IEdgeDeviceRepository.cs` - Repository interface
- `src/IoTHub.Portal.Domain/Entities/EdgeDevice.cs` - Edge device entity definition

**UI Components**:
- `src/IoTHub.Portal.Client/Pages/EdgeDevices/EdgeDeviceListPage.razor` - Device list page with search and filtering
- `src/IoTHub.Portal.Client/Pages/EdgeDevices/EdgeDeviceDetailPage.razor` - Device details and management page
- `src/IoTHub.Portal.Client/Pages/EdgeDevices/CreateEdgeDevicePage.razor` - Device creation wizard

**Data Transfer Objects**:
- `src/IoTHub.Portal.Shared/Models/v1.0/IoTEdgeDevice.cs` - Complete edge device DTO
- `src/IoTHub.Portal.Shared/Models/v1.0/IoTEdgeListItem.cs` - Lightweight list item DTO

**Client Services**:
- `src/IoTHub.Portal.Client/Services/IEdgeDeviceClientService.cs` - Client service interface
- `src/IoTHub.Portal.Client/Services/EdgeDeviceClientService.cs` - HTTP API client implementation

### Dependencies

**Depends On**:
- Edge Device Models - Defines device capabilities, modules, and commands
- Device Tag Settings - Custom metadata fields applied to edge devices
- Label Management - Organizational labels for filtering and categorization
- Role-Based Access Control - Permission enforcement (edge-device:read, edge-device:write, edge-device:execute)
- Device Model Images - Visual representation in device lists

**Depended On By**:
- Device Synchronization Jobs - Background jobs that sync edge device state from cloud providers
- Monitoring & Analytics - Aggregates edge device connectivity and health metrics
- Deployment Management - References edge devices for module deployment operations

### Related Features
- Feature 001: Standard Device Management - Manages downstream leaf devices that connect through edge gateways
- Feature 004: Device Tag Settings Management - Defines available tags for edge devices
- Feature 014: Role Management - Configures roles with edge device permissions
- Feature 024: Device Synchronization Jobs - Syncs edge device state from cloud providers
