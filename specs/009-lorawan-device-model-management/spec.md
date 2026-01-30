# Feature Specification: LoRaWAN Device Model Management

**Feature ID**: 009  
**Feature Branch**: `009-lorawan-device-model-management`  
**Created**: 2025-01-30  
**Status**: Draft  
**Source**: Analysis from `specs/009-lorawan-device-model-management/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - Browse and Search LoRaWAN Device Models (Priority: P1)

As a network administrator, I need to view all LoRaWAN device model templates in the portal so that I can understand what device types are supported and quickly find specific models to configure devices.

**Why this priority**: This is the foundation for all device model management activities. Without the ability to browse and search, administrators cannot effectively manage their device model catalog or find the right templates for device provisioning. This represents the core read capability that all other operations depend on.

**Independent Test**: Can be fully tested by accessing the device model list page, performing searches, applying filters, and verifying pagination works correctly. Delivers immediate value by providing visibility into available LoRaWAN device templates.

**Acceptance Scenarios**:

1. **Given** I am an authenticated administrator with model:read permission, **When** I navigate to the LoRaWAN device models page, **Then** I see a paginated list of all LoRaWAN device models with their names, descriptions, and labels
2. **Given** there are more than 10 device models in the system, **When** I view the device model list, **Then** I can navigate through pages using pagination controls
3. **Given** I am viewing the device model list, **When** I enter a search term in the search box, **Then** the list filters to show only models whose name or description contains the search term
4. **Given** I am viewing the device model list, **When** I click on a table column header, **Then** the list sorts by that column in ascending or descending order
5. **Given** device models have label tags assigned, **When** I view the device model list, **Then** I see label badges displayed for each model
6. **Given** some models are marked as built-in, **When** I view the device model list, **Then** built-in models are visually distinguished and lack delete buttons

---

### User Story 2 - View LoRaWAN Device Model Details (Priority: P1)

As a network engineer, I need to view the complete configuration details of a LoRaWAN device model including all protocol parameters, so that I can understand how devices using this model will behave on the network.

**Why this priority**: Understanding device model configurations is critical for troubleshooting network issues, planning device deployments, and ensuring correct LoRaWAN protocol settings. This is essential before making any modifications or creating similar models.

**Independent Test**: Can be fully tested by selecting a device model and viewing all its details including authentication mode (OTAA/ABP), class type, deduplication settings, receive window configuration, frame counters, sensor decoder, and commands. Delivers value by providing complete visibility into device model behavior.

**Acceptance Scenarios**:

1. **Given** I am viewing the device model list, **When** I click on a specific LoRaWAN device model, **Then** I see all model details including name, description, avatar image, and all LoRaWAN protocol settings
2. **Given** I am viewing a device model's details, **When** the model uses OTAA authentication, **Then** I see OTAA-specific settings including RX1 datarate offset and RX2 datarate
3. **Given** I am viewing a device model's details, **When** the model uses ABP authentication, **Then** I see ABP-specific settings including frame counter start values and ABP relax mode status
4. **Given** I am viewing a device model's details, **When** frame counter management is configured, **Then** I see FCntUpStart, FCntDownStart, and FCntResetCounter values
5. **Given** I am viewing a device model's details, **When** a sensor decoder URL is configured, **Then** I see the decoder URL for payload processing
6. **Given** I am viewing a device model's details, **When** commands are defined for the model, **Then** I see a list of all available commands with their names, frames, ports, and confirmation status

---

### User Story 3 - Create New LoRaWAN Device Model (Priority: P1)

As a device administrator, I need to create new LoRaWAN device model templates with protocol-specific configurations so that I can quickly onboard multiple devices of the same type with consistent settings.

**Why this priority**: Device model creation is essential for scaling device deployments. Without reusable templates, every device would need manual configuration, leading to errors and inconsistencies. This enables standardized, repeatable device provisioning.

**Independent Test**: Can be fully tested by creating a new device model with all required fields, selecting OTAA or ABP authentication, configuring protocol parameters, and verifying the model is saved and appears in the model list. Delivers value by enabling template-based device provisioning.

**Acceptance Scenarios**:

1. **Given** I am an authenticated user with model:write permission, **When** I click "Create Device Model" and enter a unique name, **Then** a new LoRaWAN device model is created with default settings
2. **Given** I am creating a new device model, **When** I select OTAA authentication mode, **Then** I can configure OTAA-specific settings including RX windows and datarate offsets
3. **Given** I am creating a new device model, **When** I select ABP authentication mode, **Then** I can configure ABP-specific settings including frame counter start values and relaxed mode
4. **Given** I am creating a new device model, **When** I select a device class type (A, B, or C), **Then** the model is configured with the appropriate receive window behavior
5. **Given** I am creating a new device model, **When** I configure message deduplication to "Drop", **Then** duplicate messages received by multiple gateways will be dropped
6. **Given** I am creating a new device model, **When** I enter a sensor decoder URL, **Then** device telemetry payloads will be processed by the specified decoder
7. **Given** I am creating a new device model, **When** I set a keep-alive timeout value, **Then** device connections will expire after the specified period of inactivity
8. **Given** I am creating a new device model, **When** I upload a custom avatar image, **Then** the image is associated with the model for visual identification
9. **Given** I am creating a new device model, **When** I assign labels to the model, **Then** the labels are saved for categorization and filtering
10. **Given** I am creating a new device model, **When** the model is successfully saved, **Then** an enrollment group is created in the cloud IoT platform for automatic device provisioning

---

### User Story 4 - Update Existing LoRaWAN Device Model (Priority: P2)

As a network administrator, I need to modify existing LoRaWAN device model configurations to adjust protocol parameters, update descriptions, or correct settings without disrupting devices already using the model.

**Why this priority**: As network requirements evolve or issues are discovered, device model configurations need updates. This is high priority because it affects all devices using the model, but slightly lower than creation since it's a refinement activity rather than enabling new capabilities.

**Independent Test**: Can be fully tested by editing an existing device model, changing various settings (except OTAA/ABP mode which is immutable), saving changes, and verifying all devices using the model receive updated configurations. Delivers value by allowing configuration refinement without recreating models.

**Acceptance Scenarios**:

1. **Given** I am viewing a device model, **When** I click "Edit" and modify the description, **Then** the updated description is saved and visible in the model list
2. **Given** I am editing a device model, **When** I change receive window settings (preferred window, RX delays), **Then** all devices using this model receive the updated downlink configuration
3. **Given** I am editing a device model, **When** I update frame counter management settings, **Then** the frame counter behavior changes for all associated devices
4. **Given** I am editing a device model, **When** I change the deduplication mode, **Then** duplicate message handling is updated across all devices using the model
5. **Given** I am editing a device model, **When** I modify the keep-alive timeout, **Then** connection expiration behavior is updated for all devices
6. **Given** I am editing a device model, **When** I update the sensor decoder URL, **Then** telemetry processing uses the new decoder for all devices
7. **Given** I am editing a device model with OTAA authentication, **When** I view the authentication mode field, **Then** the OTAA/ABP toggle is disabled to prevent mode changes
8. **Given** I am editing a built-in device model, **When** I attempt to access the edit screen, **Then** editing is prevented to protect system-defined models
9. **Given** I am editing a device model, **When** I add or remove labels, **Then** the label changes are reflected in the model's categorization
10. **Given** I am editing a device model, **When** I save changes, **Then** the cloud enrollment group and device configurations are updated with new parameters

---

### User Story 5 - Delete LoRaWAN Device Model (Priority: P2)

As a device administrator, I need to remove obsolete or incorrect LoRaWAN device models from the system to maintain a clean model catalog, but only when no devices are using them.

**Why this priority**: Model deletion is important for maintaining system hygiene and preventing confusion from outdated templates, but is less critical than read/create/update operations. The safety validation (preventing deletion of in-use models) is crucial.

**Independent Test**: Can be fully tested by attempting to delete unused models (should succeed) and models in use by devices (should fail with validation error). Delivers value by keeping the model catalog clean and preventing accidental deletion of active models.

**Acceptance Scenarios**:

1. **Given** I am viewing the device model list, **When** I click delete on a model that is not used by any devices, **Then** the model is permanently removed along with its enrollment group, configurations, commands, and images
2. **Given** I am viewing the device model list, **When** I click delete on a model that is currently used by devices, **Then** the deletion is blocked and I receive an error message indicating the model is in use
3. **Given** I am viewing the device model list, **When** I attempt to delete a built-in device model, **Then** the delete action is not available to protect system-defined models
4. **Given** I am deleting a device model, **When** the deletion is confirmed, **Then** all associated labels are removed from the system
5. **Given** I am deleting a device model, **When** the deletion is confirmed, **Then** all associated commands for the model are removed
6. **Given** I am deleting a device model, **When** the deletion is confirmed, **Then** the enrollment group is removed from the cloud IoT platform

---

### User Story 6 - Manage Device Model Avatar Images (Priority: P3)

As a device administrator, I need to upload, update, and remove custom avatar images for device models so that different device types are easily recognizable in the user interface.

**Why this priority**: Visual identification improves user experience and reduces errors when selecting device models, but this is a usability enhancement rather than core functionality. The system can function with default images.

**Independent Test**: Can be fully tested by uploading a custom image, viewing it in the model list, updating it with a different image, and deleting it to revert to the default. Delivers value by improving model recognition and user experience.

**Acceptance Scenarios**:

1. **Given** I am creating or editing a device model, **When** I upload a custom avatar image, **Then** the image is stored and displayed as the model's visual identifier
2. **Given** a device model has a custom avatar, **When** I view the model in the list, **Then** the custom avatar is displayed instead of the default image
3. **Given** a device model has a custom avatar, **When** I click "Change Avatar" and upload a new image, **Then** the previous image is replaced with the new one
4. **Given** a device model has a custom avatar, **When** I click "Delete Avatar", **Then** the custom image is removed and the default avatar is displayed
5. **Given** I am uploading an avatar image, **When** the image format is supported (PNG, JPG, etc.), **Then** the upload succeeds and the image is displayed
6. **Given** I am viewing device model details, **When** I request the avatar URL, **Then** I receive a valid URL to retrieve the image

---

### User Story 7 - Define and Manage LoRaWAN Commands (Priority: P2)

As a network engineer, I need to define custom commands for LoRaWAN device models so that devices using the model can receive remote control instructions with proper framing and port configuration.

**Why this priority**: Command management enables remote device control, which is a key value proposition of IoT systems. This is high priority because it directly enables business use cases like remote actuator control, but is lower than basic model management since it's an advanced capability.

**Independent Test**: Can be fully tested by defining a command with name, frame (hex), port, and confirmation settings, saving it to a device model, and verifying it appears in the command list. Delivers value by enabling standardized remote control for all devices of a model type.

**Acceptance Scenarios**:

1. **Given** I am viewing a device model, **When** I navigate to the commands section, **Then** I see all currently defined commands for the model
2. **Given** I am managing device model commands, **When** I add a new command with a name and hex frame, **Then** the command is available for all devices using this model
3. **Given** I am defining a command, **When** I specify a LoRaWAN port number between 1 and 223, **Then** the command will be transmitted on that port
4. **Given** I am defining a command, **When** I mark the command as "confirmed", **Then** devices must acknowledge receipt of the command
5. **Given** I am defining a command, **When** I mark the command as "unconfirmed", **Then** devices receive the command without acknowledgment requirement
6. **Given** I am defining a command, **When** I enter a frame with non-hexadecimal characters, **Then** validation prevents the invalid frame from being saved
7. **Given** I am defining a command, **When** the frame exceeds 255 characters, **Then** validation prevents the oversized frame from being saved
8. **Given** I am managing commands, **When** I update the command list, **Then** all existing commands are replaced with the new command set
9. **Given** commands are marked as built-in, **When** I view the command list, **Then** built-in commands are protected from modification or deletion
10. **Given** I am saving commands, **When** multiple commands have the same name, **Then** each command is stored with a unique identifier

---

### User Story 8 - Configure LoRaWAN Authentication Modes (Priority: P1)

As a security engineer, I need to configure device models with either OTAA (Over-The-Air Activation) or ABP (Activation By Personalization) authentication so that devices follow the appropriate security and provisioning strategy for their deployment environment.

**Why this priority**: Authentication mode is fundamental to LoRaWAN security and device provisioning. This decision affects network security, device deployment processes, and compliance requirements. This is critical for proper LoRaWAN network operation.

**Independent Test**: Can be fully tested by creating models with OTAA and ABP modes, configuring mode-specific parameters (OTAA: RX datarates; ABP: frame counters, relax mode), and verifying the correct parameters are available and enforced. Delivers value by enabling proper security configuration for different deployment scenarios.

**Acceptance Scenarios**:

1. **Given** I am creating a new device model, **When** I select OTAA authentication mode, **Then** OTAA-specific configuration options are available (RX1 datarate offset, RX2 datarate)
2. **Given** I am creating a new device model, **When** I select ABP authentication mode, **Then** ABP-specific configuration options are available (frame counter start values, ABP relax mode)
3. **Given** I am creating a device model with ABP, **When** I enable ABP relax mode, **Then** frame counter validation is relaxed for device restarts
4. **Given** I am creating a device model with ABP, **When** I set FCntUpStart and FCntDownStart values, **Then** frame counters initialize at these values for device security
5. **Given** I am creating a device model with OTAA, **When** I configure RX1 datarate offset, **Then** the offset is applied between received and retransmit datarates per LoRaWAN specification
6. **Given** I am creating a device model with OTAA, **When** I configure RX2 datarate, **Then** the custom datarate is used for the second receive window
7. **Given** I have created a device model with OTAA, **When** I later edit the model, **Then** the authentication mode cannot be changed to preserve device provisioning consistency
8. **Given** I am configuring frame counters for ABP, **When** I enter values outside the range 0-4294967295, **Then** validation prevents invalid frame counter values
9. **Given** I am configuring a device model, **When** I enable 32-bit frame counter support, **Then** devices can use the full 32-bit counter range

---

### User Story 9 - Configure LoRaWAN Receive Windows (Priority: P2)

As a network engineer, I need to configure receive window settings for device models (RX1/RX2 preferences, delays, datarates) so that downlink communications are optimized for device power consumption and network latency requirements.

**Why this priority**: Receive window configuration affects downlink reliability and device battery life. While important for network optimization, devices can function with default settings, making this a refinement rather than critical functionality.

**Independent Test**: Can be fully tested by configuring preferred window, RX delays, and datarates on a model, deploying a test device, and verifying downlink messages use the specified receive window parameters. Delivers value by enabling network performance optimization.

**Acceptance Scenarios**:

1. **Given** I am configuring a device model, **When** I enable downlink support, **Then** cloud-to-device messages are permitted for devices using this model
2. **Given** I am configuring a device model, **When** I disable downlink support, **Then** cloud-to-device messages are blocked for devices using this model
3. **Given** I am configuring a device model with downlink enabled, **When** I set preferred window to 1 (RX1), **Then** downlink messages are primarily sent during the first receive window
4. **Given** I am configuring a device model with downlink enabled, **When** I set preferred window to 2 (RX2), **Then** downlink messages are primarily sent during the second receive window
5. **Given** I am configuring a device model, **When** I set custom RX delay values, **Then** the wait time between device transmission and receive windows follows the custom timing
6. **Given** I am configuring an OTAA device model, **When** I configure RX1 datarate offset, **Then** the offset adjusts the downlink datarate relative to the uplink datarate
7. **Given** I am configuring an OTAA device model, **When** I configure RX2 datarate, **Then** the second receive window uses the specified datarate instead of the default

---

### User Story 10 - Configure Message Deduplication (Priority: P3)

As a network administrator, I need to configure how duplicate messages from multiple gateways are handled (None, Drop, Mark) so that telemetry data quality matches application requirements without overwhelming the system.

**Why this priority**: Deduplication affects data quality and system efficiency but is not essential for basic operation. Most deployments can start with default deduplication and adjust later based on observed behavior.

**Independent Test**: Can be fully tested by configuring deduplication mode on a model, deploying a device in range of multiple gateways, sending a test message, and verifying the system handles duplicates according to the configured mode. Delivers value by preventing data duplication issues.

**Acceptance Scenarios**:

1. **Given** I am configuring a device model, **When** I set deduplication mode to "None", **Then** all messages received from multiple gateways are processed and stored
2. **Given** I am configuring a device model, **When** I set deduplication mode to "Drop", **Then** only the first message is processed and subsequent duplicates are discarded
3. **Given** I am configuring a device model, **When** I set deduplication mode to "Mark", **Then** all messages are processed but duplicates are flagged for identification
4. **Given** I am creating a new device model, **When** no deduplication mode is explicitly set, **Then** the default mode (None) is applied

---

### User Story 11 - Categorize Models with Labels (Priority: P3)

As a device administrator, I need to assign labels to device models for categorization so that I can organize large model catalogs by manufacturer, device type, deployment location, or custom criteria.

**Why this priority**: Labels improve organization and filtering in large deployments but are not essential for core model management functionality. This is a usability enhancement for scale.

**Independent Test**: Can be fully tested by creating labels, assigning them to device models, filtering the model list by labels, and verifying labeled models are correctly categorized. Delivers value by enabling organized model management at scale.

**Acceptance Scenarios**:

1. **Given** I am creating or editing a device model, **When** I assign one or more labels, **Then** the labels are saved and displayed as badges on the model
2. **Given** device models have labels assigned, **When** I view the device model list, **Then** I see label badges displayed for each labeled model
3. **Given** device models have labels, **When** I filter the list by a specific label, **Then** only models with that label are displayed
4. **Given** I am editing a device model, **When** I remove a label, **Then** the label is no longer associated with the model
5. **Given** I am deleting a device model, **When** the deletion is confirmed, **Then** all label associations are removed

---

### Edge Cases

- **What happens when a device model name conflicts with an existing model?** System must validate uniqueness and prevent duplicate model names to avoid provisioning ambiguity.

- **How does the system handle OTAA/ABP mode immutability after model creation?** The authentication mode toggle is disabled in the edit interface to prevent changes that would break device provisioning. Models must be recreated to change authentication mode.

- **What happens when attempting to delete a model that devices are actively using?** The system queries the IoT platform for devices using the model and blocks deletion with a clear error message indicating which devices depend on the model.

- **How are built-in models protected from modification and deletion?** Built-in models have the IsBuiltin flag set, which disables edit and delete actions in both the UI and API layer through validation checks.

- **What happens when frame counter values exceed the 32-bit maximum?** Validation enforces the range 0-4294967295 (2^32-1) for all frame counter fields (FCntUpStart, FCntDownStart, FCntResetCounter).

- **How does the system handle invalid LoRaWAN command frames (non-hexadecimal)?** Client-side validation prevents non-hexadecimal characters, and server-side validation with regex pattern ^[0-9a-fA-F]{0,255}$ rejects invalid frames.

- **What happens when a command frame exceeds 255 characters?** Validation enforces maximum length to comply with LoRaWAN payload size limitations.

- **How does the system handle enrollment group creation failures in the cloud platform?** If enrollment group creation fails during model creation, the transaction is rolled back and the model is not saved, maintaining consistency between the portal and cloud platform.

- **What happens when changing receive window settings on a model with existing devices?** The updated configuration is rolled out to all devices via device twin desired properties, but active connections may need to be reset for changes to take effect.

- **How are sensor decoder URL errors handled?** Decoder URL validation occurs at configuration time (URL format), but runtime decoder failures are logged and telemetry is passed through without processing to prevent message loss.

- **What happens when keep-alive timeout is set to 0 or negative values?** Validation should prevent invalid timeout values; a null/empty value means no timeout (connection never expires).

- **How does the system handle concurrent edits to the same device model?** Last write wins; no optimistic concurrency control is implemented. Consider adding version tracking for conflict detection.

- **What happens when a user lacks model:write permission but attempts to create a model?** Authorization checks at the API layer return 403 Forbidden; UI elements for creation are hidden for users without permission.

- **How does the system handle class type changes for models with existing devices?** Class type changes are applied to the model and rolled out via device configurations, but physical device firmware must also support the target class type or devices will not function correctly.

- **What happens when the LoRaWAN feature is disabled but models exist?** The LoRaFeatureActiveFilter blocks all LoRaWAN API endpoints when the feature is disabled, preventing access to existing models until the feature is re-enabled.

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST provide paginated listing of LoRaWAN device model templates filtered to models with LoRaWAN support enabled
- **FR-002**: System MUST support full-text search filtering of device models by name or description
- **FR-003**: System MUST support sorting device model lists by multiple columns in ascending or descending order
- **FR-004**: System MUST allow authorized users to create new LoRaWAN device models with all protocol-specific configuration parameters
- **FR-005**: System MUST validate device model name uniqueness before creation
- **FR-006**: System MUST allow selection between OTAA (Over-The-Air Activation) and ABP (Activation By Personalization) authentication modes during model creation
- **FR-007**: System MUST prevent modification of authentication mode (OTAA/ABP) after model creation to maintain provisioning consistency
- **FR-008**: System MUST allow configuration of LoRaWAN device class types (A, B, or C) for receive window behavior
- **FR-009**: System MUST allow configuration of message deduplication strategies (None, Drop, Mark) to control duplicate message handling from multiple gateways
- **FR-010**: System MUST allow authorized users to view complete device model details including all LoRaWAN protocol parameters
- **FR-011**: System MUST allow authorized users to update existing device model configurations except for immutable fields
- **FR-012**: System MUST synchronize device model configuration changes to all devices using the model via cloud platform device twin updates
- **FR-013**: System MUST allow authorized users to delete device models that are not in use by any devices
- **FR-014**: System MUST prevent deletion of device models that are currently associated with one or more devices
- **FR-015**: System MUST prevent deletion and modification of built-in device models to protect system-defined templates
- **FR-016**: System MUST allow upload, update, and deletion of custom avatar images for device models
- **FR-017**: System MUST provide default avatar images when custom images are not specified
- **FR-018**: System MUST allow definition of custom LoRaWAN commands for device models with frame data, port configuration, and confirmation requirements
- **FR-019**: System MUST validate LoRaWAN command frames to ensure hexadecimal encoding and enforce maximum frame length of 255 characters
- **FR-020**: System MUST validate LoRaWAN port numbers to be within the valid range of 1-223
- **FR-021**: System MUST allow specification of whether commands require device acknowledgment (confirmed) or not (unconfirmed)
- **FR-022**: System MUST protect built-in commands from modification or deletion
- **FR-023**: System MUST support OTAA-specific configuration including RX1 datarate offset and RX2 datarate for receive window optimization
- **FR-024**: System MUST support ABP-specific configuration including frame counter start values (FCntUpStart, FCntDownStart) and relax mode
- **FR-025**: System MUST validate frame counter values to be within the range 0-4294967295 (32-bit unsigned integer)
- **FR-026**: System MUST support frame counter reset via FCntResetCounter for handling device resets
- **FR-027**: System MUST allow enabling/disabling downlink (cloud-to-device) messaging capability for device models
- **FR-028**: System MUST allow configuration of preferred receive window (RX1 or RX2) for downlink message delivery
- **FR-029**: System MUST allow configuration of custom RX delay timing between device transmission and receive windows
- **FR-030**: System MUST allow configuration of sensor decoder URL for custom payload processing before data storage
- **FR-031**: System MUST allow configuration of keep-alive timeout for automatic connection expiration after inactivity
- **FR-032**: System MUST allow assignment and removal of labels to device models for categorization and filtering
- **FR-033**: System MUST create enrollment groups in the cloud IoT platform (Azure IoT Hub or AWS IoT Core) when device models are created
- **FR-034**: System MUST delete enrollment groups from the cloud IoT platform when device models are deleted
- **FR-035**: System MUST update enrollment groups and device configurations in the cloud platform when device models are modified
- **FR-036**: System MUST translate device model configurations to cloud platform device twin desired properties for device synchronization
- **FR-037**: System MUST support both Azure IoT Hub and AWS IoT Core as cloud platform backends
- **FR-038**: System MUST enforce model:read permission for viewing device models and their properties
- **FR-039**: System MUST enforce model:write permission for creating, updating, and deleting device models
- **FR-040**: System MUST enforce LoRaWAN feature activation gate to block access when LoRaWAN functionality is disabled
- **FR-041**: System MUST rollback device model creation if enrollment group creation fails to maintain consistency
- **FR-042**: System MUST delete associated commands, labels, and images when a device model is deleted

### Key Entities

- **Device Model**: Template defining configuration and behavior for a category of LoRaWAN devices
  - Attributes: ModelId, Name, Description, IsBuiltin, SupportLoRaFeatures, Image, Labels
  - LoRaWAN-specific: UseOTAA, ClassType, Deduplication, PreferredWindow, Downlink, KeepAliveTimeout, SensorDecoder, ABPRelaxMode
  - Frame counters: FCntUpStart, FCntDownStart, FCntResetCounter, Supports32BitFCnt
  - Receive windows: RXDelay, RX1DROffset, RX2DataRate

- **Device Model Command**: Remote control instruction that can be sent to devices using a model
  - Attributes: Id, Name, Frame (hex-encoded payload), Confirmed, Port, IsBuiltin, DeviceModelId
  - Relationship: Many commands belong to one device model

- **Label**: Categorization tag for organizing device models
  - Attributes: Name, Value, Color
  - Relationship: Many-to-many with device models

- **Enrollment Group**: Cloud platform entity enabling automatic device provisioning based on model configuration
  - Created/deleted/updated by the system in response to model lifecycle operations
  - Contains device twin desired properties derived from model configuration

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Administrators can browse and search the complete device model catalog in under 3 seconds regardless of catalog size (with pagination)
- **SC-002**: Network engineers can create a new LoRaWAN device model with full protocol configuration in under 5 minutes
- **SC-003**: Device model configuration changes are synchronized to all associated devices via cloud platform within 2 minutes
- **SC-004**: System prevents 100% of attempted deletions of device models that are in use by active devices
- **SC-005**: Command frame validation catches 100% of invalid hexadecimal formats before saving
- **SC-006**: Authentication mode immutability is enforced 100% of the time after initial model creation
- **SC-007**: Built-in models are protected from deletion/modification with 100% reliability
- **SC-008**: Enrollment group creation and device model database operations maintain consistency in 100% of scenarios (all-or-nothing transaction)
- **SC-009**: Users with only model:read permission are blocked from all write operations with 100% effectiveness
- **SC-010**: Device model avatar images load and display in under 2 seconds
- **SC-011**: Full-text search returns filtered results in under 1 second for catalogs with up to 1000 models
- **SC-012**: 90% of device provisioning errors are reduced by using standardized device model templates instead of per-device configuration
- **SC-013**: Network engineers can identify device types by avatar in under 2 seconds when selecting models
- **SC-014**: Frame counter validation prevents 100% of values outside the valid range 0-4294967295
- **SC-015**: System successfully creates enrollment groups in both Azure IoT Hub and AWS IoT Core with provider abstraction layer

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/009-lorawan-device-model-management/analyze.md`
- **Analyzed By**: excavate.specifier
- **Analysis Date**: 2025-01-30

### Code References

**Controllers & Entry Points**:
- `src/IoTHub.Portal.Server/Controllers/v1.0/LoRaWAN/LoRaWANDeviceModelsController.cs` (Lines 1-138)
  - REST API for device model CRUD operations
  - Authorization: model:read, model:write
  - Feature gate: LoRaFeatureActiveFilter
- `src/IoTHub.Portal.Server/Controllers/v1.0/LoRaWAN/LoRaWANCommandsController.cs` (Lines 1-65)
  - REST API for command management
  - Endpoints: GET/POST /api/lorawan/models/{id}/commands
- `src/IoTHub.Portal.Server/Controllers/v1.0/DeviceModelControllerBase.cs` (Lines 1-138)
  - Generic base controller shared with standard device models

**Business Logic**:
- `src/IoTHub.Portal.Application/Services/IDeviceModelService.cs` (Lines 1-26)
  - Service interface: GetDeviceModels, GetDeviceModel, CreateDeviceModel, UpdateDeviceModel, DeleteDeviceModel
- `src/IoTHub.Portal.Server/Services/DeviceModelService.cs` (Lines 1-190)
  - Concrete implementation with enrollment group and configuration management
- `src/IoTHub.Portal.Application/Services/ILoRaWANCommandService.cs`
  - Command service: GetDeviceModelCommandsFromModel, PostDeviceModelCommands

**Data Layer**:
- `src/IoTHub.Portal.Domain/Entities/DeviceModel.cs` (Lines 1-41)
  - Core entity with SupportLoRaFeatures flag and LoRaWAN properties
- `src/IoTHub.Portal.Domain/Entities/DeviceModelCommand.cs` (Lines 1-20)
  - Command entity: Name, Frame, Confirmed, Port, IsBuiltin
- `src/IoTHub.Portal.Domain/Repositories/IDeviceModelRepository.cs` (Lines 1-10)
  - Repository interface: GetByNameAsync
- `src/IoTHub.Portal.Domain/Repositories/IDeviceModelCommandRepository.cs`
  - Command repository for persistence

**Data Transfer Objects**:
- `src/IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/LoRaDeviceModelDto.cs` (Lines 1-89)
  - Complete LoRaWAN device model DTO with validation
- `src/IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/LoRaDeviceBase.cs` (Lines 1-100)
  - Base class with LoRaWAN protocol properties and defaults
- `src/IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/DeviceModelCommandDto.cs`
  - Command DTO with frame validation regex

**UI Components**:
- `src/IoTHub.Portal.Client/Pages/DeviceModels/DeviceModelListPage.razor` (Lines 1-150+)
  - Paginated list with search and sorting
- `src/IoTHub.Portal.Client/Components/DeviceModels/LoRaWAN/CreateLoraDeviceModel.razor` (Lines 1-50+)
  - Creation form with LoRaWAN configuration
- `src/IoTHub.Portal.Client/Components/DeviceModels/LoRaWAN/EditLoraDeviceModel.razor` (Lines 1-100+)
  - Edit form with OTAA/ABP sections and receive window configuration

**Mappers**:
- `src/IoTHub.Portal.Infrastructure/Mappers/IDeviceModelMapper.cs` (Lines 1-16)
  - Generic mapper interface
- `src/IoTHub.Portal.Infrastructure/Mappers/LoRaDeviceModelMapper.cs` (Lines 1-79)
  - LoRaWAN-specific mapper building device twin properties

### Dependencies

**Depends On** (Prerequisites):
- Standard Device Model Management - Generic base controller and service layer
- Label Management - Model categorization system
- Authentication & Authorization - Permission enforcement (model:read, model:write)
- Configuration Management - Portal settings for CloudProvider selection
- Azure IoT Hub / AWS IoT Core - Enrollment group and device twin management

**Depended On By** (Features that need this):
- LoRaWAN Device Management - Devices must reference a LoRaWAN device model for provisioning
- Device Provisioning - Models provide templates for automatic device onboarding
- Command Execution - Commands defined on models are available to devices

### API Endpoints

**Device Model Management**:
- `GET /api/lorawan/models` - List LoRaWAN device models (paginated, searchable, sortable)
- `GET /api/lorawan/models/{id}` - Get device model details
- `POST /api/lorawan/models` - Create new device model
- `PUT /api/lorawan/models` - Update device model
- `DELETE /api/lorawan/models/{id}` - Delete device model

**Avatar Management**:
- `GET /api/lorawan/models/{id}/avatar` - Get avatar URL
- `POST /api/lorawan/models/{id}/avatar` - Upload/update avatar
- `DELETE /api/lorawan/models/{id}/avatar` - Delete avatar

**Command Management**:
- `GET /api/lorawan/models/{id}/commands` - Get model commands
- `POST /api/lorawan/models/{id}/commands` - Set model commands

### LoRaWAN Protocol Specifications

**Authentication Modes**:
- OTAA (Over-The-Air Activation): Secure activation using network and application keys
- ABP (Activation By Personalization): Pre-provisioned activation with fixed session keys

**Device Classes**:
- Class A: Bi-directional with scheduled receive windows after uplink (most power efficient)
- Class B: Bi-directional with scheduled receive windows (synchronized)
- Class C: Bi-directional with continuous receive windows (highest power consumption)

**Deduplication Modes**:
- None: Process all messages from all gateways
- Drop: Keep only first message, discard duplicates
- Mark: Process all messages but flag duplicates

**Frame Counter Range**: 0 to 4,294,967,295 (32-bit unsigned integer)

**Command Port Range**: 1 to 223 (LoRaWAN application ports)

**Frame Length**: Maximum 255 characters (hex-encoded)

### Cloud Platform Integration

**Azure IoT Hub**:
- Enrollment groups created via IDeviceRegistryProvider
- Device twin desired properties via IConfigService
- Multi-cloud abstraction via provider interfaces

**AWS IoT Core**:
- Alternative cloud backend with same provider interface
- Enrollment group equivalents via AWS IoT provisioning templates

---

## Notes

### Implementation Considerations

This specification is derived from comprehensive code analysis and represents the **what** and **why** of LoRaWAN Device Model Management. Implementation teams should note:

- **Generic Architecture**: The system uses generic base controllers and services (DeviceModelsControllerBase<TListItem, TModel>, IDeviceModelService<TListItem, TModel>) to share code between standard and LoRaWAN device models while maintaining type safety.

- **Immutability Pattern**: OTAA/ABP authentication mode is immutable after creation to prevent provisioning inconsistencies. This is enforced at both UI (disabled toggle) and potentially API layers.

- **Transactional Consistency**: Device model creation must be atomic - if enrollment group creation in the cloud platform fails, the model should not be persisted to maintain consistency.

- **Multi-Cloud Abstraction**: The system abstracts cloud platform differences (Azure IoT Hub vs AWS IoT Core) through provider interfaces, enabling portability without code duplication.

- **Mapper Pattern**: Device model configurations are translated to device twin desired properties via the mapper pattern, centralizing the logic for property transformation.

- **Feature Gating**: All LoRaWAN endpoints are protected by LoRaFeatureActiveFilter, allowing the LoRaWAN feature to be disabled without affecting standard device model functionality.

- **Default Values**: The LoRaDeviceBase and LoRaDeviceModelDto classes define sensible defaults (ClassType.A, PreferredWindow=1, Deduplication=None, ABPRelaxMode=true, Downlink=true) to simplify model creation.

- **Validation Strategy**: Multi-layer validation occurs at DTO (data annotations), service (business rules), and repository (constraints) levels to ensure data integrity.

- **Permission Model**: Coarse-grained permissions (model:read, model:write) provide role-based access control without excessive permission granularity.

- **Optimistic UI**: UI likely uses optimistic updates with error handling for better user experience during model operations.

### Business Value

LoRaWAN Device Model Management delivers value through:

1. **Standardization**: Reusable templates eliminate per-device configuration errors
2. **Scalability**: Automatic provisioning via enrollment groups enables rapid device onboarding
3. **Consistency**: Centralized configuration management ensures uniform behavior across device fleets
4. **Flexibility**: Support for both OTAA and ABP authentication accommodates different security and deployment requirements
5. **Optimization**: Granular protocol configuration enables network performance tuning and power consumption optimization
6. **Control**: Command management provides standardized remote device control capabilities
7. **Multi-Cloud**: Provider abstraction protects against cloud platform lock-in

### Testing Strategy

Comprehensive testing should cover:

- **Unit Tests**: Controller actions, service methods, mappers, validators
- **Integration Tests**: Cloud platform enrollment group operations, device twin updates
- **Authorization Tests**: Permission enforcement for all read/write operations
- **Validation Tests**: Frame format, frame counter ranges, port numbers, authentication mode immutability
- **Deletion Tests**: In-use model protection, built-in model protection, cascade deletion
- **UI Tests**: Search, filtering, pagination, conditional rendering based on permissions
- **Multi-Cloud Tests**: Both Azure IoT Hub and AWS IoT Core provider implementations
