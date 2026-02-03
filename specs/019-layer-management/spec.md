# Feature Specification: Layer Management

**Feature ID**: 019  
**Feature Branch**: `019-layer-management`  
**Created**: 2026-02-03  
**Status**: Draft  
**Source**: Analysis from `specs/019-layer-management/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - Create Building Layer (Priority: P1)

As a facilities manager, I need to create building layers/levels so that I can organize IoT devices by their physical location within a building structure.

**Why this priority**: Layer creation is foundational for spatial device organization - without layers, devices cannot be grouped by location.

**Independent Test**: Can be fully tested by creating a new layer with name and properties, then verifying it is persisted.

**Acceptance Scenarios**:

1. **Given** I have layer management privileges, **When** I create a new layer with a name and properties, **Then** the layer is saved and available for device assignment.
2. **Given** I provide required layer fields, **When** I submit the form, **Then** the layer is created with a unique identifier.
3. **Given** I submit a layer with missing required fields, **When** validation runs, **Then** appropriate error messages are displayed.

---

### User Story 2 - View All Layers (Priority: P1)

As a facilities manager, I need to view all defined layers so that I can understand the building structure and identify layers for device assignment.

**Why this priority**: Visibility into the layer hierarchy is essential for spatial device organization and planning assignment.

**Independent Test**: Can be fully tested by accessing the layers list and verifying all defined layers are displayed.

**Acceptance Scenarios**:

1. **Given** multiple layers exist, **When** I access the layers list, **Then** I see all layers with their names and key attributes.
2. **Given** no layers exist, **When** I access the list, **Then** an empty state message is displayed.

---

### User Story 3 - View Layer Details (Priority: P1)

As a facilities manager, I need to view detailed information about a specific layer so that I can understand its configuration and assigned planning.

**Why this priority**: Detailed layer information is needed for managing device assignments and planning associations.

**Independent Test**: Can be fully tested by selecting a layer and viewing its complete configuration including planning assignment.

**Acceptance Scenarios**:

1. **Given** I select a layer from the list, **When** the detail view loads, **Then** I see all layer attributes and planning assignment.
2. **Given** a layer has an assigned planning, **When** I view the layer, **Then** I see which planning controls devices on this layer.
3. **Given** I request a non-existent layer ID, **When** the system searches, **Then** a "not found" message is returned.

---

### User Story 4 - Assign Planning to Layer (Priority: P2)

As a facilities manager, I need to assign a planning to a layer so that all devices on that layer are automatically controlled according to the planning's schedules.

**Why this priority**: Planning assignment enables automated device control for entire building areas, reducing manual configuration.

**Independent Test**: Can be fully tested by assigning a planning to a layer and verifying devices on that layer are controlled by the planning.

**Acceptance Scenarios**:

1. **Given** I am editing a layer, **When** I select a planning and save, **Then** the planning is associated with the layer.
2. **Given** a layer has a planning assigned, **When** the automation job runs, **Then** all devices on this layer receive commands from the planning's schedules.
3. **Given** I change a layer's planning assignment, **When** the next automation cycle runs, **Then** the new planning controls the devices.

---

### User Story 5 - Modify Layer Configuration (Priority: P2)

As a facilities manager, I need to modify existing layers so that I can update layer properties as building configurations change.

**Why this priority**: Layer modifications happen as buildings are reconfigured but are less frequent than initial setup.

**Independent Test**: Can be fully tested by editing a layer's properties and verifying the changes are persisted.

**Acceptance Scenarios**:

1. **Given** I am editing a layer, **When** I modify properties and save, **Then** the changes are persisted.
2. **Given** I rename a layer, **When** I view devices assigned to it, **Then** the new name is reflected.

---

### User Story 6 - Delete Layer (Priority: P3)

As a facilities manager, I need to delete layers that are no longer needed so that I can maintain an accurate building structure.

**Why this priority**: Layer deletion is less frequent and requires understanding of impacts on assigned devices.

**Independent Test**: Can be fully tested by deleting a layer and verifying it is removed from the system.

**Acceptance Scenarios**:

1. **Given** I select a layer to delete, **When** I confirm the deletion, **Then** the layer is removed.
2. **Given** I delete a layer with assigned devices, **When** I view those devices, **Then** they no longer have a layer assignment.

---

### Edge Cases

- What happens to devices when their layer is deleted? (Devices should become unassigned or moved to a default layer)
- How are hierarchical parent-child layer relationships handled? (Not currently implemented; flat structure only)
- What happens when a layer's planning is deleted? (Layer loses planning reference; devices are not automated)
- How are devices moved between layers? (Device update changes layer assignment)

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST allow creating building layers with name and properties
- **FR-002**: System MUST generate unique identifiers for each layer
- **FR-003**: System MUST validate layer data on creation and update
- **FR-004**: System MUST return all layers when listing
- **FR-005**: System MUST return detailed layer information for a specific ID
- **FR-006**: System MUST return 404 for non-existent layer IDs
- **FR-007**: System MUST allow updating layer configurations
- **FR-008**: System MUST allow assigning a planning to a layer
- **FR-009**: System MUST allow deleting layers
- **FR-010**: System MUST return 204 No Content on successful deletion
- **FR-011**: Devices MUST be assignable to layers for spatial organization

### Key Entities

- **Layer**: Building level/floor structure containing:
  - Unique identifier
  - Name (descriptive label, e.g., "Ground Floor", "Level 1")
  - Planning reference (optional, for automated device control)
  - Parent layer (optional, for hierarchical structures - future)
  - Metadata (additional layer properties)

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Facilities managers can create a new layer in under 2 minutes
- **SC-002**: Layer list loads within 2 seconds regardless of count
- **SC-003**: Planning assignment to a layer takes effect in the next automation cycle
- **SC-004**: 100% of devices with layer assignments are correctly organized in spatial views
- **SC-005**: Building device organization reduces device location time by 50%

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/019-layer-management/analyze.md`
- **Analyzed By**: excavator.specifier

### Code References
- LayersController: Layer CRUD endpoints (note: uses "building" route)
- ILayerService: Layer business logic interface
- Layer entity: Domain model for layer configuration

### Dependencies
- **Depends On**: 
  - 017-planning-management (plannings are assigned to layers)
- **Depended By**: 
  - 001-standard-device-management (devices are assigned to layers)
  - 026-planning-command-jobs (uses layer-to-device mapping for automation)

### Known Issues
- Inconsistent terminology between "layer" and "level" in codebase
- Route uses "/api/building" instead of "/api/layers"
