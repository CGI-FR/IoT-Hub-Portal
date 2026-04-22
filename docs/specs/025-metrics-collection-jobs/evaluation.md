# Evaluation Report: Metrics Collection Jobs (025)

**Evaluation Date**: 2026-02-03  
**Spec Version**: Draft  
**Evaluator**: Excavate Evaluator Agent

---

## Summary

| Criteria | Score | Weight | Weighted Score |
|----------|-------|--------|----------------|
| **Correctness** | 92% | 30% | 27.6% |
| **Completeness** | 88% | 30% | 26.4% |
| **Technical Quality** | 95% | 20% | 19.0% |
| **Coverage** | 90% | 20% | 18.0% |
| **Overall Score** | | | **91.0%** |

**Verdict**: ✅ **Highly Accurate** - The specification accurately documents the metrics collection jobs with minor gaps.

---

## Verified Specifications ✅

### FR-001: System MUST collect total device count from cloud provider
- **Status**: ✅ Verified
- **Evidence**: [DeviceMetricLoaderJob.cs#L34](src/IoTHub.Portal.Infrastructure/Jobs/DeviceMetricLoaderJob.cs#L34)
- **Code**: `this.portalMetric.DeviceCount = await this.externalDeviceService.GetDevicesCount();`

### FR-002: System MUST collect connected device count from cloud provider
- **Status**: ✅ Verified
- **Evidence**: [DeviceMetricLoaderJob.cs#L44](src/IoTHub.Portal.Infrastructure/Jobs/DeviceMetricLoaderJob.cs#L44)
- **Code**: `this.portalMetric.ConnectedDeviceCount = await this.externalDeviceService.GetConnectedDevicesCount();`

### FR-003: System MUST collect edge device count and connected edge device count
- **Status**: ✅ Verified
- **Evidence**: [EdgeDeviceMetricLoaderJob.cs#L38-50](src/IoTHub.Portal.Infrastructure/Jobs/EdgeDeviceMetricLoaderJob.cs#L38-50)
- **Code**: Separate methods `LoadEdgeDevicesCountMetric()` and `LoadConnectedEdgeDevicesCountMetric()`

### FR-004: System MUST collect failed deployment count for edge devices
- **Status**: ✅ Verified
- **Evidence**: [EdgeDeviceMetricLoaderJob.cs#L56-66](src/IoTHub.Portal.Infrastructure/Jobs/EdgeDeviceMetricLoaderJob.cs#L56-66)
- **Code**: `this.portalMetric.FailedDeploymentCount = await configService.GetFailedDeploymentsCount();`

### FR-005: System MUST collect concentrator count when LoRa features are enabled
- **Status**: ✅ Verified
- **Evidence**: [ConcentratorMetricLoaderJob.cs#L28-36](src/IoTHub.Portal.Infrastructure/Jobs/ConcentratorMetricLoaderJob.cs#L28-36)
- **Code**: `this.portalMetric.ConcentratorCount = await this.externalDeviceService.GetConcentratorsCount();`

### FR-006: System MUST export metrics to Prometheus format
- **Status**: ✅ Verified
- **Evidence**: [DeviceMetricExporterJob.cs#L12-13](src/IoTHub.Portal.Infrastructure/Jobs/DeviceMetricExporterJob.cs#L12-13)
- **Code**: Uses `Metrics.CreateCounter()` from prometheus-net library

### FR-007: Prometheus metrics MUST include standard metrics
- **Status**: ✅ Verified
- **Evidence**: [MetricName.cs](src/IoTHub.Portal.Domain/Shared/Constants/MetricName.cs)
- **Details**: All specified metric names are defined:
  - `iot_hub_portal_device_count`
  - `iot_hub_portal_connected_device_count`
  - `iot_hub_portal_edge_device_count`
  - `iot_hub_portal_connected_edge_device_count`
  - `iot_hub_portal_failed_deployment_count`
  - `iot_hub_portal_concentrator_count`

### FR-008: Metrics loader jobs MUST update shared PortalMetric singleton
- **Status**: ✅ Verified
- **Evidence**: All loader jobs inject `PortalMetric` and update its properties directly
- **Code**: [PortalMetric.cs](src/IoTHub.Portal.Shared/Models/v1.0/PortalMetric.cs)

### FR-009: Metrics exporter jobs MUST read from PortalMetric singleton
- **Status**: ✅ Verified
- **Evidence**: [DeviceMetricExporterJob.cs#L25-26](src/IoTHub.Portal.Infrastructure/Jobs/DeviceMetricExporterJob.cs#L25-26)
- **Code**: `this.deviceCounter.IncTo(this.portalMetric.DeviceCount);`

### FR-010: System MUST continuously ingest LoRaWAN telemetry from Azure Event Hub
- **Status**: ✅ Verified
- **Evidence**: [SyncLoRaDeviceTelemetryJob.cs](src/IoTHub.Portal.Infrastructure/Jobs/SyncLoRaDeviceTelemetryJob.cs)
- **Code**: Uses `EventProcessorClient` for continuous event processing

### FR-011: Telemetry ingestion MUST use checkpointing for reliable processing
- **Status**: ✅ Verified
- **Evidence**: [SyncLoRaDeviceTelemetryJob.cs#L29-34](src/IoTHub.Portal.Infrastructure/Jobs/SyncLoRaDeviceTelemetryJob.cs#L29-34)
- **Code**: Uses blob storage container `iothub-portal-events-checkpoints` for EventProcessorClient checkpointing

### FR-012: Metrics collection jobs MUST prevent concurrent execution
- **Status**: ✅ Verified
- **Evidence**: All metric jobs decorated with `[DisallowConcurrentExecution]` attribute
- **Files**: DeviceMetricLoaderJob.cs, EdgeDeviceMetricLoaderJob.cs, ConcentratorMetricLoaderJob.cs, etc.

### FR-013: Metrics collection MUST handle API failures gracefully with error logging
- **Status**: ✅ Verified
- **Evidence**: [DeviceMetricLoaderJob.cs#L36-38](src/IoTHub.Portal.Infrastructure/Jobs/DeviceMetricLoaderJob.cs#L36-38)
- **Code**: `catch (InternalServerErrorException e) { this.logger.LogError(...); }`

---

## Metrics Collection Jobs Verification

| Job | Spec Status | Implementation | Verified |
|-----|-------------|----------------|----------|
| DeviceMetricLoaderJob | ✅ | [DeviceMetricLoaderJob.cs](src/IoTHub.Portal.Infrastructure/Jobs/DeviceMetricLoaderJob.cs) | ✅ |
| EdgeDeviceMetricLoaderJob | ✅ | [EdgeDeviceMetricLoaderJob.cs](src/IoTHub.Portal.Infrastructure/Jobs/EdgeDeviceMetricLoaderJob.cs) | ✅ |
| ConcentratorMetricLoaderJob | ✅ | [ConcentratorMetricLoaderJob.cs](src/IoTHub.Portal.Infrastructure/Jobs/ConcentratorMetricLoaderJob.cs) | ✅ |
| DeviceMetricExporterJob | ✅ | [DeviceMetricExporterJob.cs](src/IoTHub.Portal.Infrastructure/Jobs/DeviceMetricExporterJob.cs) | ✅ |
| EdgeDeviceMetricExporterJob | ✅ | [EdgeDeviceMetricExporterJob.cs](src/IoTHub.Portal.Infrastructure/Jobs/EdgeDeviceMetricExporterJob.cs) | ✅ |
| ConcentratorMetricExporterJob | ✅ | [ConcentratorMetricExporterJob.cs](src/IoTHub.Portal.Infrastructure/Jobs/ConcentratorMetricExporterJob.cs) | ✅ |
| SyncLoRaDeviceTelemetryJob | ✅ | [SyncLoRaDeviceTelemetryJob.cs](src/IoTHub.Portal.Infrastructure/Jobs/SyncLoRaDeviceTelemetryJob.cs) | ✅ |

---

## Prometheus Metrics Verification

| Metric Name | Spec | Actual | Match |
|-------------|------|--------|-------|
| iot_hub_portal_device_count | Counter | Counter | ✅ |
| iot_hub_portal_connected_device_count | Counter | Counter | ✅ |
| iot_hub_portal_edge_device_count | Counter | Counter | ✅ |
| iot_hub_portal_connected_edge_device_count | Counter | Counter | ✅ |
| iot_hub_portal_failed_deployment_count | Counter | Counter | ✅ |
| iot_hub_portal_concentrator_count | Counter | Counter | ✅ |

**Note**: The code also defines `iot_hub_portal_connected_concentrator_count` in [MetricName.cs](src/IoTHub.Portal.Domain/Shared/Constants/MetricName.cs#L22), which is NOT mentioned in the spec.

---

## Inaccuracies Found ⚠️

### 1. Missing Metric: connected_concentrator_count
- **Severity**: Low
- **Spec Says**: Does not mention `connected_concentrator_count`
- **Actual**: [MetricName.cs#L22](src/IoTHub.Portal.Domain/Shared/Constants/MetricName.cs#L22) defines `ConnectedConcentratorCount`
- **Impact**: Spec is incomplete regarding available metrics

### 2. Data Source for ConcentratorMetricLoaderJob
- **Severity**: Low
- **Spec Says**: "Local database" as data source
- **Actual**: Uses `IExternalDeviceService.GetConcentratorsCount()` which queries the cloud provider
- **Evidence**: [ConcentratorMetricLoaderJob.cs#L33](src/IoTHub.Portal.Infrastructure/Jobs/ConcentratorMetricLoaderJob.cs#L33)

### 3. SyncLoRaDeviceTelemetryJob Missing [DisallowConcurrentExecution]
- **Severity**: Medium
- **Spec Says**: FR-012 states "Metrics collection jobs MUST prevent concurrent execution"
- **Actual**: `SyncLoRaDeviceTelemetryJob` does NOT have `[DisallowConcurrentExecution]` attribute
- **Evidence**: [SyncLoRaDeviceTelemetryJob.cs#L6](src/IoTHub.Portal.Infrastructure/Jobs/SyncLoRaDeviceTelemetryJob.cs#L6)
- **Impact**: Concurrent execution is not prevented for this job (though its long-running nature makes this less critical)

### 4. Metric Type Clarification
- **Severity**: Low
- **Spec Says**: Metrics are "Counter" type
- **Actual**: While implemented as Prometheus Counters, they use `IncTo()` method to set absolute values rather than incrementing
- **Evidence**: [DeviceMetricExporterJob.cs#L25](src/IoTHub.Portal.Infrastructure/Jobs/DeviceMetricExporterJob.cs#L25)
- **Note**: This is technically correct usage but spec could clarify this is "gauge-like" behavior using counters

---

## Job Scheduling Configuration

| Job Type | Scheduler Interval | Configuration |
|----------|-------------------|---------------|
| Exporter Jobs | `MetricExporterRefreshIntervalInSeconds` | [QuartzConfiguratorExtension.cs#L17](src/IoTHub.Portal.Infrastructure/Extenstions/QuartzConfiguratorExtension.cs#L17) |
| Loader Jobs | `MetricLoaderRefreshIntervalInMinutes` | [QuartzConfiguratorExtension.cs#L25](src/IoTHub.Portal.Infrastructure/Extenstions/QuartzConfiguratorExtension.cs#L25) |
| SyncLoRaDeviceTelemetryJob | Starts once after 1 minute delay | [AzureServiceCollectionExtension.cs#L164](src/IoTHub.Portal.Infrastructure/Startup/AzureServiceCollectionExtension.cs#L164) |

---

## Test Coverage

| Job | Test File | Status |
|-----|-----------|--------|
| DeviceMetricLoaderJob | [DeviceMetricLoaderJobTests.cs](src/IoTHub.Portal.Tests.Unit/Infrastructure/Jobs/DeviceMetricLoaderJobTests.cs) | ✅ |
| EdgeDeviceMetricLoaderJob | [EdgeDeviceMetricLoaderJobTests.cs](src/IoTHub.Portal.Tests.Unit/Infrastructure/Jobs/EdgeDeviceMetricLoaderJobTests.cs) | ✅ |
| ConcentratorMetricLoaderJob | [ConcentratorMetricLoaderJobTests.cs](src/IoTHub.Portal.Tests.Unit/Infrastructure/Jobs/ConcentratorMetricLoaderJobTests.cs) | ✅ |
| DeviceMetricExporterJob | [DeviceMetricExporterJobTests.cs](src/IoTHub.Portal.Tests.Unit/Infrastructure/Jobs/DeviceMetricExporterJobTests.cs) | ✅ |
| EdgeDeviceMetricExporterJob | [EdgeDeviceMetricExporterJobTests.cs](src/IoTHub.Portal.Tests.Unit/Infrastructure/Jobs/EdgeDeviceMetricExporterJobTests.cs) | ✅ |
| ConcentratorMetricExporterJob | [ConcentratorMetricExporterJobTests.cs](src/IoTHub.Portal.Tests.Unit/Infrastructure/Jobs/ConcentratorMetricExporterJobTests.cs) | ✅ |
| SyncLoRaDeviceTelemetryJob | ⚠️ No dedicated test file found | ⚠️ Missing |

---

## Recommendations

1. **Add connected_concentrator_count to spec**: Document the `iot_hub_portal_connected_concentrator_count` metric that exists in the codebase.

2. **Correct ConcentratorMetricLoaderJob data source**: Update spec to state it uses "Cloud provider API" instead of "Local database".

3. **Add [DisallowConcurrentExecution] to SyncLoRaDeviceTelemetryJob**: For consistency with FR-012, though the job's infinite loop nature makes this less critical.

4. **Add SyncLoRaDeviceTelemetryJob tests**: Create unit tests for this job to improve coverage.

5. **Clarify Counter vs Gauge semantics**: The spec should clarify that while Prometheus Counters are used, they're reset to absolute values each cycle using `IncTo()`.

6. **Document LoRa feature flag**: Clarify that ConcentratorMetricLoaderJob, ConcentratorMetricExporterJob, and SyncLoRaDeviceTelemetryJob only run when `configuration.IsLoRaEnabled` is true.

---

## Code References

| Component | File Path | Lines |
|-----------|-----------|-------|
| DeviceMetricLoaderJob | [src/IoTHub.Portal.Infrastructure/Jobs/DeviceMetricLoaderJob.cs](src/IoTHub.Portal.Infrastructure/Jobs/DeviceMetricLoaderJob.cs) | 1-54 |
| DeviceMetricExporterJob | [src/IoTHub.Portal.Infrastructure/Jobs/DeviceMetricExporterJob.cs](src/IoTHub.Portal.Infrastructure/Jobs/DeviceMetricExporterJob.cs) | 1-33 |
| EdgeDeviceMetricLoaderJob | [src/IoTHub.Portal.Infrastructure/Jobs/EdgeDeviceMetricLoaderJob.cs](src/IoTHub.Portal.Infrastructure/Jobs/EdgeDeviceMetricLoaderJob.cs) | 1-69 |
| EdgeDeviceMetricExporterJob | [src/IoTHub.Portal.Infrastructure/Jobs/EdgeDeviceMetricExporterJob.cs](src/IoTHub.Portal.Infrastructure/Jobs/EdgeDeviceMetricExporterJob.cs) | 1-36 |
| ConcentratorMetricLoaderJob | [src/IoTHub.Portal.Infrastructure/Jobs/ConcentratorMetricLoaderJob.cs](src/IoTHub.Portal.Infrastructure/Jobs/ConcentratorMetricLoaderJob.cs) | 1-42 |
| ConcentratorMetricExporterJob | [src/IoTHub.Portal.Infrastructure/Jobs/ConcentratorMetricExporterJob.cs](src/IoTHub.Portal.Infrastructure/Jobs/ConcentratorMetricExporterJob.cs) | 1-30 |
| SyncLoRaDeviceTelemetryJob | [src/IoTHub.Portal.Infrastructure/Jobs/SyncLoRaDeviceTelemetryJob.cs](src/IoTHub.Portal.Infrastructure/Jobs/SyncLoRaDeviceTelemetryJob.cs) | 1-72 |
| PortalMetric | [src/IoTHub.Portal.Shared/Models/v1.0/PortalMetric.cs](src/IoTHub.Portal.Shared/Models/v1.0/PortalMetric.cs) | 1-20 |
| MetricName Constants | [src/IoTHub.Portal.Domain/Shared/Constants/MetricName.cs](src/IoTHub.Portal.Domain/Shared/Constants/MetricName.cs) | 1-24 |
| Job Configuration | [src/IoTHub.Portal.Infrastructure/Startup/AzureServiceCollectionExtension.cs](src/IoTHub.Portal.Infrastructure/Startup/AzureServiceCollectionExtension.cs) | 112-180 |
| Metrics Extension | [src/IoTHub.Portal.Infrastructure/Extenstions/QuartzConfiguratorExtension.cs](src/IoTHub.Portal.Infrastructure/Extenstions/QuartzConfiguratorExtension.cs) | 1-29 |
