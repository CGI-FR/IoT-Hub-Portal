# Feature Specification: Dashboard Metrics

**Feature ID**: 020  
**Feature Branch**: `020-dashboard-metrics`  
**Created**: 2026-02-03  
**Status**: Draft  
**Source**: Analysis from `specs/020-dashboard-metrics/analyze.md`

---

## User Scenarios & Testing

### User Story 1 - View Portal Health Overview (Priority: P1)

As a portal operator, I need to view a dashboard with key metrics about the IoT device fleet so that I can quickly assess the overall health of the system at a glance.

**Why this priority**: The dashboard is the primary entry point for operators - immediate visibility into system health is critical for effective operations.

**Independent Test**: Can be fully tested by accessing the dashboard and verifying all expected metrics are displayed with current values.

**Acceptance Scenarios**:

1. **Given** I access the portal dashboard, **When** the page loads, **Then** I see key metrics including total devices, connected devices, edge devices, and failed deployments.
2. **Given** devices are connected to the portal, **When** I view the dashboard, **Then** the device count and connected count reflect the actual state.
3. **Given** LoRa features are enabled, **When** I view the dashboard, **Then** I also see the concentrator count metric.
4. **Given** some edge deployments have failed, **When** I view the dashboard, **Then** the failed deployment count is highlighted.

---

### User Story 2 - Monitor Device Connectivity (Priority: P1)

As a portal operator, I need to see how many devices are currently connected so that I can identify connectivity issues across the fleet.

**Why this priority**: Connectivity is the primary health indicator - disconnected devices cannot be monitored or controlled.

**Independent Test**: Can be fully tested by comparing connected device count to total device count and verifying accuracy.

**Acceptance Scenarios**:

1. **Given** the dashboard is displayed, **When** I examine device metrics, **Then** I see both total devices and connected devices.
2. **Given** devices disconnect from the cloud, **When** metrics are refreshed, **Then** the connected device count decreases.
3. **Given** I want to calculate connectivity percentage, **When** I have both metrics, **Then** I can compute (connected/total) × 100%.

---

### User Story 3 - Identify Deployment Issues (Priority: P2)

As a portal operator, I need to see the count of failed edge device deployments so that I can prioritize troubleshooting efforts.

**Why this priority**: Failed deployments indicate edge devices not functioning as expected, requiring intervention.

**Independent Test**: Can be fully tested by creating failed deployments and verifying the count is reflected in dashboard metrics.

**Acceptance Scenarios**:

1. **Given** edge devices have failed deployments, **When** I view the dashboard, **Then** I see a non-zero failed deployment count.
2. **Given** I remediate a failed deployment, **When** metrics are refreshed, **Then** the failed deployment count decreases.
3. **Given** all deployments are successful, **When** I view the dashboard, **Then** the failed deployment count is zero.

---

### User Story 4 - Monitor LoRaWAN Network Infrastructure (Priority: P2)

As a LoRaWAN network operator, I need to see the total number of concentrators so that I understand my network coverage capacity.

**Why this priority**: Concentrator count indicates LoRaWAN network capacity and coverage potential.

**Independent Test**: Can be fully tested by verifying concentrator count matches the actual number of registered concentrators.

**Acceptance Scenarios**:

1. **Given** LoRa features are enabled, **When** I view the dashboard, **Then** I see the concentrator count.
2. **Given** new concentrators are added, **When** metrics are refreshed, **Then** the count increases.
3. **Given** LoRa features are disabled, **When** I view the dashboard, **Then** concentrator metrics may be omitted or zero.

---

### Edge Cases

- What happens when the cloud provider is temporarily unavailable? (Metrics show last known values; no real-time updates)
- How often are metrics updated? (Background jobs update metrics periodically, not real-time)
- What happens in a new portal with no devices? (All metrics show zero)
- How are metrics affected by mass device offline events? (Connected count drops; may trigger alerts if monitoring is configured)

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST provide a dashboard metrics endpoint returning key portal statistics
- **FR-002**: System MUST track total device count across all device types
- **FR-003**: System MUST track connected device count for standard IoT devices
- **FR-004**: System MUST track total edge device count
- **FR-005**: System MUST track connected edge device count
- **FR-006**: System MUST track failed deployment count for edge devices
- **FR-007**: System MUST track concentrator count when LoRa features are enabled
- **FR-008**: Metrics MUST be pre-computed by background jobs for fast retrieval
- **FR-009**: Dashboard endpoint MUST return metrics in under 100 milliseconds
- **FR-010**: Dashboard access MUST require authentication and appropriate permissions

### Key Entities

- **PortalMetric**: Aggregated metrics data containing:
  - DeviceCount (total standard devices)
  - ConnectedDeviceCount (devices with active connections)
  - EdgeDeviceCount (total edge devices)
  - ConnectedEdgeDeviceCount (edge devices with active connections)
  - FailedDeploymentCount (edge deployments in failed state)
  - ConcentratorCount (total LoRa concentrators)

### Metrics Overview

| Metric | Description | Source |
|--------|-------------|--------|
| DeviceCount | Total registered standard IoT devices | Azure IoT Hub / AWS IoT Core |
| ConnectedDeviceCount | Devices currently connected | Cloud provider connection state |
| EdgeDeviceCount | Total registered edge devices | Azure IoT Edge / AWS Greengrass |
| ConnectedEdgeDeviceCount | Edge devices with healthy connections | Cloud provider connection state |
| FailedDeploymentCount | Edge deployments in error state | Deployment configuration service |
| ConcentratorCount | Total LoRa gateways | LoRaWAN concentrator registry |

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Dashboard loads with current metrics in under 2 seconds
- **SC-002**: Metrics are updated at least every 5 minutes by background jobs
- **SC-003**: 100% of operators can identify connectivity issues within 30 seconds using the dashboard
- **SC-004**: Dashboard correctly reflects device state changes within one metric refresh cycle
- **SC-005**: Zero false positives in connectivity reporting (metrics match actual cloud provider state)

---

## Traceability

### Source Analysis
- **Analysis Path**: `specs/020-dashboard-metrics/analyze.md`
- **Analyzed By**: excavator.specifier

### Code References
- DashboardController: Metrics retrieval endpoint
- PortalMetric: Shared metrics model (singleton)
- Background jobs: Populate PortalMetric singleton

### Dependencies
- **Depends On**: 
  - 025-metrics-collection-jobs (background jobs populate metrics)
  - Cloud provider integration (Azure IoT Hub / AWS IoT Core)
- **Depended By**: 
  - Portal dashboard UI (displays metrics)
  - External monitoring systems (may consume metrics endpoint)
