# Feature Specification: Planning Command Jobs

**Feature ID**: 026  
**Feature Branch**: `026-planning-command-jobs`  
**Created**: 2026-02-03  
**Status**: Draft  
**Source**: Analysis from `specs/026-planning-command-jobs/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - Automatic Schedule-Based Command Execution (Priority: P1)

As an IoT operations manager, I need LoRaWAN commands automatically executed based on time schedules so that devices are controlled without manual intervention throughout the day.

**Why this priority**: Automated command execution is the core value of the planning system - enabling lights-out operations for IoT fleets.

**Independent Test**: Can be fully tested by creating a schedule, waiting for the scheduled time, and verifying commands are executed on target devices.

**Acceptance Scenarios**:

1. **Given** a planning with schedules exists, **When** the current time matches a schedule window, **Then** the scheduled command is executed on all devices in the planning's layers.
2. **Given** a schedule has day-specific commands, **When** Monday's time slot is active, **Then** Monday's specific command is executed.
3. **Given** an active planning exists, **When** the planning's date range ends, **Then** no more commands are executed for that planning.
4. **Given** devices are in a layer with a planning, **When** commands execute, **Then** all devices in the layer receive the command.

---

### User Story 2 - Day-Off Command Handling (Priority: P1)

As an IoT operations manager, I need day-off periods to use default commands so that devices are appropriately controlled on weekends and holidays.

**Why this priority**: Day-off handling prevents incorrect device operation outside normal operating periods (e.g., lights off on weekends).

**Independent Test**: Can be fully tested by configuring day-off patterns and verifying default commands execute on those days.

**Acceptance Scenarios**:

1. **Given** a planning has weekend marked as day-off, **When** Saturday arrives, **Then** the planning's default off-day command is used.
2. **Given** today is a day-off with a default command configured, **When** the job runs, **Then** devices receive the off-day command.
3. **Given** today is a normal operating day, **When** the job runs, **Then** regular schedule commands are used.

---

### User Story 3 - Layer-Based Device Targeting (Priority: P1)

As an IoT operations manager, I need commands to automatically target all devices in a layer so that I can control device groups by their physical location.

**Why this priority**: Layer-based targeting enables efficient group control - essential for managing devices by building area.

**Independent Test**: Can be fully tested by assigning devices to layers and verifying all layer devices receive planning commands.

**Acceptance Scenarios**:

1. **Given** devices are assigned to a layer, **When** that layer's planning executes, **Then** all devices in the layer receive commands.
2. **Given** a device has no layer assignment, **When** planning jobs run, **Then** that device is not automated (excluded from planning).
3. **Given** a new device is added to a layer, **When** the next planning cycle runs, **Then** the device receives commands with other layer devices.

---

### User Story 4 - Time Zone Aware Scheduling (Priority: P2)

As an IoT operations manager, I need schedules to execute at the correct local time so that automated control aligns with local business hours.

**Why this priority**: Time zone awareness ensures schedules work correctly for the intended operational region.

**Independent Test**: Can be fully tested by setting schedules and verifying they execute at the expected local time.

**Acceptance Scenarios**:

1. **Given** a schedule is set for 08:00, **When** it's 08:00 in the configured time zone (Europe/Paris), **Then** the schedule command executes.
2. **Given** daylight saving time changes occur, **When** schedules are evaluated, **Then** execution times adjust correctly.

---

### User Story 5 - Real-Time Data Refresh (Priority: P2)

As the planning system, I need fresh data from the portal APIs so that command execution uses the latest device, layer, and planning configurations.

**Why this priority**: Using current configuration ensures changes take effect promptly without job restarts.

**Independent Test**: Can be fully tested by modifying a planning and verifying the next job execution uses updated settings.

**Acceptance Scenarios**:

1. **Given** I update a planning's schedules, **When** the next job cycle starts, **Then** the updated schedules are used.
2. **Given** I add a new device to a layer, **When** the next cycle runs, **Then** the device is included in command execution.
3. **Given** I remove a planning from a layer, **When** the next cycle runs, **Then** devices in that layer are no longer automated.

---

### Edge Cases

- What happens if the LoRaWAN network is unavailable during scheduled execution? (Commands fail; errors logged; no automatic retry)
- How are overlapping schedules in the same planning handled? (First matching time slot wins)
- What happens if no schedule matches the current time? (No command execution for that cycle)
- How are devices without LoRaWAN capability handled? (Only LoRaWAN devices receive commands via this system)
- What happens if the job takes longer than the schedule interval? (DisallowConcurrentExecution prevents overlap)

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST periodically check all active plannings for scheduled command execution
- **FR-002**: System MUST evaluate schedules against current time in configured time zone
- **FR-003**: System MUST support day-specific commands within schedules
- **FR-004**: System MUST support day-off patterns with default commands
- **FR-005**: System MUST only process plannings within their active date range
- **FR-006**: System MUST target all devices in layers associated with active plannings
- **FR-007**: System MUST exclude devices without layer assignments from planning automation
- **FR-008**: System MUST execute LoRaWAN commands via the LoRaWAN command service
- **FR-009**: System MUST refresh device, layer, planning, and schedule data each cycle
- **FR-010**: System MUST prevent concurrent job execution
- **FR-011**: System MUST log command execution results for audit purposes
- **FR-012**: System MUST use Europe/Paris as the default time zone for scheduling

### Planning Command Workflow

```
1. Refresh Data
   ├── Load all devices (with layer assignments)
   ├── Load all layers (with planning references)
   ├── Load all plannings (with date ranges and day-off settings)
   └── Load all schedules (with time windows and commands)

2. Build Planning Database
   ├── Map devices → layers → plannings
   ├── Filter to active plannings (within date range)
   └── Create device-to-command associations

3. Evaluate Schedules
   ├── Check if today is a day-off (use default command)
   ├── Find schedule matching current time
   └── Get day-specific or default command

4. Execute Commands
   ├── For each planning with matching schedule
   ├── For each device in associated layers
   └── Execute LoRaWAN command via network server
```

### Key Entities

- **PlanningCommand** (internal job structure):
  - planningId (which planning is being processed)
  - listDeviceId (devices to receive commands)
  - schedules (time-based command mappings)
  - dayOffCommand (default command for non-working days)

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Scheduled commands execute within 1 minute of scheduled time
- **SC-002**: 99% of scheduled commands execute successfully (when devices are reachable)
- **SC-003**: Configuration changes take effect within one job cycle (typically 1 minute)
- **SC-004**: Planning automation reduces manual device control operations by 90%
- **SC-005**: 100% of command executions are logged for audit purposes

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/026-planning-command-jobs/analyze.md`
- **Analyzed By**: excavator.specifier

### Code References
- SendPlanningCommandJob: Main scheduling and execution job
- PlanningCommand: Internal data structure for job processing
- ILoRaWANCommandService: Command execution interface
- IPlanningService: Planning data retrieval
- IScheduleService: Schedule data retrieval
- ILayerService: Layer data retrieval
- IDeviceService: Device data retrieval

### Dependencies
- **Depends On**: 
  - 017-planning-management (planning configurations)
  - 018-schedule-management (schedule definitions)
  - 019-layer-management (device-to-layer mapping)
  - 011-lorawan-commands-management (command execution)
  - 001-standard-device-management (device inventory)
- **Depended By**: 
  - None (terminal job in the automation chain)
