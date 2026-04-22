# Feature Specification: Device Synchronization Jobs

**Feature ID**: 024  
**Feature Branch**: `024-device-synchronization-jobs`  
**Created**: 2026-02-03  
**Status**: Draft  
**Source**: Analysis from `specs/024-device-synchronization-jobs/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - Automatic Device Inventory Synchronization (Priority: P1)

As a portal operator, I need devices registered in the cloud provider to automatically appear in the portal so that I have a complete view of my IoT fleet without manual entry.

**Why this priority**: Automatic synchronization is the foundation of the portal's device visibility - without it, operators would need to manually register all devices.

**Independent Test**: Can be fully tested by creating devices in the cloud provider and verifying they appear in the portal after synchronization.

**Acceptance Scenarios**:

1. **Given** devices exist in Azure IoT Hub, **When** the sync job runs, **Then** those devices appear in the portal device list.
2. **Given** a new device is registered in the cloud, **When** the next sync cycle completes, **Then** the device is added to the portal.
3. **Given** device properties are updated in the cloud, **When** sync runs, **Then** the portal reflects the updated properties.
4. **Given** a device is deleted from the cloud, **When** sync runs, **Then** the device is removed from the portal.

---

### User Story 2 - Edge Device Synchronization with Modules (Priority: P1)

As a portal operator, I need edge devices and their module configurations synchronized so that I can monitor and manage edge deployments through the portal.

**Why this priority**: Edge devices are critical infrastructure components that require visibility for operations and troubleshooting.

**Independent Test**: Can be fully tested by deploying edge devices with modules and verifying complete sync including module data.

**Acceptance Scenarios**:

1. **Given** edge devices exist in Azure IoT Hub, **When** the edge sync job runs, **Then** edge devices appear with their module information.
2. **Given** an edge device's connection state changes, **When** sync runs, **Then** the portal shows the current connection status.
3. **Given** edge modules are deployed, **When** sync runs, **Then** module configurations are visible in the portal.
4. **Given** an edge device is deleted from the cloud, **When** sync runs, **Then** it's removed from the portal.

---

### User Story 3 - LoRaWAN Concentrator Synchronization (Priority: P2)

As a LoRaWAN network operator, I need concentrators synchronized from the cloud so that I can monitor my LoRaWAN network coverage through the portal.

**Why this priority**: Concentrator visibility is essential for LoRaWAN network management but is specific to LoRa-enabled deployments.

**Independent Test**: Can be fully tested by registering concentrators and verifying they sync to the portal.

**Acceptance Scenarios**:

1. **Given** LoRa concentrators exist in Azure IoT Hub, **When** the concentrator sync job runs, **Then** concentrators appear in the portal.
2. **Given** concentrator configuration changes, **When** sync runs, **Then** updated configuration is reflected.
3. **Given** a concentrator is decommissioned, **When** sync runs, **Then** it's removed from the portal.

---

### User Story 4 - AWS IoT Things Synchronization (Priority: P1)

As a portal operator using AWS, I need AWS IoT Things synchronized as devices so that I can manage my AWS IoT fleet through the portal.

**Why this priority**: AWS support is a core multi-cloud capability, essential for AWS-based deployments.

**Independent Test**: Can be fully tested by creating AWS IoT Things and verifying they sync as devices or edge devices.

**Acceptance Scenarios**:

1. **Given** things exist in AWS IoT Core, **When** the things sync job runs, **Then** they appear as devices in the portal.
2. **Given** a thing has Greengrass core device shadow, **When** sync runs, **Then** it's identified as an edge device.
3. **Given** AWS thing types exist, **When** the thing types sync job runs, **Then** they appear as device models.
4. **Given** a thing is deleted from AWS, **When** sync runs, **Then** the device is removed from the portal.

---

### User Story 5 - Gateway ID List Maintenance (Priority: P2)

As the LoRaWAN system, I need an up-to-date list of gateway IDs so that LoRaWAN message routing can validate gateway sources.

**Why this priority**: Gateway ID validation is important for LoRaWAN security but operates in the background.

**Independent Test**: Can be fully tested by verifying the gateway ID list matches registered concentrators after sync.

**Acceptance Scenarios**:

1. **Given** concentrators are registered, **When** the gateway ID sync runs, **Then** the in-memory gateway list is updated.
2. **Given** a new concentrator is added, **When** sync runs, **Then** its gateway ID is available for validation.

---

### Edge Cases

- What happens if cloud provider API is temporarily unavailable? (Job logs error and retries on next schedule)
- How are concurrent sync job executions prevented? (DisallowConcurrentExecution attribute prevents overlap)
- What happens if a device model referenced by a device doesn't exist? (Device creation is skipped with warning logged)
- How are version conflicts handled during updates? (Optimistic concurrency - only newer versions are applied)
- What happens with very large device fleets (100,000+ devices)? (Pagination ensures all devices are processed)

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST periodically synchronize standard devices from cloud provider
- **FR-002**: System MUST synchronize edge devices with module configurations
- **FR-003**: System MUST synchronize LoRaWAN concentrators when LoRa features are enabled
- **FR-004**: System MUST synchronize AWS IoT Things as devices for AWS deployments
- **FR-005**: System MUST synchronize AWS Thing Types as device models
- **FR-006**: System MUST synchronize AWS Greengrass deployments as edge device models
- **FR-007**: System MUST maintain an in-memory gateway ID list for LoRaWAN validation
- **FR-008**: Sync jobs MUST use pagination to handle large device fleets
- **FR-009**: Sync jobs MUST use optimistic concurrency (version checking) for updates
- **FR-010**: Sync jobs MUST remove devices deleted from the cloud provider
- **FR-011**: Sync jobs MUST prevent concurrent execution
- **FR-012**: Sync jobs MUST log errors without stopping other device processing
- **FR-013**: Sync jobs MUST validate device model existence before creating devices

### Synchronization Jobs

| Job | Frequency | Scope |
|-----|-----------|-------|
| SyncDevicesJob | Configurable (e.g., 5 min) | Standard IoT devices |
| SyncEdgeDeviceJob | Configurable (e.g., 5 min) | Edge devices with modules |
| SyncConcentratorsJob | Configurable (e.g., 5 min) | LoRa concentrators |
| SyncGatewayIDJob | Configurable (e.g., 5 min) | In-memory gateway list |
| SyncThingsJob | Configurable (e.g., 5 min) | AWS IoT Things |
| SyncThingTypesJob | Configurable (e.g., 10 min) | AWS Thing Types |
| SyncGreenGrassDeploymentsJob | Configurable (e.g., 10 min) | AWS Greengrass deployments |

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Devices created in cloud appear in portal within 10 minutes
- **SC-002**: Device deletions in cloud are reflected in portal within 10 minutes
- **SC-003**: 99.9% of sync cycles complete successfully
- **SC-004**: Sync jobs process 10,000 devices in under 5 minutes
- **SC-005**: Zero data inconsistencies between cloud provider and portal after sync

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/024-device-synchronization-jobs/analyze.md`
- **Analyzed By**: excavator.specifier

### Code References
- SyncDevicesJob: Standard device synchronization
- SyncEdgeDeviceJob: Edge device synchronization
- SyncConcentratorsJob: LoRa concentrator synchronization
- SyncGatewayIDJob: Gateway ID list maintenance
- SyncThingsJob: AWS Things synchronization
- SyncThingTypesJob: AWS Thing Types synchronization
- SyncGreenGrassDeploymentsJob: AWS Greengrass synchronization

### Dependencies
- **Depends On**: 
  - Cloud provider APIs (Azure IoT Hub / AWS IoT Core)
  - Device model configuration (to validate model references)
- **Depended By**: 
  - 001-standard-device-management (devices from sync)
  - 006-edge-device-management (edge devices from sync)
  - 010-lorawan-concentrator-management (concentrators from sync)
  - 020-dashboard-metrics (device counts from synced data)
