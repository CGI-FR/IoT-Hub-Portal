# Feature Specification: Device Model Management

**Feature ID**: 002  
**Feature Branch**: `002-device-model-management`  
**Created**: 2026-01-30  
**Status**: Draft  
**Source**: Analysis from `specs/002-device-model-management/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - View and Search Device Model Library (Priority: P1)

As an IoT administrator, I need to view a list of all device model templates with search capabilities so that I can understand what device types are supported and select appropriate models for new devices.

**Why this priority**: Core functionality for understanding available device types. Without this, users cannot discover or select models for device provisioning.

**Independent Test**: Can be fully tested by navigating to the device models page, viewing the list of models with images and descriptions, and performing searches by name or description. Delivers immediate value by providing model visibility.

**Acceptance Scenarios**:

1. **Given** I am an authorized user with model:read permission, **When** I navigate to the device models page, **Then** I see a paginated list of device models showing model image, name, description, and labels
2. **Given** I am viewing the device model list, **When** I enter text in the search field, **Then** the list filters to show only models whose name or description contains the search text (case-insensitive)
3. **Given** I am viewing the device model list, **When** I click on a model name, **Then** I navigate to the model detail page
4. **Given** I am viewing the device model list, **When** a model is marked as built-in, **Then** I do not see a delete button for that model
5. **Given** I am viewing the device model list with more than 10 models, **When** I navigate to the next page, **Then** I see the next set of results
6. **Given** I am viewing the device model list, **When** I click on a column header, **Then** the list sorts by that column
7. **Given** I am a user without model:read permission, **When** I attempt to access the device models page, **Then** I am denied access

---

### User Story 2 - View Device Model Details and Properties (Priority: P1)

As an IoT administrator, I need to view detailed information about a device model including its properties, commands, and configuration so that I can understand what capabilities devices of this model have.

**Why this priority**: Essential for understanding device capabilities before creating devices. Users need to know what properties and commands are available.

**Independent Test**: Can be tested by selecting a device model and viewing its detail page with tabs for general information, properties, LoRaWAN settings (if applicable), and labels. Delivers value by providing complete model context.

**Acceptance Scenarios**:

1. **Given** I have model:read permission, **When** I open a device model detail page, **Then** I see the model name, description, image, and whether it supports LoRaWAN features
2. **Given** I am viewing a device model detail page, **When** I navigate to the properties section, **Then** I see all defined properties with their name, display name, type, order, and writability
3. **Given** I am viewing a LoRaWAN device model, **When** I navigate to the LoRaWAN tab, **Then** I see LoRaWAN-specific configuration including OTAA/ABP settings, class type, deduplication mode, and commands
4. **Given** I am viewing a device model, **When** the model has labels, **Then** I see all assigned labels
5. **Given** I am viewing a device model, **When** the model has properties, **Then** properties are ordered by their Order field value
6. **Given** I am viewing a built-in model, **When** I am on the detail page, **Then** I do not see edit or delete buttons

---

### User Story 3 - Create Device Model Templates (Priority: P1)

As an IoT administrator, I need to create new device model templates with properties and configuration so that I can onboard new device types to the system.

**Why this priority**: Critical for supporting new device types. Without this capability, the system cannot accommodate new devices beyond built-in models.

**Independent Test**: Can be tested by clicking "Create Device Model," entering model details, defining properties, and successfully creating a model. Delivers value by enabling device type expansion.

**Acceptance Scenarios**:

1. **Given** I have model:write permission, **When** I click the "Create Device Model" button, **Then** I see a device model creation form
2. **Given** I am on the model creation form, **When** I enter a model name (required), **Then** the form validates the name is provided
3. **Given** I am on the model creation form, **When** I optionally enter a description, **Then** the description is saved with the model
4. **Given** portal supports LoRaWAN, **When** I create a model, **Then** I see a toggle to enable LoRaWAN support
5. **Given** I am creating a model, **When** I add properties, **Then** each property requires a name, display name, type, order, and writability flag
6. **Given** I am creating a model, **When** I enter a property name, **Then** the name must match the format: single word or dot-separated words
7. **Given** I am creating a LoRaWAN model, **When** I add commands, **Then** each command has a name, frame, port, and confirmed flag
8. **Given** I have completed the model creation form with valid data, **When** I click Save, **Then** the model is created in the portal database, assigned a default image, and an enrollment group is created in the cloud IoT service
9. **Given** I have created a model, **When** I return to the model list, **Then** I see the newly created model
10. **Given** the enrollment group creation fails in the cloud IoT service, **When** I attempt to save, **Then** I see an error message

---

### User Story 4 - Define Device Model Properties (Priority: P1)

As an IoT administrator, I need to define properties for device models including property type, display name, and writability so that devices of this model have a consistent configuration schema.

**Why this priority**: Essential for model-based device management. Properties define what configuration and telemetry fields are available for devices.

**Independent Test**: Can be tested by creating or editing a model, adding properties with various types (string, integer, boolean, etc.), and verifying devices created from this model inherit the property schema. Delivers value through standardized device configuration.

**Acceptance Scenarios**:

1. **Given** I am creating or editing a device model, **When** I add a property, **Then** I must provide a property name, display name, type, order, and writability
2. **Given** I am defining a property, **When** I set the type, **Then** I can choose from: String, Integer, Long, Double, Float, Boolean, or DateTime
3. **Given** I am defining a property, **When** I set writability to true, **Then** the property will be a desired property (configuration sent to device)
4. **Given** I am defining a property, **When** I set writability to false, **Then** the property will be a reported property (telemetry from device)
5. **Given** I am defining a property, **When** I set the order value, **Then** this determines the display sequence in device forms
6. **Given** I am defining properties, **When** I use dot notation in the name (e.g., "config.interval"), **Then** the property represents a nested JSON structure in the device twin
7. **Given** I have defined properties, **When** I save the model, **Then** all properties are persisted and available for devices of this model
8. **Given** I am editing model properties, **When** I remove a property, **Then** the property is deleted from the model template (existing device values are not affected)

---

### User Story 5 - Upload and Manage Device Model Images (Priority: P2)

As an IoT administrator, I need to upload custom images for device models so that models are visually distinguishable in the UI.

**Why this priority**: Important for usability and visual identification but not essential for functionality. Enhances user experience when managing multiple device types.

**Independent Test**: Can be tested by uploading an image to a model, viewing it in the model list and detail pages, and deleting the image to reset to default. Delivers value through improved visual identification.

**Acceptance Scenarios**:

1. **Given** I have model:write permission and am creating or editing a model, **When** I upload an image file (JPG, JPEG, or PNG), **Then** the image is uploaded and resized to 200x200 pixels
2. **Given** I have uploaded a model image, **When** I view the model in the list or detail page, **Then** I see the custom image
3. **Given** I have a model with a custom image, **When** I delete the image, **Then** the model reverts to the default image
4. **Given** I upload an image, **When** the upload fails, **Then** I see an error message and the existing image is unchanged
5. **Given** I create a model without uploading an image, **When** the model is created, **Then** a default image is assigned

---

### User Story 6 - Update Device Model Configuration (Priority: P2)

As an IoT administrator, I need to update device model properties, commands, and labels so that I can refine device model definitions as requirements evolve.

**Why this priority**: Important for maintaining model accuracy but not required for initial deployment. Enables model refinement without recreating models.

**Independent Test**: Can be tested by opening an existing model, modifying name, description, properties, or labels, saving changes, and verifying updates are reflected. Delivers value by enabling model evolution.

**Acceptance Scenarios**:

1. **Given** I have model:write permission and am viewing a non-built-in model, **When** I click Edit, **Then** I can modify model name, description, properties, commands, and labels
2. **Given** I am editing a model, **When** I save changes with valid data, **Then** the model is updated in the portal database and the cloud IoT service configuration is recreated
3. **Given** I am editing model properties, **When** I add, modify, or remove properties, **Then** the changes apply to future devices but do not affect existing device property values
4. **Given** I am editing a LoRaWAN model, **When** I modify LoRaWAN-specific settings, **Then** the settings are updated and reflected in the device configuration
5. **Given** I am editing a model, **When** I update labels, **Then** the label associations are updated
6. **Given** I am editing a model, **When** the cloud IoT service update fails, **Then** I see an error message
7. **Given** I attempt to edit a built-in model, **When** I open the model, **Then** I do not see edit controls

---

### User Story 7 - Delete Device Model Templates (Priority: P2)

As an IoT administrator, I need to delete device model templates that are no longer needed so that I can maintain a clean model library.

**Why this priority**: Important for housekeeping but not critical for operations. Most valuable when managing many evolving model types.

**Independent Test**: Can be tested by selecting a model not in use by devices, clicking delete, confirming the action, and verifying removal including cleanup of enrollment groups. Delivers value through model library maintenance.

**Acceptance Scenarios**:

1. **Given** I have model:write permission, **When** I click the delete button on a non-built-in model, **Then** I see a confirmation dialog
2. **Given** I see the delete confirmation dialog, **When** the model is not in use by any devices, **Then** I can confirm deletion
3. **Given** I confirm deletion of a model not in use, **When** the deletion proceeds, **Then** the model, its properties, commands, labels, enrollment group, configurations, and image are all deleted
4. **Given** I attempt to delete a model, **When** the model is in use by one or more devices, **Then** I see an error message preventing deletion
5. **Given** I attempt to delete a built-in model, **When** I view the model, **Then** I do not see a delete button
6. **Given** I delete a model, **When** the enrollment group deletion fails in the cloud IoT service, **Then** I see an error message

---

### User Story 8 - Manage Device Model Labels (Priority: P3)

As an IoT administrator, I need to assign labels to device models for categorization so that I can organize models by characteristics like vendor, device type, or use case.

**Why this priority**: Useful for organization but not essential for functionality. Most valuable for environments with many model types.

**Independent Test**: Can be tested by assigning labels to models and viewing them in the model list. Delivers value through improved model organization.

**Acceptance Scenarios**:

1. **Given** I am creating or editing a device model, **When** I access the labels section, **Then** I can assign one or more labels
2. **Given** I am managing model labels, **When** I select labels, **Then** those labels are associated with the model
3. **Given** I am managing model labels, **When** I remove a label, **Then** the association is removed
4. **Given** I am viewing the device model list, **When** I look at a model row, **Then** I see all labels assigned to that model

---

### Edge Cases

- **What happens when a device model is created in the portal but the enrollment group creation fails in the cloud IoT service?**  
  The system should rollback the database transaction and display an error message, preventing orphaned model records without cloud service backing.

- **How does the system handle duplicate device model names?**  
  The system allows duplicate names (models are identified by ModelId GUID). However, duplicate names may confuse users and should be avoided through UI guidance.

- **What happens when a model has properties and a user later changes a property type?**  
  Existing device property values may not match the new type, potentially causing validation errors. Type changes should be avoided or handled with data migration.

- **What happens when trying to delete a model that has associated devices?**  
  The system validates model usage and prevents deletion if any devices reference the model, displaying an error message to the user.

- **How does the system handle model property name collisions (e.g., "temperature" defined twice)?**  
  UI validation should prevent duplicate property names within a model. Database constraints may enforce uniqueness per model.

- **What happens when uploading an image larger than the maximum size?**  
  The system automatically resizes images to 200x200 pixels. Extremely large files may fail with size limit errors.

- **How does the system behave when the cloud IoT service is unavailable during model operations?**  
  Model creation and updates that require enrollment group management fail with error messages. Read operations on existing models continue to work.

- **What happens when a LoRaWAN model is created but the portal LoRaWAN support is later disabled?**  
  Existing LoRaWAN models remain in the database. The system should handle this gracefully, potentially hiding LoRaWAN-specific features.

- **How does pagination behave when models are deleted while browsing?**  
  Server-side pagination may result in page shifts. Users may see inconsistent results if models are added/removed during pagination.

- **What happens when model properties use dot notation with conflicting paths (e.g., "config" as both a property and a parent)?**  
  This creates JSON structure conflicts in device twins. Validation should prevent this scenario or document proper usage patterns.

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST provide a paginated list of all device models showing model image, name, description, and labels
- **FR-002**: System MUST support full-text search of device models by name or description with case-insensitive matching
- **FR-003**: System MUST enable creation of new device models with required field: model name
- **FR-004**: System MUST allow optional model description during creation
- **FR-005**: System MUST support toggle for enabling LoRaWAN features on device models when portal has LoRaWAN enabled
- **FR-006**: System MUST validate device model name is provided before saving
- **FR-007**: System MUST assign a default image to newly created models if no custom image is uploaded
- **FR-008**: System MUST create an enrollment group in the cloud IoT service (Azure IoT Hub or AWS IoT Core) when a model is created
- **FR-009**: System MUST allow definition of device model properties with fields: name, display name, type, order, and writability
- **FR-010**: System MUST validate property name format as single word or dot-separated words matching regex: `^([\w]+\.)+[\w]+|[\w]+$`
- **FR-011**: System MUST support property types: String, Integer, Long, Double, Float, Boolean, DateTime
- **FR-012**: System MUST persist model properties in the portal database for use as device property templates
- **FR-013**: System MUST enable viewing of device model details including properties, commands, labels, and configuration
- **FR-014**: System MUST allow updating of device model name, description, properties, commands, and labels
- **FR-015**: System MUST recreate cloud IoT service configuration when a model is updated
- **FR-016**: System MUST allow deletion of device models with confirmation dialogs
- **FR-017**: System MUST validate that a device model is not in use by any devices before allowing deletion
- **FR-018**: System MUST delete enrollment group, configurations, commands, properties, labels, and image when a model is deleted
- **FR-019**: System MUST support image upload for device models in JPG, JPEG, or PNG formats
- **FR-020**: System MUST resize uploaded images to 200x200 pixels automatically
- **FR-021**: System MUST allow deletion of model images to reset to default image
- **FR-022**: System MUST prevent editing or deletion of built-in device models
- **FR-023**: System MUST support LoRaWAN-specific model configuration including OTAA/ABP settings, class type, deduplication mode, preferred window, downlink settings, and sensor decoder
- **FR-024**: System MUST allow definition of LoRaWAN commands with fields: name, frame, port, confirmed flag
- **FR-025**: System MUST allow assignment and removal of labels to device models for categorization
- **FR-026**: System MUST enforce authorization policies: model:read for viewing, model:write for modifications
- **FR-027**: System MUST provide pagination controls with configurable page size (default 10 items)
- **FR-028**: System MUST handle both Azure IoT Hub and AWS IoT Core as cloud providers based on configuration
- **FR-029**: System MUST order properties by the Order field value when displaying in device forms

### Key Entities

- **DeviceModel**: Represents a device type template with properties including ID, name, description, built-in flag, LoRaWAN support flag, LoRaWAN-specific configuration, and labels
- **DeviceModelProperty**: Property template defining name, display name, type, order, writability, and associated model ID
- **DeviceModelCommand**: LoRaWAN command definition with name, frame, port, confirmed flag, built-in flag, and associated model ID
- **DeviceModelDto**: Complete device model information DTO used for API communication including model metadata and labels
- **DeviceProperty**: Property definition DTO including name, display name, type, order, and writability used for property management
- **Label**: Categorization label that can be assigned to device models and devices
- **DevicePropertyType**: Enumeration of supported property types (String, Integer, Long, Double, Float, Boolean, DateTime)

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Users can view a complete list of device models within 2 seconds for up to 500 models with pagination
- **SC-002**: Users can search device models with results appearing in under 2 seconds
- **SC-003**: Device model creation including enrollment group provisioning completes in under 10 seconds
- **SC-004**: Device model property updates save in under 3 seconds
- **SC-005**: Device model deletion including cloud resource cleanup completes in under 10 seconds
- **SC-006**: 95% of model property definitions are correctly applied to devices created from the model
- **SC-007**: System prevents 100% of attempts to delete models in use by devices
- **SC-008**: Users can define a new device model with properties in under 5 minutes
- **SC-009**: 98% of model CRUD operations succeed without errors under normal cloud service availability
- **SC-010**: Image uploads complete in under 5 seconds with automatic resizing to 200x200 pixels

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/002-device-model-management/analyze.md`
- **Analyzed By**: excavator.analyze
- **Analysis Date**: 2026-01-30

### Code References

**Controllers**:
- `src/IoTHub.Portal.Server/Controllers/v1.0/DeviceModelsController.cs` (Lines 1-133)
- `src/IoTHub.Portal.Server/Controllers/v1.0/DeviceModelControllerBase.cs` (Lines 1-138)
- `src/IoTHub.Portal.Server/Controllers/v1.0/DeviceModelPropertiesController.cs` (Lines 1-52)

**Business Logic**:
- `src/IoTHub.Portal.Application/Services/IDeviceModelService.cs` (Lines 1-26)
- `src/IoTHub.Portal.Server/Services/DeviceModelService.cs` (Lines 1-190)
- `src/IoTHub.Portal.Application/Services/IDeviceModelPropertiesService.cs` (Lines 1-13)
- `src/IoTHub.Portal.Infrastructure/Services/DeviceModelPropertiesService.cs`

**Data Access**:
- `src/IoTHub.Portal.Domain/Repositories/IDeviceModelRepository.cs` (Lines 1-10)
- `src/IoTHub.Portal.Domain/Entities/DeviceModel.cs` (Lines 1-41)
- `src/IoTHub.Portal.Domain/Entities/DeviceModelProperty.cs` (Lines 1-45)
- `src/IoTHub.Portal.Domain/Entities/DeviceModelCommand.cs` (Lines 1-20)

**UI Components**:
- `src/IoTHub.Portal.Client/Pages/DeviceModels/DeviceModelListPage.razor` (Lines 1-211)
- `src/IoTHub.Portal.Client/Pages/DeviceModels/DeviceModelDetailPage.razor` (Lines 1-411)
- `src/IoTHub.Portal.Client/Pages/DeviceModels/CreateDeviceModelPage.razor` (Lines 1-357)

### Dependencies

**Depends On**:
- Azure IoT Hub / AWS IoT Core - External cloud IoT enrollment group and configuration management
- Blob Storage (Azure or AWS S3) - Image storage via IDeviceModelImageManager

**Depended On By**:
- Standard Device Management (001) - Devices must be associated with a model
- Edge Device Management (006) - Edge devices use device model templates
- LoRaWAN Device Management (008) - LoRaWAN devices use model templates with LoRaWAN-specific features
- Device Properties Management (003) - Model properties serve as templates for device properties
- Device Configurations Management (005) - Configurations reference model properties
