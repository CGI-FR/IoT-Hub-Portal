# Feature Specification: Remove Connection State and Status Update Columns

**Feature Branch**: `001-remove-connection-columns`  
**Created**: 2026-02-08  
**Status**: Draft  
**Input**: User description: "Remove 'Connection State' and 'Last status update' columns from device and gateway views"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Device List Without Connection State (Priority: P1)

Users managing LoRaWAN devices that communicate intermittently (every 30 minutes to daily) need to view their device inventory without confusion about connectivity status. The current "Connection State" column shows devices as "Disconnected" most of the time because connections are only active during data transmission, leading users to believe devices are malfunctioning when they are operating normally.

**Why this priority**: This is the primary issue causing user confusion and unnecessary support requests. Removing this misleading column eliminates the main source of confusion for non-technical users managing infrequently-communicating devices.

**Independent Test**: Can be fully tested by viewing the device list page and confirming that the Connection State column is no longer displayed, while all other device information remains visible and functional.

**Acceptance Scenarios**:

1. **Given** a user navigates to the device list page, **When** the page loads, **Then** no "Connection State" or "Connected" column is displayed in the device table
2. **Given** a user views the device list, **When** looking at device information, **Then** all other device information (name, ID, model, labels) remains visible and unchanged
3. **Given** a user with search functionality, **When** using the search panel, **Then** the connection status filter options are removed

---

### User Story 2 - View Edge Device List Without Connection State (Priority: P1)

Users managing edge devices (gateways) face the same confusion as regular device users when viewing connection states. Edge devices that aggregate data from leaf devices may not maintain persistent connections, causing them to appear disconnected in the list view even when functioning properly.

**Why this priority**: This addresses the same core issue as User Story 1 but for edge devices. Both must be completed together to provide a consistent user experience across device types.

**Independent Test**: Can be fully tested by viewing the edge device list page and confirming that the Connection State column is no longer displayed, while edge device functionality remains intact.

**Acceptance Scenarios**:

1. **Given** a user navigates to the edge device list page, **When** the page loads, **Then** no "Status" or "Connected" column is displayed in the edge device table
2. **Given** a user views edge devices, **When** looking at device information, **Then** the count of connected leaf devices remains visible
3. **Given** a user with search functionality, **When** using the search panel, **Then** the connection status filter options (Connected/Disconnected/All) are removed

---

### User Story 3 - View Device Details Without Last Status Update (Priority: P2)

Users viewing detailed device information should not see the "Last status update" field which only updates when the Device Twin changes, not when the device sends data. This field creates confusion as it doesn't reflect actual device activity.

**Why this priority**: While less prominent than the list view columns, this detail page field contributes to the same confusion about device activity. It should be removed to maintain consistency, but is lower priority as fewer users access detail pages regularly.

**Independent Test**: Can be fully tested by viewing device detail pages and confirming the "Last status update" field is removed while other device details remain accessible.

**Acceptance Scenarios**:

1. **Given** a user views a device detail page, **When** examining device information, **Then** no "Last status update" field is displayed
2. **Given** a user needs to verify device activity, **When** viewing the detail page, **Then** the last activity time (if implemented separately) accurately reflects the last data transmission

---

### User Story 4 - Replace Status Update with Last Activity Time (Priority: P3)

For users who need to track actual device activity, the system should use the `lastActivityTime` field from Azure IoT Hub (and equivalent from AWS IoT Core if available) instead of the Device Twin update timestamp. This provides accurate information about when the device last communicated with the platform.

**Why this priority**: This is an enhancement that provides value but is not essential to solving the immediate problem. The primary goal is removing misleading information; adding accurate activity tracking is a secondary improvement that can be implemented after the main issue is resolved.

**Independent Test**: Can be tested independently by verifying that device lists display a "Last Activity Time" column showing the timestamp of the device's last communication with the platform, with data sourced from the appropriate cloud provider API.

**Acceptance Scenarios**:

1. **Given** devices are sending data to the platform, **When** viewing the device list, **Then** a "Last Activity Time" column displays the timestamp of the most recent data transmission
2. **Given** a device has never sent data, **When** viewing the device in the list, **Then** the Last Activity Time shows an appropriate indicator (e.g., "Never" or "-")
3. **Given** the system is using Azure IoT Hub, **When** displaying activity times, **Then** the data is sourced from the `lastActivityTime` field
4. **Given** the system is using AWS IoT Core, **When** displaying activity times, **Then** the data is sourced from the equivalent AWS field if available

---

### Edge Cases

- What happens when viewing devices that have never connected to the platform? (Last Activity Time should show "Never" or similar indicator)
- How does the system handle edge devices where Connection State is used in logic (e.g., for enabling/disabling remote execution)? (Connection State remains in backend models for business logic, only UI display is removed)
- What happens during API calls or data synchronization that reference Connection State? (Backend models retain the field for compatibility and business logic; only frontend display changes)
- How are existing search filters updated for users who have saved search preferences? (Search filters are updated to remove connection state options; saved preferences gracefully handle missing fields)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST remove the "Connection State" or "Connected" column from the device list page UI
- **FR-002**: System MUST remove the "Status" or "Connected" column from the edge device list page UI
- **FR-003**: System MUST remove connection status filter options (Connected/Disconnected/All) from the device list search panel
- **FR-004**: System MUST remove connection status filter options from the edge device list search panel
- **FR-005**: System MUST remove the "Last status update" field from device detail pages where it currently appears
- **FR-006**: System MUST preserve backend Connection State data in models and entities for any business logic that depends on it
- **FR-007**: System MUST maintain the ability to retrieve Connection State from cloud providers for backend operations even if not displayed in UI
- **FR-008**: System MAY add a "Last Activity Time" column to device list pages that displays the timestamp from the `lastActivityTime` field
- **FR-009**: System MAY source Last Activity Time from Azure IoT Hub's `lastActivityTime` field for Azure deployments
- **FR-010**: System MAY source Last Activity Time from AWS IoT Core's equivalent field for AWS deployments (if available)
- **FR-011**: System MUST ensure device list sorting and filtering continue to work with remaining columns
- **FR-012**: System MUST preserve the display of "Connected leaf devices" count on edge device lists

### Key Entities

- **DeviceListItem**: Display model for devices in list views - contains UI-visible device properties including connection state and status timestamps (StatusUpdatedTime, LastActivityTime)
- **IoTEdgeListItem**: Display model for edge devices in list views - contains UI-visible edge device properties including status/connection state
- **IoTEdgeDevice**: Detailed model for edge devices - contains ConnectionState property used in detail views and business logic
- **Device (Domain Entity)**: Backend entity representing physical devices - stores persistent state including connection information
- **EdgeDevice (Domain Entity)**: Backend entity representing edge gateway devices - stores persistent state including connection information

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can view device lists without seeing connection state information that previously caused confusion
- **SC-002**: Support requests related to "disconnected" devices that are functioning normally decrease by at least 50%
- **SC-003**: Users can successfully search and filter devices using remaining criteria without the removed connection status filters
- **SC-004**: All existing device management operations (view, edit, delete, deploy) continue to function without connection state columns visible
- **SC-005**: System response times for loading device lists remain unchanged or improve after column removal
- **SC-006**: If activity tracking is implemented, users can accurately determine when devices last communicated with the platform, eliminating confusion caused by ephemeral connection states
