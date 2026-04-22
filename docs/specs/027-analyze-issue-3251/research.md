# Research: Device Import Data Overwrite Bug Fix

**Date**: 2026-01-30  
**Feature**: Fix LoRaWAN device import data loss during synchronization

## Problem Analysis

### Current Flow
1. User imports CSV with complete LoRaWAN device data (30+ fields)
2. `ExportManager.ImportLoRaDevice()` reads CSV and creates `LoRaDeviceDetails` object
3. Only **basic authentication fields** are written to Azure IoT Hub as desired properties:
   - AppKey, AppEUI (OTAA mode) OR AppSKey, NwkSKey, DevAddr (ABP mode)
   - GatewayID
4. Device is saved to local database with all CSV data
5. **Synchronization job runs** (`SyncDevicesJob.Execute()`)
6. Job fetches device Twin from Azure IoT Hub
7. AutoMapper maps Twin → LorawanDevice entity
8. **Data loss occurs**: Fields not in IoT Hub are mapped to null/default values
9. Database entity is updated, overwriting CSV-imported values

### Root Cause
The import process in `ExportManager.ImportLoRaDevice()` only writes a minimal subset of LoRaWAN properties to IoT Hub:
- Only reads 5 authentication properties from CSV (lines 277-288)
- Other 20+ LoRaWAN-specific properties (ClassType, PreferredWindow, Deduplication, RX1DROffset, RX2DataRate, RXDelay, ABPRelaxMode, FCntUpStart, FCntDownStart, FCntResetCounter, Supports32BitFCnt, KeepAliveTimeout, SensorDecoder, Downlink) are never pushed to IoT Hub
- These fields exist in `LoRaDeviceBase.cs` and are stored locally but not synced to cloud
- Sync job's AutoMapper (lines 71-98 in `DeviceProfile.cs`) correctly reads from IoT Hub desired properties, but finds nothing there

## Technical Decisions

### Decision 1: Expand CSV Import to Write All LoRaWAN Properties

**Decision**: Modify `ExportManager.ImportLoRaDevice()` to write all LoRaWAN-specific properties from CSV to Azure IoT Hub desired properties.

**Rationale**: 
- Azure IoT Hub is the source of truth for device configuration
- Synchronization job correctly reads from IoT Hub - the issue is import doesn't write there
- This maintains architectural consistency: local DB is a cache, IoT Hub is authoritative
- The export template already includes these properties in the CSV header (via `GetPropertiesToExport()`)

**Properties to add** (from `LoRaDeviceBase` and `LoRaDeviceDetails`):
- ClassType
- PreferredWindow  
- Deduplication
- RX1DROffset
- RX2DataRate
- RXDelay
- ABPRelaxMode
- FCntUpStart
- FCntDownStart
- FCntResetCounter
- Supports32BitFCnt
- KeepAliveTimeout
- SensorDecoder
- Downlink

**Implementation approach**:
```csharp
// In ImportLoRaDevice() after line 288, add calls like:
TryReadProperty(csvReader, newDevice, c => c.ClassType, ClassType.A);
TryReadProperty(csvReader, newDevice, c => c.PreferredWindow, 1);
TryReadProperty(csvReader, newDevice, c => c.Deduplication, DeduplicationMode.None);
// ... etc for all properties
```

**Alternatives considered**:
1. **Modify sync job to preserve local-only fields**: Rejected - violates "IoT Hub as source of truth" principle
2. **Skip sync for recently imported devices**: Rejected - creates inconsistency, complex timing logic
3. **Store import metadata to prevent overwrite**: Rejected - adds unnecessary complexity and state tracking

### Decision 2: Reuse Existing TryReadProperty Helper

**Decision**: Use the existing `TryReadProperty<T, TValue>()` generic helper method (lines 328-335) for all new property reads.

**Rationale**:
- Already handles CSV field naming convention (`PROPERTY:{PropertyName}`)
- Provides default value fallback mechanism
- Type-safe with compile-time checks via Expression trees
- Consistent with existing code patterns

**Example usage**:
```csharp
TryReadProperty(csvReader, newDevice, c => c.RX1DROffset, (int?)null);
TryReadProperty(csvReader, newDevice, c => c.Downlink, true);
```

### Decision 3: Handle Default Values Carefully

**Decision**: When reading from CSV, use the same defaults as defined in `LoRaDeviceBase.cs` (via `[DefaultValue]` attributes).

**Rationale**:
- Maintains consistency between import and normal device creation
- Users who leave fields empty in CSV get sensible defaults
- Prevents null reference issues for non-nullable properties

**Default values mapping**:
```csharp
ClassType = ClassType.A
PreferredWindow = 1
Deduplication = DeduplicationMode.None  // Note: DTO has None, Base has Drop - use None for consistency with LoRaDeviceDetails constructor
ABPRelaxMode = true
FCntUpStart = 0
FCntDownStart = 0
FCntResetCounter = 0
Supports32BitFCnt = true
RX1DROffset = 0
RX2DataRate = 0
Downlink = true
```

### Decision 4: Preserve Backward Compatibility

**Decision**: Make all new CSV columns optional - if not present in CSV, use defaults.

**Rationale**:
- Existing CSV files in the wild may not have new columns
- Users may be importing older export files
- `CsvReader.TryGetField()` returns false if column doesn't exist - handle gracefully

**Implementation**: The `TryReadProperty()` method already handles missing fields via `TryGetField()`, returning the provided default value.

## Technology Best Practices

### CSV Handling with CsvHelper
- **Use TryGetField**: Never assume column exists, always handle missing gracefully
- **Type conversion**: CsvHelper handles primitive type parsing; for enums use explicit parsing
- **Culture invariance**: Already configured with `CultureInfo.InvariantCulture` for consistent parsing

### AutoMapper with Azure IoT Hub Twins
- **Desired vs Reported properties**: 
  - Desired = configuration set by portal → device
  - Reported = state reported by device → portal
- **Use `DeviceHelper.RetrieveDesiredPropertyValue()`**: Existing helper safely extracts desired properties
- **Null handling**: Properties not in Twin return null - AutoMapper handles this correctly

### LoRaWAN Device Lifecycle
1. Import/Create → writes to IoT Hub desired properties
2. Device connects → reads desired properties from IoT Hub
3. Device reports status → writes to IoT Hub reported properties  
4. Sync job runs → reads both desired and reported, updates local DB
5. User views device → reads from local DB (cached)

### Testing Strategy
- **Unit tests for import**: Mock CsvReader with complete property set
- **Unit tests for sync**: Mock Twin with all desired properties populated
- **Integration test**: Import CSV → trigger sync → verify data persists
- **Verify roundtrip**: Export → Import → Export again should be identical

## Integration Patterns

### Azure IoT Hub Device Twin Structure
```json
{
  "deviceId": "ABC123...",
  "tags": {
    "deviceName": "My Device",
    "modelId": "model-guid",
    "supportLoRaFeatures": "true"
  },
  "properties": {
    "desired": {
      "AppKey": "...",
      "ClassType": "A",
      "PreferredWindow": 1,
      "Downlink": true,
      ...
    },
    "reported": {
      "DevAddr": "...",
      "DataRate": "SF12BW125",
      "GatewayID": "gateway-1"
    }
  }
}
```

### Device Service Update Flow
When `LoRaWanDeviceService.UpdateDevice()` is called:
1. Maps DTO → Entity via AutoMapper
2. Deletes existing tags and labels (lines 87-95)
3. Updates entity in database
4. Calls `DeviceServiceBase.UpdateDevice()` → pushes to IoT Hub via `externalDeviceService`
5. IoT Hub Twin is updated with new desired properties

**Key insight**: The service correctly pushes to IoT Hub, but only with properties that exist on the DTO. Import must populate all properties on the DTO before calling service.

## Dependencies and Versions

### NuGet Packages (Already in Project)
- **CsvHelper**: CSV parsing library - no version constraints
- **AutoMapper**: Object mapping - current patterns work correctly
- **Microsoft.Azure.Devices**: IoT Hub SDK - existing integration sufficient

### Enum Dependencies
- `ClassType`: Already defined in `IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/ClassType.cs`
- `DeduplicationMode`: Already defined in `IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/DeduplicationMode.cs`

No new dependencies required.

## Risk Assessment

### Low Risk
- ✅ Changes are localized to import method
- ✅ Existing CSV column order unchanged (only adds optional columns)
- ✅ Default values ensure safe fallback
- ✅ No database schema changes required

### Medium Risk  
- ⚠️ CSV export template needs verification - does it already export these fields?
  - **Resolution**: Code review shows `GetPropertiesToExport()` already includes LoRaWAN fields (lines 96-103)
- ⚠️ Enum string conversion in CSV may need explicit handling
  - **Resolution**: Use `Enum.TryParse()` with default fallback

### Mitigations
- Add comprehensive unit tests covering all new properties
- Test with real CSV files (both new format with all columns, and old format with minimal columns)
- Verify export → import → export cycle preserves all data

## Open Questions (Resolved)

### Q1: Should device model defaults override CSV imports?
**Answer**: No. CSV explicitly provided values should take precedence. Device model defaults should only apply when creating a new device from UI, not during CSV import. This matches user expectation: "I imported this value, why was it changed?"

### Q2: What about properties in device model but not in CSV?
**Answer**: Device model properties are separate from LoRaWAN-specific properties. Model properties are already handled correctly via `ImportDevice()` method (lines 295-326) and are not part of this bug fix scope.

### Q3: Do we need to update the CSV export template?
**Answer**: Yes, but it's already done! The `ExportTemplateFile()` method calls `GetPropertiesToExport()` which includes all LoRaWAN properties (AppKey, AppEUI, AppSKey, NwkSKey, DevAddr, GatewayID). The template already has columns for these - the bug is that import doesn't read them all.

## Conclusion

The fix is straightforward:
1. Expand `ImportLoRaDevice()` to read all 14 additional LoRaWAN properties from CSV
2. Set appropriate default values for missing/empty fields
3. Let existing service layer push complete data to IoT Hub
4. Sync job will then correctly retrieve and preserve all data

No architectural changes needed - this is a straightforward expansion of the import logic to handle all properties that export already supports.
