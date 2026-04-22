# Feature: Dashboard Metrics

**Category**: Monitoring & Analytics  
**Status**: Analyzed  

---

## Description

The Dashboard Metrics feature provides real-time visibility into the overall health and status of the IoT Hub Portal by aggregating and exposing key performance indicators and statistics. This feature serves as the foundation for monitoring portal operations, enabling administrators and operators to quickly assess the state of their IoT device ecosystem at a glance.

Key capabilities include:

- Retrieving comprehensive portal-wide metrics in a single API call
- Tracking total device count across all device types
- Monitoring connected device status for standard IoT devices
- Tracking Edge device inventory and connectivity status
- Monitoring failed deployment counts for troubleshooting
- Tracking LoRaWAN concentrator counts (when LoRa features are enabled)
- Providing metrics suitable for dashboard UI rendering
- Real-time metric updates reflecting current portal state
- Simple REST API endpoint with authorization controls
- Metrics aggregated from multiple underlying IoT Hub services

This feature provides critical business value by:
- Enabling proactive monitoring of IoT infrastructure health
- Providing at-a-glance operational visibility for support teams
- Supporting capacity planning through device count tracking
- Facilitating troubleshooting through deployment failure visibility
- Enabling quick identification of connectivity issues
- Supporting SLA compliance monitoring
- Reducing mean time to detection (MTTD) for infrastructure issues
- Providing data foundation for operational dashboards and reporting

The metrics are sourced from Azure IoT Hub device twins, deployment status, and LoRaWAN-specific components (when enabled). The feature uses ASP.NET Core's dependency injection to provide a singleton PortalMetric instance that's updated periodically by background services, ensuring low-latency metric retrieval without direct IoT Hub queries on every request.

---

## Code Locations

### Entry Points / Endpoints
- `src/IoTHub.Portal.Server/Controllers/v1.0/DashboardController.cs` (Lines 1-27)
  - **Snippet**: Main REST API controller for dashboard metrics retrieval
    ```csharp
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/dashboard")]
    [ApiExplorerSettings(GroupName = "Metrics")]
    public class DashboardController : ControllerBase
    {
        private readonly PortalMetric portalMetric;
        
        public DashboardController(PortalMetric portalMetric)
        {
            this.portalMetric = portalMetric;
        }
        
        [HttpGet("metrics", Name = "Get Portal Metrics")]
        [Authorize("dashboard:read")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PortalMetric))]
        public ActionResult<PortalMetric> GetPortalMetrics()
        {
            return this.portalMetric;
        }
    }
    ```

### Data Models
- `src/IoTHub.Portal.Shared/Models/v1.0/PortalMetric.cs` (Lines 1-20)
  - **Snippet**: Metrics data model containing all portal statistics
    ```csharp
    public class PortalMetric
    {
        public int DeviceCount { get; set; }
        public int ConnectedDeviceCount { get; set; }
        public int EdgeDeviceCount { get; set; }
        public int ConnectedEdgeDeviceCount { get; set; }
        public int FailedDeploymentCount { get; set; }
        public int ConcentratorCount { get; set; }
    }
    ```

### Authorization
- Authorization policy: `dashboard:read`
- Requires authenticated user with dashboard read permissions
- Enforces RBAC through ASP.NET Core authorization policies

---

## Data Flow

```
1. Client Request → GET /api/dashboard/metrics
2. Authorization Check → Validate "dashboard:read" permission
3. Controller → Returns pre-populated PortalMetric instance
4. Response → JSON with 6 metric values
```

**Key aspects:**
- **Singleton Pattern**: PortalMetric is registered as a singleton, shared across all requests
- **Background Updates**: Metrics are updated by background services/jobs (not visible in controller code)
- **Low Latency**: No direct IoT Hub queries during request handling
- **Cache-like Behavior**: Returns current snapshot without real-time computation

---

## Dependencies

### Internal Services/Components
- **PortalMetric** (injected singleton): Pre-populated metrics object updated by background services

### External Services
- Indirectly depends on Azure IoT Hub for device data (via background metric collection)
- Potential dependency on LoRaWAN network server for concentrator counts

### Configuration
- No direct configuration in controller
- Authorization policy `dashboard:read` must be configured in authorization setup

---

## Business Logic

### Core Workflows

**1. Get Portal Metrics**
```
Input: None (authenticated request only)
Process:
  1. Validate user has "dashboard:read" permission
  2. Return current PortalMetric singleton instance
Output: PortalMetric JSON with 6 counters
```

### Validation Rules
- **Authentication Required**: All endpoints require valid authentication token
- **Authorization Required**: User must have `dashboard:read` permission
- **No Input Validation**: Endpoint accepts no parameters

### Error Handling
- **401 Unauthorized**: Missing or invalid authentication token
- **403 Forbidden**: User lacks `dashboard:read` permission
- **500 Internal Server Error**: Unexpected exceptions during metric retrieval

---

## User Interface Integration

### Frontend Integration Points
- **Dashboard Home Page**: Primary consumer of metrics
- **Header/Navigation**: May display device counts
- **Monitoring Widgets**: Can embed individual metric tiles

### Expected UI Behaviors
```
Dashboard Page Load:
  → Fetch /api/dashboard/metrics
  → Display 6 metric cards with counts
  → Show connectivity percentages (ConnectedDeviceCount / DeviceCount)
  → Highlight failed deployments if count > 0
  → Auto-refresh metrics periodically (e.g., every 30 seconds)
```

### UI Component Suggestions
- Metric cards with icon, label, and numeric value
- Color coding: Green (healthy), Yellow (warnings), Red (failures)
- Trend indicators if historical data available
- Click-through to detailed views (e.g., failed deployments list)

---

## Testing Considerations

### Unit Testing
- **Mock Dependencies**: Mock PortalMetric instance with test data
- **Authorization Testing**: Verify policy enforcement for `dashboard:read`
- **Response Format**: Validate JSON structure matches PortalMetric model

### Integration Testing
- **Background Service Integration**: Verify metrics are populated correctly
- **Multiple Scenarios**:
  - Empty portal (all zeros)
  - Mixed device states
  - High failure counts
  - LoRa enabled/disabled scenarios

### End-to-End Testing
- **Dashboard Load**: Verify metrics display on dashboard page
- **Authorization Flows**: Test with various user roles
- **Real-time Updates**: Verify metrics reflect actual device changes
- **Performance**: Load test with high device counts

---

## Performance Considerations

### Scalability
- **O(1) Complexity**: Direct singleton access, no iteration
- **No Database Queries**: Pre-computed metrics in memory
- **Minimal CPU/Memory**: Simple object serialization
- **High Concurrency**: Thread-safe singleton access

### Optimization Strategies
- Use HTTP caching headers (Cache-Control) if metrics update frequency is known
- Implement ETag support for conditional requests
- Consider adding metric timestamps for staleness detection

### Monitoring Recommendations
- Track API response times (should be <50ms)
- Monitor metric staleness (last update timestamp)
- Alert on metric retrieval failures
- Track authorization failure rates

---

## Security Analysis

### Authentication & Authorization
- **Authentication Required**: `[Authorize]` attribute on controller
- **Fine-grained Authorization**: `dashboard:read` permission required
- **No Anonymous Access**: Prevents unauthorized metric exposure

### Data Sensitivity
- **Low Sensitivity**: Aggregated counts, no PII or device details
- **Potential Information Disclosure**: Counts reveal infrastructure scale
- **No Filtering**: Returns portal-wide metrics (not tenant-scoped)

### Security Recommendations
- ✅ Proper authorization in place
- ✅ No sensitive data exposure
- ⚠️ Consider tenant-scoped metrics for multi-tenant deployments
- ⚠️ Rate limiting for metric endpoint to prevent abuse
- ⚠️ Add audit logging for metric access by privileged users

---

## Configuration & Deployment

### Configuration Requirements
- **Authorization Policy**: `dashboard:read` must be defined in policy configuration
- **Metric Collection**: Background services must be configured to populate PortalMetric singleton
- **DI Registration**: PortalMetric must be registered as singleton in Startup/Program.cs

### Environment Variables
- No direct environment variables in this feature
- Authorization configuration may reference environment-specific settings

### Deployment Checklist
- ✅ Verify PortalMetric singleton registration
- ✅ Confirm background metric collection services are enabled
- ✅ Validate `dashboard:read` permission exists in role definitions
- ✅ Test metric retrieval in target environment
- ✅ Configure monitoring/alerting for metric staleness

---

## Known Issues & Limitations

### Current Limitations
1. **No Historical Data**: Only current snapshot, no trends
2. **No Tenant Filtering**: Portal-wide metrics only
3. **No Metric Timestamps**: Cannot determine data freshness
4. **No Partial Failures**: No indication if some metrics failed to update
5. **Fixed Metric Set**: Cannot customize which metrics are returned

### Technical Debt
- Missing metric update timestamp in response
- No metric health/staleness indicator
- No support for custom metric aggregations
- Limited extensibility for new metric types

### Future Considerations
- Add timestamp field to PortalMetric model
- Implement metric health checks
- Support tenant-scoped metrics
- Add historical data endpoints (trends, time-series)
- Implement metric subscriptions (WebSocket/SignalR)
- Add custom metric definitions via configuration

---

## Related Features

### Directly Related
- Device Management (source of DeviceCount metrics)
- Edge Device Management (source of EdgeDeviceCount metrics)
- Deployment Management (source of FailedDeploymentCount)
- LoRaWAN Integration (source of ConcentratorCount)

### Dependent Features
- Role Management (defines `dashboard:read` permission)
- Background Jobs/Services (metric collection and update)

### Integration Points
- Dashboard UI (primary consumer)
- Monitoring systems (may consume metrics via API)
- Alerting systems (may trigger on metric thresholds)

---

## Migration & Compatibility

### API Versioning
- Current version: 1.0
- Endpoint: `/api/dashboard/metrics`
- Version included in route via `[ApiVersion("1.0")]`

### Breaking Change Considerations
- Adding new metrics: Non-breaking (additive)
- Removing metrics: Breaking change
- Changing metric semantics: Breaking change
- Renaming metrics: Breaking change

### Backward Compatibility
- Current API is stable
- New metrics should be added as optional fields
- Consider API v2.0 for significant metric model changes

---

## Documentation & Examples

### API Documentation
```http
GET /api/dashboard/metrics
Authorization: Bearer {token}

Response 200 OK:
{
  "deviceCount": 1523,
  "connectedDeviceCount": 1401,
  "edgeDeviceCount": 45,
  "connectedEdgeDeviceCount": 42,
  "failedDeploymentCount": 3,
  "concentratorCount": 12
}
```

### Usage Examples

**PowerShell:**
```powershell
$token = "your-jwt-token"
$headers = @{ Authorization = "Bearer $token" }
$response = Invoke-RestMethod -Uri "https://portal.example.com/api/dashboard/metrics" -Headers $headers
Write-Host "Total Devices: $($response.deviceCount)"
Write-Host "Connected: $($response.connectedDeviceCount)"
```

**cURL:**
```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
     https://portal.example.com/api/dashboard/metrics
```

**JavaScript (fetch):**
```javascript
const response = await fetch('/api/dashboard/metrics', {
  headers: { 'Authorization': `Bearer ${token}` }
});
const metrics = await response.json();
console.log(`Devices: ${metrics.deviceCount}, Connected: ${metrics.connectedDeviceCount}`);
```

---

## References

### Related Documentation
- API Documentation: Swagger/OpenAPI definition at `/swagger`
- Authorization Guide: Role and permission configuration
- Deployment Guide: Failed deployment troubleshooting

### External Resources
- ASP.NET Core Authorization: https://docs.microsoft.com/aspnet/core/security/authorization/
- Azure IoT Hub Metrics: https://docs.microsoft.com/azure/iot-hub/monitor-iot-hub

---

**Last Updated**: 2025-01-27  
**Analyzed By**: GitHub Copilot  
**Next Review**: When metric model changes or new aggregations are needed
