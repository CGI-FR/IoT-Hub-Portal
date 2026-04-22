# Data Model: Device Import Fix

**Date**: 2026-01-30  
**Feature**: Fix LoRaWAN device import data overwrite

## Overview

This is a bug fix, not a new feature. No new entities or schema changes are required. This document describes the existing data model and clarifies the relationships involved in the import/sync flow.

## Entities

### LorawanDevice (Domain Entity)
**File**: `src/IoTHub.Portal.Domain/Entities/LorawanDevice.cs`  
**Purpose**: Database entity representing a LoRaWAN device  
**Inherits**: `Device` (base entity with Id, Name, ModelId, Tags, etc.)

**LoRaWAN-Specific Fields**:

| Field | Type | Description | Source |
|-------|------|-------------|--------|
| UseOTAA | bool | OTAA vs ABP authentication | CSV / IoT Hub |
| AppKey | string? | OTAA Application Key | CSV / IoT Hub desired |
| AppEUI | string? | OTAA Application EUI | CSV / IoT Hub desired |
| AppSKey | string? | ABP Application Session Key | CSV / IoT Hub desired |
| NwkSKey | string? | ABP Network Session Key | CSV / IoT Hub desired |
| DevAddr | string? | ABP Device Address | CSV / IoT Hub desired |
| AlreadyLoggedInOnce | bool | Has device ever connected | IoT Hub reported |
| DataRate | string? | Current data rate (ADR) | IoT Hub reported |
| TxPower | string? | Current transmit power (ADR) | IoT Hub reported |
| NbRep | string? | Current repetition count (ADR) | IoT Hub reported |
| ReportedRX2DataRate | string? | Reported RX2 window data rate | IoT Hub reported |
| ReportedRX1DROffset | string? | Reported RX1 DR offset | IoT Hub reported |
| ReportedRXDelay | string? | Reported RX delay | IoT Hub reported |
| GatewayID | string? | Preferred gateway ID | CSV / IoT Hub (both) |
| Downlink | bool? | Downlinks enabled (default: true) | **MISSING FROM IMPORT** |
| ClassType | ClassType | LoRaWAN class (A/B/C) | **MISSING FROM IMPORT** |
| PreferredWindow | int | Preferred RX window (1 or 2) | **MISSING FROM IMPORT** |
| Deduplication | DeduplicationMode | Duplicate message handling | **MISSING FROM IMPORT** |
| RX1DROffset | int? | Desired RX1 DR offset | **MISSING FROM IMPORT** |
| RX2DataRate | int? | Desired RX2 data rate | **MISSING FROM IMPORT** |
| RXDelay | int? | Desired RX delay | **MISSING FROM IMPORT** |
| ABPRelaxMode | bool? | ABP relaxed frame counter (default: true) | **MISSING FROM IMPORT** |
| FCntUpStart | int? | Uplink frame counter start | **MISSING FROM IMPORT** |
| FCntDownStart | int? | Downlink frame counter start | **MISSING FROM IMPORT** |
| FCntResetCounter | int? | Frame counter reset value | **MISSING FROM IMPORT** |
| Supports32BitFCnt | bool? | 32-bit frame counters (default: true) | **MISSING FROM IMPORT** |
| KeepAliveTimeout | int? | Connection timeout (seconds) | **MISSING FROM IMPORT** |
| SensorDecoder | string? | Decoder API URL | **MISSING FROM IMPORT** |

**Relationships**:
- `Device.DeviceModelId` → `DeviceModel.Id` (many-to-one)
- `Device.Tags` → `DeviceTagValue[]` (one-to-many)
- `Device.Labels` → `Label[]` (one-to-many)
- `Telemetry` → `LoRaDeviceTelemetry[]` (one-to-many)

### LoRaDeviceDetails (DTO)
**File**: `src/IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/LoRaDeviceDetails.cs`  
**Purpose**: Data Transfer Object for API and import/export  
**Inherits**: `LoRaDeviceBase` (contains all LoRaWAN-specific properties)

**Additional DTO Fields** (not in entity):
- `DeviceName` - mapped from `Device.Name`
- `ModelId` - mapped from `Device.DeviceModelId`
- `ModelName` - lookup from `DeviceModel.Name`
- `Image` - device model image URL
- `IsConnected` - from IoT Hub Twin connection state
- `IsEnabled` - from IoT Hub Twin status
- `StatusUpdatedTime` - from IoT Hub Twin metadata
- `LastActivityTime` - from IoT Hub Twin metadata
- `Tags` - dictionary of tag name/value pairs

## Value Objects

### ClassType (Enum)
**File**: `src/IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/ClassType.cs`  
**Values**: `A` (default), `B`, `C`  
**Purpose**: LoRaWAN device class determining downlink behavior

### DeduplicationMode (Enum)
**File**: `src/IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/DeduplicationMode.cs`  
**Values**: `None`, `Drop`, `Mark`  
**Purpose**: How to handle duplicate messages from multiple gateways

## Data Flow

### Import Flow (Current - Buggy)
```
CSV File
  ↓ (CsvHelper reads)
LoRaDeviceDetails DTO (partially populated)
  ↓ (LoRaWanDeviceService.CreateDevice/UpdateDevice)
[IoT Hub Twin]  ← Only auth fields written
  ↓ (also)
[Database: LorawanDevice] ← All fields written
  ↓ (later: SyncDevicesJob runs)
[IoT Hub Twin] → Read by sync job
  ↓ (AutoMapper: Twin → LorawanDevice)
[Database: LorawanDevice] ← **OVERWRITTEN** with incomplete data
```

### Import Flow (After Fix)
```
CSV File
  ↓ (CsvHelper reads ALL LoRaWAN properties)
LoRaDeviceDetails DTO (fully populated)
  ↓ (LoRaWanDeviceService.CreateDevice/UpdateDevice)
[IoT Hub Twin] ← ALL LoRaWAN fields written to desired properties
  ↓ (also)
[Database: LorawanDevice] ← All fields written
  ↓ (later: SyncDevicesJob runs)
[IoT Hub Twin] → Read by sync job
  ↓ (AutoMapper: Twin → LorawanDevice)
[Database: LorawanDevice] ← Refreshed with COMPLETE data (no loss)
```

## Validation Rules

### Existing Rules (No Changes)
- `DeviceID`: Required, max 128 chars, must match regex `^[A-Z0-9]{16}$` (16 hex chars)
- `DeviceName`: Required
- `ModelId`: Required, must reference existing DeviceModel
- Authentication mode: Either (AppKey + AppEUI) OR (AppSKey + NwkSKey + DevAddr)

### CSV Import Rules
- `Id`, `Name`, `ModelId` columns are mandatory
- `TAG:supportLoRaFeatures` must be "true" for LoRaWAN device import
- Authentication fields: Must provide OTAA (AppKey+AppEUI) OR ABP (AppSKey+NwkSKey+DevAddr)
- All other LoRaWAN fields are optional (use defaults if missing)

## Field Mappings

### CSV Column to DTO Property
| CSV Column | DTO Property | Type | Default |
|------------|--------------|------|---------|
| Id | DeviceID | string | (required) |
| Name | DeviceName | string | (required) |
| ModelId | ModelId | string | (required) |
| TAG:supportLoRaFeatures | (special) | bool | false |
| PROPERTY:AppKey | AppKey | string | null |
| PROPERTY:AppEUI | AppEUI | string | null |
| PROPERTY:AppSKey | AppSKey | string | null |
| PROPERTY:NwkSKey | NwkSKey | string | null |
| PROPERTY:DevAddr | DevAddr | string | null |
| PROPERTY:GatewayID | GatewayID | string | null |
| PROPERTY:ClassType | ClassType | ClassType | A |
| PROPERTY:PreferredWindow | PreferredWindow | int | 1 |
| PROPERTY:Deduplication | Deduplication | DeduplicationMode | None |
| PROPERTY:Downlink | Downlink | bool? | true |
| PROPERTY:RX1DROffset | RX1DROffset | int? | null (or 0) |
| PROPERTY:RX2DataRate | RX2DataRate | int? | null (or 0) |
| PROPERTY:RXDelay | RXDelay | int? | null |
| PROPERTY:ABPRelaxMode | ABPRelaxMode | bool? | true |
| PROPERTY:FCntUpStart | FCntUpStart | int? | null (or 0) |
| PROPERTY:FCntDownStart | FCntDownStart | int? | null (or 0) |
| PROPERTY:FCntResetCounter | FCntResetCounter | int? | null (or 0) |
| PROPERTY:Supports32BitFCnt | Supports32BitFCnt | bool? | true |
| PROPERTY:KeepAliveTimeout | KeepAliveTimeout | int? | null |
| PROPERTY:SensorDecoder | SensorDecoder | string | null |

### DTO to IoT Hub Twin (Desired Properties)
All LoRaWAN properties from DTO are written as desired properties with the same name:
- `AppKey`, `AppEUI`, `AppSKey`, `NwkSKey`, `DevAddr`
- `ClassType`, `PreferredWindow`, `Deduplication`, `Downlink`
- `RX1DROffset`, `RX2DataRate`, `RXDelay`
- `ABPRelaxMode`, `FCntUpStart`, `FCntDownStart`, `FCntResetCounter`
- `Supports32BitFCnt`, `KeepAliveTimeout`, `SensorDecoder`
- `GatewayID` (special: can be desired OR reported)

### IoT Hub Twin to Entity (Sync Mapping)
AutoMapper (`DeviceProfile.cs` lines 62-98) already correctly maps:
- Desired properties → Entity properties (via `DeviceHelper.RetrieveDesiredPropertyValue()`)
- Reported properties → Entity reported fields (DataRate, TxPower, etc.)

**No changes needed to AutoMapper** - it already handles all properties. The fix is ensuring import populates the DTO before it's saved.

## State Transitions

### Device Import States
1. **New device** (`CheckIfDeviceExists()` = false):
   - `CreateDevice()` called
   - New entity inserted into database
   - Twin created in IoT Hub with desired properties

2. **Existing device** (`CheckIfDeviceExists()` = true):
   - `UpdateDevice()` called
   - Entity updated in database (tags/labels deleted and recreated)
   - Twin updated in IoT Hub with new desired properties

3. **Post-sync** (happens periodically via Quartz job):
   - Sync job reads all Twins from IoT Hub
   - For each Twin: maps to entity and updates database
   - After fix: desired properties now contain all imported data, so no data loss

## Database Schema

**No schema changes required.** All fields already exist in the `LorawanDevice` entity and corresponding database table. The bug is in the data flow, not the schema.

## Backward Compatibility

### CSV Format Compatibility
- **Existing CSV files** (without new columns): Will import successfully with default values
- **New CSV files** (with all columns): Will import successfully with explicit values
- **Export format**: Already includes all LoRaWAN properties (no change needed)

### API Compatibility
- No API contract changes
- `POST /api/admin/devices/_import` endpoint unchanged
- Response format unchanged

### Data Migration
- No database migration required
- No data backfill required
- Fix is forward-compatible only (previously imported devices won't auto-heal, but new imports will work correctly)

## Summary

This bug fix involves NO new data structures - only ensuring existing data flows completely through the import pipeline to Azure IoT Hub, so the sync job can correctly retrieve and maintain all fields. The data model already supports all required properties; the import logic just wasn't populating them.
