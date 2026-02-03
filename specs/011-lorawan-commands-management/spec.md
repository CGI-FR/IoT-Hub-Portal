# Feature Specification: LoRaWAN Commands Management

**Feature ID**: 011  
**Feature Branch**: `011-lorawan-commands-management`  
**Created**: 2026-02-03  
**Status**: Draft  
**Source**: Analysis from `specs/011-lorawan-commands-management/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - Define Commands for Device Model (Priority: P1)

As an IoT administrator, I need to define reusable command templates for a LoRaWAN device model so that operators can execute standardized commands on any device using that model without needing to know the technical payload details.

**Why this priority**: This is the foundational capability - without command definitions, no command execution is possible. It enables the entire LoRaWAN remote control workflow.

**Independent Test**: Can be fully tested by creating a device model, defining commands with hex payloads and port numbers, and verifying commands are persisted and retrievable.

**Acceptance Scenarios**:

1. **Given** I have a LoRaWAN device model, **When** I define a new command with name, hex frame payload, and LoRaWAN port, **Then** the command is saved and associated with that model.
2. **Given** a command exists for a device model, **When** I modify the command's frame payload or port, **Then** the changes are persisted.
3. **Given** I provide an invalid hex frame (odd characters, non-hex values, or >255 characters), **When** I attempt to save the command, **Then** validation errors are displayed.
4. **Given** I provide a port outside the valid range (1-223), **When** I attempt to save the command, **Then** a port range error is displayed.

---

### User Story 2 - Execute Command on Device (Priority: P1)

As an IoT operator, I need to execute a defined command on a specific LoRaWAN device so that I can remotely control or configure the device without physical access.

**Why this priority**: Command execution is the primary business value - operators need to remotely control devices for operations, configuration changes, and troubleshooting.

**Independent Test**: Can be fully tested by selecting a device, choosing a command, executing it, and observing success/failure feedback.

**Acceptance Scenarios**:

1. **Given** a device with a model that has defined commands, **When** I select and execute a command, **Then** the command is sent to the device and I receive confirmation.
2. **Given** downlink is disabled on the device model, **When** I attempt to execute a command, **Then** the system prevents execution with an appropriate message.
3. **Given** the LoRa network server is unavailable, **When** I attempt to execute a command, **Then** I receive an error message indicating the failure.
4. **Given** I execute a confirmed command, **When** the device acknowledges receipt, **Then** the confirmation status is displayed.

---

### User Story 3 - View Commands for Device Model (Priority: P2)

As an IoT administrator, I need to view all commands defined for a device model so that I can audit existing configurations and understand available device capabilities.

**Why this priority**: Visibility into existing commands is essential for ongoing management and troubleshooting, but secondary to creation and execution.

**Independent Test**: Can be fully tested by navigating to a device model's command configuration and verifying the complete list is displayed.

**Acceptance Scenarios**:

1. **Given** a device model with multiple commands, **When** I view the model's command configuration, **Then** all commands are listed with their names, ports, and frame previews.
2. **Given** a device model with no commands defined, **When** I view the command configuration, **Then** an empty state message is displayed.

---

### User Story 4 - Manage Built-in Commands (Priority: P3)

As an IoT administrator, I need to understand that certain commands are protected as built-in commands so that I don't accidentally modify critical device functionality.

**Why this priority**: Built-in command protection prevents accidental misconfiguration but is less frequently encountered than general command management.

**Independent Test**: Can be fully tested by attempting to edit or delete a built-in command and verifying the action is blocked.

**Acceptance Scenarios**:

1. **Given** a built-in command exists for a device model, **When** I attempt to edit it, **Then** the edit is prevented and a message explains it's protected.
2. **Given** a built-in command exists, **When** I attempt to delete it, **Then** the deletion is blocked with an explanation.

---

### Edge Cases

- What happens when a device model is deleted that has associated commands? (Commands should be cascade deleted)
- How does the system handle command execution when the device is offline? (Command is queued for delivery when device reconnects)
- What happens if two operators execute different commands on the same device simultaneously? (Both commands are queued and executed in order)
- How are commands handled for devices that have been moved to a different model? (Device inherits new model's commands)

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST allow defining commands for LoRaWAN device models with name, hex frame payload, and LoRaWAN port
- **FR-002**: System MUST validate hex frame payloads contain only valid hexadecimal characters (0-9, A-F) with even character count
- **FR-003**: System MUST validate hex frame payloads do not exceed 255 characters
- **FR-004**: System MUST validate LoRaWAN port is within the valid range (1-223)
- **FR-005**: System MUST support both confirmed and unconfirmed downlink modes for command delivery
- **FR-006**: System MUST encode command payloads to Base64 format for transmission
- **FR-007**: System MUST prevent modification or deletion of built-in commands
- **FR-008**: System MUST only allow command execution on devices whose model has downlink enabled
- **FR-009**: Users MUST be able to view all commands associated with a device model
- **FR-010**: System MUST provide execution feedback indicating success or failure with error details
- **FR-011**: System MUST log all command execution attempts with device ID, command name, and result

### Key Entities

- **DeviceModelCommand**: A command template defined at the model level containing:
  - Name (descriptive command identifier)
  - Frame (hexadecimal payload data)
  - Port (LoRaWAN port number 1-223)
  - Confirmed (boolean for delivery confirmation requirement)
  - IsBuiltin (boolean for system-protected commands)
  - DeviceModelId (association to parent model)

- **Command Execution Request**: Runtime data for executing a command:
  - Device identifier (target device)
  - Command identifier (which command to execute)

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Administrators can define a new command for a device model in under 2 minutes
- **SC-002**: Operators can execute a command on a device with confirmation within 5 seconds (network dependent)
- **SC-003**: 95% of command executions complete successfully on first attempt (when device is reachable)
- **SC-004**: Reduce manual on-site device configuration visits by 80% through remote command execution
- **SC-005**: 100% of command execution attempts are logged for audit purposes

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/011-lorawan-commands-management/analyze.md`
- **Analyzed By**: excavator.specifier

### Code References
- LoRaWANCommandsController: Command CRUD operations
- LoRaWANDevicesController: Command execution endpoint
- LoRaWANCommandService: Business logic for command management
- LoRaWanManagementService: Network server integration

### Dependencies
- **Depends On**: 
  - 009-lorawan-device-model-management (commands are defined at model level)
  - 008-lorawan-device-management (commands are executed on devices)
- **Depended By**: 
  - 026-planning-command-jobs (scheduled command execution)
