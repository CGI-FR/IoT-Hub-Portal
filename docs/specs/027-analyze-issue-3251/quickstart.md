# Quickstart: Device Import Bug Fix

**Feature**: Fix LoRaWAN device import data overwrite  
**Issue**: #3251  
**Status**: Implementation Planning Complete

## Overview

This guide helps developers understand and implement the fix for the device import bug where LoRaWAN configuration properties are lost after synchronization.

## Problem Summary

**Symptom**: After importing LoRaWAN devices via CSV, only basic fields are retained:
- ✅ Retained: Id, Name, ModelId, AppKey, AppEUI, AppSKey, NwkSKey, DevAddr, GatewayID
- ❌ Lost: ClassType, PreferredWindow, Deduplication, Downlink, RX1DROffset, RX2DataRate, RXDelay, ABPRelaxMode, FCntUpStart, FCntDownStart, FCntResetCounter, Supports32BitFCnt, KeepAliveTimeout, SensorDecoder

**Root Cause**: Import only writes authentication properties to Azure IoT Hub; sync job then overwrites database with incomplete IoT Hub data.

**Solution**: Expand import to write ALL LoRaWAN properties to IoT Hub.

## Quick Reference

### Files to Modify
| File | Purpose | Lines |
|------|---------|-------|
| `src/IoTHub.Portal.Server/Managers/ExportManager.cs` | Add property reads in `ImportLoRaDevice()` | After line 288 |
| `src/IoTHub.Portal.Tests.Unit/Server/Managers/ExportManagerTests.cs` | Update/add unit tests | Multiple locations |

### Files to Review (No Changes)
- `src/IoTHub.Portal.Application/Mappers/DeviceProfile.cs` - AutoMapper already correct
- `src/IoTHub.Portal.Infrastructure/Jobs/SyncDevicesJob.cs` - Sync logic already correct
- `src/IoTHub.Portal.Infrastructure/Services/LoRaWanDeviceService.cs` - Service already correct

## Implementation Steps

### Step 1: Understand Current Import Logic

**File**: `ExportManager.cs`, method `ImportLoRaDevice()` (lines 263-293)

Current implementation:
```csharp
private async Task ImportLoRaDevice(
    CsvReader csvReader,
    string deviceId, string deviceName, string modelId,
    Dictionary<string, string> deviceTags)
{
    var newDevice = new LoRaDeviceDetails()
    {
        DeviceID = deviceId,
        DeviceName = deviceName,
        ModelId = modelId,
        Tags = deviceTags,
        IsEnabled = true
    };

    // Only reads 5 authentication properties
    TryReadProperty(csvReader, newDevice, c => c.AppKey, string.Empty);
    TryReadProperty(csvReader, newDevice, c => c.AppEUI, string.Empty);
    if (string.IsNullOrEmpty(newDevice.AppKey) && string.IsNullOrEmpty(newDevice.AppEUI))
    {
        TryReadProperty(csvReader, newDevice, c => c.AppSKey, string.Empty);
        TryReadProperty(csvReader, newDevice, c => c.NwkSKey, string.Empty);
        TryReadProperty(csvReader, newDevice, c => c.DevAddr, string.Empty);
        newDevice.AppEUI = null;
        newDevice.AppKey = null;
    }
    TryReadProperty(csvReader, newDevice, c => c.GatewayID, string.Empty);

    // Missing: 14 other LoRaWAN configuration properties!

    await this.loraDeviceService.CheckIfDeviceExists(newDevice.DeviceID)
        ? await this.loraDeviceService.UpdateDevice(newDevice)
        : await this.loraDeviceService.CreateDevice(newDevice);
}
```

### Step 2: Add Missing Property Reads

After line 288 (`TryReadProperty(csvReader, newDevice, c => c.GatewayID, string.Empty);`), add:

```csharp
// LoRaWAN Configuration Properties
TryReadProperty(csvReader, newDevice, c => c.ClassType, ClassType.A);
TryReadProperty(csvReader, newDevice, c => c.PreferredWindow, 1);
TryReadProperty(csvReader, newDevice, c => c.Deduplication, DeduplicationMode.None);
TryReadProperty(csvReader, newDevice, c => c.Downlink, true);

// Advanced RX Settings
TryReadProperty(csvReader, newDevice, c => c.RX1DROffset, (int?)0);
TryReadProperty(csvReader, newDevice, c => c.RX2DataRate, (int?)0);
TryReadProperty(csvReader, newDevice, c => c.RXDelay, (int?)null);

// ABP Settings
TryReadProperty(csvReader, newDevice, c => c.ABPRelaxMode, true);
TryReadProperty(csvReader, newDevice, c => c.FCntUpStart, (int?)0);
TryReadProperty(csvReader, newDevice, c => c.FCntDownStart, (int?)0);
TryReadProperty(csvReader, newDevice, c => c.FCntResetCounter, (int?)0);
TryReadProperty(csvReader, newDevice, c => c.Supports32BitFCnt, true);

// Connection & Decoder
TryReadProperty(csvReader, newDevice, c => c.KeepAliveTimeout, (int?)null);
TryReadProperty(csvReader, newDevice, c => c.SensorDecoder, string.Empty);
```

**Key points**:
- Use existing `TryReadProperty()` helper - it handles missing columns gracefully
- Default values match `LoRaDeviceBase.cs` `[DefaultValue]` attributes
- `(int?)null` vs `(int?)0`: nullable fields use null for "not set", others use 0

### Step 3: Update Unit Tests

**File**: `ExportManagerTests.cs`

Add test for complete property import:
```csharp
[Fact]
public async Task ImportLoRaDevice_WithAllProperties_ShouldPersistAllFields()
{
    // Arrange
    var csvContent = @"Id,Name,ModelId,TAG:supportLoRaFeatures,PROPERTY:AppKey,PROPERTY:AppEUI,PROPERTY:ClassType,PROPERTY:PreferredWindow,PROPERTY:Deduplication,PROPERTY:Downlink,PROPERTY:RX1DROffset,PROPERTY:RX2DataRate,PROPERTY:RXDelay,PROPERTY:ABPRelaxMode,PROPERTY:FCntUpStart,PROPERTY:FCntDownStart,PROPERTY:FCntResetCounter,PROPERTY:Supports32BitFCnt,PROPERTY:KeepAliveTimeout,PROPERTY:SensorDecoder,PROPERTY:GatewayID
ABCD1234ABCD1234,TestDevice,test-model-id,true,TESTAPPKEY123456,TESTEUI123456789,C,2,Drop,false,3,5,1,false,100,200,0,false,300,http://decoder.example.com,gateway-1";

    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
    
    // Mock service to capture the device passed
    LoRaDeviceDetails capturedDevice = null;
    this.mockLoRaDeviceService
        .Setup(x => x.CreateDevice(It.IsAny<LoRaDeviceDetails>()))
        .Callback<LoRaDeviceDetails>(d => capturedDevice = d)
        .ReturnsAsync(new LoRaDeviceDetails());
    
    this.mockLoRaDeviceService
        .Setup(x => x.CheckIfDeviceExists(It.IsAny<string>()))
        .ReturnsAsync(false);

    // Act
    var result = await this.exportManager.ImportDeviceList(stream);

    // Assert
    Assert.Empty(result); // No errors
    Assert.NotNull(capturedDevice);
    Assert.Equal("ABCD1234ABCD1234", capturedDevice.DeviceID);
    Assert.Equal(ClassType.C, capturedDevice.ClassType);
    Assert.Equal(2, capturedDevice.PreferredWindow);
    Assert.Equal(DeduplicationMode.Drop, capturedDevice.Deduplication);
    Assert.False(capturedDevice.Downlink);
    Assert.Equal(3, capturedDevice.RX1DROffset);
    Assert.Equal(5, capturedDevice.RX2DataRate);
    Assert.Equal(1, capturedDevice.RXDelay);
    Assert.False(capturedDevice.ABPRelaxMode);
    Assert.Equal(100, capturedDevice.FCntUpStart);
    Assert.Equal(200, capturedDevice.FCntDownStart);
    Assert.Equal(0, capturedDevice.FCntResetCounter);
    Assert.False(capturedDevice.Supports32BitFCnt);
    Assert.Equal(300, capturedDevice.KeepAliveTimeout);
    Assert.Equal("http://decoder.example.com", capturedDevice.SensorDecoder);
    Assert.Equal("gateway-1", capturedDevice.GatewayID);
}
```

Add test for backward compatibility (old CSV without new columns):
```csharp
[Fact]
public async Task ImportLoRaDevice_WithMinimalProperties_ShouldUseDefaults()
{
    // Arrange
    var csvContent = @"Id,Name,ModelId,TAG:supportLoRaFeatures,PROPERTY:AppKey,PROPERTY:AppEUI
ABCD1234ABCD1234,TestDevice,test-model-id,true,TESTAPPKEY123456,TESTEUI123456789";

    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
    
    LoRaDeviceDetails capturedDevice = null;
    this.mockLoRaDeviceService
        .Setup(x => x.CreateDevice(It.IsAny<LoRaDeviceDetails>()))
        .Callback<LoRaDeviceDetails>(d => capturedDevice = d)
        .ReturnsAsync(new LoRaDeviceDetails());
    
    this.mockLoRaDeviceService
        .Setup(x => x.CheckIfDeviceExists(It.IsAny<string>()))
        .ReturnsAsync(false);

    // Act
    var result = await this.exportManager.ImportDeviceList(stream);

    // Assert
    Assert.Empty(result);
    Assert.NotNull(capturedDevice);
    
    // Verify defaults are applied
    Assert.Equal(ClassType.A, capturedDevice.ClassType);
    Assert.Equal(1, capturedDevice.PreferredWindow);
    Assert.Equal(DeduplicationMode.None, capturedDevice.Deduplication);
    Assert.True(capturedDevice.Downlink);
    Assert.True(capturedDevice.ABPRelaxMode);
}
```

### Step 4: Verify Data Flow

**Check Azure IoT Hub Twin** after import (manually or via integration test):

```json
{
  "deviceId": "ABCD1234ABCD1234",
  "properties": {
    "desired": {
      "AppKey": "TESTAPPKEY123456",
      "AppEUI": "TESTEUI123456789",
      "ClassType": "C",
      "PreferredWindow": 2,
      "Deduplication": "Drop",
      "Downlink": false,
      "RX1DROffset": 3,
      "RX2DataRate": 5,
      "RXDelay": 1,
      "ABPRelaxMode": false,
      "FCntUpStart": 100,
      "FCntDownStart": 200,
      "FCntResetCounter": 0,
      "Supports32BitFCnt": false,
      "KeepAliveTimeout": 300,
      "SensorDecoder": "http://decoder.example.com",
      "GatewayID": "gateway-1"
    }
  }
}
```

### Step 5: Test Synchronization

After import, trigger sync job (or wait for scheduled run) and verify:
1. Database still contains all imported values
2. No data loss occurs

```sql
-- Check LorawanDevice table
SELECT 
    Id, Name, ClassType, PreferredWindow, Deduplication, 
    Downlink, RX1DROffset, RX2DataRate, ABPRelaxMode
FROM LorawanDevice 
WHERE Id = 'ABCD1234ABCD1234';
```

Expected: All values match imported CSV, not defaults.

## Testing Checklist

- [ ] Unit test: Import with all LoRaWAN properties
- [ ] Unit test: Import with minimal properties (backward compatibility)
- [ ] Unit test: Import with OTAA authentication
- [ ] Unit test: Import with ABP authentication
- [ ] Unit test: Update existing device preserves new properties
- [ ] Manual test: Import CSV → Check IoT Hub Twin has all desired properties
- [ ] Manual test: Run sync job → Check database retains all properties
- [ ] Manual test: Export → Import → Export should be identical

## Common Issues & Solutions

### Issue: Enum parsing fails
**Symptom**: "Unable to cast object of type 'System.String' to type 'ClassType'"  
**Solution**: `TryReadProperty` expects typed value. Use explicit parsing:
```csharp
// If TryReadProperty doesn't handle enums automatically:
if (csvReader.TryGetField($"PROPERTY:ClassType", out string classTypeStr))
{
    newDevice.ClassType = Enum.TryParse<ClassType>(classTypeStr, out var ct) ? ct : ClassType.A;
}
```

### Issue: CSV template doesn't have new columns
**Symptom**: Export template missing new property columns  
**Resolution**: `GetPropertiesToExport()` already includes these (lines 96-103). If not, add:
```csharp
if (this.loRaWANOptions.Value.Enabled)
{
    properties.AddRange(new[] {
        nameof(LoRaDeviceDetails.ClassType),
        nameof(LoRaDeviceDetails.PreferredWindow),
        // ... etc
    });
}
```

### Issue: Null values in database after import
**Symptom**: Database has null where default expected  
**Solution**: Ensure default values in `TryReadProperty` calls match `LoRaDeviceBase` defaults

## Rollback Plan

If issues arise:
1. Revert `ExportManager.cs` changes
2. Redeploy previous version
3. Data already in IoT Hub is safe (no schema changes)
4. Devices imported with old version will continue to work (just lose config on sync)

## Next Steps

After implementation:
1. Create pull request with changes
2. Request code review
3. Merge to main branch
4. Deploy to staging environment
5. Test import/sync cycle in staging
6. Deploy to production
7. Monitor for sync job errors
8. Document in release notes

## Resources

- **Issue**: [#3251](https://github.com/CGI-FR/IoT-Hub-Portal/issues/3251)
- **Research**: `specs/027-analyze-issue-3251/research.md`
- **Data Model**: `specs/027-analyze-issue-3251/data-model.md`
- **API Contract**: `specs/027-analyze-issue-3251/contracts/device-import-api.md`
- **Implementation Plan**: `specs/027-analyze-issue-3251/plan.md`

## Support

For questions or issues during implementation:
- Tag: @kbeaugrand (issue assignee)
- Team: CGI-FR/IoT-Hub-Portal maintainers
- Milestone: v6.0
