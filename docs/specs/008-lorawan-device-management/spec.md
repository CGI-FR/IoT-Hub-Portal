# Feature Specification: LoRaWAN Device Management

**Feature ID**: 008  
**Feature Branch**: `008-lorawan-device-management`  
**Created**: January 30, 2025  
**Status**: Draft  
**Source**: Analysis from `specs/008-lorawan-device-management/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - Register New LoRaWAN Device with OTAA Authentication (Priority: P1)

An IoT administrator needs to onboard a new LoRaWAN sensor to the network using Over-The-Air Activation (OTAA), which provides secure, dynamic key negotiation. The administrator enters the device's unique 16-character identifier, assigns a descriptive name, selects the device model, and configures the LoRaWAN authentication credentials (Application EUI and Application Key). The system validates the device ID format, stores the device configuration, and provisions it to the cloud platform for immediate connectivity.

**Why this priority**: Device registration is the foundational operation for any device management system. OTAA is the recommended production authentication method for LoRaWAN devices, providing enhanced security through dynamic session key negotiation. Without this capability, no devices can be managed.

**Independent Test**: Can be fully tested by registering a single OTAA device with valid credentials and verifying the device appears in the device list with correct configuration, delivering immediate value for organizations deploying their first LoRaWAN sensors.

**Acceptance Scenarios**:

1. **Given** an administrator has device credentials from the manufacturer, **When** they submit a new device with a valid 16-character hexadecimal device ID, device name, model selection, Application EUI, and Application Key, **Then** the system creates the device, provisions it to the cloud platform, and displays a success confirmation
2. **Given** an administrator attempts to register a device, **When** they provide an invalid device ID (not 16 hexadecimal characters), **Then** the system displays a validation error explaining the required format before submission
3. **Given** a device already exists in the system, **When** an administrator attempts to register another device with the same device ID, **Then** the system prevents duplicate registration and displays an error indicating the device already exists
4. **Given** an administrator is creating an OTAA device, **When** they omit the Application Key or Application EUI, **Then** the system displays validation errors indicating these fields are required for OTAA authentication
5. **Given** an administrator successfully creates a device, **When** the device connects to the network for the first time, **Then** the system records the first connection timestamp and updates the device status

---

### User Story 2 - View and Search Device Inventory (Priority: P1)

An IoT operations team needs to view all LoRaWAN devices in their fleet with the ability to search by name or identifier, filter by connection status (connected/disconnected), filter by enabled state, and filter by device model. The system displays a paginated list showing key information: device name, identifier, model, connection status, and last activity time. Users can navigate through pages of results and adjust the number of devices shown per page.

**Why this priority**: Visibility into the device fleet is essential for operations teams to monitor their IoT deployment. The ability to search and filter enables rapid troubleshooting and fleet management at scale. This is a core capability used daily by administrators.

**Independent Test**: Can be fully tested by registering multiple devices with varying statuses and models, then verifying search, filtering, pagination, and sorting functions work correctly, providing immediate operational visibility.

**Acceptance Scenarios**:

1. **Given** the system contains multiple LoRaWAN devices, **When** a user views the device list, **Then** the system displays devices in a paginated table showing device name, identifier, model, connection status, enabled state, and last activity time
2. **Given** a user is viewing the device list, **When** they enter a search term matching a device name or identifier, **Then** the system filters the list to show only matching devices in real-time
3. **Given** a user wants to find disconnected devices, **When** they filter by connection status "disconnected", **Then** the system shows only devices that are not currently connected to the network
4. **Given** a user is viewing many devices, **When** they change the page size setting, **Then** the system adjusts the number of devices displayed per page accordingly
5. **Given** a user has filtered the device list, **When** they navigate to subsequent pages, **Then** the filters remain applied across all pages
6. **Given** a device list contains devices from multiple models, **When** a user filters by a specific device model, **Then** the system shows only devices of that model type

---

### User Story 3 - Update Device Configuration and LoRaWAN Parameters (Priority: P1)

An IoT administrator needs to modify a device's configuration including changing the device name, updating tags for organization, adjusting LoRaWAN-specific parameters (device class, preferred receive window, frame counter settings), changing gateway assignments, or updating authentication credentials. The system retrieves the current device configuration, allows the administrator to modify editable fields through an intuitive interface, validates changes, and synchronizes the updated configuration to the cloud platform.

**Why this priority**: Device configurations need adjustment throughout the device lifecycle as network conditions change, devices are relocated, or operational requirements evolve. The ability to update LoRaWAN-specific parameters like class type and frame counters directly impacts device battery life and network performance.

**Independent Test**: Can be fully tested by updating an existing device's configuration parameters, verifying the changes persist and synchronize correctly, and confirming the device operates with the new settings.

**Acceptance Scenarios**:

1. **Given** an administrator is viewing a device's details, **When** they modify the device name and save, **Then** the system updates the device name and displays it throughout the interface
2. **Given** a device is configured for Class A operation, **When** an administrator changes the device class to Class C for continuous receive mode, **Then** the system updates the class type and synchronizes to the cloud platform
3. **Given** a device has specific frame counter values, **When** an administrator resets the frame counters, **Then** the system updates the uplink and downlink frame counter start values as specified
4. **Given** a device is assigned to one gateway, **When** an administrator changes the gateway assignment, **Then** the system updates the gateway routing configuration
5. **Given** an administrator updates multiple device parameters, **When** a validation error occurs on one field, **Then** the system displays the specific error without discarding other valid changes
6. **Given** a device is using OTAA authentication, **When** an administrator switches to ABP mode, **Then** the system prompts for required ABP credentials (Device Address, Application Session Key, Network Session Key) before allowing the save
7. **Given** an administrator modifies device tags, **When** they add, update, or remove custom tags, **Then** the system saves the tag changes and makes them available for filtering and search

---

### User Story 4 - Delete Device from System (Priority: P2)

An IoT administrator needs to permanently remove a device from the system when it is decommissioned, lost, or no longer needed. The system removes the device configuration from both the local database and the cloud platform, including all associated data such as device tags, labels, and historical telemetry. The operation is irreversible and requires appropriate permissions.

**Why this priority**: Device removal is important for maintaining an accurate device inventory and ensuring decommissioned devices cannot connect to the network. While critical for security and lifecycle management, it occurs less frequently than registration and updates, making it P2.

**Independent Test**: Can be fully tested by deleting a previously registered device and verifying it no longer appears in the device list, cannot be retrieved via the API, and is removed from the cloud platform.

**Acceptance Scenarios**:

1. **Given** an administrator has identified a device for decommissioning, **When** they delete the device and confirm the action, **Then** the system removes the device from the database and cloud platform and displays a success confirmation
2. **Given** a device has associated telemetry data, **When** the device is deleted, **Then** the system removes all related telemetry records to maintain data integrity
3. **Given** a device has custom tags and labels, **When** the device is deleted, **Then** the system removes all device-specific tag values and label associations
4. **Given** a user attempts to access a deleted device, **When** they navigate to the device's detail page or request it via the API, **Then** the system returns a "not found" error
5. **Given** a device deletion is requested, **When** the cloud platform synchronization fails, **Then** the system handles the error gracefully and informs the administrator of the issue

---

### User Story 5 - Monitor Device Telemetry and Activity (Priority: P2)

An IoT operations engineer needs to view telemetry data transmitted by LoRaWAN devices to verify sensor readings, troubleshoot connectivity issues, and monitor device health. The system displays recent telemetry messages with timestamps and decoded payload data. Engineers can view telemetry history for individual devices to identify patterns or anomalies.

**Why this priority**: Telemetry monitoring is essential for operational oversight and troubleshooting. While critical for maintaining device health, it's a monitoring function rather than a configuration function, making it P2. It's used regularly but not as frequently as device registration or updates.

**Independent Test**: Can be fully tested by retrieving telemetry for a device that has sent messages, verifying the data displays correctly with timestamps and decoded values, and confirming the telemetry history is maintained.

**Acceptance Scenarios**:

1. **Given** a device has transmitted telemetry messages, **When** an engineer views the device's telemetry page, **Then** the system displays the recent telemetry entries with timestamps and decoded payload data
2. **Given** a device has a configured sensor decoder, **When** telemetry arrives with binary payload, **Then** the system decodes the payload into human-readable sensor values
3. **Given** telemetry history exceeds the retention limit, **When** new telemetry arrives, **Then** the system maintains only the most recent entries and removes older data
4. **Given** a device is in range of multiple gateways, **When** the same telemetry message is received by multiple gateways, **Then** the system applies the configured deduplication mode (none, drop, or mark) to handle duplicates
5. **Given** an engineer is troubleshooting connectivity, **When** they view a device's last activity time, **Then** the system displays the timestamp of the most recent communication
6. **Given** a device has never sent telemetry, **When** an engineer views the telemetry page, **Then** the system displays an appropriate message indicating no data is available yet

---

### User Story 6 - Execute Commands on LoRaWAN Devices (Priority: P2)

An IoT administrator needs to remotely execute commands on LoRaWAN devices to trigger actions such as configuration changes, sensor calibration, or device resets. The administrator selects a device, chooses from available commands defined in the device model, and sends the command. The system transmits the command as a cloud-to-device message through the LoRaWAN network.

**Why this priority**: Remote command execution enables critical device management without physical access, particularly important for devices in remote or hard-to-reach locations. It's essential for operational efficiency but not required for basic device connectivity, making it P2.

**Independent Test**: Can be fully tested by executing a command on a connected device and verifying the command is transmitted through the cloud platform, providing immediate remote management capability.

**Acceptance Scenarios**:

1. **Given** a device model defines available commands, **When** an administrator views a device's command panel, **Then** the system displays all commands available for that device model
2. **Given** an administrator selects a command, **When** they execute it on a connected device, **Then** the system sends the command as a cloud-to-device message and displays a confirmation
3. **Given** a device is disconnected, **When** an administrator attempts to execute a command, **Then** the system warns that the device is offline and the command will be delivered when the device reconnects
4. **Given** a command execution fails due to network issues, **When** the error occurs, **Then** the system displays an error message with details about the failure
5. **Given** an administrator lacks command execution permissions, **When** they attempt to execute a command, **Then** the system denies the action and displays an authorization error

---

### User Story 7 - Configure ABP Authentication for Testing Devices (Priority: P3)

An IoT developer needs to quickly onboard test devices using Activation By Personalization (ABP) for faster initial connectivity during development and testing. The developer registers the device with ABP mode selected, provides the pre-configured Device Address, Application Session Key, and Network Session Key, and configures frame counter settings with relaxed validation for testing scenarios.

**Why this priority**: ABP authentication is valuable for testing and development scenarios where rapid device onboarding is prioritized over maximum security. Production deployments should use OTAA, making this a lower priority feature for specialized use cases.

**Independent Test**: Can be fully tested by registering an ABP device with manual credentials, verifying it connects without a join procedure, and confirming frame counter configurations work in relaxed mode.

**Acceptance Scenarios**:

1. **Given** a developer has ABP credentials from a test device, **When** they create a device with ABP mode selected and provide Device Address, Application Session Key, and Network Session Key, **Then** the system creates the device with ABP authentication
2. **Given** an ABP device is registered, **When** it attempts to communicate, **Then** the device connects immediately without performing an OTAA join procedure
3. **Given** a developer is configuring an ABP test device, **When** they enable ABP relaxed mode, **Then** the system allows more flexible frame counter validation for testing purposes
4. **Given** an ABP device has frame counter issues, **When** a developer resets the frame counters, **Then** the system updates the starting frame counter values to resolve the issue
5. **Given** a developer creates an ABP device, **When** they omit required ABP credentials, **Then** the system displays validation errors for the missing fields

---

### User Story 8 - Assign Gateway Routing for Multi-Gateway Deployments (Priority: P3)

A network administrator managing a LoRaWAN deployment with multiple gateways needs to assign specific devices to preferred gateways for optimized routing or to isolate device traffic. The administrator views available gateways, selects a gateway for a device, and saves the configuration. The system routes the device's traffic through the assigned gateway.

**Why this priority**: Gateway assignment is important for advanced network optimization in multi-gateway deployments but is optional for basic device operation. Most devices can operate without explicit gateway assignment, making this a P3 enhancement feature.

**Independent Test**: Can be fully tested by assigning a device to a specific gateway and verifying the routing configuration is applied, useful for network segmentation and optimization scenarios.

**Acceptance Scenarios**:

1. **Given** the system has multiple configured gateways, **When** an administrator views the gateway assignment field, **Then** the system displays a list of available gateways to choose from
2. **Given** an administrator assigns a device to a specific gateway, **When** they save the configuration, **Then** the system routes the device's traffic through the assigned gateway
3. **Given** a device is assigned to a gateway, **When** the administrator removes the gateway assignment, **Then** the device can communicate through any available gateway
4. **Given** network conditions require device reassignment, **When** an administrator changes the gateway assignment, **Then** the system updates the routing configuration immediately

---

### User Story 9 - Manage Device Class Types for Power Optimization (Priority: P3)

An IoT operations engineer needs to configure device class types (Class A, B, or C) to balance power consumption with downlink communication requirements. Class A devices minimize power consumption with receive windows only after uplinks. Class B devices support scheduled downlink windows. Class C devices maintain continuous receive for immediate downlink but consume more power.

**Why this priority**: Device class configuration is important for optimizing battery life and downlink latency, but devices can operate with default Class A settings. Class reconfiguration is typically done during initial deployment or when requirements change, making it P3.

**Independent Test**: Can be fully tested by changing a device's class type and verifying the cloud platform reflects the new class configuration, enabling power consumption optimization.

**Acceptance Scenarios**:

1. **Given** a device is configured as Class A, **When** an engineer changes it to Class C for continuous receive mode, **Then** the system updates the device class and synchronizes to the cloud platform
2. **Given** a battery-powered device needs power optimization, **When** an engineer sets it to Class A, **Then** the device operates with minimal power consumption and receive windows only after transmissions
3. **Given** a device requires scheduled downlinks, **When** an engineer configures it as Class B, **Then** the system enables synchronized receive windows based on network beacons
4. **Given** an engineer is selecting device class, **When** they view the options, **Then** the system displays descriptions of each class type to guide the selection

---

### User Story 10 - Configure Receive Window Parameters for Network Optimization (Priority: P3)

A network engineer needs to fine-tune receive window parameters including preferred window (RX1 or RX2), RX1 data rate offset, RX2 data rate, and receive delay to optimize network performance and reliability. These parameters control when and how devices receive downlink messages from the network.

**Why this priority**: Receive window tuning is an advanced feature for network optimization, typically needed only in specific deployment scenarios or to address network coverage issues. Default parameters work for most deployments, making this P3.

**Independent Test**: Can be fully tested by configuring custom receive window parameters and verifying they synchronize to the cloud platform, providing advanced network tuning capabilities.

**Acceptance Scenarios**:

1. **Given** a device uses default receive window settings, **When** an engineer configures a custom RX2 data rate, **Then** the system updates the RX2 window data rate for OTAA devices
2. **Given** network conditions require receive window adjustment, **When** an engineer sets the RX1 data rate offset, **Then** the system configures the offset between RX and TX data rates
3. **Given** a device needs custom timing, **When** an engineer configures the receive delay, **Then** the system sets the wait time between receive and transmit per LoRaWAN specifications
4. **Given** an engineer is optimizing downlink reliability, **When** they select the preferred window (RX1 or RX2), **Then** the system prioritizes the selected window for downlink communication

---

### User Story 11 - Configure Sensor Decoder for Payload Interpretation (Priority: P3)

An IoT administrator needs to configure a sensor decoder URL for devices that transmit custom binary payloads. The decoder service converts the binary LoRaWAN payload into human-readable JSON telemetry data. The administrator enters the decoder service URL when creating or updating a device.

**Why this priority**: Sensor decoders enable interpretation of custom device payloads, essential for displaying meaningful telemetry data. However, devices can operate and transmit data without decoders (raw payload is still stored), making this a P3 enhancement feature.

**Independent Test**: Can be fully tested by configuring a decoder URL and verifying that incoming telemetry is processed through the decoder service to produce human-readable values.

**Acceptance Scenarios**:

1. **Given** a device transmits binary sensor data, **When** an administrator configures a decoder URL, **Then** the system uses the decoder service to convert binary payloads to JSON telemetry
2. **Given** a device model has standard payload format, **When** an administrator assigns a device to that model, **Then** the system automatically applies the model's decoder configuration
3. **Given** a decoder service is unavailable, **When** telemetry arrives, **Then** the system stores the raw payload and attempts decoding on subsequent messages
4. **Given** an administrator updates a decoder URL, **When** new telemetry arrives, **Then** the system uses the updated decoder for payload interpretation

---

### User Story 12 - Organize Devices with Tags and Labels (Priority: P4)

An IoT administrator needs to organize devices using custom tags (name-value pairs) and labels (categorical markers) for filtering, searching, and reporting. Tags might include location, department, or custom metadata. Labels provide simple categorization. The administrator adds, updates, or removes tags and labels when creating or editing devices.

**Why this priority**: Tags and labels enhance device organization and enable advanced filtering, but are not required for core device operation. They provide operational efficiency improvements, making them P4 nice-to-have features.

**Independent Test**: Can be fully tested by adding tags and labels to devices and verifying they can be used for filtering and searching in the device list.

**Acceptance Scenarios**:

1. **Given** an administrator is editing a device, **When** they add custom tags with name-value pairs (e.g., "location: warehouse-A"), **Then** the system saves the tags and makes them searchable
2. **Given** devices have been tagged, **When** a user filters the device list by a specific tag, **Then** the system shows only devices matching that tag
3. **Given** an administrator assigns labels to devices, **When** they select from available labels, **Then** the system associates the labels with the device for categorization
4. **Given** multiple devices share common characteristics, **When** an administrator applies the same label to all, **Then** the system enables bulk filtering and reporting by that label
5. **Given** tag or label requirements change, **When** an administrator removes tags or labels from a device, **Then** the system updates the device's metadata immediately

---

### User Story 13 - View Adaptive Data Rate (ADR) Reported Values (Priority: P4)

A network engineer needs to view adaptive data rate (ADR) values reported by devices to understand how the network server has optimized transmission parameters. Reported values include current data rate, transmit power, number of repetitions, and receive window parameters. These values show the actual operating parameters negotiated between the device and network server.

**Why this priority**: ADR reported values provide visibility into network optimization but are read-only monitoring data. They're useful for advanced troubleshooting but not required for device operation, making them P4.

**Independent Test**: Can be fully tested by viewing a device's details and verifying ADR reported values display correctly when present from the cloud platform.

**Acceptance Scenarios**:

1. **Given** a device has connected and negotiated ADR parameters, **When** an engineer views the device details, **Then** the system displays reported data rate, transmit power, and repetition values
2. **Given** ADR is actively optimizing device parameters, **When** reported values change, **Then** the system updates the displayed values from the cloud platform
3. **Given** a device has not yet reported ADR values, **When** an engineer views the device, **Then** the system indicates that ADR values are not yet available
4. **Given** an engineer is troubleshooting battery life, **When** they review reported transmit power levels, **Then** they can assess whether the network has optimized for power consumption

---

### User Story 14 - Duplicate Device Configuration for Rapid Deployment (Priority: P4)

An IoT administrator deploying multiple similar devices needs to duplicate an existing device's configuration to speed up registration of new devices with similar settings. The administrator selects "Save and Duplicate" when editing a device, which creates a new device form pre-filled with the current device's configuration except for unique identifiers.

**Why this priority**: Device duplication is a convenience feature that improves operational efficiency during bulk deployments but is not essential. Devices can always be registered individually, making this P4.

**Independent Test**: Can be fully tested by duplicating a device configuration, updating the unique device ID, and verifying the new device is created with copied settings.

**Acceptance Scenarios**:

1. **Given** an administrator has configured a device with specific settings, **When** they select "Save and Duplicate", **Then** the system opens a new device form with all settings copied except device ID and name
2. **Given** a duplicated device form is displayed, **When** the administrator updates the device ID and name with unique values, **Then** the system creates a new device with the duplicated configuration
3. **Given** multiple similar devices need deployment, **When** an administrator uses duplication repeatedly, **Then** they can rapidly onboard devices without re-entering common configuration parameters

---

### Edge Cases

**Device ID Validation**:
- What happens when a user attempts to register a device with a device ID containing lowercase letters or special characters? The system must reject the registration and display a clear error message explaining the required format (16 uppercase hexadecimal characters).
- How does the system handle device IDs that are too short or too long? The system validates the exact 16-character length and provides specific feedback.

**Authentication Mode Switching**:
- What happens when a user switches from OTAA to ABP mode without providing ABP credentials? The system must validate that all required ABP fields (DevAddr, AppSKey, NwkSKey) are populated before allowing the save.
- How does the system handle switching from ABP to OTAA on a device that has already connected? The device will require a new join procedure, and frame counters will reset according to OTAA negotiation.

**Frame Counter Boundaries**:
- What happens when frame counters reach the maximum value (4,294,967,295 for 32-bit counters)? The system should allow frame counter reset or device re-provisioning to prevent counter exhaustion.
- How does the system handle frame counter resets in ABP mode? The ABP relaxed mode should be configured to prevent replay attack protection from blocking legitimate resets during testing.

**Gateway Assignment**:
- What happens when a device is assigned to a gateway that goes offline? The device should still be able to communicate through other available gateways if no explicit assignment is made.
- How does the system handle deletion of a gateway that has devices assigned to it? The system should either prevent gateway deletion or clear gateway assignments from affected devices.

**Telemetry Deduplication**:
- What happens when the same message is received by multiple gateways with deduplication set to "Drop"? The system processes the first message received and ignores subsequent duplicates based on sequence number.
- How does the system handle "Mark" deduplication mode? The system processes all messages but marks duplicates with metadata indicating they are duplicates.

**Telemetry Storage Limits**:
- What happens when telemetry history reaches the retention limit? The system automatically removes the oldest telemetry entries while retaining the most recent messages.
- How does the system handle high-frequency telemetry from devices? Rate limiting or aggregation may be necessary to prevent database growth.

**Sensor Decoder Failures**:
- What happens when the configured decoder URL is unreachable? The system stores the raw binary payload and logs the decoder failure, allowing retry or manual decoding later.
- How does the system handle decoder responses that return invalid JSON? Error handling should gracefully store the raw payload and alert administrators to the decoder issue.

**Cloud Platform Synchronization**:
- What happens when device creation succeeds locally but fails to provision to the cloud platform? The system should roll back the local creation or mark the device as pending synchronization for retry.
- How does the system handle cloud platform disconnections during device updates? Transactional integrity should ensure either both local and cloud updates succeed or both fail.

**Permission Boundaries**:
- What happens when a user with read-only permissions attempts to update a device? The system displays appropriate read-only interfaces and prevents modification attempts.
- How does the system handle users with device:write permission but not device:execute permission? Command execution buttons are hidden or disabled, and API attempts return authorization errors.

**Device Model Dependencies**:
- What happens when an administrator creates a device with a model that doesn't exist? The system validates model existence and prevents device creation with invalid model references.
- How does the system handle model deletion when devices are assigned to that model? The system should either prevent model deletion or handle orphaned device references gracefully.

**Concurrent Modifications**:
- What happens when two administrators simultaneously edit the same device? The system should detect concurrent modifications and either use optimistic locking to prevent conflicts or notify the second user their changes may overwrite the first.

**Bulk Operations**:
- How does the system handle requests to view or export large device lists (1000+ devices)? Pagination ensures only a subset loads at once, and export operations should handle large datasets efficiently.

**Feature Toggle Scenarios**:
- What happens when the LoRa feature is disabled while users are viewing LoRaWAN devices? The system returns "not found" errors for LoRa endpoints and displays appropriate messages.

---

## Requirements

### Functional Requirements

**Device Registration**:
- **FR-001**: System MUST allow administrators to register new LoRaWAN devices with a unique 16-character hexadecimal device identifier (DevEUI)
- **FR-002**: System MUST validate device identifiers are exactly 16 characters containing only uppercase letters A-F and numbers 0-9
- **FR-003**: System MUST require a device name and device model selection for each registered device
- **FR-004**: System MUST prevent registration of duplicate device identifiers
- **FR-005**: System MUST provision registered devices to the cloud IoT platform for connectivity

**Authentication Configuration**:
- **FR-006**: System MUST support Over-The-Air Activation (OTAA) authentication requiring Application EUI and Application Key
- **FR-007**: System MUST support Activation By Personalization (ABP) authentication requiring Device Address, Application Session Key, and Network Session Key
- **FR-008**: System MUST enforce that either OTAA or ABP credentials are provided based on the selected authentication mode
- **FR-009**: System MUST default new devices to OTAA authentication mode as the recommended secure method

**Device Inventory Management**:
- **FR-010**: System MUST display a paginated list of all registered LoRaWAN devices
- **FR-011**: System MUST allow users to search devices by name or identifier
- **FR-012**: System MUST allow users to filter devices by connection status (connected/disconnected)
- **FR-013**: System MUST allow users to filter devices by enabled state (enabled/disabled)
- **FR-014**: System MUST allow users to filter devices by device model
- **FR-015**: System MUST display device name, identifier, model, connection status, enabled state, and last activity time for each device
- **FR-016**: System MUST support configurable page sizes for device lists
- **FR-017**: System MUST maintain filter and search criteria when navigating between pages

**Device Configuration Updates**:
- **FR-018**: System MUST allow administrators to update device names
- **FR-019**: System MUST allow administrators to change device model assignments
- **FR-020**: System MUST allow administrators to modify LoRaWAN device class (Class A, B, or C)
- **FR-021**: System MUST allow administrators to configure preferred receive window (RX1 or RX2)
- **FR-022**: System MUST allow administrators to configure receive window parameters (RX1 data rate offset, RX2 data rate, receive delay)
- **FR-023**: System MUST allow administrators to configure frame counter settings (uplink start, downlink start, reset counter, 32-bit support, relaxed mode)
- **FR-024**: System MUST allow administrators to assign devices to specific gateways
- **FR-025**: System MUST allow administrators to configure sensor decoder URLs for payload interpretation
- **FR-026**: System MUST allow administrators to enable or disable downlink communication
- **FR-027**: System MUST allow administrators to set keep-alive timeout values
- **FR-028**: System MUST allow administrators to switch between OTAA and ABP authentication modes with appropriate credential updates
- **FR-029**: System MUST synchronize configuration changes to the cloud IoT platform
- **FR-030**: System MUST validate all configuration changes before persisting

**Device Deletion**:
- **FR-031**: System MUST allow administrators to permanently delete devices
- **FR-032**: System MUST remove deleted devices from both local storage and cloud IoT platform
- **FR-033**: System MUST cascade deletion to remove associated device tags, labels, and telemetry data
- **FR-034**: System MUST prevent access to deleted devices through any interface

**Telemetry Management**:
- **FR-035**: System MUST receive and store telemetry messages transmitted by LoRaWAN devices
- **FR-036**: System MUST display telemetry history for individual devices with timestamps
- **FR-037**: System MUST decode telemetry payloads using configured sensor decoders
- **FR-038**: System MUST handle duplicate telemetry messages using configured deduplication mode (none, drop, or mark)
- **FR-039**: System MUST maintain a limited history of recent telemetry entries per device
- **FR-040**: System MUST automatically remove oldest telemetry entries when retention limits are reached
- **FR-041**: System MUST update device last activity time when telemetry is received

**Command Execution**:
- **FR-042**: System MUST allow administrators to execute commands on LoRaWAN devices
- **FR-043**: System MUST display available commands based on device model definitions
- **FR-044**: System MUST transmit commands as cloud-to-device messages through the LoRaWAN network
- **FR-045**: System MUST provide feedback on command execution success or failure

**Gateway Management**:
- **FR-046**: System MUST provide a list of available gateways for device assignment
- **FR-047**: System MUST allow optional gateway assignment to devices for traffic routing
- **FR-048**: System MUST support devices operating without explicit gateway assignment (automatic routing)

**Device Organization**:
- **FR-049**: System MUST allow administrators to add custom tags (name-value pairs) to devices
- **FR-050**: System MUST allow administrators to assign labels to devices for categorization
- **FR-051**: System MUST allow filtering and searching devices by tags and labels
- **FR-052**: System MUST synchronize device tags to cloud IoT platform metadata

**Adaptive Data Rate (ADR) Monitoring**:
- **FR-053**: System MUST display adaptive data rate values reported by devices including data rate, transmit power, and number of repetitions
- **FR-054**: System MUST display reported receive window parameters from connected devices
- **FR-055**: System MUST update displayed ADR values when they change on the cloud platform

**Deduplication Configuration**:
- **FR-056**: System MUST allow administrators to configure deduplication mode (none, drop, or mark) for handling messages received by multiple gateways
- **FR-057**: System MUST apply the configured deduplication strategy when processing telemetry

**Frame Counter Management**:
- **FR-058**: System MUST support frame counter configuration with values from 0 to 4,294,967,295
- **FR-059**: System MUST allow configuration of uplink and downlink frame counter start values
- **FR-060**: System MUST support frame counter reset functionality
- **FR-061**: System MUST support both 32-bit and 16-bit frame counter modes
- **FR-062**: System MUST support ABP relaxed mode for flexible frame counter validation during testing

**Connection Status**:
- **FR-063**: System MUST track and display device connection status (connected/disconnected)
- **FR-064**: System MUST track and display device enabled status
- **FR-065**: System MUST record device first connection timestamp
- **FR-066**: System MUST update device status based on cloud platform state

**Device Duplication**:
- **FR-067**: System MUST allow administrators to duplicate existing device configurations
- **FR-068**: System MUST copy all device settings except unique identifiers when duplicating
- **FR-069**: System MUST require unique device ID and name for duplicated devices

**Authorization and Access Control**:
- **FR-070**: System MUST enforce read permission for viewing devices and telemetry
- **FR-071**: System MUST enforce write permission for creating, updating, and deleting devices
- **FR-072**: System MUST enforce execute permission for sending commands to devices
- **FR-073**: System MUST restrict access to LoRaWAN features when LoRa support is disabled in configuration

**Data Validation**:
- **FR-074**: System MUST validate device identifier format before allowing registration
- **FR-075**: System MUST validate required fields are populated based on authentication mode
- **FR-076**: System MUST validate frame counter values are within valid range
- **FR-077**: System MUST validate receive window parameters are within specification limits
- **FR-078**: System MUST provide clear error messages for validation failures

**Cloud Platform Integration**:
- **FR-079**: System MUST create device twins in cloud IoT platform when devices are registered
- **FR-080**: System MUST synchronize device configuration to cloud platform desired properties
- **FR-081**: System MUST retrieve device reported properties from cloud platform
- **FR-082**: System MUST support both Azure IoT Hub and AWS IoT Core platforms
- **FR-083**: System MUST handle cloud platform communication failures gracefully

---

### Key Entities

**LoRaWAN Device**: Represents a LoRaWAN IoT sensor or endpoint with a unique 16-character hexadecimal identifier (DevEUI). Contains authentication credentials (OTAA or ABP), LoRaWAN-specific configuration parameters (class type, frame counters, receive windows), connectivity information (connection status, last activity time), and organizational metadata (tags, labels). Inherits common device properties such as name, model, enabled state, and cloud platform provisioning details.

**Device Model**: Defines the template for a category of devices including available commands, sensor decoder configuration, default settings, and visual representation. Devices are assigned to models that determine their capabilities and interface.

**Device Tag**: Custom name-value pair metadata attached to devices for organization, filtering, and reporting purposes. Examples include location information, department ownership, or environment designation (production/test).

**Device Label**: Simple categorical marker for device classification enabling filtering and grouping. Multiple labels can be assigned to a single device.

**Gateway**: LoRaWAN concentrator or base station that provides network connectivity for devices. Devices can be optionally assigned to specific gateways for routing control in multi-gateway deployments.

**Telemetry Entry**: Time-series data point transmitted by a device containing timestamp, raw binary payload, and decoded sensor values. Multiple telemetry entries form the device's communication history.

**Device Command**: Executable action defined by a device model that can be remotely triggered on a device. Commands are transmitted as cloud-to-device messages through the LoRaWAN network.

**Sensor Decoder**: Service endpoint that converts binary LoRaWAN payloads to human-readable JSON telemetry data. Each device model can specify a decoder for its payload format.

**Cloud Device Twin**: Cloud platform representation of a device containing desired properties (configuration), reported properties (device state), and tags (metadata). The twin synchronizes device configuration between local database and cloud platform.

---

## Success Criteria

### Measurable Outcomes

**Device Onboarding Efficiency**:
- **SC-001**: Administrators can register a new LoRaWAN device with OTAA authentication in under 2 minutes including credential entry and model selection
- **SC-002**: Device registration validation errors are displayed within 1 second of form submission with clear guidance on required corrections
- **SC-003**: 95% of device registrations succeed on first attempt without validation errors

**Operational Visibility**:
- **SC-004**: Device list displays 50+ devices with search, filter, and pagination in under 2 seconds
- **SC-005**: Search and filter operations return results within 1 second of user input
- **SC-006**: Administrators can locate a specific device by name or identifier in under 10 seconds regardless of fleet size

**Configuration Management**:
- **SC-007**: Device configuration updates synchronize to cloud platform within 5 seconds of save operation
- **SC-008**: LoRaWAN parameter changes (class type, receive windows, frame counters) take effect on next device communication
- **SC-009**: 98% of device updates complete successfully without synchronization errors

**Telemetry Monitoring**:
- **SC-010**: Telemetry data appears in device history within 30 seconds of device transmission
- **SC-011**: Sensor decoder processes and displays human-readable telemetry values within 5 seconds of receipt
- **SC-012**: System maintains telemetry history for at least the most recent 100 messages per device

**Remote Device Management**:
- **SC-013**: Command execution requests transmit to connected devices within 10 seconds
- **SC-014**: Administrators receive confirmation of command transmission within 15 seconds of execution request
- **SC-015**: Command execution success rate exceeds 95% for connected devices

**Fleet Management at Scale**:
- **SC-016**: System supports management of at least 1,000 LoRaWAN devices without performance degradation
- **SC-017**: Bulk filtering operations (e.g., "show all disconnected devices") complete in under 3 seconds for fleets up to 1,000 devices
- **SC-018**: Concurrent device updates by multiple administrators complete without data loss or conflicts

**User Productivity**:
- **SC-019**: Duplicate device function reduces time to register similar devices by at least 50%
- **SC-020**: Tag and label filtering reduces time to locate specific device categories by at least 60% compared to manual searching
- **SC-021**: Reduce administrator training time for device management tasks to under 1 hour through intuitive interface design

**Reliability and Data Integrity**:
- **SC-022**: Device data remains synchronized between local database and cloud platform with 99.9% accuracy
- **SC-023**: Device deletion successfully removes all associated data (tags, labels, telemetry) in 100% of cases
- **SC-024**: Telemetry deduplication prevents processing duplicate messages with 99% accuracy

**Security and Access Control**:
- **SC-025**: Unauthorized access attempts to device management functions result in appropriate error responses in 100% of cases
- **SC-026**: Device credentials (AppKey, AppSKey, NwkSKey) are stored and transmitted securely in all scenarios
- **SC-027**: Frame counter security prevents replay attacks on ABP devices in production deployments

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/008-lorawan-device-management/analyze.md`
- **Analyzed By**: excavator.specifier agent
- **Analysis Date**: January 30, 2025

### Code References

**Controllers**:
- `src/IoTHub.Portal.Server/Controllers/v1.0/LoRaWAN/LoRaWANDevicesController.cs` - REST API endpoints for LoRaWAN device CRUD operations, telemetry retrieval, command execution, and gateway management

**Business Logic**:
- `src/IoTHub.Portal.Infrastructure/Services/LoRaWanDeviceService.cs` - Core device service implementation with LoRa-specific business logic
- `src/IoTHub.Portal.Application/Services/ILoRaWANCommandService.cs` - Command execution service interface
- `src/IoTHub.Portal.Application/Services/IDeviceService.cs` - Generic device service interface

**Data Access**:
- `src/IoTHub.Portal.Domain/Repositories/ILorawanDeviceRepository.cs` - Device repository interface
- `src/IoTHub.Portal.Domain/Repositories/ILoRaDeviceTelemetryRepository.cs` - Telemetry repository interface
- `src/IoTHub.Portal.Domain/Entities/LorawanDevice.cs` - Device entity with comprehensive LoRa properties
- `src/IoTHub.Portal.Domain/Entities/LoRaDeviceTelemetry.cs` - Telemetry entity

**DTOs and Models**:
- `src/IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/LoRaDeviceDetails.cs` - Complete device DTO with validation
- `src/IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/LoRaDeviceBase.cs` - Base LoRa configuration properties
- `src/IoTHub.Portal.Shared/Models/v1.0/LoRaDeviceTelemetryDto.cs` - Telemetry DTO

**UI Components**:
- `src/IoTHub.Portal.Client/Pages/Devices/DeviceListPage.razor` - Device inventory list page
- `src/IoTHub.Portal.Client/Components/Devices/EditDevice.razor` - Device create/edit form with LoRa-specific sections
- `src/IoTHub.Portal.Client/Pages/Devices/DeviceDetailPage.razor` - Device detail view

**Client Services**:
- `src/IoTHub.Portal.Client/Services/LoRaWanDeviceClientService.cs` - HTTP client service for API communication

**Mappers**:
- `src/IoTHub.Portal.Infrastructure/Mappers/LoRaDeviceTwinMapper.cs` - IoT Hub device twin mapper
- `src/IoTHub.Portal.Infrastructure/Mappers/LoRaDeviceMapper.cs` - Entity to DTO mapper

### Dependencies

**Internal Feature Dependencies**:
- Device Models - Define available commands, decoder settings, and device capabilities
- Device Tag Settings - Provide custom tag definitions for device organization
- Label Management - Supply label definitions for device categorization
- IoT Hub Integration - Cloud platform device twin synchronization
- Role-Based Access Control - Enforce device:read, device:write, and device:execute permissions
- LoRaWAN Commands - Enable remote command execution on devices
- Gateway Management - Provide gateway list for device routing

**Features That Depend on This Feature**:
- LoRaWAN Device Models - Use device instances to test model configurations
- Telemetry Analytics - Aggregate telemetry data across LoRaWAN device fleet
- Device Monitoring Dashboards - Display LoRaWAN device health and connectivity
- Alert Management - Trigger alerts based on LoRaWAN device conditions
- Reporting Systems - Generate reports on LoRaWAN device inventory and usage

### Related Documentation
- LoRaWAN 1.0.x Specification - Protocol standards for device communication
- Azure IoT Hub Documentation - Cloud platform integration patterns
- AWS IoT Core Documentation - Alternative cloud platform support
- Feature Analysis Master File - `docs/analyze.md` containing feature inventory

---

## Notes

### LoRaWAN Protocol Context

This feature implements comprehensive support for the LoRaWAN protocol, which is specifically designed for low-power, wide-area network (LPWAN) IoT devices. LoRaWAN devices typically operate on battery power for years, communicate over distances of several kilometers, and transmit small amounts of data infrequently. The protocol includes sophisticated power management through device classes, security through frame counters and encrypted keys, and network optimization through adaptive data rate algorithms.

### Authentication Mode Guidance

OTAA (Over-The-Air Activation) is the recommended authentication method for production deployments because it provides dynamic session key generation during the join procedure, enhancing security. ABP (Activation By Personalization) uses pre-shared keys and is simpler but requires careful frame counter management to prevent replay attacks. ABP is suitable for testing and development scenarios where rapid connectivity is prioritized.

### Device Class Selection

Class A devices open receive windows only after transmitting, minimizing power consumption and maximizing battery life. Class C devices maintain continuous receive mode, enabling immediate downlink but consuming significantly more power. Class B provides a middle ground with scheduled receive windows. The class selection directly impacts battery life, downlink latency, and operational costs.

### Frame Counter Security

Frame counters are critical security features that prevent replay attacks where an attacker re-transmits captured messages. Each uplink and downlink message increments the respective counter, and the network validates counter progression. ABP relaxed mode loosens this validation for testing but should not be used in production. Devices should support 32-bit counters to prevent counter exhaustion over the device lifetime.

### Multi-Gateway Scenarios

In LoRaWAN networks, devices often communicate with multiple gateways simultaneously due to the long-range nature of LoRa radio. The same message may be received by multiple gateways, requiring deduplication to prevent duplicate processing. The deduplication mode controls whether duplicates are dropped, marked, or all processed. Gateway assignment can force routing through a specific gateway for network optimization.

### Telemetry Processing

Telemetry arrives as binary LoRaWAN payloads that require decoding based on device-specific formats. Sensor decoders convert these binary messages to human-readable JSON. The system maintains a limited telemetry history per device to prevent unbounded storage growth while providing recent operational visibility.

### Cloud Platform Integration

The feature abstracts cloud platform integration to support both Azure IoT Hub and AWS IoT Core. Device twins (or Thing Shadows in AWS) provide bidirectional synchronization: desired properties contain configuration sent from the cloud to devices, reported properties contain device state sent from devices to the cloud, and tags contain metadata for organization and filtering.

### Performance and Scale

The system is designed to manage fleets of thousands of LoRaWAN devices with pagination, efficient database queries, and cloud platform integration. Eager loading prevents N+1 query problems, telemetry retention limits prevent unbounded growth, and feature toggles allow disabling LoRa support when not needed.

### User Experience Considerations

The interface provides a unified experience for both standard IoT devices and LoRaWAN devices, with LoRa-specific sections appearing contextually. Validation provides real-time feedback, and the tabbed interface organizes complex LoRaWAN parameters logically. Device duplication reduces repetitive data entry during bulk deployments.
