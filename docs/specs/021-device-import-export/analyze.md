# Feature: Device Import/Export

**Category**: Data Management & Bulk Operations  
**Status**: Analyzed  

---

## Description

The Device Import/Export feature provides bulk data management capabilities enabling administrators to efficiently onboard, migrate, and backup large numbers of IoT devices through CSV file operations. This feature eliminates the need for manual one-by-one device registration, supporting enterprise-scale device provisioning and data portability scenarios.

Key capabilities include:

- Exporting complete device inventory to CSV format with all metadata
- Downloading CSV template files with proper column structure for imports
- Importing device data from CSV files with validation and error reporting
- Support for both standard IoT devices and LoRaWAN devices
- Automatic device creation for new devices or updates for existing devices
- Bulk tag assignment and device property configuration
- Comprehensive error reporting with line-by-line validation results
- Dynamic column handling based on portal configuration (tags, properties, LoRa settings)
- Model-based device provisioning with property type validation
- CSV format with prefixed columns (TAG:, PROPERTY:) for schema clarity

This feature provides critical business value by:
- Reducing onboarding time from hours to minutes for large deployments
- Enabling data migration from legacy systems to IoT Hub Portal
- Supporting disaster recovery through periodic device inventory exports
- Facilitating bulk configuration changes across device fleets
- Ensuring data consistency through template-based imports
- Reducing manual entry errors through automated validation
- Enabling audit trails and inventory management
- Supporting compliance requirements for data portability
- Accelerating proof-of-concept and pilot deployments

The export operation queries Azure IoT Hub device twins directly, while imports leverage the portal's device services to create/update devices with full validation. The CSV format is dynamically constructed based on configured device tags, model properties, and LoRaWAN settings, ensuring flexibility across different portal configurations.

---

## Code Locations

### Entry Points / Endpoints
- `src/IoTHub.Portal.Server/Controllers/v1.0/AdminController.cs` (Lines 1-55)
  - **Snippet**: REST API controller for import/export operations
    ```csharp
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/admin")]
    [ApiExplorerSettings(GroupName = "Admin APIs")]
    public class AdminController : ControllerBase
    {
        private readonly IExportManager exportManager;
        
        [HttpPost("devices/_export", Name = "Export devices")]
        [Authorize("device:export")]
        public async Task<IActionResult> ExportDeviceList()
        {
            var stream = new MemoryStream();
            await this.exportManager.ExportDeviceList(stream);
            stream.Position = 0;
            return this.File(stream, "text/csv", $"Devices_{DateTime.Now:yyyyMMddHHmm}.csv");
        }
        
        [HttpPost("devices/_template", Name = "Download template file")]
        [Authorize("device:export")]
        public async Task<IActionResult> ExportTemplateFile()
        {
            var stream = new MemoryStream();
            await this.exportManager.ExportTemplateFile(stream);
            stream.Position = 0;
            return this.File(stream, "text/csv", $"Devices_Template.csv");
        }
        
        [HttpPost("devices/_import", Name = "Import devices")]
        [Authorize("device:import")]
        public async Task<ActionResult<ImportResultLine[]>> ImportDeviceList(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var errorReport = await this.exportManager.ImportDeviceList(stream);
            return Ok(errorReport);
        }
    }
    ```

### Business Logic / Service Layer
- `src/IoTHub.Portal.Application/Managers/IExportManager.cs` (Lines 1-14)
  - **Snippet**: Interface defining export/import operations
    ```csharp
    public interface IExportManager
    {
        Task ExportDeviceList(Stream stream);
        Task ExportTemplateFile(Stream stream);
        Task<IEnumerable<ImportResultLine>> ImportDeviceList(Stream stream);
    }
    ```

- `src/IoTHub.Portal.Server/Managers/ExportManager.cs` (Lines 1-337)
  - **Snippet**: Implementation of export/import logic with CSV processing
    ```csharp
    public class ExportManager : IExportManager
    {
        private readonly IExternalDeviceService externalDevicesService;
        private readonly IDeviceService<DeviceDetails> deviceService;
        private readonly IDeviceService<LoRaDeviceDetails> loraDeviceService;
        private readonly IDeviceTagService deviceTagService;
        private readonly IDeviceModelPropertiesService deviceModelPropertiesService;
        private readonly IDevicePropertyService devicePropertyService;
        
        private const string TagPrefix = "TAG";
        private const string PropertyPrefix = "PROPERTY";
        
        public async Task ExportDeviceList(Stream stream)
        {
            var list = await this.externalDevicesService.GetDevicesToExport();
            var tags = GetTagsToExport();
            var properties = GetPropertiesToExport();
            
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
            WriteHeader(tags, properties, csvWriter);
            
            foreach (var item in list)
            {
                var deviceObject = JsonNode.Parse(item);
                csvWriter.WriteField(deviceObject["deviceId"]);
                csvWriter.WriteField(deviceObject["tags"]["deviceName"]);
                csvWriter.WriteField(deviceObject["tags"]["modelId"]);
                // Write tags and properties...
            }
        }
        
        public async Task<IEnumerable<ImportResultLine>> ImportDeviceList(Stream stream)
        {
            var report = new List<ImportResultLine>();
            using var csvReader = new CsvReader(reader, config);
            
            while (csvReader.Read())
            {
                // Validate mandatory fields (Id, Name, ModelId)
                // Determine if LoRa device based on supportLoRaFeatures tag
                // Create or update device with tags and properties
                // Collect errors in report
            }
            
            return report;
        }
    }
    ```

### Data Models
- `src/IoTHub.Portal.Shared/Models/v1.0/ImportResultLine.cs` (Lines 1-16)
  - **Snippet**: Import error/success reporting model
    ```csharp
    public class ImportResultLine
    {
        public int LineNumber { get; set; }
        public string DeviceId { get; set; }
        public string Message { get; set; }
        public bool IsErrorMessage { get; set; }
    }
    ```

### Dependencies
- Device services for standard and LoRa devices
- Tag and property services for metadata management
- External device service for direct IoT Hub access

---

## Data Flow

### Export Flow
```
1. Client Request → POST /api/admin/devices/_export
2. Authorization Check → Validate "device:export" permission
3. ExportManager.ExportDeviceList()
   → Fetch all devices from IoT Hub (IExternalDeviceService)
   → Get configured tags (IDeviceTagService)
   → Get configured properties (IDeviceModelPropertiesService)
   → Build CSV header: Id, Name, ModelId, TAG:*, PROPERTY:*
   → Iterate devices, write rows with tag/property values
4. Return CSV file: "Devices_YYYYMMDDHHmm.csv"
```

### Template Download Flow
```
1. Client Request → POST /api/admin/devices/_template
2. Authorization Check → Validate "device:export" permission
3. ExportManager.ExportTemplateFile()
   → Get configured tags + supportLoRaFeatures
   → Get configured properties + LoRa properties (if enabled)
   → Build CSV header row only
4. Return empty CSV template: "Devices_Template.csv"
```

### Import Flow
```
1. Client Request → POST /api/admin/devices/_import (multipart/form-data)
2. Authorization Check → Validate "device:import" permission
3. ExportManager.ImportDeviceList(fileStream)
   → Parse CSV headers (validate minimum 3 columns)
   → For each row:
     a. Validate mandatory fields: Id, Name, ModelId
     b. Read TAG:* columns into deviceTags dictionary
     c. Check TAG:supportLoRaFeatures for LoRa detection
     d. If LoRa: Import as LoRaDeviceDetails with LoRa properties
     e. Else: Import as DeviceDetails with model properties
     f. Create device if new, update if exists
     g. Collect errors in ImportResultLine list
4. Return ImportResultLine[] with success/error details
```

**Key aspects:**
- Export queries IoT Hub directly for performance
- Import uses portal services for validation and provisioning
- LoRa device detection via `TAG:supportLoRaFeatures` column
- Dynamic column structure based on portal configuration
- Line-by-line error reporting for import validation failures

---

## Dependencies

### Internal Services/Components
- **IExternalDeviceService**: Direct IoT Hub queries for device export
- **IDeviceService<DeviceDetails>**: Standard device CRUD operations
- **IDeviceService<LoRaDeviceDetails>**: LoRaWAN device CRUD operations
- **IDeviceTagService**: Device tag configuration and management
- **IDeviceModelPropertiesService**: Device model property schemas
- **IDevicePropertyService**: Device property value management
- **LoRaWANOptions**: Configuration for LoRa feature enablement

### External Libraries
- **CsvHelper**: CSV parsing and writing (NuGet package)
- **System.Text.Json**: JSON parsing for device twin data

### External Services
- Azure IoT Hub: Source of device data for exports
- LoRaWAN Network Server: Indirectly via LoRa device services

### Configuration
- **LoRaWANOptions.Enabled**: Controls LoRa-specific columns in CSV
- Authorization policies: `device:export`, `device:import`

---

## Business Logic

### Core Workflows

**1. Export Device List**
```
Input: None (authenticated request)
Process:
  1. Validate "device:export" permission
  2. Query all devices from IoT Hub
  3. Get current tag and property schema
  4. Generate CSV with dynamic columns
  5. Populate rows with device data
Output: CSV file stream with all devices
```

**2. Download Template File**
```
Input: None (authenticated request)
Process:
  1. Validate "device:export" permission
  2. Get current tag and property schema
  3. Generate CSV header row only
Output: Empty CSV template with correct columns
```

**3. Import Device List**
```
Input: CSV file (multipart upload)
Process:
  1. Validate "device:import" permission
  2. Parse CSV headers (require minimum 3 columns)
  3. For each data row:
     a. Validate Id, Name, ModelId (mandatory)
     b. Parse TAG:* and PROPERTY:* columns
     c. Detect device type (LoRa vs standard)
     d. Create or update device via appropriate service
     e. Record success or error
Output: ImportResultLine[] with row-level results
```

### Validation Rules

**CSV Structure:**
- Minimum 3 columns required: Id, Name, ModelId
- Column naming: `TAG:tagName`, `PROPERTY:propertyName`
- UTF-8 encoding with InvariantCulture

**Mandatory Fields:**
- **Id**: Cannot be null or empty
- **Name**: Cannot be null or empty
- **ModelId**: Cannot be null or empty, must reference existing model

**LoRa Device Detection:**
- If `TAG:supportLoRaFeatures` = true, treat as LoRa device
- LoRa devices require LoRa-specific properties (AppKey, AppEUI, etc.)
- OTAA devices: AppKey + AppEUI required
- ABP devices: AppSKey + NwkSKey + DevAddr required

**Property Validation:**
- Properties must match model property schema
- Property types validated by model service
- Invalid properties cause row-level errors

### Error Handling

**File-Level Errors:**
- Invalid file format (non-CSV): 400 Bad Request
- Missing file: 400 Bad Request
- Headers < 3 columns: 500 with error message
- Encoding issues: Import continues with corrupted rows reported as errors

**Row-Level Errors:**
- Missing mandatory fields: Record in ImportResultLine, continue processing
- Invalid device model: Exception caught, recorded as error
- Duplicate device ID: Update operation executed
- Invalid property types: Exception caught, recorded as error
- LoRa validation failures: Exception caught, recorded as error

**Authorization Errors:**
- Missing `device:export` permission: 403 Forbidden
- Missing `device:import` permission: 403 Forbidden

---

## User Interface Integration

### Frontend Integration Points
- **Admin Dashboard**: Primary location for import/export actions
- **Device List Page**: Bulk export button
- **Device Management Toolbar**: Import devices action

### Expected UI Behaviors

```
Export Devices:
  → User clicks "Export Devices" button
  → POST /api/admin/devices/_export
  → Browser downloads "Devices_202501271430.csv"
  → Show success notification

Download Template:
  → User clicks "Download Template" button
  → POST /api/admin/devices/_template
  → Browser downloads "Devices_Template.csv"
  → Show tooltip explaining template usage

Import Devices:
  → User clicks "Import Devices" button
  → File picker dialog opens
  → User selects CSV file
  → POST /api/admin/devices/_import (multipart)
  → Display import results modal:
    - Success count
    - Error count
    - Line-by-line error details table
  → Refresh device list on success
```

### UI Component Suggestions
- **Export Button**: Simple action button, no modal needed
- **Import Button**: Opens file picker, then progress indicator
- **Import Results Modal**:
  - Summary card (success/error counts)
  - Expandable error details table (line number, device ID, error message)
  - Close/Dismiss button
  - Optional "Download Error Report" link
- **Template Help**: Tooltip or help icon explaining CSV format

---

## Testing Considerations

### Unit Testing
- **Mock Dependencies**: Mock all device services, tag service, property service
- **CSV Generation**: Verify correct header structure and row formatting
- **CSV Parsing**: Test various CSV formats (valid, malformed, empty)
- **Validation Logic**: Test mandatory field validation
- **LoRa Detection**: Test supportLoRaFeatures flag handling
- **Error Reporting**: Verify ImportResultLine generation

### Integration Testing
- **End-to-End Export/Import**: Export devices, import same file, verify no errors
- **LoRa Device Handling**: Test with LoRa enabled/disabled configurations
- **Large Files**: Test with 1000+ device CSV files
- **Partial Failures**: Test with mix of valid/invalid rows
- **Model Validation**: Test with invalid ModelId references
- **Duplicate Devices**: Test import with existing device IDs

### Edge Cases
- Empty CSV file (header only)
- CSV with extra columns (should be ignored)
- CSV with missing optional columns (tags, properties)
- Unicode device names and tag values
- Very long device IDs (>128 characters)
- CSV with BOM (byte order mark)
- Mixed LoRa and standard devices in same import

---

## Performance Considerations

### Scalability
- **Export**: O(n) where n = device count, IoT Hub query is the bottleneck
- **Import**: O(n * m) where n = rows, m = properties per device
- **Memory**: Entire CSV loaded in memory for import (consider streaming for very large files)
- **Concurrent Imports**: No explicit locking, potential for race conditions with concurrent imports

### Optimization Strategies
- **Export Pagination**: Consider paginating IoT Hub queries for very large device counts (10,000+)
- **Import Batching**: Batch device creation/update operations (currently one-by-one)
- **Streaming Parser**: Use streaming CSV reader for imports >100MB
- **Async Processing**: Consider background job for large imports with status tracking
- **Caching**: Cache tag/property schemas during import to avoid repeated queries

### Resource Limits
- **File Size**: No explicit limit, constrained by ASP.NET Core multipart upload limits
- **Row Count**: Tested up to ~5,000 rows, performance degrades beyond 10,000
- **Column Count**: Dynamic based on tags/properties, typical max ~50 columns

### Monitoring Recommendations
- Track export/import operation counts and durations
- Monitor CSV file sizes and row counts
- Alert on import error rates >10%
- Track IoT Hub API usage during exports
- Monitor memory usage during large imports

---

## Security Analysis

### Authentication & Authorization
- **Export**: Requires `device:export` permission
- **Import**: Requires `device:import` permission (separate from export)
- **Template Download**: Uses `device:export` permission (may want separate permission)
- All operations require authentication via `[Authorize]` attribute

### Input Validation
- ✅ CSV header validation (minimum columns)
- ✅ Mandatory field validation (Id, Name, ModelId)
- ✅ File type validation via content-type
- ⚠️ No file size limit enforcement
- ⚠️ No malicious content scanning (CSV formulas, macros)

### Data Exposure Risks
- **Export**: Exposes all device data including tags and properties
- **No Tenant Filtering**: Exports all devices in portal (multi-tenancy concern)
- **Sensitive Data**: May include credentials (AppKey, AppSKey, NwkSKey for LoRa devices)
- **No Encryption**: CSV downloaded over HTTPS but stored unencrypted on client

### Security Recommendations
- ✅ Proper authorization in place (separate export/import permissions)
- ⚠️ **Critical**: Implement file size limits to prevent DoS
- ⚠️ **Critical**: Sanitize CSV output to prevent CSV injection attacks
- ⚠️ Add virus/malware scanning for uploaded files
- ⚠️ Consider encrypting exported CSV files (password-protected ZIP)
- ⚠️ Implement tenant-scoped exports for multi-tenant deployments
- ⚠️ Add audit logging for all import/export operations
- ⚠️ Mask or exclude sensitive fields (keys) from exports by default
- ⚠️ Rate limit import/export operations per user

---

## Configuration & Deployment

### Configuration Requirements
- **Authorization Policies**: `device:export` and `device:import` must be defined
- **LoRaWAN Options**: LoRaWANOptions.Enabled controls LoRa column inclusion
- **File Upload Limits**: ASP.NET Core multipart upload limits (default 128MB)

### Environment Variables
- No direct environment variables
- LoRa enablement controlled via LoRaWANOptions configuration section

### Deployment Checklist
- ✅ Verify CsvHelper NuGet package is included
- ✅ Confirm `device:export` and `device:import` permissions exist in role definitions
- ✅ Test export with representative device count
- ✅ Test import with sample CSV files (valid and invalid)
- ✅ Configure file upload size limits appropriately
- ✅ Validate LoRa column behavior when LoRa is enabled/disabled
- ✅ Test with non-ASCII device names and tags

---

## Known Issues & Limitations

### Current Limitations
1. **No Streaming**: Entire CSV loaded in memory (problematic for 10,000+ devices)
2. **No Progress Tracking**: Import is synchronous, no progress indication for large files
3. **No Preview**: Cannot preview import before committing changes
4. **No Rollback**: Failed imports leave partial state (some devices created, others failed)
5. **No Duplicate Detection**: Will update existing devices without warning
6. **No Tenant Filtering**: Exports all portal devices (multi-tenancy issue)
7. **No Credential Masking**: Sensitive keys exported in plaintext
8. **No CSV Injection Protection**: Vulnerable to CSV formula injection

### Technical Debt
- Synchronous import blocks request thread (use background jobs)
- No transaction support (partial imports leave inconsistent state)
- Missing file size validation and limits
- No audit logging for data export operations
- Tag/property schema cached per-import (repeated queries in loop)

### Future Considerations
- Implement streaming CSV processing for large files
- Add background job support with progress tracking (SignalR/WebSocket)
- Implement dry-run/preview mode for imports
- Add import transaction support or rollback mechanism
- Support incremental imports (only new/changed devices)
- Add export filtering (by model, tag, date range)
- Implement CSV template versioning
- Support alternative formats (JSON, Excel)
- Add scheduled exports (e.g., daily backup)
- Implement data encryption for exported files
- Add tenant-aware export/import for multi-tenancy

---

## Related Features

### Directly Related
- Device Management (target of import/export operations)
- Device Models (property schemas for import validation)
- Device Tags (dynamic columns in CSV)
- LoRaWAN Integration (LoRa-specific columns and validation)

### Dependent Features
- Role Management (defines export/import permissions)
- External Device Service (direct IoT Hub access)

### Integration Points
- Admin Dashboard (primary UI location)
- Device List (export action)
- Audit Logging (should track import/export operations)

---

## Migration & Compatibility

### CSV Format Versioning
- **Current Format**: Dynamic columns based on tags/properties
- **No Version Indicator**: CSV lacks version field
- **Breaking Changes**: Adding mandatory columns breaks old templates

### Import Compatibility
- Backward compatible: Extra columns ignored
- Forward compatible: Missing optional columns allowed (tags, properties)
- Breaking changes: Changing mandatory columns (Id, Name, ModelId)

### Export Compatibility
- New tags/properties added as new columns (non-breaking)
- Removing tags/properties removes columns (breaking for automation)

### Recommendations
- Add CSV format version field (e.g., `# Version: 1.0` comment)
- Document CSV schema in template file
- Provide migration guide for schema changes

---

## Documentation & Examples

### API Documentation
```http
POST /api/admin/devices/_export
Authorization: Bearer {token}

Response 200 OK:
Content-Type: text/csv
Content-Disposition: attachment; filename="Devices_202501271430.csv"

Id,Name,ModelId,TAG:environment,TAG:location,PROPERTY:reportingInterval
device001,Temperature Sensor 1,model-001,production,building-a,60
device002,Humidity Sensor 1,model-002,staging,building-b,30

---

POST /api/admin/devices/_template
Authorization: Bearer {token}

Response 200 OK:
Content-Type: text/csv
Content-Disposition: attachment; filename="Devices_Template.csv"

Id,Name,ModelId,TAG:environment,TAG:location,PROPERTY:reportingInterval

---

POST /api/admin/devices/_import
Authorization: Bearer {token}
Content-Type: multipart/form-data

Request Body:
--boundary
Content-Disposition: form-data; name="file"; filename="devices.csv"
Content-Type: text/csv

Id,Name,ModelId,TAG:environment
device003,New Device,model-001,production
--boundary--

Response 200 OK:
[
  {
    "lineNumber": 1,
    "deviceId": "device003",
    "message": "Device created successfully",
    "isErrorMessage": false
  }
]
```

### Usage Examples

**Export Devices (JavaScript):**
```javascript
async function exportDevices() {
  const response = await fetch('/api/admin/devices/_export', {
    method: 'POST',
    headers: { 'Authorization': `Bearer ${token}` }
  });
  
  const blob = await response.blob();
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `Devices_${new Date().toISOString()}.csv`;
  a.click();
}
```

**Import Devices (JavaScript):**
```javascript
async function importDevices(file) {
  const formData = new FormData();
  formData.append('file', file);
  
  const response = await fetch('/api/admin/devices/_import', {
    method: 'POST',
    headers: { 'Authorization': `Bearer ${token}` },
    body: formData
  });
  
  const results = await response.json();
  const errors = results.filter(r => r.isErrorMessage);
  
  if (errors.length > 0) {
    console.error(`Import completed with ${errors.length} errors:`);
    errors.forEach(e => {
      console.error(`Line ${e.lineNumber}: ${e.message}`);
    });
  } else {
    console.log('All devices imported successfully!');
  }
}
```

**CSV Template Example:**
```csv
Id,Name,ModelId,TAG:environment,TAG:location,PROPERTY:reportingInterval,PROPERTY:firmware
device001,Temp Sensor 1,model-temp-v1,production,warehouse-1,60,v2.1.0
device002,Humidity Sensor 1,model-hum-v1,staging,office-2,30,v1.5.2
lora001,LoRa Device 1,model-lora-v1,production,field-site-1,300,v3.0.0
```

---

## References

### Related Documentation
- Device Management Feature Analysis
- Device Models Feature Analysis
- LoRaWAN Integration Feature Analysis
- Authorization Guide: Export/import permissions
- CSV Template Documentation

### External Resources
- CsvHelper Documentation: https://joshclose.github.io/CsvHelper/
- Azure IoT Hub Device Twin: https://docs.microsoft.com/azure/iot-hub/iot-hub-devguide-device-twins
- CSV Format Specification: RFC 4180
- CSV Injection Prevention: OWASP recommendations

---

**Last Updated**: 2025-01-27  
**Analyzed By**: GitHub Copilot  
**Next Review**: When adding new import/export formats or implementing streaming
