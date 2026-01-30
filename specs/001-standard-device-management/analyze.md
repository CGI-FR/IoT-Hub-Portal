# Feature: Standard Device Management

**Category**: Device Management  
**Status**: Analyzed  

---

## Description

The Standard Device Management feature provides comprehensive CRUD (Create, Read, Update, Delete) operations for IoT devices in the portal. It enables users to manage standard (non-LoRaWAN, non-Edge) devices including their configuration, properties, tags, and labels. The feature includes:

- Paginated device listing with advanced search and filtering capabilities (by name, status, state, model, tags, and labels)
- Device creation and configuration with model-based templates
- Device editing with real-time property updates
- Device deletion with confirmation dialogs
- Device credentials retrieval for connection management
- Device property management (read and write device twin properties)
- Import/export functionality for bulk device management
- Label management for device categorization
- Integration with Azure IoT Hub or AWS IoT Core for device twin synchronization

This feature serves as the foundation for IoT device lifecycle management, providing business value through streamlined device onboarding, monitoring, and configuration capabilities.

---

## Code Locations

### Entry Points / Endpoints
- `src/IoTHub.Portal.Server/Controllers/v1.0/DevicesController.cs` (Lines 1-137)
  - **Snippet**: Main REST API controller inheriting from DevicesControllerBase
    ```csharp
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/devices")]
    [ApiExplorerSettings(GroupName = "IoT Devices")]
    public class DevicesController : DevicesControllerBase<DeviceDetails>
    {
        [HttpGet(Name = "GET Device list")]
        [Authorize("device:read")]
        public Task<PaginationResult<DeviceListItem>> SearchItems(...)
        
        [HttpGet("{deviceID}", Name = "GET Device details")]
        [Authorize("device:read")]
        public override Task<DeviceDetails> GetItem(string deviceID)
        
        [HttpPost(Name = "POST Create device")]
        [Authorize("device:write")]
        public override Task<IActionResult> CreateDeviceAsync(DeviceDetails device)
        
        [HttpPut(Name = "PUT Update device")]
        [Authorize("device:write")]
        public override Task<IActionResult> UpdateDeviceAsync(DeviceDetails device)
        
        [HttpDelete("{deviceID}", Name = "DELETE Remove device")]
        [Authorize("device:write")]
        public override Task<IActionResult> Delete(string deviceID)
        
        [HttpGet("{deviceID}/properties", Name = "GET Device Properties")]
        [Authorize("device:read")]
        public async Task<IEnumerable<DevicePropertyValue>> GetProperties(string deviceID)
        
        [HttpPost("{deviceID}/properties", Name = "POST Device Properties")]
        [Authorize("device:write")]
        public async Task<ActionResult<IEnumerable<DevicePropertyValue>>> SetProperties(...)
    }
    ```

- `src/IoTHub.Portal.Server/Controllers/v1.0/DevicesControllerBase.cs` (Lines 1-185)
  - Base controller implementation with shared logic for device operations

### Business Logic
- `src/IoTHub.Portal.Application/Services/IDeviceService.cs` (Lines 1-39)
  - **Snippet**: Core service interface
    ```csharp
    public interface IDeviceService<TDto> where TDto : IDeviceDetails
    {
        Task<PaginatedResult<DeviceListItem>> GetDevices(...);
        Task<TDto> GetDevice(string deviceId);
        Task<bool> CheckIfDeviceExists(string deviceId);
        Task<TDto> CreateDevice(TDto device);
        Task<TDto> UpdateDevice(TDto device);
        Task DeleteDevice(string deviceId);
        Task<DeviceCredentials> GetCredentials(TDto device);
        Task<IEnumerable<LoRaDeviceTelemetryDto>> GetDeviceTelemetry(string deviceId);
        Task ProcessTelemetryEvent(EventData eventMessage);
        Task<IEnumerable<LabelDto>> GetAvailableLabels();
    }
    ```

- `src/IoTHub.Portal.Infrastructure/Services/DeviceService.cs` (Lines 1-134)
  - Concrete implementation for standard devices
  - Handles database operations for device CRUD
  - Manages device tags and labels

- `src/IoTHub.Portal.Infrastructure/Services/DeviceServiceBase.cs` (Lines 1-220+)
  - Abstract base implementation with shared device management logic
  - Implements pagination, filtering, and external device service integration
  - Coordinates with device twin mapper for IoT Hub synchronization

- `src/IoTHub.Portal.Application/Services/IDevicePropertyService.cs` (Lines 1-12)
  - Service interface for device property management

- `src/IoTHub.Portal.Server/Services/DevicePropertyService.cs` (Lines 1-129)
  - Manages device twin properties (desired and reported)
  - Retrieves properties from device model templates

### Data Access
- `src/IoTHub.Portal.Domain/Repositories/IDeviceRepository.cs` (Lines 1-9)
  - Generic repository interface for Device entity

- `src/IoTHub.Portal.Domain/Entities/Device.cs` (Lines 1-63)
  - **Snippet**: Core device entity
    ```csharp
    public class Device : EntityBase
    {
        public string Name { get; set; }
        public string DeviceModelId { get; set; }
        public bool IsConnected { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime StatusUpdatedTime { get; set; }
        public DateTime LastActivityTime { get; set; }
        public int Version { get; set; }
        public DeviceModel DeviceModel { get; set; }
        public ICollection<Label> Labels { get; set; }
        public ICollection<DeviceTagValue> Tags { get; set; }
        public string? LayerId { get; set; }
    }
    ```

### UI Components
- `src/IoTHub.Portal.Client/Pages/Devices/DeviceListPage.razor` (Lines 1-480)
  - Main device listing page with search panel
  - Server-side paginated table with sorting
  - Implements search by ID, tags, labels, model, status
  - Import/export device list functionality
  - Conditional rendering based on permissions (device:read, device:write, device:import, device:export)

- `src/IoTHub.Portal.Client/Pages/Devices/DeviceDetailPage.razor` (Lines 1-41)
  - Device detail view wrapper
  - Permission checks for device:read, device:write, device:execute

- `src/IoTHub.Portal.Client/Pages/Devices/CreateDevicePage.razor` (Lines 1-13)
  - Device creation page
  - Requires device:write permission

- `src/IoTHub.Portal.Client/Pages/Devices/DeleteDevicePage.razor` (Lines 1-64)
  - Confirmation dialog for device deletion
  - Handles both standard and LoRaWAN devices

- `src/IoTHub.Portal.Client/Components/Devices/EditDevice.razor` (Lines 1-150+)
  - Comprehensive device editor component
  - Used for both create and edit modes
  - Includes device details, tags, properties, and labels
  - Model-based device configuration

### Data Transfer Objects
- `src/IoTHub.Portal.Shared/Models/v1.0/DeviceDetails.cs` (Lines 1-81)
  - Complete device details DTO with validation attributes
  - Properties: DeviceID, DeviceName, ModelId, Image, IsConnected, IsEnabled, StatusUpdatedTime, LastActivityTime, Tags, Labels, LayerId

- `src/IoTHub.Portal.Shared/Models/v1.0/DeviceListItem.cs` (Lines 1-71)
  - Lightweight DTO for device list display
  - Properties: DeviceID, DeviceName, DeviceModelId, Image, IsConnected, IsEnabled, SupportLoRaFeatures, HasLoRaTelemetry, StatusUpdatedTime, LastActivityTime, Labels, LayerId

- `src/IoTHub.Portal.Shared/Models/v1.0/IDeviceDetails.cs`
  - Interface defining common device detail properties

### Client Services
- `src/IoTHub.Portal.Client/Services/IDeviceClientService.cs` (Lines 1-32)
  - Client-side service interface for HTTP API calls
  - Methods: GetDevices, GetDevice, CreateDevice, UpdateDevice, GetDeviceProperties, SetDeviceProperties, GetEnrollmentCredentials, DeleteDevice, ExportDeviceList, ExportTemplateFile, ImportDeviceList, GetAvailableLabels

---

## API Endpoints

### Device Management
- `GET /api/devices` - Get paginated device list
  - Query parameters: searchText, searchStatus, searchState, pageSize, pageNumber, orderBy[], modelId, labels[], tag.*
  - Returns: PaginationResult<DeviceListItem>
  - Authorization: device:read

- `GET /api/devices/{deviceID}` - Get device details
  - Returns: DeviceDetails
  - Authorization: device:read

- `POST /api/devices` - Create new device
  - Body: DeviceDetails
  - Returns: DeviceDetails
  - Authorization: device:write
  - Creates device in both IoT Hub and database

- `PUT /api/devices` - Update device
  - Body: DeviceDetails
  - Returns: 200 OK
  - Authorization: device:write
  - Updates device in both IoT Hub and database

- `DELETE /api/devices/{deviceID}` - Delete device
  - Returns: 200 OK
  - Authorization: device:write
  - Removes device from both IoT Hub and database

### Device Credentials & Properties
- `GET /api/devices/{deviceID}/credentials` - Get device enrollment credentials
  - Returns: DeviceCredentials
  - Authorization: device:read

- `GET /api/devices/{deviceID}/properties` - Get device properties (twin)
  - Returns: IEnumerable<DevicePropertyValue>
  - Authorization: device:read

- `POST /api/devices/{deviceID}/properties` - Set device properties (twin)
  - Body: IEnumerable<DevicePropertyValue>
  - Returns: 200 OK
  - Authorization: device:write

### Supporting Endpoints
- `GET /api/devices/available-labels` - Get available labels on devices
  - Returns: IEnumerable<LabelDto>
  - Authorization: device:read

---

## Authorization

### Required Permissions
- **device:read** - View device list, details, properties, credentials, and labels
- **device:write** - Create, update, delete devices and modify device properties
- **device:execute** - Execute device commands (referenced in detail page)
- **device:import** - Import device lists from file
- **device:export** - Export device lists to file

### Authorization Implementation
- Attribute-based authorization using `[Authorize("permission")]` attributes on controller methods
- Permission checks in UI components using `HasPermissionAsync(PortalPermissions.*)` for conditional rendering
- Base authorization requirement: `[Authorize]` on DevicesController

---

## Dependencies

### Internal Feature Dependencies
- **Device Model Management** - Devices must be associated with a device model; model defines device properties and behavior
- **Device Tags** - Custom searchable and filterable metadata fields for devices
- **Label Management** - Device categorization and filtering system
- **Layer Management** - Optional hierarchical organization of devices (LayerId property)
- **IoT Hub Integration** - External device service for Azure IoT Hub or AWS IoT Core synchronization

### Service Dependencies
- `IExternalDeviceService` - IoT Hub/AWS device operations (create, update, delete, get twin)
- `IDeviceTagService` - Device tag management
- `IDeviceModelService` - Device model retrieval and validation
- `IDeviceModelPropertiesService` - Device model property templates
- `IDeviceModelImageManager` - Device model image retrieval
- `IDeviceTwinMapper` - Mapping between DTOs and device twins
- `IDeviceTagValueRepository` - Device tag value persistence
- `ILabelRepository` - Label persistence

### External Dependencies
- **Azure IoT Hub** or **AWS IoT Core** - Cloud IoT device management service
- **Entity Framework Core** - Database access via PortalDbContext
- **AutoMapper** - Entity to DTO mapping

### UI Dependencies
- **MudBlazor** - UI component library (MudTable, MudForm, MudTextField, etc.)
- Device Tag Settings (for searchable tags)
- Device Models (for model selection and properties)

---

## Key Features & Behaviors

### Search and Filtering
- Full-text search by device ID or device name
- Filter by device status (enabled/disabled)
- Filter by device state (connected/disconnected)
- Filter by device model
- Filter by custom tags (searchable tags defined in settings)
- Filter by labels (multi-select)
- Sorting support on multiple columns

### Pagination
- Server-side pagination with configurable page size
- Default page size: 10 items
- Uses cursor-based navigation with nextPage URL

### Import/Export
- Template file download for bulk device creation
- CSV/Excel import with validation and error reporting
- Device list export with current filters applied

### Device Twin Synchronization
- Automatic synchronization with Azure IoT Hub or AWS IoT Core
- Device twin properties (desired and reported) management
- Status synchronization (enabled/disabled, connected/disconnected)
- Tag synchronization between portal and IoT Hub

### Validation
- Device ID format validation (128 characters, alphanumeric with special characters)
- Device name required
- Model ID required
- Model state validation before form submission

---

## Notes

### Architecture Patterns
- **Generic Controller Base** - DevicesControllerBase<TDto> allows reuse for different device types (standard, LoRaWAN)
- **Service Layer Abstraction** - IDeviceService<TDto> enables polymorphic device handling
- **Repository Pattern** - Clean separation of data access concerns
- **External Service Integration** - IExternalDeviceService abstracts cloud provider differences

### Multi-Cloud Support
- Supports both Azure IoT Hub and AWS IoT Core
- Cloud provider selection via PortalSettings.CloudProvider
- Provider-specific implementations of IExternalDeviceService

### Performance Considerations
- Server-side pagination reduces data transfer
- Lazy loading with Include() for related entities (Labels, Tags, DeviceModel)
- Image URLs retrieved asynchronously
- Query optimization with predicate building

### Security Considerations
- Comprehensive authorization checks at controller and UI levels
- Device credentials handled securely through dedicated endpoint
- Input validation on all user-submitted data
- XSS protection through Blazor's automatic encoding

### Testing Coverage
- Unit tests exist for controllers, services, and UI components
- Test files: DevicesControllerTests.cs, DeviceServiceTests.cs, DeviceDetailValidatorTests.cs, etc.

### Future Enhancement Opportunities
- Real-time device status updates via SignalR
- Bulk device operations (enable/disable multiple devices)
- Device health monitoring and alerting
- Advanced telemetry visualization
- Device group management
