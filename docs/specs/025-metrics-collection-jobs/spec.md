# Feature Specification: Metrics Collection Jobs

**Feature ID**: 025  
**Feature Branch**: `025-metrics-collection-jobs`  
**Created**: 2026-02-03  
**Status**: Draft  
**Source**: Analysis from `specs/025-metrics-collection-jobs/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - Automatic Device Metrics Collection (Priority: P1)

As a portal operator, I need device count metrics automatically collected so that the dashboard displays current device inventory statistics without manual intervention.

**Why this priority**: Automated metrics collection is the foundation for real-time dashboard visibility - the primary operational monitoring capability.

**Independent Test**: Can be fully tested by verifying device count metrics update as devices are added or removed.

**Acceptance Scenarios**:

1. **Given** devices exist in the portal, **When** the device metrics loader job runs, **Then** total device count is updated.
2. **Given** devices have varying connection states, **When** metrics are collected, **Then** connected device count reflects current connectivity.
3. **Given** new devices are added to the cloud, **When** the next metrics cycle completes, **Then** device count increases.

---

### User Story 2 - Edge Device Metrics Collection (Priority: P1)

As a portal operator, I need edge device metrics collected so that I can monitor edge infrastructure health including deployment failures.

**Why this priority**: Edge device health is critical for IoT edge operations, requiring dedicated metrics visibility.

**Independent Test**: Can be fully tested by verifying edge device counts and deployment failure counts update correctly.

**Acceptance Scenarios**:

1. **Given** edge devices exist, **When** the edge metrics loader job runs, **Then** edge device count and connected count are updated.
2. **Given** some edge deployments have failed, **When** metrics are collected, **Then** failed deployment count is accurate.
3. **Given** a failed deployment is remediated, **When** metrics update, **Then** failed deployment count decreases.

---

### User Story 3 - Prometheus Metrics Export (Priority: P1)

As an operations team, I need portal metrics exported to Prometheus format so that we can integrate with our monitoring infrastructure and alerting systems.

**Why this priority**: Prometheus integration enables enterprise monitoring and alerting workflows that are critical for production operations.

**Independent Test**: Can be fully tested by querying the Prometheus endpoint and verifying expected metrics are present.

**Acceptance Scenarios**:

1. **Given** the device metrics exporter job runs, **When** Prometheus scrapes the endpoint, **Then** `iot_hub_portal_device_count` metric is available.
2. **Given** connected device count is updated, **When** exported, **Then** `iot_hub_portal_connected_device_count` reflects current value.
3. **Given** edge metrics are collected, **When** exported, **Then** edge device and failed deployment counters are available.
4. **Given** concentrators exist, **When** exported, **Then** `iot_hub_portal_concentrator_count` is available.

---

### User Story 4 - Concentrator Metrics Collection (Priority: P2)

As a LoRaWAN network operator, I need concentrator count metrics collected so that I can monitor network coverage capacity.

**Why this priority**: Concentrator metrics are specific to LoRaWAN deployments but important for network health visibility.

**Independent Test**: Can be fully tested by verifying concentrator count matches registered concentrators.

**Acceptance Scenarios**:

1. **Given** concentrators are registered, **When** the concentrator metrics job runs, **Then** concentrator count is updated.
2. **Given** new concentrators are added, **When** metrics update, **Then** the count increases.

---

### User Story 5 - LoRaWAN Telemetry Ingestion (Priority: P2)

As a LoRaWAN operator, I need device telemetry continuously ingested from the network so that real-time device data is available in the portal.

**Why this priority**: Real-time telemetry is essential for LoRaWAN device monitoring but is specific to LoRa deployments.

**Independent Test**: Can be fully tested by sending LoRaWAN device messages and verifying they appear in device telemetry.

**Acceptance Scenarios**:

1. **Given** LoRaWAN devices send messages, **When** the telemetry sync job processes them, **Then** telemetry data is stored.
2. **Given** the event hub has queued messages, **When** the job runs, **Then** messages are processed and checkpointed.
3. **Given** the job is cancelled, **When** restarted, **Then** processing resumes from the last checkpoint.

---

### Edge Cases

- What happens if cloud provider API is unavailable during metrics collection? (Error logged; metric retains last known value)
- How are Prometheus counter overflows handled? (Counters are set to current absolute value, not incremented)
- What happens if Event Hub has a backlog of messages? (Job processes in batches with checkpointing)
- How are metrics affected by partial sync failures? (Metrics may be temporarily stale; next successful run corrects them)

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST collect total device count from cloud provider
- **FR-002**: System MUST collect connected device count from cloud provider
- **FR-003**: System MUST collect edge device count and connected edge device count
- **FR-004**: System MUST collect failed deployment count for edge devices
- **FR-005**: System MUST collect concentrator count when LoRa features are enabled
- **FR-006**: System MUST export metrics to Prometheus format
- **FR-007**: Prometheus metrics MUST include: device_count, connected_device_count, edge_device_count, connected_edge_device_count, failed_deployment_count, concentrator_count
- **FR-008**: Metrics loader jobs MUST update shared PortalMetric singleton
- **FR-009**: Metrics exporter jobs MUST read from PortalMetric singleton
- **FR-010**: System MUST continuously ingest LoRaWAN telemetry from Azure Event Hub
- **FR-011**: Telemetry ingestion MUST use checkpointing for reliable processing
- **FR-012**: Metrics collection jobs MUST prevent concurrent execution
- **FR-013**: Metrics collection MUST handle API failures gracefully with error logging

### Prometheus Metrics

| Metric Name | Type | Description |
|-------------|------|-------------|
| iot_hub_portal_device_count | Counter | Total registered standard devices |
| iot_hub_portal_connected_device_count | Counter | Devices currently connected |
| iot_hub_portal_edge_device_count | Counter | Total registered edge devices |
| iot_hub_portal_connected_edge_device_count | Counter | Edge devices currently connected |
| iot_hub_portal_failed_deployment_count | Counter | Edge deployments in failed state |
| iot_hub_portal_concentrator_count | Counter | Total LoRa concentrators |

### Metrics Collection Jobs

| Job | Responsibility | Data Source |
|-----|---------------|-------------|
| DeviceMetricLoaderJob | Device counts | Cloud provider API |
| EdgeDeviceMetricLoaderJob | Edge device counts, deployment failures | Cloud provider API + Config service |
| ConcentratorMetricLoaderJob | Concentrator count | Local database |
| DeviceMetricExporterJob | Export device metrics | PortalMetric singleton |
| EdgeDeviceMetricExporterJob | Export edge metrics | PortalMetric singleton |
| ConcentratorMetricExporterJob | Export concentrator metrics | PortalMetric singleton |
| SyncLoRaDeviceTelemetryJob | LoRaWAN telemetry ingestion | Azure Event Hub |

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Metrics are updated at least every 5 minutes
- **SC-002**: Prometheus endpoint returns all metrics in under 100 milliseconds
- **SC-003**: Dashboard displays metrics that match cloud provider state within one refresh cycle
- **SC-004**: 99.9% of metrics collection cycles complete successfully
- **SC-005**: LoRaWAN telemetry messages are processed within 30 seconds of receipt

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/025-metrics-collection-jobs/analyze.md`
- **Analyzed By**: excavator.specifier

### Code References
- DeviceMetricLoaderJob: Device count collection
- DeviceMetricExporterJob: Device metrics to Prometheus
- EdgeDeviceMetricLoaderJob: Edge device count collection
- EdgeDeviceMetricExporterJob: Edge metrics to Prometheus
- ConcentratorMetricLoaderJob: Concentrator count collection
- ConcentratorMetricExporterJob: Concentrator metrics to Prometheus
- SyncLoRaDeviceTelemetryJob: LoRaWAN telemetry ingestion
- PortalMetric: Shared metrics singleton

### Dependencies
- **Depends On**: 
  - Cloud provider APIs (for device counts)
  - Prometheus-net library (for metrics export)
  - Azure Event Hub (for LoRaWAN telemetry)
- **Depended By**: 
  - 020-dashboard-metrics (displays collected metrics)
  - External monitoring systems (consume Prometheus endpoint)
