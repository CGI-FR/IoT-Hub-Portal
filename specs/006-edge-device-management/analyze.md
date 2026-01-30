# Feature: Edge Device Management

**Category**: IoT Device Management  
**Status**: Analyzed  

---

## Description

The Edge Device Management feature provides comprehensive management capabilities for Azure IoT Edge and AWS IoT Greengrass devices within the IoT Hub Portal. IoT Edge devices are special gateway devices that can run containerized workloads (Edge modules) at the edge of the network and act as transparent gateways for downstream IoT devices. This feature includes:

- Creating and provisioning new edge devices with automatic cloud registration
- Listing and filtering edge devices with pagination, search, and status filtering
- Viewing detailed edge device information including runtime status, modules, and telemetry
- Updating edge device properties, tags, and labels
- Deleting edge devices with cleanup in both local database and cloud provider
- Monitoring edge device connectivity and runtime health status
- Managing edge modules deployed to devices
- Executing commands on edge modules (Azure only)
- Viewing edge module logs in real-time (Azure only)
- Generating enrollment credentials and scripts for device provisioning
- Filtering devices by model, labels, and connection status
- Tracking connected leaf devices and deployed modules count
- Supporting both Azure IoT Hub and AWS IoT Greengrass platforms

This feature provides business value by enabling centralized management of edge computing infrastructure, allowing organizations to deploy intelligence at the edge, manage gateway devices that aggregate data from downstream devices, and monitor the health and status of distributed edge workloads. Edge devices reduce cloud bandwidth costs, enable offline operation, and provide low-latency processing for time-sensitive scenarios.

---

## Code Locations

### Entry Points / Endpoints
- `src/IoTHub.Portal.Server/Controllers/v1.0/EdgeDevicesController.cs` (Lines 1-277)
  - **Snippet**: Main REST API controller for edge device management
    ```csharp
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/edge/devices")]
    [ApiExplorerSettings(GroupName = "IoT Edge Devices")]
    public class EdgeDevicesController : ControllerBase
    {
        [HttpGet(Name = "GET IoT Edge devices")]
        [Authorize("edge-device:read")]
        public async Task<PaginationResult<IoTEdgeListItem>> Get(
            string searchText, bool? searchStatus, int pageSize, 
            int pageNumber, string[] orderBy, string modelId, string[] labels)
        
        [HttpGet("{deviceId}", Name = "GET IoT Edge device")]
        [Authorize("edge-device:read")]
        public async Task<IActionResult> Get(string deviceId)
        
        [HttpPost(Name = "POST Create IoT Edge")]
        [Authorize("edge-device:write")]
        public async Task<IActionResult> CreateEdgeDeviceAsync(IoTEdgeDevice edgeDevice)
        
        [HttpPut(Name = "PUT Update IoT Edge")]
        [Authorize("edge-device:write")]
        public async Task<IActionResult> UpdateDeviceAsync(IoTEdgeDevice edgeDevice)
        
        [HttpDelete("{deviceId}", Name = "DELETE Remove IoT Edge")]
        [Authorize("edge-device:write")]
        public async Task<IActionResult> DeleteDeviceAsync(string deviceId)
        
        [HttpGet("{deviceId}/credentials", Name = "GET Device enrollment credentials")]
        [Authorize("edge-device:read")]
        public async Task<ActionResult<SymmetricCredentials>> GetCredentials(string deviceId)
        
        [HttpGet("{deviceId}/enrollementScript/{templateName}")]
        [Authorize("edge-device:read")]
        public ActionResult<string> GetEnrollementScriptUrl(string deviceId, string templateName)
        
        [HttpGet("enroll", Name = "GET Device enrollment script")]
        [Authorize("edge-device:read")]
        public async Task<ActionResult<string>> GetEnrollementScript([FromQuery] string code)
        
        [HttpPost("{deviceId}/{moduleName}/{methodName}", Name = "POST Execute module command")]
        [Authorize("edge-device:execute")]
        public async Task<C2Dresult> ExecuteModuleMethod(string deviceId, string moduleName, string methodName)
        
        [HttpPost("{deviceId}/logs", Name = "Get Edge Device logs")]
        [Authorize("edge-device:read")]
        public async Task<IEnumerable<IoTEdgeDeviceLog>> GetEdgeDeviceLogs(string deviceId, IoTEdgeModule edgeModule)
        
        [HttpGet("available-labels", Name = "GET Available Labels on Edge Devices")]
        [Authorize("edge-device:read")]
        public Task<IEnumerable<LabelDto>> GetAvailableLabels()
    }
    ```

### Business Logic
- `src/IoTHub.Portal.Application/Services/IEdgeDevicesService.cs` (Lines 1-33)
  - **Snippet**: Core service interface for edge device operations
    ```csharp
    public interface IEdgeDevicesService
    {
        Task<PaginatedResult<IoTEdgeListItem>> GetEdgeDevicesPage(
            string? searchText, bool? searchStatus, int pageSize, 
            int pageNumber, string[]? orderBy, string? modelId, 
            List<string>? labels);
        
        Task<IoTEdgeDevice> GetEdgeDevice(string edgeDeviceId);
        Task<IoTEdgeDevice> CreateEdgeDevice(IoTEdgeDevice edgeDevice);
        Task<IoTEdgeDevice> UpdateEdgeDevice(IoTEdgeDevice edgeDevice);
        Task DeleteEdgeDeviceAsync(string deviceId);
        Task<C2Dresult> ExecuteModuleMethod(string deviceId, string moduleName, string methodName);
        Task<C2Dresult> ExecuteModuleCommand(string deviceId, string moduleName, string commandName);
        Task<IEnumerable<LabelDto>> GetAvailableLabels();
        Task<string> GetEdgeDeviceEnrollementScript(string deviceId, string templateName);
    }
    ```

- `src/IoTHub.Portal.Infrastructure/Services/EdgeDevicesServiceBase.cs` (Lines 1-100+)
  - Base implementation shared across cloud providers
  - Handles database operations for edge device CRUD
  - Provides pagination with filtering by status, model, labels, and keyword
  - Maps entities to DTOs with device model images
  - Manages device tags and labels
  - Coordinates between database and cloud provider services

- `src/IoTHub.Portal.Server/Services/AzureEdgeDevicesService.cs`
  - Azure-specific implementation of IEdgeDevicesService
  - Integrates with Azure IoT Hub for device twin operations
  - Handles edge module management and command execution
  - Retrieves module logs via IoT Hub APIs
  - Generates enrollment scripts for Azure IoT Edge runtime

- `src/IoTHub.Portal.Infrastructure/Services/AWS/AWSEdgeDevicesService.cs`
  - AWS-specific implementation for IoT Greengrass
  - Integrates with AWS IoT Core for thing management
  - Maps Greengrass core devices to edge device model
  - Handles AWS-specific deployment and configuration

### Data Access
- `src/IoTHub.Portal.Domain/Repositories/IEdgeDeviceRepository.cs` (Lines 1-9)
  - Generic repository interface for EdgeDevice entity
  - Inherits from IRepository<EdgeDevice>
  - Provides standard CRUD and query operations

- `src/IoTHub.Portal.Domain/Entities/EdgeDevice.cs` (Lines 1-52)
  - **Snippet**: Edge device entity definition
    ```csharp
    public class EdgeDevice : EntityBase
    {
        public string Name { get; set; } = default!;
        public string DeviceModelId { get; set; } = default!;
        public int Version { get; set; }
        public string ConnectionState { get; set; } = default!;
        public bool IsEnabled { get; set; }
        public string? Scope { get; set; }
        public int NbDevices { get; set; }           // Connected leaf devices
        public int NbModules { get; set; }           // Deployed modules
        public ICollection<DeviceTagValue> Tags { get; set; } = default!;
        public EdgeDeviceModel DeviceModel { get; set; } = default!;
        public ICollection<Label> Labels { get; set; } = default!;
    }
    ```

### UI Components
- `src/IoTHub.Portal.Client/Pages/EdgeDevices/EdgeDeviceListPage.razor` (Lines 1-332)
  - Main edge device list page with search and filtering
  - Features:
    - Server-side pagination with MudTable
    - Search panel with filters (name, status, model, labels)
    - Real-time connection status indicators
    - Count of connected leaf devices per edge device
    - Delete device confirmation dialog
    - Navigation to device details
    - Conditional write operations based on permissions
    - Label filtering with multi-select
  - Route: `/edge/devices`
  - Authorization: `edge-device:read` required, `edge-device:write` for delete

- `src/IoTHub.Portal.Client/Pages/EdgeDevices/EdgeDeviceDetailPage.razor` (Lines 1-494)
  - Comprehensive device details and management page
  - Features:
    - Device information display (name, model, status)
    - Runtime health status with visual indicators
    - Connected devices and modules count
    - Module list with status and actions (Azure only)
    - Module command execution (reboot, custom commands)
    - Module log viewer dialog
    - Device tags editor with validation
    - Labels editor
    - Last deployment information
    - Connection string/credentials dialog
    - Device enable/disable toggle (Azure only)
    - Save and duplicate functionality
    - Delete device action
  - Route: `/edge/devices/{deviceId}`
  - Authorization: `edge-device:read` required, `edge-device:write` for updates, `edge-device:execute` for commands

- `src/IoTHub.Portal.Client/Pages/EdgeDevices/CreateEdgeDevicePage.razor` (Lines 1-393)
  - Edge device creation wizard
  - Features:
    - Model selection with autocomplete
    - Device ID input (Azure) or auto-generation (AWS)
    - Device name input with validation
    - Status selection (enabled/disabled)
    - Device tags with required field validation
    - Labels assignment
    - Save, Save and add new, Save and duplicate options
    - Device duplication from existing device
  - Route: `/edge/devices/new`
  - Authorization: `edge-device:write` required

### Data Transfer Objects
- `src/IoTHub.Portal.Shared/Models/v1.0/IoTEdgeDevice.cs` (Lines 1-96)
  - **Snippet**: Complete edge device DTO
    ```csharp
    public class IoTEdgeDevice
    {
        public string DeviceId { get; set; } = default!;
        
        [Required(ErrorMessage = "The device should have a name.")]
        public string DeviceName { get; set; } = default!;
        
        [Required(ErrorMessage = "The device should use a model.")]
        public string ModelId { get; set; } = default!;
        
        public string Image { get; set; } = default!;
        public string ConnectionState { get; set; } = default!;
        public string Scope { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string RuntimeResponse { get; set; } = default!;
        public int NbDevices { get; set; }
        public int NbModules { get; set; }
        public ConfigItem LastDeployment { get; set; }
        public IReadOnlyCollection<IoTEdgeModule> Modules { get; set; }
        public Dictionary<string, string> Tags { get; set; } = new();
        public bool IsEnabled { get; set; }
        public List<LabelDto> Labels { get; set; } = new();
    }
    ```

- `src/IoTHub.Portal.Shared/Models/v1.0/IoTEdgeListItem.cs` (Lines 1-42)
  - **Snippet**: Lightweight DTO for device list
    ```csharp
    public class IoTEdgeListItem
    {
        [Required(ErrorMessage = "The device identifier is required.")]
        public string DeviceId { get; set; } = default!;
        public string DeviceName { get; set; } = default!;
        public string Status { get; set; } = default!;
        public int NbDevices { get; set; }
        public string Image { get; set; } = default!;
        public IEnumerable<LabelDto> Labels { get; set; } = new List<LabelDto>();
    }
    ```

### Client Services
- `src/IoTHub.Portal.Client/Services/IEdgeDeviceClientService.cs` (Lines 1-29)
  - Client-side service interface for HTTP API calls
  - Methods: GetDevices, GetDevice, CreateDevice, UpdateDevice, DeleteDevice
  - Methods: GetEnrollmentCredentials, GetEnrollmentScriptUrl
  - Methods: GetEdgeDeviceLogs, ExecuteModuleMethod, GetAvailableLabels

- `src/IoTHub.Portal.Client/Services/EdgeDeviceClientService.cs` (Lines 1-73)
  - Implementation using HttpClient for API communication
  - GET /api/edge/devices with pagination URI
  - POST /api/edge/devices for creation
  - PUT /api/edge/devices/{deviceId} for updates
  - DELETE /api/edge/devices/{deviceId} for deletion
  - GET /api/edge/devices/{deviceId}/credentials for enrollment
  - POST /api/edge/devices/{deviceId}/logs for log retrieval
  - POST /api/edge/devices/{deviceId}/{moduleName}/{methodName} for command execution

### Mappers
- `src/IoTHub.Portal.Application/Mappers/EdgeDeviceProfile.cs` (Lines 1-78)
  - AutoMapper profile for edge device entity to DTO mapping
  - **Snippet**: Key mappings
    ```csharp
    // Azure Twin to EdgeDevice entity
    CreateMap<Twin, EdgeDevice>()
        .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.DeviceId))
        .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Tags["deviceName"]))
        .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => GetTags(src)))
        .ForMember(dest => dest.NbDevices, opts => opts.MapFrom(...))
        .ForMember(dest => dest.NbModules, opts => opts.MapFrom(...));
    
    // Entity to DTO
    CreateMap<EdgeDevice, IoTEdgeDevice>()
        .ForMember(dest => dest.Tags, opts => opts.MapFrom(
            src => src.Tags.ToDictionary(tag => tag.Name, tag => tag.Value)));
    
    // DTO to Entity
    CreateMap<IoTEdgeDevice, EdgeDevice>()
        .ForMember(dest => dest.Tags, opts => opts.MapFrom(
            src => src.Tags.Select(pair => new DeviceTagValue {...})));
    
    // Entity to List Item
    CreateMap<EdgeDevice, IoTEdgeListItem>()
        .ForMember(dest => dest.Labels, opts => opts.MapFrom(
            src => src.Labels.Union(src.DeviceModel.Labels)));
    ```
  - Extracts connected devices count from device twin reported properties
  - Filters system tags (modelId, deviceName) from custom tags
  - Merges device-specific and model-level labels

---

## API Endpoints

### Edge Device Management
- `GET /api/edge/devices` - Get paginated edge device list
  - Query Parameters:
    - searchText (string): Filter by device ID or name
    - searchStatus (bool?): Filter by connection status
    - pageSize (int, default 10): Results per page
    - pageNumber (int, default 0): Page number
    - orderBy (string[]): Sort order (e.g., "Id asc", "NbDevices desc")
    - modelId (string): Filter by device model ID
    - labels (string[]): Filter by label names
  - Returns: PaginationResult<IoTEdgeListItem>
  - Authorization: edge-device:read
  - Used for device list page with server-side filtering and pagination

- `GET /api/edge/devices/{deviceId}` - Get specific edge device details
  - Route Parameter: deviceId (string)
  - Returns: IoTEdgeDevice
  - Authorization: edge-device:read
  - Returns 404 if device not found
  - Includes modules, tags, labels, and deployment information

- `POST /api/edge/devices` - Create new edge device
  - Body: IoTEdgeDevice
  - Returns: IoTEdgeDevice (with generated IDs)
  - Authorization: edge-device:write
  - Provisions device in cloud provider (Azure IoT Hub or AWS IoT Core)
  - Creates database entry with tags and labels
  - Returns 400 on validation errors

- `PUT /api/edge/devices` - Update edge device
  - Body: IoTEdgeDevice (full object with DeviceId)
  - Returns: IoTEdgeDevice
  - Authorization: edge-device:write
  - Updates both database and cloud provider device twin/thing
  - Synchronizes tags, labels, and device properties

- `DELETE /api/edge/devices/{deviceId}` - Delete edge device
  - Route Parameter: deviceId (string)
  - Returns: 200 OK with confirmation message
  - Authorization: edge-device:write
  - Removes device from cloud provider
  - Deletes database entry with cascade delete for tags and labels

### Edge Device Credentials & Enrollment
- `GET /api/edge/devices/{deviceId}/credentials` - Get enrollment credentials
  - Route Parameter: deviceId (string)
  - Returns: SymmetricCredentials (primary/secondary keys)
  - Authorization: edge-device:read
  - Used for manual device configuration
  - Returns 404 if device not found

- `GET /api/edge/devices/{deviceId}/enrollementScript/{templateName}` - Get enrollment script URL
  - Route Parameters: deviceId (string), templateName (string)
  - Returns: Time-limited protected URL string
  - Authorization: edge-device:read
  - Generates secure URL valid for 15 minutes
  - Uses data protection to encrypt parameters

- `GET /api/edge/devices/enroll` - Download enrollment script
  - Query Parameter: code (string, encrypted parameters)
  - Returns: Shell script content for device provisioning
  - Authorization: edge-device:read
  - Decrypts and validates time-limited code
  - Returns 400 on expired or invalid code
  - Script configures IoT Edge runtime with connection string

### Edge Module Management
- `POST /api/edge/devices/{deviceId}/{moduleName}/{methodName}` - Execute module method
  - Route Parameters: deviceId, moduleName, methodName (strings)
  - Returns: C2Dresult (status code and payload)
  - Authorization: edge-device:execute
  - Invokes direct method on edge module via cloud provider
  - Azure only: Custom commands defined in edge model
  - Common methods: RestartModule, custom business logic
  - Returns command execution status and response payload

- `POST /api/edge/devices/{deviceId}/logs` - Get edge module logs
  - Route Parameter: deviceId (string)
  - Body: IoTEdgeModule (specifies which module)
  - Returns: IEnumerable<IoTEdgeDeviceLog>
  - Authorization: edge-device:read
  - Azure only: Retrieves logs via IoT Hub
  - Returns recent log entries for specified module
  - Used for troubleshooting and monitoring

### Label Management
- `GET /api/edge/devices/available-labels` - Get all labels used by edge devices
  - Returns: IEnumerable<LabelDto>
  - Authorization: edge-device:read
  - Returns distinct labels across all edge devices
  - Used for label filter in device list

---

## Authorization

### Required Permissions
- **edge-device:read** - View edge devices, device details, and retrieve credentials
- **edge-device:write** - Create, update, and delete edge devices
- **edge-device:execute** - Execute commands on edge modules

### Authorization Implementation
- Attribute-based authorization using `[Authorize("permission")]` on controller methods
- Permission strings defined in PortalPermissionsHelper
- Base authorization: `[Authorize]` on EdgeDevicesController and all UI pages
- Permissions managed through RBAC system
- Default Administrator role includes all three edge device permissions
- UI conditionally renders action buttons based on user permissions
  - Write operations hidden if user lacks edge-device:write
  - Module commands disabled if user lacks edge-device:execute or device is disconnected

### Permission Mapping
- `PortalPermissions.EdgeDeviceRead` → "edge-device:read"
- `PortalPermissions.EdgeDeviceWrite` → "edge-device:write"
- `PortalPermissions.EdgeDeviceExecute` → "edge-device:execute"

---

## Dependencies

### Internal Feature Dependencies
- **Edge Device Models** - Defines edge device capabilities, modules, and commands
- **Device Tag Settings** - Custom metadata fields applied to edge devices
- **Label Management** - Organizational labels for filtering and categorization
- **Role-Based Access Control** - Permission enforcement through RBAC system
- **Device Model Images** - Visual representation in UI via IDeviceModelImageManager
- **External Device Service** - Cloud provider abstraction (Azure/AWS)

### Service Dependencies
- `IEdgeDeviceRepository` - Edge device entity persistence
- `IDeviceTagService` - Device tag definitions and retrieval
- `IDeviceTagValueRepository` - Device-specific tag values
- `ILabelRepository` - Label persistence and queries
- `IExternalDeviceService` - Cloud provider device operations
- `IDeviceModelImageManager` - Device model image resolution
- `IMapper` (AutoMapper) - Entity to DTO mapping
- `IUnitOfWork` - Transaction management for database operations
- `IDataProtectionProvider` - Time-limited enrollment URL encryption

### Related Entities
- **EdgeDevice** - Primary entity with navigation to DeviceModel, Tags, Labels
- **EdgeDeviceModel** - Defines device capabilities and modules
- **DeviceTagValue** - Custom tag values per device
- **Label** - Organizational labels
- **IoTEdgeModule** - Module deployed to edge device
- **ConfigItem** - Last deployment configuration

### External Dependencies
- **Entity Framework Core** - Database access via PortalDbContext
- **AutoMapper** - Entity to DTO mapping
- **Azure IoT Hub SDK** - Azure-specific device management
  - Microsoft.Azure.Devices (Device Registry, Twin operations)
  - Direct method invocation on modules
  - Module log retrieval
- **AWS IoT SDK** - AWS-specific device management
  - Thing registry operations
  - Greengrass core device management
- **Data Protection API** - Secure enrollment URL generation

### UI Dependencies
- **MudBlazor** - UI component library
  - MudTable - Paginated device list with server-side data
  - MudExpansionPanel - Collapsible sections for device details
  - MudAutocomplete - Model selection with search
  - MudRadioGroup - Status and filter selections
  - MudSelect - Multi-select for labels
  - MudIconButton - Action buttons
  - MudDialog - Confirmation dialogs and log viewer
  - MudSnackbar - User notifications
  - MudForm - Form validation

---

## Key Features & Behaviors

### Edge Device Lifecycle Management
- **Device Provisioning**: Creates device in cloud provider (Azure IoT Hub or AWS IoT Core) with automatic configuration
- **Device Twin Synchronization**: Maintains device metadata in cloud provider device twin/thing shadow
- **Connection State Tracking**: Real-time monitoring of device connectivity (Connected/Disconnected)
- **Runtime Health Monitoring**: Tracks IoT Edge runtime status (running/stopped/unhealthy)
- **Soft Delete**: Device removal from both database and cloud provider

### Edge Module Management
- **Module Discovery**: Automatically lists modules deployed to edge device
- **Module Status**: Shows runtime status of each module
- **Module Commands**: Execute direct methods on modules (RestartModule, custom commands)
- **Module Logs**: Real-time log retrieval for troubleshooting (Azure only)
- **Module Count**: Tracks number of deployed modules per device

### Gateway Functionality
- **Leaf Device Aggregation**: Tracks number of downstream devices connected through edge device
- **Transparent Gateway**: Edge devices act as gateways for devices without direct cloud connectivity
- **Client Count**: Displays number of devices using edge device as gateway
- **Scope Management**: Assigns device scope for hierarchical organization

### Search and Filtering
- **Text Search**: Filter by device ID or device name (case-insensitive, partial match)
- **Status Filter**: Filter by connection status (Connected, Disconnected, All)
- **Model Filter**: Filter by edge device model with autocomplete
- **Label Filter**: Multi-select label filter with visual chips
- **Combined Filters**: All filters work together (AND logic)
- **Server-Side Pagination**: Efficient large dataset handling
- **Sortable Columns**: Click headers to sort by device ID, status, or connected devices

### Device Enrollment
- **Credential Generation**: Provides symmetric keys for device authentication
- **Enrollment Scripts**: Generates platform-specific installation scripts
- **Secure URLs**: Time-limited (15 minutes) protected script download links
- **Template Support**: Multiple enrollment templates for different scenarios
- **Connection String Dialog**: Interactive UI for credential viewing

### Device Tags and Labels
- **Custom Tags**: User-defined metadata fields with validation
- **Required Tags**: Enforced during device creation
- **Tag Synchronization**: Tags stored in cloud provider device twin
- **Labels**: Organizational labels inherited from model or device-specific
- **Label Filtering**: Search devices by assigned labels
- **Tag Validation**: Required field enforcement with visual error indicators

### Multi-Cloud Support
- **Azure IoT Hub**: Full feature support including module commands and logs
- **AWS IoT Greengrass**: Core functionality with Greengrass-specific adaptations
- **Provider-Specific UI**: Conditional rendering based on CloudProvider setting
- **Abstracted Service Layer**: Common interface with provider-specific implementations

### Device Duplication
- **Save and Duplicate**: Creates new device from existing configuration
- **Template Reuse**: Copies tags, labels, and model selection
- **ID Generation**: Auto-generates new device ID for duplicate
- **Shared State**: Uses layout service to pass device state between pages

### Real-Time Status Indicators
- **Connection Status Icons**: Visual indicators (checkmark/error icon) with tooltips
- **Runtime Status Icons**: Health indicators for IoT Edge runtime
- **Color Coding**: Success (green) for healthy, error (red) for issues
- **Tooltip Context**: Hover tooltips explain status meanings

### Validation and Error Handling
- **Form Validation**: Required fields enforced (device name, model, required tags)
- **Tag Validation**: Required tags checked before save
- **Model Validation**: Edge device model must be selected
- **Error Messages**: User-friendly Snackbar notifications
- **Problem Details**: Structured error responses from API
- **Device Not Found**: 404 handling with appropriate user messaging

### Performance Optimizations
- **Pagination**: Server-side pagination reduces data transfer
- **Image Caching**: Device model images cached via IDeviceModelImageManager
- **Lazy Loading**: Modules and logs loaded on-demand
- **Filtered Queries**: Database queries filtered at repository level
- **Predicate Builder**: Efficient dynamic query construction

---

## Notes

### Architecture Patterns
- **Repository Pattern** - Clean separation of data access concerns
- **Unit of Work Pattern** - Transactional consistency across operations
- **Service Layer Pattern** - Business logic abstraction from controllers
- **DTO Pattern** - Data transfer between layers with validation
- **Template Method Pattern** - EdgeDevicesServiceBase with cloud-specific implementations
- **Adapter Pattern** - IExternalDeviceService abstracts cloud provider differences

### Edge Device Specialization
- Edge devices are distinct from regular IoT devices (separate tables, controllers)
- Edge devices can run containerized modules (Docker containers)
- Edge devices act as gateways for downstream leaf devices
- Edge runtime provides offline capabilities and local processing
- Edge modules defined by edge device models (not device models)

### Azure IoT Edge Integration
- Device twin tags store device metadata (modelId, deviceName, custom tags)
- Reported properties contain runtime status and connected clients count
- Direct methods enable module command execution
- IoT Hub APIs provide module log streaming
- Symmetric key authentication for device provisioning
- Device scope enables hierarchical device relationships

### AWS IoT Greengrass Integration
- Thing registry stores device metadata and attributes
- Greengrass core devices map to edge devices
- Thing shadows used for state synchronization
- Component deployment via Greengrass deployment service
- Certificate-based authentication
- Limited command execution compared to Azure

### Device Model Relationship
- EdgeDevice references EdgeDeviceModel (not DeviceModel)
- EdgeDeviceModel defines available edge modules
- Each module can have custom commands
- Model determines device capabilities and image
- Labels can be inherited from model or device-specific

### Tag Management
- Tags stored as DeviceTagValue collection (many-to-many relationship)
- Tags defined globally in Device Tag Settings
- Tag values unique per device instance
- Required tags enforced during creation
- Tags synchronized to cloud provider twin/thing
- Tag validation occurs on both client and server

### Module Command Execution
- Commands defined in EdgeDeviceModel per module
- RestartModule is standard across all modules
- Custom commands specific to module type
- Commands disabled when device disconnected
- Command results include status code and payload
- Azure-specific feature (not available in AWS)

### Module Logs
- Real-time log retrieval from IoT Hub
- Logs specific to individual modules
- Used for troubleshooting runtime issues
- Azure-specific feature
- Displayed in modal dialog
- No persistent log storage in portal database

### Enrollment Script Generation
- Scripts automate IoT Edge runtime installation
- Include connection string and configuration
- Template-based for different scenarios (bash, PowerShell)
- Secure time-limited URLs prevent unauthorized access
- Data protection ensures parameter integrity
- Scripts stored as embedded resources or templates

### Connection State Synchronization
- Connection state retrieved from cloud provider in real-time
- Stored in database for query performance
- Background jobs sync state periodically
- State change notifications via device twin updates
- Disconnected devices have limited available actions

### Label System
- Labels provide flexible categorization
- Devices can have multiple labels
- Labels filterable in device list
- Labels inherited from model
- Device-specific labels override model labels
- Labels stored in database (not cloud provider)

### Pagination Implementation
- Server-side pagination via PaginatedResult<T>
- Total count provided for UI page control
- NextPage URL generated by controller
- MudTable handles pagination UI
- Page size configurable per request
- Default page size: 10 items

### Security Considerations
- Authorization required at both controller and UI levels
- Permission-based access control prevents unauthorized operations
- Time-limited enrollment URLs prevent replay attacks
- Credentials encrypted via data protection API
- DeviceId validation prevents injection attacks
- Cloud provider credentials isolated in service implementations

### Database Schema
- EdgeDevice table with navigation properties
- Foreign key to EdgeDeviceModel
- Many-to-many with DeviceTagValue via collection
- Many-to-many with Label via collection
- Cascade delete configured for dependent entities
- Indexes on DeviceModelId, ConnectionState for query performance

### Background Synchronization
- SyncEdgeDeviceJob periodically syncs from cloud provider
- Updates connection state, module count, client count
- Handles new devices created outside portal
- Detects deleted devices in cloud
- Configurable sync interval via job scheduler

### Testing Coverage
- Unit tests: EdgeDeviceServiceTest.cs
- Controller tests: EdgeDevicesControllerTests.cs (integration)
- UI tests: EdgeDeviceListPageTests.cs, EdgeDeviceDetailPageTests.cs, CreateEdgeDevicePageTest.cs
- Client service tests: EdgeDeviceClientServiceTests.cs
- Mapper tests: EdgeDeviceProfileTests.cs

### Future Enhancement Opportunities
- Bulk device operations (delete, update tags)
- Device configuration management (desired properties)
- Deployment history tracking
- Module metrics and telemetry visualization
- Automatic alerts for device health issues
- Device groups for batch operations
- Export device list to CSV/Excel
- Import devices from CSV/Excel
- Device lifecycle events and audit trail
- Advanced filtering (by last seen, deployment status)
- Custom device dashboards per edge device
- Edge device geo-location on map
- Firmware/module update orchestration
- Device commissioning workflow
- Parent-child device relationships visualization
- Module restart scheduling
- Log export and archival
- Integration with external monitoring tools (Prometheus, Grafana)
- Device certificate management
- Role-based device access (not just feature access)

### Known Limitations
- Module commands only available on Azure IoT Hub
- Module logs only available on Azure IoT Hub
- No real-time telemetry display on device details page
- Cannot edit device ID after creation
- No bulk device provisioning workflow
- Limited AWS IoT Greengrass feature parity
- No device hierarchy visualization
- Cannot manage module deployments from portal (use Azure/AWS consoles)
- No offline device synchronization queue
- Device scope management limited
- No device configuration templates
- Cannot filter by deployment status
- Limited module lifecycle management beyond restart
- No integration with edge device local APIs
- Cannot schedule commands for future execution

### Provider-Specific Behaviors
| Feature | Azure IoT Hub | AWS IoT Greengrass |
|---------|---------------|-------------------|
| Module Commands | ✅ Supported | ❌ Not Supported |
| Module Logs | ✅ Supported | ❌ Not Supported |
| Device Status | Enabled/Disabled | Active only |
| Authentication | Symmetric Keys | X.509 Certificates |
| Device Scope | Supported | Not Supported |
| Direct Methods | Supported | Limited |
| Module Count | From Desired Config | From Deployed Components |
| Client Count | From Reported Properties | Not Available |

### Performance Considerations
- Edge device list queries optimized with includes
- Device model images cached to reduce storage calls
- Pagination reduces memory footprint
- Background sync jobs prevent UI blocking
- Lazy loading of modules and logs
- Database indexes on frequently queried fields
- Predicate builder for efficient dynamic queries
- AutoMapper for efficient object mapping
