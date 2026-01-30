# Feature Specification: Device Properties Management

**Feature ID**: 003  
**Feature Branch**: `003-device-properties-management`  
**Created**: 2026-01-30  
**Status**: Draft  
**Source**: Analysis from `specs/003-device-properties-management/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - View Device Instance Properties (Priority: P1)

As an IoT administrator, I need to view current property values for a device retrieved from the cloud device twin so that I can understand the device's current configuration and telemetry state.

**Why this priority**: Core functionality for device monitoring and troubleshooting. Without this, users cannot see device configuration or status.

**Independent Test**: Can be fully tested by navigating to a device detail page, viewing the Properties tab, and verifying that properties match the device model template with current values from the device twin. Delivers immediate value by providing device state visibility.

**Acceptance Scenarios**:

1. **Given** I have device:read permission and am viewing a device detail page, **When** I navigate to the Properties tab, **Then** I see all properties defined by the device model with their current values
2. **Given** I am viewing device properties, **When** a property is writable (IsWritable=true), **Then** I see it as an editable field with appropriate input control for its type
3. **Given** I am viewing device properties, **When** a property is read-only (IsWritable=false), **Then** I see it as a display-only field showing the reported value from the device
4. **Given** I am viewing device properties, **When** properties are defined with dot notation (e.g., "config.interval"), **Then** the values are correctly retrieved from nested JSON structures in the device twin
5. **Given** I am viewing device properties, **When** a property has not been set, **Then** I see an empty or default value
6. **Given** I am viewing device properties, **When** the device twin is unavailable, **Then** I see an error message indicating properties cannot be retrieved
7. **Given** I am viewing device properties for different types, **When** properties are Boolean, **Then** I see checkboxes; **When** properties are numeric, **Then** I see text fields with validation; **When** properties are String, **Then** I see text areas

---

### User Story 2 - Update Device Writable Properties (Priority: P1)

As an IoT administrator, I need to update writable device properties so that I can send configuration changes to devices through the cloud device twin.

**Why this priority**: Critical for remote device configuration. Without this, users cannot update device settings remotely.

**Independent Test**: Can be tested by opening a device with writable properties, modifying values, saving, and verifying the device twin desired properties are updated in the cloud IoT service. Delivers value by enabling remote device configuration.

**Acceptance Scenarios**:

1. **Given** I have device:write permission and am viewing device properties, **When** I modify a writable property value, **Then** the new value is validated according to the property type
2. **Given** I am modifying an Integer property, **When** I enter a non-integer value, **Then** I see a validation error
3. **Given** I am modifying a Double property, **When** I enter a non-numeric value, **Then** I see a validation error
4. **Given** I am modifying a Boolean property, **When** I toggle the checkbox, **Then** the value changes between true and false
5. **Given** I have modified one or more writable properties, **When** I click Save, **Then** the changes are written to the device twin desired properties in the cloud IoT service
6. **Given** I attempt to modify a read-only property, **When** I view the property, **Then** it is not editable
7. **Given** I save property changes, **When** the cloud IoT service is unavailable, **Then** I see an error message and changes are not persisted
8. **Given** I save property changes successfully, **When** I reload the device properties, **Then** I see the updated values

---

### User Story 3 - Define Model Property Templates (Priority: P1)

As an IoT administrator, I need to define property templates at the device model level so that all devices of a model share a consistent property schema.

**Why this priority**: Essential for model-driven device management. Property templates ensure consistency across device fleets.

**Independent Test**: Can be tested by creating or editing a device model, defining properties with name, type, and writability, saving, and verifying devices created from this model inherit the property definitions. Delivers value through standardized device schemas.

**Acceptance Scenarios**:

1. **Given** I have model:write permission and am creating or editing a device model, **When** I add a property, **Then** I must provide property name, display name, type, order, and writability
2. **Given** I am defining a model property, **When** I select the property type, **Then** I can choose from: String, Integer, Long, Double, Float, Boolean, DateTime
3. **Given** I am defining a model property, **When** I enter a property name, **Then** it must match the format: single word or dot-separated words (validated by regex)
4. **Given** I am defining a model property, **When** I set IsWritable to true, **Then** the property will map to device twin desired properties (configuration)
5. **Given** I am defining a model property, **When** I set IsWritable to false, **Then** the property will map to device twin reported properties (telemetry)
6. **Given** I am defining a model property, **When** I set the order value, **Then** this determines the display sequence in device property forms
7. **Given** I have defined multiple properties, **When** I save the model, **Then** all properties are persisted in the portal database
8. **Given** I retrieve model properties, **When** I call the API, **Then** properties are returned ordered by the Order field

---

### User Story 4 - Update Model Property Templates (Priority: P2)

As an IoT administrator, I need to update property definitions for a device model so that I can refine property schemas as requirements evolve.

**Why this priority**: Important for maintaining accurate property schemas but not required for initial deployment. Enables schema evolution.

**Independent Test**: Can be tested by editing a device model's properties, adding new properties, modifying existing ones, or removing properties, and verifying changes are saved. Delivers value by allowing property schema refinement.

**Acceptance Scenarios**:

1. **Given** I have model:write permission and am editing a device model, **When** I modify property definitions, **Then** I can add, update, or remove properties
2. **Given** I am updating model properties, **When** I save changes, **Then** the system performs an upsert operation: adds new properties, updates existing ones, and removes properties not in the request
3. **Given** I remove a property from a model, **When** I save the model, **Then** the property template is deleted but existing device property values are not affected
4. **Given** I add a new property to a model, **When** I save the model, **Then** devices of this model will see the new property in their property forms (with empty values initially)
5. **Given** I change a property type, **When** I save the model, **Then** existing device values may not match the new type, requiring data migration or cleanup
6. **Given** the model does not exist, **When** I attempt to save properties, **Then** I receive a 404 Not Found error

---

### User Story 5 - Support Hierarchical Properties with Dot Notation (Priority: P2)

As an IoT administrator, I need to use dot notation in property names to represent nested JSON structures in device twins so that I can organize related properties hierarchically.

**Why this priority**: Important for complex device configurations but not essential for basic operations. Most valuable for devices with many configuration parameters.

**Independent Test**: Can be tested by defining a property with dot notation (e.g., "config.telemetryInterval"), setting its value, and verifying it creates a nested JSON structure in the device twin. Delivers value through logical property grouping.

**Acceptance Scenarios**:

1. **Given** I am defining a model property, **When** I use dot notation in the property name (e.g., "config.interval"), **Then** the system validates it matches the pattern for dot-separated words
2. **Given** I have defined a property with dot notation, **When** I retrieve the property value from the device twin, **Then** the system uses JSON path selection to navigate to the nested value
3. **Given** I have a property "config.interval" set to 30, **When** I view the device twin, **Then** I see the structure: `{ "config": { "interval": 30 } }`
4. **Given** I update a property with dot notation, **When** I save the change, **Then** the nested JSON structure is updated in the device twin
5. **Given** I have conflicting property paths (e.g., "config" as both a property and a parent), **When** I attempt to use both, **Then** the system should handle conflicts gracefully or prevent this scenario through validation

---

### User Story 6 - Retrieve All Property Names (Priority: P3)

As a developer or administrator, I need to retrieve a list of all distinct property names used across all device models so that I can understand the property landscape and potentially standardize naming.

**Why this priority**: Useful for analysis and standardization but not required for operations. Most valuable in mature deployments with many models.

**Independent Test**: Can be tested by calling an API or viewing a report that shows all unique property names across models. Delivers value through property naming insights.

**Acceptance Scenarios**:

1. **Given** I am an authorized user, **When** I request all property names, **Then** I receive a list of distinct property names used across all device models
2. **Given** multiple models use the same property name, **When** I retrieve all property names, **Then** each name appears only once in the result
3. **Given** no models have properties defined, **When** I retrieve all property names, **Then** I receive an empty list

---

### Edge Cases

- **What happens when a property value in the device twin doesn't match the expected type?**  
  The system performs client-side and server-side type validation. Invalid values are rejected with error messages. However, the device twin may contain non-conforming data from device-side updates, which should be handled gracefully with error indicators.

- **How does the system handle properties with dot notation that conflict (e.g., "config" and "config.interval")?**  
  This creates JSON structure conflicts where "config" cannot be both a value and an object. The system should validate and prevent this or document that properties should be consistently structured.

- **What happens when the cloud IoT service is unavailable during property retrieval?**  
  Property retrieval fails and users see an error message. The portal does not cache property values, so they cannot be viewed during cloud service outages.

- **What happens when updating properties for a device that has been deleted in the cloud IoT service but still exists in the portal?**  
  The update fails with an error indicating the device twin cannot be found. Users should delete the device from the portal.

- **How does the system handle very long property names or values?**  
  Property names are validated by regex but length limits depend on cloud service constraints. Very long values are transmitted as-is, subject to device twin size limits (typically 32KB for Azure IoT Hub).

- **What happens when a model property is deleted but devices still have values for that property in their twins?**  
  The property template is removed from the model, so the property won't appear in device property forms. However, the value remains in the device twin until explicitly removed or overwritten.

- **How does the system handle concurrent updates to the same device properties?**  
  The cloud IoT service handles concurrency with last-write-wins semantics. The portal does not implement optimistic concurrency for properties.

- **What happens when property ordering values are duplicated or non-sequential?**  
  Properties are sorted by the Order field. Duplicate order values result in undefined relative ordering between those properties. Gaps in numbering do not cause issues.

- **How does the system behave when a property type changes from Integer to String?**  
  Existing device values may not match the new type, causing validation errors when users attempt to edit. Type changes require careful data migration or device twin cleanup.

- **What happens when LoRaWAN devices access the properties feature?**  
  LoRaWAN devices do not support custom properties. The properties tab should not be shown for LoRaWAN devices, or should display a message indicating properties are not supported.

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST retrieve device instance property values from cloud device twin (Azure IoT Hub or AWS IoT Core device shadow)
- **FR-002**: System MUST display device properties with distinction between writable (desired) and read-only (reported) properties
- **FR-003**: System MUST enable viewing of property values for devices with device:read permission
- **FR-004**: System MUST enable updating of writable property values for devices with device:write permission
- **FR-005**: System MUST validate property values according to their type before allowing updates
- **FR-006**: System MUST write writable property changes to device twin desired properties in the cloud IoT service
- **FR-007**: System MUST ignore read-only properties when updating device properties (they cannot be modified via portal)
- **FR-008**: System MUST support property types: String, Integer, Long, Double, Float, Boolean, DateTime
- **FR-009**: System MUST render type-specific input controls: checkboxes for Boolean, validated text fields for numeric types, text areas for String
- **FR-010**: System MUST support dot notation in property names to represent nested JSON structures (e.g., "config.interval" maps to `{"config": {"interval": value}}`)
- **FR-011**: System MUST use JSON path selection (JObject.SelectToken) to navigate nested properties in device twins
- **FR-012**: System MUST define property templates at the device model level with fields: name, display name, type, order, writability, and model ID
- **FR-013**: System MUST validate property names match the regex pattern: `^([\w]+\.)+[\w]+|[\w]+$` (single word or dot-separated words)
- **FR-014**: System MUST persist model property templates in the portal database
- **FR-015**: System MUST enable retrieval of model properties ordered by the Order field
- **FR-016**: System MUST allow creation and update of model properties with model:write permission
- **FR-017**: System MUST perform upsert operations when saving model properties: add new, update existing, delete properties not in request
- **FR-018**: System MUST validate device model existence before allowing property operations
- **FR-019**: System MUST return 404 Not Found when attempting property operations on non-existent models
- **FR-020**: System MUST provide an endpoint to retrieve all distinct property names used across all device models
- **FR-021**: System MUST handle both Azure IoT Hub device twins and AWS IoT Core device shadows with provider-specific implementations
- **FR-022**: System MUST act as a thin management layer over cloud IoT services (no caching of property values)
- **FR-023**: System MUST display validation errors when property values do not match expected types
- **FR-024**: System MUST retrieve properties on-demand from cloud service (not cached in portal database)
- **FR-025**: System MUST not apply custom properties feature to LoRaWAN devices

### Key Entities

- **DeviceModelProperty**: Property template entity stored in portal database with fields: ID, Name, DisplayName, IsWritable, Order, PropertyType, and ModelId
- **DeviceProperty**: Property definition DTO with fields: Name, DisplayName, IsWritable, Order, and PropertyType used for model property management
- **DevicePropertyValue**: Property value DTO extending DeviceProperty with an additional Value field (string representation of the current value)
- **DevicePropertyType**: Enumeration of supported types: String, Integer, Long, Double, Float, Boolean, DateTime
- **Device Twin (Azure) / Device Shadow (AWS)**: Cloud IoT service entity storing device configuration (desired properties) and device-reported state (reported properties)

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Users can view device properties retrieved from cloud device twins within 3 seconds under normal conditions
- **SC-002**: Users can update device writable properties with changes synchronized to cloud device twins within 5 seconds
- **SC-003**: 98% of property updates succeed without errors under normal cloud service availability
- **SC-004**: Property type validation catches 100% of type mismatches before submitting to cloud service
- **SC-005**: Model property template changes save within 2 seconds
- **SC-006**: Device property forms display properties in the correct order based on Order field 100% of the time
- **SC-007**: Dot notation properties correctly map to nested JSON structures in device twins with 100% accuracy
- **SC-008**: System handles concurrent device property updates by 10 users without data loss or corruption
- **SC-009**: Property retrieval failures due to cloud service unavailability result in clear error messages 100% of the time
- **SC-010**: Users can define model property templates with all required fields in under 2 minutes per property

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/003-device-properties-management/analyze.md`
- **Analyzed By**: excavator.analyze
- **Analysis Date**: 2026-01-30

### Code References

**Controllers**:
- `src/IoTHub.Portal.Server/Controllers/v1.0/DeviceModelPropertiesController.cs` (Lines 1-53)
- `src/IoTHub.Portal.Server/Controllers/v1.0/DeviceModelPropertiesControllerBase.cs` (Lines 1-88)
- `src/IoTHub.Portal.Server/Controllers/v1.0/DevicesController.cs` (Lines 111-126)

**Business Logic**:
- `src/IoTHub.Portal.Application/Services/IDeviceModelPropertiesService.cs` (Lines 1-13)
- `src/IoTHub.Portal.Infrastructure/Services/DeviceModelPropertiesService.cs` (Lines 1-64)
- `src/IoTHub.Portal.Application/Services/IDevicePropertyService.cs` (Lines 1-12)
- `src/IoTHub.Portal.Server/Services/DevicePropertyService.cs` (Lines 1-129)
- `src/IoTHub.Portal.Infrastructure/Services/AWS/AWSDevicePropertyService.cs` (Lines 1-130+)

**Data Access**:
- `src/IoTHub.Portal.Domain/Repositories/IDeviceModelPropertiesRepository.cs` (Lines 1-11)
- `src/IoTHub.Portal.Infrastructure/Repositories/DeviceModelPropertiesRepository.cs` (Lines 1-49)
- `src/IoTHub.Portal.Domain/Entities/DeviceModelProperty.cs` (Lines 1-44)

**UI Components**:
- `src/IoTHub.Portal.Client/Pages/DeviceModels/DeviceModelDetailPage.razor` (Lines 78-141)
- `src/IoTHub.Portal.Client/Components/Devices/EditDevice.razor` (Lines 252-317)

### Dependencies

**Depends On**:
- Device Model Management (002) - Properties are always associated with a device model
- Standard Device Management (001) - Device instance properties require device to exist with a model assigned
- Azure IoT Hub / AWS IoT Core - Device instance property values stored in and retrieved from cloud device twins/shadows

**Depended On By**:
- Device Configurations Management (005) - Configurations can reference and set device model properties
- Device Import Export (021) - Device exports include property values, imports can set property values
