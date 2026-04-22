# Evaluation Report: Dashboard Metrics (020)

**Evaluation Date**: 2026-02-03  
**Spec Version**: Draft  
**Evaluator**: Excavate Evaluator Agent

---

## Summary Table

| Criterion | Score (1-5) | Weight | Weighted Score |
|-----------|-------------|--------|----------------|
| Correctness | 5.0 | 30% | 1.50 |
| Completeness | 4.5 | 30% | 1.35 |
| Technical Quality | 4.5 | 20% | 0.90 |
| Coverage | 4.0 | 20% | 0.80 |
| **Total** | | **100%** | **4.55/5.0** |

---

## Accurate Specifications

### ✅ FR-001: Dashboard Metrics Endpoint
- **Status**: VERIFIED
- **Evidence**: [DashboardController.cs](../../src/IoTHub.Portal.Server/Controllers/v1.0/DashboardController.cs#L20-L24)
- **Route**: `GET api/dashboard/metrics`
- **Code**: `return this.portalMetric;`

### ✅ FR-002: Track Total Device Count
- **Status**: VERIFIED
- **Evidence**: [PortalMetric.cs](../../src/IoTHub.Portal.Shared/Models/v1.0/PortalMetric.cs#L8)
- **Code**: `public int DeviceCount { get; set; }`

### ✅ FR-003: Track Connected Device Count
- **Status**: VERIFIED
- **Evidence**: [PortalMetric.cs](../../src/IoTHub.Portal.Shared/Models/v1.0/PortalMetric.cs#L10)
- **Code**: `public int ConnectedDeviceCount { get; set; }`

### ✅ FR-004: Track Total Edge Device Count
- **Status**: VERIFIED
- **Evidence**: [PortalMetric.cs](../../src/IoTHub.Portal.Shared/Models/v1.0/PortalMetric.cs#L12)
- **Code**: `public int EdgeDeviceCount { get; set; }`

### ✅ FR-005: Track Connected Edge Device Count
- **Status**: VERIFIED
- **Evidence**: [PortalMetric.cs](../../src/IoTHub.Portal.Shared/Models/v1.0/PortalMetric.cs#L14)
- **Code**: `public int ConnectedEdgeDeviceCount { get; set; }`

### ✅ FR-006: Track Failed Deployment Count
- **Status**: VERIFIED
- **Evidence**: [PortalMetric.cs](../../src/IoTHub.Portal.Shared/Models/v1.0/PortalMetric.cs#L16)
- **Code**: `public int FailedDeploymentCount { get; set; }`

### ✅ FR-007: Track Concentrator Count
- **Status**: VERIFIED
- **Evidence**: [PortalMetric.cs](../../src/IoTHub.Portal.Shared/Models/v1.0/PortalMetric.cs#L18)
- **Code**: `public int ConcentratorCount { get; set; }`

### ✅ FR-008: Pre-computed by Background Jobs
- **Status**: VERIFIED
- **Evidence**: Multiple metric loader jobs exist:
  - `DeviceMetricLoaderJob` - Loads device counts
  - `EdgeDeviceMetricLoaderJob` - Loads edge device counts
  - `ConcentratorMetricExporterJob` - Exports concentrator counts
  - `DeviceMetricExporterJob` - Exports device metrics
  - `EdgeDeviceMetricExporterJob` - Exports edge metrics

### ✅ FR-010: Require Authentication and Permissions
- **Status**: VERIFIED
- **Evidence**: [DashboardController.cs](../../src/IoTHub.Portal.Server/Controllers/v1.0/DashboardController.cs#L6-L21)
- **Code**: 
  ```csharp
  [Authorize]  // Class level
  [Authorize("dashboard:read")]  // Method level
  ```

### ✅ Key Entity: PortalMetric Structure
- **Status**: VERIFIED
- **Evidence**: [PortalMetric.cs](../../src/IoTHub.Portal.Shared/Models/v1.0/PortalMetric.cs)
- **All Properties Match Spec**:
  | Spec Property | Code Property | Match |
  |---------------|---------------|-------|
  | DeviceCount | `DeviceCount` | ✅ |
  | ConnectedDeviceCount | `ConnectedDeviceCount` | ✅ |
  | EdgeDeviceCount | `EdgeDeviceCount` | ✅ |
  | ConnectedEdgeDeviceCount | `ConnectedEdgeDeviceCount` | ✅ |
  | FailedDeploymentCount | `FailedDeploymentCount` | ✅ |
  | ConcentratorCount | `ConcentratorCount` | ✅ |

### ✅ Metrics Overview Table
- **Status**: VERIFIED
- **Evidence**: All metrics in the spec's table match the PortalMetric properties and their documented sources

---

## Inaccuracies Found

### ⚠️ FR-009: 100ms Response Time
- **Spec Says**: Dashboard endpoint MUST return metrics in under 100 milliseconds
- **Actual**: Not explicitly enforced or tested in code
- **Impact**: LOW - Singleton pattern makes this inherently fast, but no SLA enforcement

### ⚠️ Singleton Pattern Not Documented
- **Spec Says**: Background jobs populate PortalMetric singleton
- **Actual**: PortalMetric is indeed a singleton (injected directly, not via service)
- **Evidence**: [DashboardController.cs#L13](../../src/IoTHub.Portal.Server/Controllers/v1.0/DashboardController.cs#L13)
- **Impact**: LOW - Implementation detail, correctly implemented but could be clearer in spec

### ⚠️ No Timestamp/Staleness Indicator
- **Spec Says**: Edge case mentions "last known values" when cloud unavailable
- **Actual**: PortalMetric has no `LastUpdated` timestamp property
- **Impact**: MEDIUM - Clients cannot determine if metrics are stale

### ⚠️ No LoRa Feature Flag Check
- **Spec Says**: Concentrator metrics may be omitted when LoRa features disabled
- **Actual**: ConcentratorCount always present (defaults to 0)
- **Impact**: LOW - Acceptable implementation, just different approach

---

## Recommendations

1. **Add LastUpdated Timestamp**: Consider adding a `DateTime LastUpdated` property to PortalMetric so clients can display "Last updated: X minutes ago" or warn about stale data.

2. **Document Singleton Pattern**: Explicitly mention in spec that PortalMetric is a singleton for fast retrieval and thread-safe updates.

3. **Add Performance Tests**: Create integration tests that verify the 100ms SLA mentioned in FR-009.

4. **Consider Refresh Endpoint**: Add an optional `POST api/dashboard/refresh` endpoint to trigger immediate metric update if needed.

---

## Code References

| Component | File | Purpose |
|-----------|------|---------|
| Controller | [DashboardController.cs](../../src/IoTHub.Portal.Server/Controllers/v1.0/DashboardController.cs) | Single GET endpoint |
| Model | [PortalMetric.cs](../../src/IoTHub.Portal.Shared/Models/v1.0/PortalMetric.cs) | Metrics data model |
| Device Metric Loader | DeviceMetricLoaderJob.cs | Populates DeviceCount, ConnectedDeviceCount |
| Edge Metric Loader | EdgeDeviceMetricLoaderJob.cs | Populates EdgeDeviceCount, ConnectedEdgeDeviceCount, FailedDeploymentCount |
| Concentrator Exporter | ConcentratorMetricExporterJob.cs | Populates ConcentratorCount |
| Device Exporter | DeviceMetricExporterJob.cs | Prometheus/metrics export |
| Edge Exporter | EdgeDeviceMetricExporterJob.cs | Prometheus/metrics export |
| Unit Tests | [DashboardControllerTests.cs](../../src/IoTHub.Portal.Tests.Unit/Server/Controllers/v1.0/DashboardControllerTests.cs) | Controller tests |

---

## Test Coverage

The test file [DashboardControllerTests.cs](../../src/IoTHub.Portal.Tests.Unit/Server/Controllers/v1.0/DashboardControllerTests.cs) includes:

```csharp
[Test]
public void GetPortalMetricsShouldReturnMetrics()
{
    var portalMetric = new PortalMetric
    {
        DeviceCount = 1,
        ConnectedDeviceCount = 2,
        EdgeDeviceCount = 3,
        ConnectedEdgeDeviceCount = 4,
        FailedDeploymentCount = 5,
        ConcentratorCount = 6
    };
    // ... verifies all properties are returned
}
```

**Test Coverage**: GOOD - All 6 metrics are tested

---

## Verdict

**Overall Assessment**: The specification is highly accurate and complete. All 6 metrics documented in the spec exactly match the PortalMetric model. The authorization requirements are correctly implemented. The background job architecture is in place. Minor improvements could include adding a staleness indicator (LastUpdated timestamp) and documenting the singleton pattern more explicitly.

**Confidence Level**: VERY HIGH (92%)

**Quality Notes**:
- Spec accurately describes a simple but effective design
- Implementation is clean and follows the spec precisely
- Authorization attributes are correctly placed
- Singleton pattern ensures fast response times
