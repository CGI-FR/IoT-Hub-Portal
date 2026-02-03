# Feature Specification: Schedule Management

**Feature ID**: 018  
**Feature Branch**: `018-schedule-management`  
**Created**: 2026-02-03  
**Status**: Draft  
**Source**: Analysis from `specs/018-schedule-management/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - Create Schedule for Planning (Priority: P1)

As an IoT operations manager, I need to create schedules within a planning so that I can define specific time windows and commands for automated device control.

**Why this priority**: Schedules define when commands execute - without schedules, plannings have no time-based automation.

**Independent Test**: Can be fully tested by creating a schedule with time range and command, then verifying it is persisted.

**Acceptance Scenarios**:

1. **Given** I have a planning configured, **When** I create a schedule with start time, end time, and command, **Then** the schedule is saved and associated with the planning.
2. **Given** I am creating a schedule, **When** I specify day-specific commands, **Then** different commands can be configured for different weekdays.
3. **Given** I submit a schedule with missing required fields, **When** validation runs, **Then** appropriate error messages are displayed.

---

### User Story 2 - View All Schedules (Priority: P1)

As an IoT operations manager, I need to view all defined schedules so that I can understand when automated commands will execute.

**Why this priority**: Visibility into schedules is essential for understanding and debugging automation behavior.

**Independent Test**: Can be fully tested by accessing the schedules list and verifying all schedules are displayed with their time windows.

**Acceptance Scenarios**:

1. **Given** multiple schedules exist, **When** I access the schedules list, **Then** I see all schedules with their time ranges and associated plannings.
2. **Given** no schedules exist, **When** I access the list, **Then** an empty state is displayed.

---

### User Story 3 - View Schedule Details (Priority: P1)

As an IoT operations manager, I need to view detailed information about a specific schedule so that I can understand its complete configuration.

**Why this priority**: Detailed schedule information is needed for troubleshooting automation issues.

**Independent Test**: Can be fully tested by selecting a schedule and viewing its complete configuration including all day-specific commands.

**Acceptance Scenarios**:

1. **Given** I select a schedule from the list, **When** the detail view loads, **Then** I see all schedule attributes including time windows and commands.
2. **Given** a schedule has day-specific commands, **When** I view the details, **Then** I see commands mapped to each configured weekday.
3. **Given** I request a non-existent schedule ID, **When** the system searches, **Then** a "not found" message is returned.

---

### User Story 4 - Modify Schedule Configuration (Priority: P2)

As an IoT operations manager, I need to modify existing schedules so that I can adjust automation timing as requirements change.

**Why this priority**: Schedule modifications happen as operational patterns change but are less frequent than viewing.

**Independent Test**: Can be fully tested by editing schedule times and commands, then verifying changes are persisted.

**Acceptance Scenarios**:

1. **Given** I am editing a schedule, **When** I modify time windows and save, **Then** the changes are persisted.
2. **Given** I modify an active schedule, **When** the next automation cycle runs, **Then** the new configuration is used.
3. **Given** I change a schedule's commands, **When** devices are next controlled, **Then** the new commands are executed.

---

### User Story 5 - Delete Schedule (Priority: P3)

As an IoT operations manager, I need to delete schedules that are no longer needed so that I can prevent unintended command execution.

**Why this priority**: Schedule deletion is less frequent but necessary for maintaining clean automation configurations.

**Independent Test**: Can be fully tested by deleting a schedule and verifying it no longer affects device automation.

**Acceptance Scenarios**:

1. **Given** I select a schedule to delete, **When** I confirm the deletion, **Then** the schedule is removed.
2. **Given** I delete a schedule, **When** the automation job runs, **Then** that time window no longer triggers commands.

---

### Edge Cases

- What happens with overlapping schedules for the same planning? (Later schedule in processing order takes precedence, or first match wins)
- How are time zone differences handled? (System uses Europe/Paris timezone by default for scheduling)
- What happens when a schedule spans midnight? (Schedule should handle cross-day time ranges)
- How are daylight saving time transitions handled? (Schedule times should adjust with DST)

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST allow creating schedules with start time, end time, and commands
- **FR-002**: System MUST associate schedules with a parent planning
- **FR-003**: System MUST generate unique identifiers for each schedule
- **FR-004**: System MUST validate schedule data on creation and update
- **FR-005**: System MUST support day-specific command configurations (different commands per weekday)
- **FR-006**: System MUST return all schedules when listing
- **FR-007**: System MUST return detailed schedule information for a specific ID
- **FR-008**: System MUST return 404 for non-existent schedule IDs
- **FR-009**: System MUST allow updating schedule configurations
- **FR-010**: System MUST allow deleting schedules
- **FR-011**: System MUST return 204 No Content on successful deletion

### Key Entities

- **Schedule**: Time-based command configuration containing:
  - Unique identifier
  - Planning reference (parent planning ID)
  - Start time (daily activation time)
  - End time (daily deactivation time)
  - Day-specific commands (commands per weekday)
  - Command reference (default command to execute)

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Operations managers can create a new schedule in under 2 minutes
- **SC-002**: Schedule list loads within 2 seconds regardless of count
- **SC-003**: Schedule changes take effect in the next automation cycle (within 1 minute)
- **SC-004**: 100% of scheduled commands execute within 1 minute of their scheduled time
- **SC-005**: Time-based automation operates correctly across DST transitions

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/018-schedule-management/analyze.md`
- **Analyzed By**: excavator.specifier

### Code References
- SchedulesController: Schedule CRUD endpoints
- IScheduleService: Schedule business logic interface
- Schedule entity: Domain model for schedule configuration

### Dependencies
- **Depends On**: 
  - 017-planning-management (schedules belong to plannings)
  - 011-lorawan-commands-management (schedules reference commands)
- **Depended By**: 
  - 026-planning-command-jobs (uses schedules for time-based command execution)
