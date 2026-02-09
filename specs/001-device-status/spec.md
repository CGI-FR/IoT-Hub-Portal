# Feature Specification: Improve Device Status Display

**Feature Branch**: `001-device-status`  
**Created**: 2026-02-09  
**Status**: Draft  
**Input**: User description: "Remove confusing Connection State and Last Status Update columns from device and gateway views"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Device Status Without Confusion (Priority: P1)

Non-technical users need to view their device list without being confused by misleading status indicators. Currently, LoRaWAN devices that send data infrequently (every 30 minutes to 24 hours) appear as "disconnected" most of the time because the Connection State column only shows "connected" when there's an active connection to IoT Hub. This causes users to think devices are malfunctioning and generates unnecessary support requests.

**Why this priority**: This is the core problem causing user confusion and unnecessary support overhead. Removing the misleading Connection State column immediately solves the primary pain point.

**Independent Test**: Can be fully tested by viewing the device list page with LoRaWAN devices that transmit infrequently and verifying that no misleading connection status is displayed, delivering clarity about device operational state.

**Acceptance Scenarios**:

1. **Given** I am viewing the device list page, **When** I look at the column headers, **Then** I should NOT see a "Connection State" column
2. **Given** I have LoRaWAN devices that send data every 30 minutes, **When** I view the device list, **Then** the devices should not be incorrectly labeled as "disconnected"
3. **Given** I am a non-technical user, **When** I view device information, **Then** I should only see status information that accurately reflects device health and activity

---

### User Story 2 - View Accurate Device Activity Timestamp (Priority: P2)

Users need to see when a device last communicated with the system to understand device activity patterns. The current "Last Status Update" column only updates when the Device Twin is modified, which doesn't reflect actual device communication and creates confusion about device activity.

**Why this priority**: This enhances the fix by replacing misleading information with accurate activity tracking. It's P2 because removing the confusing column (P1) already solves the critical problem, but this provides better information.

**Independent Test**: Can be tested by viewing the device list after devices send telemetry data and verifying the timestamp reflects actual device communication, not Device Twin updates.

**Acceptance Scenarios**:

1. **Given** I am viewing the device list page, **When** I look at the column headers, **Then** I should NOT see a "Last Status Update" column
2. **Given** a device has sent telemetry data, **When** I view the device information, **Then** I should see the timestamp of the last activity (lastActivityTime)
3. **Given** the Device Twin was updated but no device activity occurred, **When** I view the device information, **Then** the activity timestamp should reflect the actual last device communication, not the Twin update

---

### User Story 3 - View Gateway Status Without Confusion (Priority: P2)

Users managing gateways need the same clarity as device management. Gateways also suffer from misleading Connection State and Last Status Update columns that create confusion about gateway operational status.

**Why this priority**: This ensures consistency across the platform. It's P2 because gateways may be less numerous than devices, but the same confusion exists and should be fixed for a consistent user experience.

**Independent Test**: Can be tested by viewing the gateway list page and verifying the same improvements (no Connection State column, accurate activity information) are applied as for devices.

**Acceptance Scenarios**:

1. **Given** I am viewing the gateway list page, **When** I look at the column headers, **Then** I should NOT see a "Connection State" column
2. **Given** I am viewing the gateway list page, **When** I look at the column headers, **Then** I should NOT see a "Last Status Update" column
3. **Given** I am viewing gateway information, **When** checking gateway activity, **Then** I should see accurate activity timestamp information consistent with the device list view

---

### Edge Cases

- What happens when a device has never sent any data (lastActivityTime is null)?
  - Display should show "No activity recorded" or similar placeholder text rather than an error or blank field
- How does the system handle devices that were recently registered but haven't communicated yet?
  - Should display registration timestamp or "Awaiting first contact" status
- What if lastActivityTime data is not available from the cloud provider (Azure IoT Hub or AWS IoT Core)?
  - System should gracefully handle missing data with appropriate fallback display

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST remove the "Connection State" column from the device list view
- **FR-002**: System MUST remove the "Connection State" column from the gateway list view
- **FR-003**: System MUST remove the "Last Status Update" column from the device list view
- **FR-004**: System MUST remove the "Last Status Update" column from the gateway list view
- **FR-005**: System MUST display device activity information using the lastActivityTime field from the cloud provider (Azure IoT Hub or AWS IoT Core)
- **FR-006**: System MUST display gateway activity information using the lastActivityTime field from the cloud provider
- **FR-007**: System MUST handle cases where lastActivityTime is not available (null or missing) by displaying appropriate placeholder text
- **FR-008**: System MUST maintain all other existing columns and functionality in device and gateway list views

### Key Entities

- **Device**: Represents an IoT device connected to the portal. Key attributes include device ID, name, model, and activity timestamp (lastActivityTime)
- **Gateway**: Represents an IoT gateway managing multiple devices. Key attributes include gateway ID, name, model, and activity timestamp (lastActivityTime)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Zero users report confusion about devices appearing as "disconnected" when they are functioning normally
- **SC-002**: Support requests related to "device shows as offline but is sending data" are reduced by 90% or more
- **SC-003**: Device and gateway list views display activity information that accurately reflects when devices last communicated with the system
- **SC-004**: Users can identify inactive devices based on lastActivityTime being older than their expected transmission interval
- **SC-005**: All device and gateway views load and display correctly without the removed columns, maintaining existing performance characteristics
