# API Contract: Device Import Endpoint

**Endpoint**: `POST /api/admin/devices/_import`  
**Purpose**: Import devices from CSV file  
**Status**: No changes - this documents existing behavior with bug fix applied

## Request

### HTTP Method
`POST`

### Headers
```
Content-Type: multipart/form-data
Authorization: Bearer {jwt_token}
```

### Body
Form data with single file upload:
- **Field name**: `file`
- **File type**: CSV (text/csv)
- **Max size**: (configured by server)

### CSV Format

**Mandatory columns**:
- `Id`: Device identifier (16 hex characters, uppercase)
- `Name`: Device name (free text)
- `ModelId`: Device model GUID

**Optional tag columns** (format: `TAG:{tagName}`):
- `TAG:supportLoRaFeatures`: "true" for LoRaWAN devices, empty/"false" for standard
- `TAG:{anyCustomTag}`: Custom device tags from device tag configuration

**Optional LoRaWAN property columns** (format: `PROPERTY:{propertyName}`):

Authentication (OTAA):
- `PROPERTY:AppKey`: Application Key (hex string)
- `PROPERTY:AppEUI`: Application EUI (hex string)

Authentication (ABP):
- `PROPERTY:AppSKey`: Application Session Key (hex string)
- `PROPERTY:NwkSKey`: Network Session Key (hex string)
- `PROPERTY:DevAddr`: Device Address (hex string)

Connection:
- `PROPERTY:GatewayID`: Preferred gateway identifier

Configuration:
- `PROPERTY:ClassType`: LoRaWAN class ("A", "B", or "C")
- `PROPERTY:PreferredWindow`: Preferred receive window (1 or 2)
- `PROPERTY:Deduplication`: Deduplication mode ("None", "Drop", or "Mark")
- `PROPERTY:Downlink`: Enable downlinks ("true" or "false")

Advanced OTAA Settings:
- `PROPERTY:RX1DROffset`: RX1 data rate offset (integer 0-7)
- `PROPERTY:RX2DataRate`: RX2 data rate (integer 0-15)
- `PROPERTY:RXDelay`: RX delay in seconds (integer 0-15)

Advanced ABP Settings:
- `PROPERTY:ABPRelaxMode`: Enable ABP relaxed mode ("true" or "false")
- `PROPERTY:FCntUpStart`: Uplink frame counter start (integer)
- `PROPERTY:FCntDownStart`: Downlink frame counter start (integer)
- `PROPERTY:FCntResetCounter`: Frame counter reset value (integer)
- `PROPERTY:Supports32BitFCnt`: Support 32-bit frame counters ("true" or "false")

Other:
- `PROPERTY:KeepAliveTimeout`: Keep-alive timeout in seconds (integer)
- `PROPERTY:SensorDecoder`: Sensor decoder API URL (string)

### Example CSV Content
```csv
Id,Name,ModelId,TAG:supportLoRaFeatures,TAG:location,PROPERTY:AppKey,PROPERTY:AppEUI,PROPERTY:ClassType,PROPERTY:PreferredWindow,PROPERTY:Downlink
ABCD1234ABCD1234,Sensor-01,550e8400-e29b-41d4-a716-446655440000,true,Building-A,FEDCBA9876543210FEDCBA9876543210,0102030405060708,A,1,true
EFGH5678EFGH5678,Sensor-02,550e8400-e29b-41d4-a716-446655440000,true,Building-B,0123456789ABCDEF0123456789ABCDEF,0807060504030201,C,2,false
```

## Response

### Success Response (200 OK)
```json
{
  "message": "Import completed",
  "totalProcessed": 2,
  "errors": []
}
```

Or with partial success:
```json
{
  "message": "Import completed with errors",
  "totalProcessed": 5,
  "errors": [
    {
      "lineNumber": 3,
      "deviceId": "BADDEVICE123456",
      "isErrorMessage": true,
      "message": "The parameter Id cannot be null or empty"
    },
    {
      "lineNumber": 7,
      "deviceId": "1234567890ABCDEF",
      "isErrorMessage": true,
      "message": "The device identifier must contain 16 hexadecimal characters"
    }
  ]
}
```

**Response Model**:
```csharp
public class ImportResultLine
{
    public string DeviceId { get; set; }
    public int LineNumber { get; set; }
    public bool IsErrorMessage { get; set; }
    public string Message { get; set; }
}
```

### Error Responses

**400 Bad Request** - Invalid file format
```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid file format: The submitted file should be a comma-separated values (CSV) file. A template file showing the mandatory fields is available to download on the portal."
}
```

**401 Unauthorized** - Missing or invalid authentication
```json
{
  "title": "Unauthorized",
  "status": 401
}
```

**403 Forbidden** - User lacks required permission
```json
{
  "title": "Forbidden",
  "status": 403,
  "detail": "User does not have permission to import devices"
}
```

## Validation Rules

### Device ID Validation
- **Required**: Yes
- **Format**: Exactly 16 hexadecimal characters (0-9, A-F)
- **Regex**: `^[A-Z0-9]{16}$`
- **Error**: "The parameter Id cannot be null or empty" or "The device identifier must contain 16 hexadecimal characters"

### Device Name Validation
- **Required**: Yes
- **Error**: "The parameter Name cannot be null or empty"

### Model ID Validation
- **Required**: Yes
- **Must exist**: Must reference an existing device model in the database
- **Error**: "The parameter ModelId cannot be null or empty" or "Device model not found"

### LoRaWAN Authentication Validation
For LoRaWAN devices (`TAG:supportLoRaFeatures = true`):
- **OTAA mode**: Requires `PROPERTY:AppKey` AND `PROPERTY:AppEUI`
- **ABP mode**: Requires `PROPERTY:AppSKey` AND `PROPERTY:NwkSKey` AND `PROPERTY:DevAddr`
- **Detection**: If AppKey and AppEUI are provided → OTAA; if empty → ABP mode expected

### Property Type Validation
- **ClassType**: Must be "A", "B", or "C" (case-sensitive)
- **Deduplication**: Must be "None", "Drop", or "Mark"
- **Boolean fields**: "true"/"false" (case-insensitive)
- **Integer fields**: Valid integer within range (e.g., RX1DROffset: 0-7)

## Behavior Changes (Bug Fix)

### Before Fix
- Only authentication properties were persisted to Azure IoT Hub
- Configuration properties (ClassType, PreferredWindow, Deduplication, etc.) were stored in local database only
- Synchronization job would overwrite local database with incomplete data from IoT Hub
- **Result**: Imported configuration properties were lost after sync

### After Fix
- **All LoRaWAN properties** are persisted to Azure IoT Hub as desired properties
- Local database remains in sync with IoT Hub
- Synchronization job refreshes local database from IoT Hub without data loss
- **Result**: Imported configuration properties are preserved

## Security Considerations

### Authorization
- Requires authenticated user with `AdminRole` permission
- Policy: `Policies.AdminAccess` (defined in `IoTHub.Portal.Shared.Security.Policies`)

### Data Validation
- All CSV input is validated before processing
- Invalid rows are logged but do not stop processing of valid rows
- Error messages do not expose sensitive system information

### Rate Limiting
- (Not currently implemented - consider for production)

## Related Endpoints

### Export Devices
`GET /api/admin/devices/_export` - Export all devices to CSV

### Download Template
`GET /api/admin/devices/_template` - Download CSV template with headers

## Testing Contract

### Happy Path Test Cases
1. **New LoRaWAN device (OTAA)**: All required fields + all optional LoRaWAN fields
2. **New LoRaWAN device (ABP)**: ABP auth fields + configuration fields
3. **Update existing device**: Same device ID with new configuration
4. **Mixed standard and LoRaWAN**: CSV with both device types

### Error Test Cases
1. **Missing required field**: Id, Name, or ModelId empty
2. **Invalid device ID format**: Less than 16 chars, invalid characters
3. **Invalid enum value**: ClassType = "D", Deduplication = "Invalid"
4. **Incomplete authentication**: AppKey without AppEUI
5. **Invalid file format**: Not a CSV, missing headers

### Edge Cases
1. **Empty CSV**: Only headers, no data rows
2. **CSV with only old columns**: No new LoRaWAN configuration columns (should use defaults)
3. **CSV with extra unknown columns**: Should be ignored
4. **Duplicate device IDs in CSV**: Last entry wins (update behavior)

## Notes

- The import is idempotent: importing the same device twice updates it
- Existing devices are updated, not replaced (version tracking maintained)
- Tags and labels are completely replaced on update (delete + recreate)
- Device Twin version in IoT Hub is incremented on each update
- Import is transactional per-device (one device failure doesn't rollback others)

## Change Log

**2026-01-30**: Bug fix - Added persistence of all LoRaWAN configuration properties to IoT Hub during import
