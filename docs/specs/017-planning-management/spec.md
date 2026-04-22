# Feature Specification: Planning Management

**Feature ID**: 017  
**Feature Branch**: `017-planning-management`  
**Created**: 2026-02-03  
**Status**: Draft  
**Source**: Analysis from `specs/017-planning-management/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - Create Planning Configuration (Priority: P1)

As an IoT operations manager, I need to create planning configurations so that I can define time-bounded operational policies for automated device control.

**Why this priority**: Planning creation is the foundation for scheduled device automation - without plannings, no automated command execution is possible.

**Independent Test**: Can be fully tested by creating a new planning with required fields and verifying it is persisted.

**Acceptance Scenarios**:

1. **Given** I have planning management privileges, **When** I create a new planning with valid data, **Then** the planning is saved and available for assignment.
2. **Given** I provide required planning fields, **When** I submit the form, **Then** the planning is created with a unique identifier.
3. **Given** I submit a planning with missing required fields, **When** validation runs, **Then** appropriate error messages are displayed.

---

### User Story 2 - View All Plannings (Priority: P1)

As an IoT operations manager, I need to view all defined plannings so that I can understand current automation configurations and identify plannings to modify.

**Why this priority**: Visibility into existing plannings is essential for operational management and troubleshooting.

**Independent Test**: Can be fully tested by accessing the plannings list and verifying all defined plannings are displayed.

**Acceptance Scenarios**:

1. **Given** multiple plannings exist, **When** I access the plannings list, **Then** I see all plannings with their key attributes.
2. **Given** no plannings exist, **When** I access the list, **Then** an empty state message is displayed.

---

### User Story 3 - View Planning Details (Priority: P1)

As an IoT operations manager, I need to view detailed information about a specific planning so that I can understand its configuration and associated schedules.

**Why this priority**: Detailed planning information is needed for troubleshooting and verifying correct configuration.

**Independent Test**: Can be fully tested by selecting a planning and viewing its complete configuration.

**Acceptance Scenarios**:

1. **Given** I select a planning from the list, **When** the detail view loads, **Then** I see all planning attributes and related information.
2. **Given** a planning has associated schedules, **When** I view the planning, **Then** I can see or navigate to its schedules.
3. **Given** I request a non-existent planning ID, **When** the system searches, **Then** a "not found" message is returned.

---

### User Story 4 - Modify Planning Configuration (Priority: P2)

As an IoT operations manager, I need to modify existing plannings so that I can adjust automation rules as operational requirements change.

**Why this priority**: Planning modifications happen as business needs evolve but are less frequent than initial creation.

**Independent Test**: Can be fully tested by editing a planning's fields and verifying the changes are persisted.

**Acceptance Scenarios**:

1. **Given** I am editing a planning, **When** I modify fields and save, **Then** the changes are persisted.
2. **Given** I modify an active planning, **When** the next automation cycle runs, **Then** the new configuration is used.

---

### User Story 5 - Delete Planning (Priority: P3)

As an IoT operations manager, I need to delete plannings that are no longer needed so that I can maintain a clean configuration and prevent unintended automation.

**Why this priority**: Planning deletion is less frequent and requires understanding of impacts on dependent schedules and device automation.

**Independent Test**: Can be fully tested by deleting a planning and verifying it is removed from the system.

**Acceptance Scenarios**:

1. **Given** I select a planning to delete, **When** I confirm the deletion, **Then** the planning is removed.
2. **Given** I delete a planning, **When** the automation job runs, **Then** devices previously using this planning are no longer automated.

---

### Edge Cases

- What happens to schedules when their parent planning is deleted? (Schedules should be orphaned or cascade deleted)
- What happens when a planning's date range ends? (Planning becomes inactive; devices are not controlled)
- How are overlapping plannings handled for the same device layer? (Single planning per layer; new assignment replaces old)
- What happens to in-progress command execution when a planning is deleted mid-cycle? (Current cycle completes; next cycle uses updated configuration)

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST allow creating planning configurations with required attributes
- **FR-002**: System MUST generate unique identifiers for each planning
- **FR-003**: System MUST validate planning data on creation and update
- **FR-004**: System MUST return all plannings when listing
- **FR-005**: System MUST return detailed planning information for a specific ID
- **FR-006**: System MUST return 404 for non-existent planning IDs
- **FR-007**: System MUST allow updating planning configurations
- **FR-008**: System MUST allow deleting plannings
- **FR-009**: System MUST return 204 No Content on successful deletion
- **FR-010**: Plannings MUST be assignable to device layers for automation

### Key Entities

- **Planning**: Automation policy configuration containing:
  - Unique identifier
  - Name (descriptive label)
  - Start date (when planning becomes active)
  - End date (when planning expires)
  - Day-off patterns (days when default commands apply)
  - Default command for off-days
  - Layer assignments (which device groups use this planning)

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Operations managers can create a new planning in under 3 minutes
- **SC-002**: Planning list loads within 2 seconds regardless of count
- **SC-003**: Planning changes take effect in the next automation cycle (within 1 minute)
- **SC-004**: 100% of deleted plannings stop affecting device automation immediately
- **SC-005**: Reduce manual device control operations by 80% through planning automation

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/017-planning-management/analyze.md`
- **Analyzed By**: excavator.specifier

### Code References
- PlanningsController: Planning CRUD endpoints
- IPlanningService: Planning business logic interface
- Planning entity: Domain model for planning configuration

### Dependencies
- **Depends On**: 
  - 019-layer-management (plannings are assigned to layers)
  - 011-lorawan-commands-management (plannings reference commands)
- **Depended By**: 
  - 018-schedule-management (schedules belong to plannings)
  - 026-planning-command-jobs (executes planning-based automation)
