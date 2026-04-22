# Feature Specification: LoRaWAN Concentrator Management

**Feature ID**: 010  
**Feature Branch**: `010-lorawan-concentrator-management`  
**Created**: January 30, 2025  
**Status**: Draft  
**Source**: Analysis from `specs/010-lorawan-concentrator-management/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - View and Search Concentrator Inventory (Priority: P1)

Network administrators need to view all registered LoRaWAN concentrators (gateways) to understand network infrastructure, identify disconnected devices, and manage their LoRaWAN gateway fleet. Administrators can search by device identifier or name and filter by operational status and connection state.

**Why this priority**: This is the foundation of concentrator management. Without the ability to view and locate concentrators, no other management operations are possible. This represents the core read capability that every user role with concentrator access needs.

**Independent Test**: Can be fully tested by authenticating with concentrator:read permission, navigating to the concentrator list, and verifying that all concentrators are displayed with pagination. Delivers value by providing visibility into gateway infrastructure.

**Acceptance Scenarios**:

1. **Given** user has concentrator:read permission, **When** user accesses concentrator list page, **Then** system displays all concentrators in a paginated table with device ID, name, region, connection status, and enabled/disabled status
2. **Given** user views concentrator list with 50+ devices, **When** user navigates between pages, **Then** system displays results in pages with configurable page size and provides next page navigation
3. **Given** user needs to find a specific concentrator, **When** user enters search text matching device ID or name, **Then** system filters results to show only matching concentrators
4. **Given** user wants to view only enabled concentrators, **When** user selects "Enabled" status filter, **Then** system displays only concentrators where IsEnabled is true
5. **Given** user needs to identify disconnected gateways, **When** user selects "Disconnected" state filter, **Then** system displays only concentrators where IsConnected is false
6. **Given** user sorts by name column, **When** user clicks column header, **Then** system reorders concentrators alphabetically by name
7. **Given** LoRaWAN feature is disabled in portal settings, **When** user attempts to access concentrator endpoints, **Then** system returns 404 Not Found

---

### User Story 2 - Provision New LoRaWAN Gateway (Priority: P1)

IT administrators need to provision new LoRaWAN concentrators during network expansion or deployment. They must configure the device with a valid 16-character hexadecimal device identifier, assign it to a frequency plan/region based on regulatory requirements, optionally configure client certificate authentication for secure connections, and set its operational status.

**Why this priority**: Gateway provisioning is essential for establishing LoRaWAN network infrastructure. Without the ability to register new concentrators, no LoRaWAN devices can communicate with the cloud platform. This is a core write capability required for network setup.

**Independent Test**: Can be fully tested by authenticating with concentrator:write permission, creating a new concentrator with required fields (DeviceID, DeviceName, LoraRegion), and verifying it appears in the concentrator list and is created in the IoT Hub with proper device twin configuration.

**Acceptance Scenarios**:

1. **Given** user has concentrator:write permission, **When** user submits new concentrator form with valid DeviceID (16 hex chars), DeviceName, and LoraRegion, **Then** system creates concentrator in database and IoT Hub with router configuration
2. **Given** user creates concentrator with DeviceID "1234567890ABCDEF", **When** system processes creation request, **Then** system validates DeviceID matches regex pattern ^[A-F0-9]{16}$ and accepts the request
3. **Given** user creates concentrator without DeviceName, **When** user submits form, **Then** system returns validation error indicating DeviceName is required
4. **Given** user selects LoraRegion "EU868", **When** system creates concentrator, **Then** system retrieves appropriate router configuration for EU868 region from LoRaWAN management service
5. **Given** user provides ClientThumbprint in format "AB:CD:EF:...", **When** user submits form, **Then** system validates thumbprint matches pattern ^(([A-F0-9]{2}:){19}[A-F0-9]{2}|)$ (40 hex chars with colons)
6. **Given** user creates concentrator with IsEnabled set to false, **When** system creates device, **Then** system sets device status to Disabled in IoT Hub
7. **Given** user submits concentrator with invalid DeviceID format, **When** system validates request, **Then** system returns 422 Unprocessable Entity with validation error
8. **Given** system creates concentrator successfully, **When** creation completes, **Then** system creates device twin in IoT Hub with router configuration and returns created concentrator details

---

### User Story 3 - View Concentrator Details and Connection Status (Priority: P2)

Network operators need to view detailed information about a specific concentrator, including its configuration, connection status, and operational state, to troubleshoot connectivity issues and verify proper configuration.

**Why this priority**: While list view provides overview, detailed view is essential for operations teams to diagnose connection issues, verify certificate configuration, and confirm router settings. This supports day-to-day network monitoring and troubleshooting.

**Independent Test**: Can be fully tested by selecting a concentrator from the list, viewing its detail page, and verifying all properties are displayed including connection status indicator. Delivers value by providing comprehensive device information for troubleshooting.

**Acceptance Scenarios**:

1. **Given** user has concentrator:read permission and valid deviceId, **When** user requests concentrator details, **Then** system returns complete concentrator information including DeviceId, DeviceName, LoraRegion, ClientThumbprint, IsConnected, IsEnabled, and RouterConfig
2. **Given** user views concentrator with IsConnected=true, **When** detail page renders, **Then** system displays green WiFi icon indicating connected status
3. **Given** user views concentrator with IsConnected=false, **When** detail page renders, **Then** system displays red WiFi icon indicating disconnected status
4. **Given** user requests details for non-existent concentrator, **When** system processes request, **Then** system throws ResourceNotFoundException with message "The concentrator with id {deviceId} doesn't exist"
5. **Given** concentrator has AlreadyLoggedInOnce=true, **When** user views details, **Then** system displays this property indicating gateway has successfully connected at least once

---

### User Story 4 - Update Concentrator Configuration (Priority: P2)

Network administrators need to modify concentrator configuration after deployment, such as changing the frequency plan for regional migrations, updating client certificate thumbprints during certificate rotation, changing the device name for better organization, or enabling/disabling the gateway for maintenance.

**Why this priority**: Configuration changes are common in production environments due to certificate expiration, regional relocations, or maintenance activities. This enables ongoing management of deployed gateways without requiring device re-provisioning.

**Independent Test**: Can be fully tested by editing an existing concentrator's properties (name, region, certificate, status), saving changes, and verifying updates are persisted in database and synchronized to IoT Hub device twin.

**Acceptance Scenarios**:

1. **Given** user has concentrator:write permission and valid concentrator, **When** user updates DeviceName, LoraRegion, or ClientThumbprint and submits, **Then** system updates database record and synchronizes changes to IoT Hub device twin
2. **Given** user changes LoraRegion from "EU868" to "US915", **When** system processes update, **Then** system retrieves new router configuration for US915 and updates device twin with new router config
3. **Given** user updates IsEnabled from true to false, **When** system processes update, **Then** system sets device status to Disabled in IoT Hub and updates database
4. **Given** user updates IsEnabled from false to true, **When** system processes update, **Then** system sets device status to Enabled in IoT Hub and updates database
5. **Given** user updates ClientThumbprint with invalid format, **When** system validates request, **Then** system returns 422 Unprocessable Entity with validation error
6. **Given** user updates concentrator that doesn't exist in database, **When** system processes update, **Then** system throws ResourceNotFoundException with message "The device {deviceId} doesn't exist"
7. **Given** user successfully updates concentrator, **When** update completes, **Then** system updates device twin properties, device status, and database record atomically

---

### User Story 5 - Decommission Gateway (Priority: P2)

IT administrators need to remove concentrators from the system when gateways are decommissioned, relocated to different networks, or replaced due to hardware failure. Removal must ensure the device is deleted from both the IoT Hub and local database to maintain data consistency.

**Why this priority**: Gateway decommissioning is less frequent than viewing or creating but still essential for lifecycle management. Proper cleanup prevents orphaned devices in IoT Hub and maintains accurate inventory.

**Independent Test**: Can be fully tested by deleting an existing concentrator and verifying it is removed from both the database and IoT Hub, and no longer appears in the concentrator list.

**Acceptance Scenarios**:

1. **Given** user has concentrator:write permission and valid deviceId, **When** user requests concentrator deletion, **Then** system removes concentrator from IoT Hub and deletes database record
2. **Given** user deletes concentrator, **When** deletion process executes, **Then** system first deletes device from IoT Hub, then removes database record
3. **Given** concentrator doesn't exist in database but exists in IoT Hub, **When** user deletes concentrator, **Then** system removes device from IoT Hub and gracefully handles missing database record without error
4. **Given** user successfully deletes concentrator, **When** deletion completes, **Then** system returns 200 OK status
5. **Given** user attempts to view deleted concentrator, **When** user requests details, **Then** system returns ResourceNotFoundException
6. **Given** deletion fails at IoT Hub level, **When** system processes deletion, **Then** system propagates exception and database record remains intact

---

### User Story 6 - Manage Client Certificate Authentication (Priority: P3)

Security-conscious organizations need to configure mutual TLS authentication for concentrators using client certificate thumbprints to ensure only authorized gateways can connect to the network. The system must validate thumbprint format (40 hexadecimal characters) and support certificate updates during rotation.

**Why this priority**: While security is important, certificate authentication is optional for many deployments. Organizations with strict security requirements need this capability, but basic gateway management works without it.

**Independent Test**: Can be fully tested by creating/updating a concentrator with a valid ClientThumbprint value in format "XX:XX:XX:..." (40 hex chars with colon separators), verifying validation, and confirming the thumbprint is stored in device twin for authentication.

**Acceptance Scenarios**:

1. **Given** user creates concentrator with ClientThumbprint, **When** system validates thumbprint, **Then** system accepts format matching ^(([A-F0-9]{2}:){19}[A-F0-9]{2}|)$ (20 pairs of hex with colon separators)
2. **Given** user provides ClientThumbprint without colons or incorrect length, **When** system validates, **Then** system returns validation error "ClientThumbprint must contain 40 hexadecimal characters"
3. **Given** user creates concentrator without ClientThumbprint, **When** system processes creation, **Then** system sets ClientThumbprint to empty string (optional field)
4. **Given** user updates concentrator to add ClientThumbprint, **When** update completes, **Then** system updates device twin with new thumbprint for subsequent authentication
5. **Given** UI input field for ClientThumbprint, **When** user types hex characters, **Then** system applies pattern mask "XX:XX:XX:XX:XX:XX:XX:XX:XX:XX:XX:XX:XX:XX:XX:XX:XX:XX:XX:XX" and auto-formats with colons
6. **Given** concentrator has valid ClientThumbprint configured, **When** gateway attempts connection, **Then** IoT Hub validates certificate against configured thumbprint

---

### User Story 7 - Filter by Multiple Criteria (Priority: P3)

Power users and operations teams need to combine multiple filters (search text, status, and connection state) to perform complex queries like "find all disconnected enabled concentrators in production" or "locate all disabled gateways with names containing 'warehouse'."

**Why this priority**: While basic filtering is P1, advanced multi-criteria filtering is valuable for large deployments with many gateways but not essential for basic operations. Most users start with simple searches.

**Independent Test**: Can be fully tested by applying multiple filters simultaneously (e.g., searchText + status filter + state filter) and verifying the system returns only concentrators matching all criteria.

**Acceptance Scenarios**:

1. **Given** user enters searchText="gateway" AND status="Enabled" AND state="Disconnected", **When** system executes query, **Then** system applies all three predicates using AND logic and returns concentrators matching all conditions
2. **Given** user applies searchText filter matching DeviceID or Name, **When** system builds query, **Then** system performs case-insensitive search using ToLower() on both fields
3. **Given** user selects status filter, **When** filter value is not null, **Then** system adds predicate "concentrator.IsEnabled == filter.Status" to query
4. **Given** user selects state filter, **When** filter value is not null, **Then** system adds predicate "concentrator.IsConnected == filter.State" to query
5. **Given** user resets all filters, **When** system executes query, **Then** system uses predicate PredicateBuilder.True<Concentrator>() returning all concentrators

---

### User Story 8 - Sort Concentrator List (Priority: P4)

Users with large concentrator fleets need to sort the list by various columns (name, enabled status, connection state) to organize and prioritize their view, such as viewing all concentrators alphabetically or grouping by status.

**Why this priority**: Sorting improves usability but is not essential for core functionality. Users can accomplish most tasks with search and filtering alone. This is a quality-of-life enhancement.

**Independent Test**: Can be fully tested by clicking column headers in the concentrator list and verifying results are reordered according to the selected column and direction (ascending/descending).

**Acceptance Scenarios**:

1. **Given** user clicks "Name" column header, **When** system processes request, **Then** system adds orderBy parameter to query and returns concentrators sorted alphabetically by Name
2. **Given** user clicks "IsEnabled" column header, **When** system processes request, **Then** system sorts concentrators by enabled status (enabled first or disabled first based on direction)
3. **Given** user applies orderBy parameter in API request, **When** system executes query, **Then** system passes orderBy array to GetPaginatedListAsync for database-level sorting
4. **Given** user sorts and paginates, **When** user navigates to next page, **Then** system preserves orderBy parameter in nextPage URL for consistent sorting across pages

---

### Edge Cases

**Device Identifier Validation:**
- What happens when user provides DeviceID with lowercase hex characters? (System expects uppercase A-F)
- What happens when user provides DeviceID with less than or more than 16 characters?
- What happens when user attempts to create duplicate DeviceID?

**Certificate Thumbprint Handling:**
- What happens when ClientThumbprint contains lowercase hex characters?
- What happens when ClientThumbprint has incorrect colon separator positions?
- What happens when ClientThumbprint is null vs empty string?

**Concurrent Modifications:**
- What happens when two administrators update the same concentrator simultaneously?
- What happens when concentrator is deleted while another user is viewing its details?

**External Service Failures:**
- What happens when IoT Hub is unavailable during creation/update/deletion?
- What happens when LoRaWAN management service fails to return router configuration?
- What happens when database save succeeds but IoT Hub operation fails (partial failure scenario)?

**Data Consistency:**
- What happens when concentrator exists in IoT Hub but not in database?
- What happens when concentrator exists in database but not in IoT Hub?
- What happens when IsConnected status in database doesn't match actual IoT Hub connection state?

**Permission Boundary Conditions:**
- What happens when user has concentrator:read but not concentrator:write and attempts modification?
- What happens when user loses concentrator:write permission while editing a concentrator?
- What happens when LoRaWAN feature is disabled mid-operation?

**Pagination Edge Cases:**
- What happens when page size exceeds total number of concentrators?
- What happens when concentrator is deleted while user is viewing a paginated list?
- What happens when page number exceeds available pages?

**Filtering Edge Cases:**
- What happens when search text contains special regex characters?
- What happens when all filters are applied and no concentrators match?
- What happens when status and state filters contradict each other logically?

**Frequency Plan Changes:**
- What happens when concentrator's LoraRegion is changed while LoRaWAN devices are actively communicating through it?
- What happens when selected frequency plan becomes unavailable or deprecated?

---

## Requirements

### Functional Requirements

**Concentrator Data Management:**
- **FR-001**: System MUST store concentrator records with device identifier, name, frequency plan/region, device type, optional client certificate thumbprint, connection status, enabled status, and version
- **FR-002**: System MUST enforce device identifier format as exactly 16 uppercase hexadecimal characters (0-9, A-F)
- **FR-003**: System MUST validate client certificate thumbprint as exactly 40 hexadecimal characters formatted with colon separators (XX:XX:XX:...) or empty
- **FR-004**: System MUST require device name and frequency plan/region for all concentrators
- **FR-005**: System MUST automatically set device type to "LoRa Concentrator" during creation

**Concentrator CRUD Operations:**
- **FR-006**: System MUST provide ability to create new concentrators with validation of all required fields and format constraints
- **FR-007**: System MUST retrieve individual concentrator details by device identifier
- **FR-008**: System MUST support updating concentrator name, frequency plan/region, client certificate thumbprint, and enabled status
- **FR-009**: System MUST prevent modification of device identifier after creation
- **FR-010**: System MUST support deletion of concentrators with cascading removal from all dependent systems

**IoT Hub Synchronization:**
- **FR-011**: System MUST create corresponding device in cloud IoT platform when concentrator is created locally
- **FR-012**: System MUST create and configure device twin with router configuration during concentrator creation
- **FR-013**: System MUST update device twin properties when concentrator configuration is modified
- **FR-014**: System MUST synchronize enabled/disabled status between local database and cloud IoT platform device status
- **FR-015**: System MUST delete device from cloud IoT platform when concentrator is deleted locally

**Router Configuration Management:**
- **FR-016**: System MUST retrieve appropriate router configuration from LoRaWAN management service based on selected frequency plan/region
- **FR-017**: System MUST store router configuration in device twin during creation and update operations
- **FR-018**: System MUST update router configuration when frequency plan/region is changed

**List and Search Operations:**
- **FR-019**: System MUST provide paginated list of all concentrators with configurable page size
- **FR-020**: System MUST support searching concentrators by device identifier or device name with case-insensitive matching
- **FR-021**: System MUST support filtering concentrators by enabled status (enabled/disabled/all)
- **FR-022**: System MUST support filtering concentrators by connection state (connected/disconnected/all)
- **FR-023**: System MUST allow combining search text with status and state filters using AND logic
- **FR-024**: System MUST support sorting concentrator list by name and enabled status columns
- **FR-025**: System MUST provide next page URL in pagination results when additional pages exist

**Connection Status Tracking:**
- **FR-026**: System MUST track and display connection status (connected/disconnected) for each concentrator
- **FR-027**: System MUST display visual indicators for connection status (e.g., green for connected, red for disconnected)
- **FR-028**: System MUST track whether concentrator has successfully authenticated at least once (AlreadyLoggedInOnce property)

**Validation and Error Handling:**
- **FR-029**: System MUST validate all input data against defined constraints before processing
- **FR-030**: System MUST return 422 Unprocessable Entity with detailed validation errors when input validation fails
- **FR-031**: System MUST throw ResourceNotFoundException with descriptive message when attempting to retrieve, update, or delete non-existent concentrator
- **FR-032**: System MUST gracefully handle missing database records during deletion without throwing exceptions

**Authorization and Security:**
- **FR-033**: System MUST require "concentrator:read" permission for viewing concentrator list and details
- **FR-034**: System MUST require "concentrator:write" permission for creating, updating, and deleting concentrators
- **FR-035**: System MUST enforce feature gate to disable all concentrator endpoints when LoRaWAN feature is not enabled in portal settings
- **FR-036**: System MUST return 404 Not Found for all concentrator endpoints when LoRaWAN feature is disabled
- **FR-037**: System MUST support mutual TLS authentication using client certificate thumbprint for gateway connections

**Transaction Management:**
- **FR-038**: System MUST use unit of work pattern to ensure database operations are committed atomically
- **FR-039**: System MUST coordinate database operations with IoT Hub operations to maintain consistency

**User Interface:**
- **FR-040**: System MUST provide input masking for client certificate thumbprint field to enforce format XX:XX:XX:...
- **FR-041**: System MUST provide dropdown selection for frequency plan/region populated from available frequency plans
- **FR-042**: System MUST conditionally render edit and delete operations based on user permissions
- **FR-043**: System MUST display confirmation dialog before deleting concentrator

### Key Entities

**Concentrator:**
- Represents a LoRaWAN gateway device (concentrator) that bridges LoRa radio communications to IP networks
- Unique identifier (DeviceId) - 16 hexadecimal characters
- Name - human-readable name for identification
- LoraRegion - frequency plan/region (e.g., EU868, US915) for regulatory compliance
- DeviceType - classification as "LoRa Concentrator"
- ClientThumbprint - optional SHA1 certificate thumbprint for mutual TLS authentication (40 hex chars)
- IsConnected - real-time connection status
- IsEnabled - operational status (enabled/disabled)
- Version - entity version for concurrency control

**ConcentratorDto:**
- Data transfer object for API communication
- Contains all Concentrator properties plus additional fields:
  - AlreadyLoggedInOnce - tracks first successful connection
  - RouterConfig - LoRaWAN-specific router configuration embedded from management service
- Includes validation attributes for DeviceId and ClientThumbprint format enforcement

**ConcentratorFilter:**
- Query filter object for list operations
- SearchText - keyword search for DeviceID or DeviceName
- Status - optional boolean filter for enabled/disabled state
- State - optional boolean filter for connected/disconnected state
- Inherits pagination properties (PageNumber, PageSize, OrderBy)

**RouterConfig:**
- LoRaWAN-specific configuration for packet routing
- Retrieved from LoRaWAN management service based on frequency plan/region
- Stored in device twin for concentrator runtime configuration

---

## Success Criteria

### Measurable Outcomes

**Provisioning Efficiency:**
- **SC-001**: Network administrators can provision a new LoRaWAN concentrator from start to finish in under 2 minutes
- **SC-002**: System validates concentrator configuration data and provides immediate feedback (within 2 seconds) for validation errors

**Inventory Management:**
- **SC-003**: Operations teams can locate any concentrator in a fleet of 500+ devices using search and filters in under 10 seconds
- **SC-004**: System displays paginated concentrator list with all status information (connection state, enabled status) within 3 seconds for up to 100 devices per page

**Configuration Updates:**
- **SC-005**: Administrators can update concentrator configuration (name, region, certificate, status) and synchronize changes to cloud platform in under 30 seconds
- **SC-006**: System successfully synchronizes 100% of configuration changes to device twin without manual intervention

**Connection Monitoring:**
- **SC-007**: Operations teams have real-time visibility into concentrator connection status with visual indicators (green/red) refreshed within 5 seconds of state change
- **SC-008**: System accurately tracks and displays connection history (AlreadyLoggedInOnce) for 100% of concentrators

**Data Consistency:**
- **SC-009**: System maintains consistency between local database and cloud IoT platform with zero orphaned devices after create/update/delete operations
- **SC-010**: System handles partial failures (e.g., IoT Hub unavailable) gracefully and provides clear error messages to users

**Search and Filter Performance:**
- **SC-011**: Combined search and multi-criteria filtering returns results for fleets of 1000+ concentrators in under 5 seconds
- **SC-012**: Pagination navigation maintains filter and sort state across 100% of page transitions

**Security and Compliance:**
- **SC-013**: System enforces permission-based access control with zero unauthorized access to concentrator management operations
- **SC-014**: System validates 100% of client certificate thumbprints against format requirements before allowing configuration
- **SC-015**: Organizations using mutual TLS authentication achieve zero unauthorized gateway connections

**User Experience:**
- **SC-016**: 90% of users can successfully create, view, update, or delete concentrators on first attempt without consulting documentation
- **SC-017**: Administrators can identify and troubleshoot disconnected gateways within 1 minute using connection status filters

**System Reliability:**
- **SC-018**: System handles concurrent concentrator management operations from multiple administrators without data corruption
- **SC-019**: System maintains LoRaWAN feature gate enforcement with 100% accuracy (no endpoint access when feature disabled)

**Validation Quality:**
- **SC-020**: System rejects 100% of invalid DeviceID formats (non-hex, incorrect length) before attempting device creation
- **SC-021**: System provides specific, actionable validation error messages for all input validation failures

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/010-lorawan-concentrator-management/analyze.md`
- **Analyzed By**: excavator.specifier
- **Analysis Date**: January 30, 2025

### Code References

**Controllers:**
- `src/IoTHub.Portal.Server/Controllers/v1.0/LoRaWAN/LoRaWANConcentratorsController.cs` - REST API endpoints for concentrator CRUD operations

**Business Logic:**
- `src/IoTHub.Portal.Application/Services/ILoRaWANConcentratorService.cs` - Service interface
- `src/IoTHub.Portal.Server/Services/LoRaWANConcentratorService.cs` - Core business logic for concentrator management, IoT Hub synchronization, and router configuration

**Data Layer:**
- `src/IoTHub.Portal.Domain/Entities/Concentrator.cs` - Concentrator domain entity
- `src/IoTHub.Portal.Domain/Repositories/IConcentratorRepository.cs` - Repository interface

**DTOs:**
- `src/IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/ConcentratorDto.cs` - API data transfer object with validation attributes
- `src/IoTHub.Portal.Shared/Models/v1.0/Filters/ConcentratorFilter.cs` - Query filter model
- `src/IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/RouterConfig.cs` - Router configuration model

**UI Components:**
- `src/IoTHub.Portal.Client/Pages/LoRaWAN/Concentrator/ConcentratorListPage.razor` - List page with search and filters
- `src/IoTHub.Portal.Client/Pages/LoRaWAN/Concentrator/ConcentratorDetailPage.razor` - Detail and edit page
- `src/IoTHub.Portal.Client/Pages/LoRaWAN/Concentrator/CreateConcentratorPage.razor` - Creation page
- `src/IoTHub.Portal.Client/Pages/LoRaWAN/Concentrator/DeleteConcentratorPage.razor` - Deletion confirmation dialog
- `src/IoTHub.Portal.Client/Components/Concentrators/ConcentratorSearch.razor` - Search and filter component

**Client Services:**
- `src/IoTHub.Portal.Client/Services/ILoRaWanConcentratorClientService.cs` - Client service interface
- `src/IoTHub.Portal.Client/Services/LoRaWanConcentratorClientService.cs` - HTTP client implementation

**Validators:**
- `src/IoTHub.Portal.Client/Validators/ConcentratorValidator.cs` - FluentValidation rules

**Mappers:**
- `src/IoTHub.Portal.Application/Mappers/IConcentratorTwinMapper.cs` - Twin mapper interface
- `src/IoTHub.Portal.Infrastructure/Mappers/ConcentratorTwinMapper.cs` - Device twin mapping implementation

### Dependencies

**Internal Feature Dependencies:**
- **Feature 012 - LoRaWAN Frequency Plans**: Concentrators must be associated with valid frequency plan/region from this feature
- **Feature 008 - LoRaWAN Device Management**: LoRaWAN end-devices communicate through concentrators managed by this feature
- **Feature 011 - LoRaWAN Commands Management**: Commands to LoRaWAN devices are routed through concentrators

**Service Dependencies:**
- **IExternalDeviceService**: Cloud IoT platform operations (create device with twin, update device, update device twin, delete device, get device, get device twin)
- **IConcentratorTwinMapper**: Mapping between ConcentratorDto and device twin properties
- **ILoRaWanManagementService**: LoRaWAN router configuration management (GetRouterConfig by region)
- **IConcentratorRepository**: Database persistence for concentrator entities
- **IUnitOfWork**: Transaction management for atomic database operations
- **IMapper**: AutoMapper for entity-DTO transformations

**External Dependencies:**
- **Azure IoT Hub**: Cloud IoT device management platform for device twin and device status
- **LoRaWAN Network Server**: Manages LoRaWAN-specific configuration and routing rules
- **Entity Framework Core**: Database access via PortalDbContext

### Related Documentation
- LoRaWAN specification documentation for frequency plans and router configuration
- Azure IoT Hub device twin documentation
- Certificate authentication configuration guide for mutual TLS

---

## Notes

### Design Decisions

**Service Layer Pattern:**
- Implemented service layer abstraction (ILoRaWANConcentratorService) to separate business logic from controller and enable unit testing in isolation
- Service coordinates multiple external dependencies (IoT Hub, LoRaWAN management, database) in single operations

**DTO Pattern:**
- Clear separation between API models (ConcentratorDto) and domain entities (Concentrator) enables independent evolution and adds API-specific properties like RouterConfig

**Twin Mapper Pattern:**
- Extracted device twin mapping logic into dedicated mapper (IConcentratorTwinMapper) to isolate cloud platform specifics and enable future multi-cloud support

**Repository Pattern:**
- Generic repository interface for Concentrator entity provides clean data access abstraction and supports unit of work for transactions

**Predicate Builder:**
- Dynamic query construction using PredicateBuilder enables flexible combination of search and filter criteria without complex conditional logic

### Implementation Patterns

**Two-Phase Operations:**
- Create/Update/Delete operations follow pattern: 1) Perform cloud platform operation, 2) Perform database operation
- Ensures cloud platform is source of truth and database reflects cloud state

**Device Status Mapping:**
- IsEnabled (bool) maps to DeviceStatus enum (Enabled/Disabled) for cloud platform compatibility
- Simplifies UI binding while maintaining platform-specific requirements

**Validation Strategy:**
- Three-layer validation: 1) Attribute validation on DTO, 2) FluentValidation in client, 3) ModelState validation in controller
- Provides comprehensive validation with early feedback in UI

**Feature Gate:**
- LoRaFeatureActiveFilter attribute on controller ensures endpoints return 404 when LoRaWAN feature disabled
- Centralizes feature toggle logic at controller level

### Business Context

**LoRaWAN Architecture:**
- Concentrators are gateway devices, not end-devices, in LoRaWAN network architecture
- Act as bridge between LoRa radio (end-devices) and IP network (cloud platform)
- Single concentrator can serve hundreds of LoRaWAN end-devices

**Regulatory Compliance:**
- Frequency plan/region selection ensures compliance with regional radio regulations (e.g., EU868 for Europe, US915 for North America)
- Critical for legal operation in different jurisdictions

**Security Model:**
- Optional mutual TLS using client certificate thumbprint for high-security environments
- Prevents rogue gateways from joining network

### Performance Considerations

**Pagination:**
- Server-side pagination with database-level filtering reduces data transfer and memory usage
- Supports large deployments with thousands of concentrators

**Query Optimization:**
- Predicate building constructs efficient database queries with combined WHERE clauses
- Lazy loading not applicable (Concentrator entity has no navigation properties)

**Caching Opportunities (not currently implemented):**
- Router configuration could be cached per region to reduce calls to LoRaWAN management service
- Frequency plan list could be cached as it changes infrequently

### Limitations and Constraints

**Single Cloud Platform:**
- Only supports Azure IoT Hub; no support for AWS IoT Core or other platforms for LoRaWAN concentrators

**No Bulk Operations:**
- No import/export functionality for concentrators (unlike standard devices)
- Each concentrator must be created/updated/deleted individually

**Passive Connection Monitoring:**
- Connection status (IsConnected) is passively reported by device, not actively monitored with health checks
- No real-time connection alerts or monitoring dashboard

**Limited Telemetry:**
- Feature focuses on configuration management only
- No display of concentrator telemetry, traffic statistics, or LoRaWAN packet metrics

**No Certificate Management:**
- System accepts certificate thumbprint but doesn't provide certificate upload, renewal, or revocation workflows
- Organizations must manage certificates externally

### Future Enhancement Opportunities

**Real-Time Updates:**
- Implement SignalR for real-time connection status updates without page refresh
- Push notifications when concentrators connect/disconnect

**Telemetry Dashboard:**
- Display concentrator health metrics (CPU, memory, uptime)
- Show LoRaWAN packet statistics (packets received, error rate, SNR/RSSI distribution)

**Bulk Operations:**
- Enable/disable multiple concentrators simultaneously
- Bulk certificate rotation for multiple concentrators

**Advanced Management:**
- Firmware update management for remote concentrator updates
- Advanced router configuration editing for expert users
- Concentrator group management for logical organization

**Certificate Lifecycle:**
- Certificate upload and storage
- Automatic renewal reminders
- Certificate revocation workflow

**Geographic Visualization:**
- Map view showing concentrator locations and coverage areas
- Network topology visualization

**Analytics:**
- Historical connection uptime reports
- Gateway performance comparison
- Network coverage gap analysis
