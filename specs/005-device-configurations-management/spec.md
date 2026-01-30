# Feature Specification: Device Configurations Management

**Feature ID**: 005  
**Feature Branch**: `005-device-configurations-management`  
**Created**: January 30, 2025  
**Status**: Draft  
**Source**: Analysis from `specs/005-device-configurations-management/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - View All Device Configurations (Priority: P1)

As an IoT administrator, I need to view a list of all device configurations with their deployment metrics so I can monitor the status and health of my fleet-wide configuration deployments.

**Why this priority**: This is the foundation for configuration management visibility. Without being able to see configurations and their status, administrators cannot manage their IoT fleet effectively. It provides immediate value by showing what configurations exist and how they're performing.

**Independent Test**: Can be fully tested by authenticating as an administrator with read permissions and navigating to the configurations list page. Delivers standalone value by providing visibility into existing configurations without needing any other features.

**Acceptance Scenarios**:

1. **Given** I am an authenticated administrator with device-configuration:read permission, **When** I navigate to the device configurations page, **Then** I see a list of all configurations with columns for Configuration ID, Target Conditions, Priority, Creation Date, and Metrics (Targeted, Applied, Success, Failure)
2. **Given** configurations exist in the system, **When** I view the list, **Then** each configuration shows real-time metrics from the IoT platform indicating how many devices are targeted, how many have the configuration applied, and success/failure counts
3. **Given** I am viewing the configuration list, **When** I click the refresh button, **Then** the list reloads with updated metrics from the IoT platform
4. **Given** multiple configurations exist, **When** I view the list, **Then** configurations are displayed with their priority values visible for understanding configuration precedence

---

### User Story 2 - View Configuration Details (Priority: P1)

As an IoT administrator, I need to view detailed information about a specific device configuration including which devices it targets and what properties it configures, so I can understand and verify configuration settings before making changes.

**Why this priority**: Understanding what a configuration does is essential before modifying or troubleshooting it. This is a critical read-only capability that enables safe operations and troubleshooting.

**Independent Test**: Can be tested by selecting any configuration from the list and viewing its details page. Delivers value by showing complete configuration information including target conditions, properties, and deployment metrics.

**Acceptance Scenarios**:

1. **Given** I have selected a configuration from the list, **When** I view its detail page, **Then** I see the configuration ID, associated device model name and description, priority, and creation date
2. **Given** I am viewing configuration details, **When** the page loads, **Then** I see all target condition tags (key-value pairs) that determine which devices receive this configuration
3. **Given** I am viewing configuration details, **When** the page loads, **Then** I see all configured properties with their names and values that will be applied to matching devices
4. **Given** I am viewing configuration details, **When** the page loads, **Then** I see deployment metrics showing how many devices are targeted, how many have successfully applied the configuration, and how many failures occurred
5. **Given** a configuration targets specific device cohorts, **When** I view the target conditions, **Then** I see the device model identifier and any additional filtering tags clearly separated

---

### User Story 3 - Create Device Configuration (Priority: P1)

As an IoT administrator, I need to create new device configurations targeting specific device models and cohorts so I can deploy settings and properties to multiple devices simultaneously without manual per-device configuration.

**Why this priority**: Configuration creation is the core capability that enables fleet-wide device management. Without this, administrators must configure each device individually, which doesn't scale. This provides immediate business value by enabling batch operations.

**Independent Test**: Can be tested end-to-end by creating a new configuration with a unique ID, selecting a device model, adding target tags, setting property values, and verifying the configuration is created and visible in the list.

**Acceptance Scenarios**:

1. **Given** I have device-configuration:write permission, **When** I navigate to create a new configuration, **Then** I am presented with a multi-section form for Configuration Info, Target Conditions, and Properties
2. **Given** I am creating a configuration, **When** I enter a unique configuration ID and select a device model from the autocomplete, **Then** the system loads available writable properties for that model
3. **Given** I have selected a device model, **When** I view the Target Conditions section, **Then** I can add one or more tags (key-value pairs) to filter which devices receive this configuration
4. **Given** I am adding target tags, **When** I select a tag name, **Then** the system shows only tags that haven't already been added to avoid duplicates
5. **Given** I am in the Properties section, **When** I add a property, **Then** I see only writable properties from the selected device model that haven't already been configured
6. **Given** I am setting a property value, **When** I enter the value, **Then** the input field is type-aware (checkbox for Boolean, numeric validation for numbers, text for strings) based on the property type
7. **Given** I have completed all required fields, **When** I submit the configuration, **Then** the system validates the data, creates the configuration, rolls it out to the IoT platform, and confirms successful creation
8. **Given** the configuration is successfully created, **When** I return to the list, **Then** the new configuration appears with initial metrics showing targeted device count

---

### User Story 4 - Update Device Configuration (Priority: P2)

As an IoT administrator, I need to modify existing device configurations by changing target conditions or property values so I can adapt configurations to evolving business requirements or correct mistakes without recreating configurations.

**Why this priority**: While creating configurations is essential, the ability to modify them reduces operational overhead and enables iterative refinement. However, organizations can initially work around this by deleting and recreating configurations if needed.

**Independent Test**: Can be tested by opening an existing configuration, modifying tags or properties, saving changes, and verifying the updates are reflected both in the UI and in device behavior.

**Acceptance Scenarios**:

1. **Given** I have device-configuration:write permission and am viewing a configuration detail page, **When** the page loads, **Then** I see edit controls for target conditions (tags) and properties
2. **Given** I am editing a configuration, **When** I add or remove target tags, **Then** I can modify which device cohorts this configuration targets while the device model remains fixed
3. **Given** I am editing a configuration, **When** I modify property values, **Then** I can change the values that will be applied to devices while maintaining type validation
4. **Given** I am editing a configuration, **When** I add new properties, **Then** I see only writable properties from the device model that aren't already configured
5. **Given** I have made changes to tags or properties, **When** I save the configuration, **Then** the system validates the changes, updates the configuration in the IoT platform, and confirms successful update
6. **Given** a configuration has been updated, **When** the IoT platform processes the change, **Then** matching devices receive the updated desired properties according to their synchronization schedule

---

### User Story 5 - Delete Device Configuration (Priority: P2)

As an IoT administrator, I need to delete device configurations that are no longer needed so I can maintain a clean configuration inventory and prevent obsolete settings from affecting devices.

**Why this priority**: Deletion is important for lifecycle management but less critical than create/read operations. Configurations can temporarily remain unused if deletion isn't available, though this adds clutter.

**Independent Test**: Can be tested by selecting a configuration, clicking delete, confirming the action, and verifying the configuration is removed from both the portal and the IoT platform.

**Acceptance Scenarios**:

1. **Given** I have device-configuration:write permission and am viewing a configuration detail page, **When** I click the delete button, **Then** a confirmation dialog appears asking me to confirm the deletion
2. **Given** a delete confirmation dialog is displayed, **When** I confirm the deletion, **Then** the system removes the configuration from the IoT platform and redirects me to the configuration list
3. **Given** a configuration has been deleted, **When** I view the configuration list, **Then** the deleted configuration no longer appears
4. **Given** a configuration has been deleted, **When** the IoT platform processes the removal, **Then** devices that previously matched this configuration no longer receive its desired properties
5. **Given** I attempt to delete a configuration but cancel the confirmation dialog, **When** I cancel, **Then** the configuration remains unchanged and no deletion occurs

---

### User Story 6 - Monitor Configuration Metrics (Priority: P2)

As an IoT administrator, I need to view detailed deployment metrics for each configuration so I can understand deployment success rates, identify problematic configurations, and ensure devices are receiving expected settings.

**Why this priority**: While viewing basic metrics in the list is essential, accessing detailed metrics helps with troubleshooting and validation. Organizations can initially rely on list-view metrics for basic monitoring.

**Independent Test**: Can be tested by accessing metrics for any configuration and validating that the counts accurately reflect the state of devices in the IoT platform.

**Acceptance Scenarios**:

1. **Given** I am viewing a configuration detail page, **When** the page loads, **Then** I see metrics showing: creation date, number of devices targeted, number of devices with configuration applied, success count, and failure count
2. **Given** configuration metrics are displayed, **When** I interpret the metrics, **Then** I understand how many devices match the target conditions (targeted), how many have received the configuration (applied), how many successfully adopted it (success), and how many failed (failure)
3. **Given** devices in the field report their configuration status, **When** I refresh the metrics, **Then** the counts update to reflect the current state reported by the IoT platform
4. **Given** a configuration has failures, **When** I view the failure count, **Then** I can identify that investigation is needed (though specific device identification may require IoT platform tools)

---

### User Story 7 - Manage Configuration Priority (Priority: P3)

As an IoT administrator, I need to understand and work with configuration priority so I can control which configuration takes precedence when multiple configurations target the same device.

**Why this priority**: Priority management is important for complex scenarios with overlapping configurations, but most organizations initially deploy non-overlapping configurations. This is an advanced feature for sophisticated use cases.

**Independent Test**: Can be tested by creating multiple configurations targeting the same device cohort with different priorities and verifying that higher-priority settings override lower-priority ones on actual devices.

**Acceptance Scenarios**:

1. **Given** I am creating or viewing a configuration, **When** I see the priority value, **Then** I understand that higher priority numbers take precedence over lower priority numbers when multiple configurations target the same device
2. **Given** multiple configurations target overlapping device cohorts, **When** the IoT platform applies configurations, **Then** property values from higher-priority configurations override conflicting values from lower-priority configurations
3. **Given** I am viewing the configuration list, **When** I see priority values, **Then** I can quickly identify which configurations will take precedence based on priority ordering
4. **Given** a default priority of 100 is assigned to new configurations, **When** I need to create exception configurations, **Then** I understand I can use higher priorities (e.g., 200) for critical override scenarios

---

### User Story 8 - Filter Devices with Tags (Priority: P3)

As an IoT administrator, I need to use tag-based targeting in configurations so I can precisely control which device cohorts receive specific configuration settings without affecting the entire fleet.

**Why this priority**: Tag-based filtering enables sophisticated fleet segmentation, but organizations can initially deploy configurations to all devices of a model. This adds flexibility for staged rollouts and targeted updates.

**Independent Test**: Can be tested by creating configurations with specific tag combinations and verifying that only devices matching all tag conditions receive the configuration.

**Acceptance Scenarios**:

1. **Given** I am defining target conditions, **When** I add multiple tags, **Then** all tags are combined with AND logic, meaning devices must match ALL tags to receive the configuration
2. **Given** I have defined tags like "location=building-a" and "department=manufacturing", **When** the configuration is deployed, **Then** only devices with both tags receive the configuration
3. **Given** I am creating a configuration, **When** I select tag names, **Then** I see available tag definitions from the device tag management system
4. **Given** I want to update a cohort of devices, **When** I use tags like "firmware-version=1.0.0", **Then** I can target just devices needing an update without affecting devices already on newer versions

---

### Edge Cases

- **What happens when no devices match the target conditions?** The configuration is created successfully, but the "targeted" metric shows 0. No devices receive the configuration until the target conditions match at least one device.

- **How does the system handle invalid property values during creation?** The service layer performs type conversion based on device model property types. Invalid values that fail type parsing result in null values being sent to the IoT platform, which may reject or ignore them.

- **What happens when a device model is deleted but configurations still reference it?** The configuration remains in the system and continues to target devices by modelId tag, but administrators cannot view the model details. Editing the configuration may be problematic if model properties are no longer retrievable.

- **How does the system handle concurrent updates to the same configuration?** The IoT platform uses the most recent update. Last-write-wins semantics apply, potentially overwriting concurrent changes without merge conflict detection.

- **What happens when target conditions are modified to exclude devices that previously matched?** The IoT platform stops applying the configuration to those devices. However, the previously applied desired properties may remain in the device twin unless explicitly removed or overwritten by another configuration.

- **How are metrics calculated when devices are offline?** Metrics reflect the IoT platform's knowledge. Offline devices may show as "targeted" but not "applied" or may show stale status until they reconnect and report their state.

- **What happens if property names in a configuration don't match any writable properties of the target device model?** The service layer filters properties during creation/update. Properties not matching the device model are silently ignored and not included in the IoT platform configuration.

- **How does the system handle configurations with priority conflicts?** The IoT platform resolves conflicts automatically. When multiple configurations target the same device and set the same property, the configuration with the highest priority wins. If priorities are equal, behavior is platform-specific (typically last-updated wins).

- **What happens when a required device model property is not included in a configuration?** Only properties explicitly defined in the configuration are set as desired properties. Missing properties retain their existing values on devices or remain unset.

- **How does the system handle special characters or spaces in configuration IDs, tag values, or property values?** Configuration IDs must be compatible with IoT platform naming constraints. Tag values are limited to alphanumeric characters and hyphens by the regex parsing logic. Property values support various types but may have platform-specific encoding requirements.

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST allow authorized users with device-configuration:read permission to retrieve a list of all device configurations with summary information including configuration ID, target conditions, priority, creation date, and deployment metrics (targeted, applied, success, failure counts)

- **FR-002**: System MUST allow authorized users with device-configuration:read permission to retrieve detailed information about a specific configuration including configuration ID, associated device model, priority, target condition tags, configured property values, and deployment metrics

- **FR-003**: System MUST allow authorized users with device-configuration:write permission to create new device configurations by specifying a unique configuration ID, selecting a device model, defining target condition tags, and setting values for writable device model properties

- **FR-004**: System MUST allow authorized users with device-configuration:write permission to update existing device configurations by modifying target condition tags and property values

- **FR-005**: System MUST allow authorized users with device-configuration:write permission to delete device configurations, removing them from the IoT platform and stopping configuration application to matching devices

- **FR-006**: System MUST validate that configuration IDs are unique within the system and prevent duplicate configuration IDs from being created

- **FR-007**: System MUST retrieve available device model properties when creating or updating configurations and allow only writable properties to be configured

- **FR-008**: System MUST perform type conversion of property values from string representations to appropriate data types (Boolean, Double, Float, Integer, Long, String) based on device model property type definitions before deploying to the IoT platform

- **FR-009**: System MUST construct target conditions by combining device model ID and user-defined tags using AND logic (e.g., "tags.modelId='model123' AND tags.location='building-a'")

- **FR-010**: System MUST deploy configurations to the underlying IoT platform with desired properties formatted as "properties.desired.{propertyName}" for device twin synchronization

- **FR-011**: System MUST retrieve real-time deployment metrics from the IoT platform including targeted device count, applied configuration count, success count, and failure count

- **FR-012**: System MUST assign a default priority of 100 to new configurations and persist the priority value with the configuration

- **FR-013**: System MUST enforce authorization policies requiring device-configuration:read permission for read operations and device-configuration:write permission for create, update, and delete operations

- **FR-014**: System MUST provide user interface pages for listing configurations, viewing configuration details, creating new configurations, and editing existing configurations

- **FR-015**: System MUST display only writable device model properties when adding properties to a configuration, filtering out read-only properties

- **FR-016**: System MUST prevent duplicate tags from being added to the same configuration by filtering already-selected tags from available options

- **FR-017**: System MUST prevent duplicate properties from being added to the same configuration by filtering already-configured properties from available options

- **FR-018**: System MUST parse configuration target conditions using regex pattern matching to extract tag names and values when retrieving configurations from the IoT platform

- **FR-019**: System MUST separate the modelId tag from user-defined tags in the user interface display while maintaining the modelId in the underlying target condition string

- **FR-020**: System MUST provide type-specific input controls for property values (checkbox for Boolean, numeric validation for numeric types, text input for strings) in the user interface

- **FR-021**: System MUST synchronize configuration changes to the IoT platform immediately upon create or update operations

- **FR-022**: System MUST handle gracefully when device model properties cannot be retrieved, providing informative error messages to users

- **FR-023**: System MUST display confirmation dialogs before executing destructive operations such as configuration deletion

- **FR-024**: System MUST provide user feedback notifications for successful and failed operations via non-blocking notification mechanisms

- **FR-025**: System MUST allow users to navigate between configuration list, detail, and create pages with appropriate authorization checks at each entry point

---

### Key Entities

- **Device Configuration**: Represents a set of desired properties and target conditions for deploying settings to multiple IoT devices. Key attributes include unique configuration ID, associated device model identifier, priority level, creation timestamp, target condition tags (key-value pairs), and configured property values (key-value pairs). Relates to Device Model entity for property validation and available property definitions.

- **Configuration Metrics**: Represents deployment statistics for a device configuration. Key attributes include targeted device count (devices matching target conditions), applied count (devices where configuration has been delivered), success count (devices reporting successful application), failure count (devices reporting application failure), and creation date. Derived from IoT platform telemetry and updated dynamically.

- **Target Condition**: Represents filtering criteria for determining which devices receive a configuration. Composed of device model identifier (required) and optional user-defined tag filters (key-value pairs). All conditions are combined with AND logic, requiring devices to match all specified tags.

- **Configuration Property**: Represents a single desired property setting within a device configuration. Key attributes include property name (matching device model property definition), property value (stored as string in UI, converted to appropriate type during deployment), and property type (Boolean, Double, Float, Integer, Long, String). Must reference a writable property from the associated device model.

- **Device Model**: External entity representing the template/schema for a device type. Provides available property definitions including property name, display name, type, and writability. Referenced by configurations to determine valid properties and enable type-safe property configuration.

- **Device Tag**: External entity representing metadata key-value pairs attached to devices for grouping and filtering. Tags like "location", "department", "firmware-version" enable cohort-based configuration targeting. Tag definitions managed by separate Device Tag Settings feature.

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Administrators can view all device configurations with summary metrics in under 3 seconds, enabling rapid assessment of configuration deployment status across the fleet

- **SC-002**: Administrators can create a new device configuration targeting a specific device model and cohort in under 2 minutes from start to submission, reducing time-to-deploy compared to manual per-device configuration

- **SC-003**: Configuration changes are synchronized to the IoT platform within 5 seconds of saving, ensuring timely rollout of desired properties to matching devices

- **SC-004**: Deployment metrics (targeted, applied, success, failure counts) accurately reflect IoT platform state with refresh capability, providing reliable visibility into configuration effectiveness

- **SC-005**: Type conversion errors for property values are prevented through client-side validation, resulting in a 95%+ success rate for configuration creation on first attempt

- **SC-006**: Users with read-only permissions can view all configuration details without being presented with edit or delete controls, ensuring appropriate access control enforcement

- **SC-007**: Configuration deletion removes configurations from the IoT platform within 10 seconds, stopping application to devices and cleaning up obsolete configurations

- **SC-008**: Configuration priority precedence is correctly enforced by the IoT platform, with higher-priority configurations overriding lower-priority ones when targeting the same devices, measurable through device twin verification

- **SC-009**: Tag-based targeting accurately filters devices, with only devices matching ALL specified tags receiving the configuration, verifiable through targeted device count metrics

- **SC-010**: Reduce fleet-wide configuration deployment time by 90%+ compared to manual per-device configuration for fleets exceeding 100 devices

- **SC-011**: Configuration management operations complete successfully for simultaneous management of configurations targeting device fleets of 10,000+ devices without performance degradation

- **SC-012**: Zero unauthorized configuration modifications occur due to permission-based access control, with all write operations requiring device-configuration:write permission

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/005-device-configurations-management/analyze.md`
- **Analyzed By**: excavator.specifier
- **Analysis Date**: January 30, 2025

### Code References

**Controllers**:
- `src/IoTHub.Portal.Server/Controllers/v1.0/DeviceConfigurationsController.cs` - REST API endpoints for configuration CRUD operations and metrics retrieval

**Services**:
- `src/IoTHub.Portal.Server/Services/DeviceConfigurationsService.cs` - Business logic for configuration management, property type conversion, and IoT platform integration
- `src/IoTHub.Portal.Application/Services/IDeviceConfigurationsService.cs` - Service interface defining configuration operations contract
- `src/IoTHub.Portal.Application/Services/IConfigService.cs` - Cloud provider abstraction for IoT platform configuration management
- `src/IoTHub.Portal.Application/Services/IDeviceModelPropertiesService.cs` - Device model property retrieval for validation

**Helpers**:
- `src/IoTHub.Portal.Application/Helpers/ConfigHelper.cs` - Transformation utilities for converting between IoT platform configurations and portal DTOs, target condition parsing

**Data Transfer Objects**:
- `src/IoTHub.Portal.Shared/Models/v1.0/DeviceConfig.cs` - Configuration data structure with ID, model, tags, properties, priority
- `src/IoTHub.Portal.Shared/Models/v1.0/ConfigListItem.cs` - Configuration list item with summary and metrics
- `src/IoTHub.Portal.Shared/Models/v1.0/ConfigurationMetrics.cs` - Detailed deployment metrics structure

**UI Components**:
- `src/IoTHub.Portal.Client/Pages/DeviceConfigurations/DeviceConfigurationListPage.razor` - Configuration list view page
- `src/IoTHub.Portal.Client/Pages/DeviceConfigurations/CreateDeviceConfigurationsPage.razor` - Configuration creation wizard
- `src/IoTHub.Portal.Client/Pages/DeviceConfigurations/DeviceConfigurationDetailPage.razor` - Configuration detail and edit page
- `src/IoTHub.Portal.Client/Pages/DeviceConfigurations/DeleteDeviceConfiguration.razor` - Deletion confirmation dialog

**Client Services**:
- `src/IoTHub.Portal.Client/Services/DeviceConfigurationsClientService.cs` - HTTP client for API communication

### Dependencies

**Internal Features**:
- **002-device-model-management**: Configurations target specific device models and validate properties against model definitions
- **003-device-properties-management**: Device model properties define what can be configured and property types for validation
- **004-device-tag-settings-management**: Tags used for target condition filtering and device cohort selection
- **IoT Hub Integration**: Configurations synchronized to Azure IoT Hub or AWS IoT Core for automatic device management

**External Services**:
- **Azure IoT Hub / AWS IoT Core**: Cloud platform for storing configurations and automatically applying desired properties to device twins
- **Microsoft.Azure.Devices SDK**: Azure IoT Hub interaction for configuration management operations

**Authorization**:
- **Role-Based Access Control (RBAC)**: Permissions enforced through device-configuration:read and device-configuration:write policies
