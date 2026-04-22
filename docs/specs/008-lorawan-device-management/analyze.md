# Feature: LoRaWAN Device Management

**Category**: Device Management  
**Status**: Analyzed  

---

## Description

The LoRaWAN Device Management feature provides comprehensive lifecycle management for LoRaWAN IoT devices with specialized LoRa network protocol support. LoRaWAN devices are low-power, wide-area network devices that communicate through LoRa gateways using the LoRaWAN protocol. This feature extends standard device management with LoRa-specific capabilities including:

- Creating and managing LoRaWAN devices with 16-character hexadecimal device IDs (DevEUI)
- Supporting both OTAA (Over-The-Air Activation) and ABP (Activation By Personalization) authentication modes
- Configuring LoRa-specific parameters: device class (A/B/C), frame counters, receive windows, data rates, and transmit power
- Managing gateway assignments and downlink/uplink communication settings
- Executing LoRaWAN-specific commands on devices
- Viewing device telemetry from LoRa gateway messages
- Configuring sensor decoders for payload decoding
- Managing deduplication modes for messages received by multiple gateways
- Supporting adaptive data rate (ADR) parameters and reported values
- Full CRUD operations through REST API and interactive UI
- Integration with Azure IoT Hub and AWS IoT Core via device twins
- Device tags and labels for organization and filtering

This feature provides business value by enabling organizations to manage their LoRaWAN device fleets with protocol-specific controls, optimizing battery life through adaptive parameters, ensuring secure authentication, and monitoring device connectivity and telemetry across distributed LoRa networks.

---

## Code Locations

### Entry Points / Endpoints
- `src/IoTHub.Portal.Server/Controllers/v1.0/LoRaWAN/LoRaWANDevicesController.cs` (Lines 1-142)
  - **Snippet**: Main REST API controller for LoRaWAN devices
    ```csharp
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/lorawan/devices")]
    [ApiExplorerSettings(GroupName = "LoRa WAN")]
    [LoRaFeatureActiveFilter]
    public class LoRaWANDevicesController : DevicesControllerBase<LoRaDeviceDetails>
    {
        private readonly ILoRaWANCommandService loRaWanCommandService;
        private readonly LoRaGatewayIDList gatewayIdList;
        private readonly IDeviceService<LoRaDeviceDetails> deviceService;
        
        [HttpGet(Name = "GET LoRaWAN device list")]
        [Authorize("device:read")]
        public Task<PaginationResult<DeviceListItem>> SearchItems(...)
        
        [HttpGet("{deviceID}", Name = "GET LoRaWAN device details")]
        [Authorize("device:read")]
        public override Task<LoRaDeviceDetails> GetItem(string deviceID)
        
        [HttpPost(Name = "POST Create LoRaWAN device")]
        [Authorize("device:write")]
        public override Task<IActionResult> CreateDeviceAsync(LoRaDeviceDetails device)
        
        [HttpPut(Name = "PUT Update LoRaWAN device")]
        [Authorize("device:write")]
        public override Task<IActionResult> UpdateDeviceAsync(LoRaDeviceDetails device)
        
        [HttpDelete("{deviceID}", Name = "DELETE Remove LoRaWAN device")]
        [Authorize("device:write")]
        public override Task<IActionResult> Delete(string deviceID)
        
        [HttpPost("{deviceId}/_command/{commandId}", Name = "POST Execute LoRaWAN command")]
        [Authorize("device:execute")]
        public async Task<IActionResult> ExecuteCommand(string deviceId, string commandId)
        
        [HttpGet("gateways", Name = "Get Gateways")]
        [Authorize("device:read")]
        public ActionResult<LoRaGatewayIDList> GetGateways()
        
        [HttpGet("{deviceId}/telemetry")]
        [Authorize("device:read")]
        public Task<IEnumerable<LoRaDeviceTelemetryDto>> GetDeviceTelemetry(string deviceId)
        
        [HttpGet("available-labels", Name = "GET Available Labels on LoRaWAN Devices")]
        [Authorize("device:read")]
        public override Task<IEnumerable<LabelDto>> GetAvailableLabels()
    }
    ```

### Business Logic
- `src/IoTHub.Portal.Application/Services/IDeviceService.cs` (Lines 1-39)
  - **Snippet**: Generic device service interface
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

- `src/IoTHub.Portal.Infrastructure/Services/LoRaWanDeviceService.cs` (Lines 1-200+)
  - Concrete implementation of IDeviceService<LoRaDeviceDetails>
  - Inherits from DeviceServiceBase with LoRa-specific implementations
  - Key methods:
    - GetDevice: Retrieves LoRaWAN device with tags and labels
    - CreateDeviceInDatabase: Inserts new LorawanDevice entity
    - UpdateDeviceInDatabase: Updates device with tag/label management
    - DeleteDeviceInDatabase: Removes device and cascades to tags/labels
    - GetDeviceTelemetry: Retrieves device-specific telemetry data
    - ProcessTelemetryEvent: Processes incoming LoRa telemetry messages
    - KeepOnlyLatestTelemetry: Maintains telemetry history limit

- `src/IoTHub.Portal.Application/Services/ILoRaWANCommandService.cs` (Lines 1-12)
  - **Snippet**: Service for executing LoRaWAN commands
    ```csharp
    public interface ILoRaWANCommandService
    {
        Task<DeviceModelCommandDto[]> GetDeviceModelCommandsFromModel(string id);
        Task PostDeviceModelCommands(string id, DeviceModelCommandDto[] commands);
        Task ExecuteLoRaWANCommand(string deviceId, string commandId);
    }
    ```

### Data Access
- `src/IoTHub.Portal.Domain/Repositories/ILorawanDeviceRepository.cs` (Lines 1-9)
  - Generic repository interface for LorawanDevice entity
  - Inherits from IRepository<LorawanDevice>

- `src/IoTHub.Portal.Domain/Repositories/ILoRaDeviceTelemetryRepository.cs`
  - Repository interface for LoRaDeviceTelemetry entity
  - Manages telemetry data storage

- `src/IoTHub.Portal.Domain/Entities/LorawanDevice.cs` (Lines 1-176)
  - **Snippet**: LoRaWAN device entity with comprehensive properties
    ```csharp
    public class LorawanDevice : Device
    {
        // Authentication
        public bool UseOTAA { get; set; }
        public string? AppKey { get; set; }          // OTAA App Key
        public string? AppEUI { get; set; }          // OTAA Application EUI
        public string? AppSKey { get; set; }         // ABP AppSKey
        public string? NwkSKey { get; set; }         // ABP NwkSKey
        public string? DevAddr { get; set; }         // Unique device address
        
        // Status
        public bool AlreadyLoggedInOnce { get; set; }
        
        // Adaptive Data Rate (ADR) reported values
        public string? DataRate { get; set; }
        public string? TxPower { get; set; }
        public string? NbRep { get; set; }
        public string? ReportedRX2DataRate { get; set; }
        public string? ReportedRX1DROffset { get; set; }
        public string? ReportedRXDelay { get; set; }
        
        // LoRa Configuration
        public string? GatewayID { get; set; }
        public bool? Downlink { get; set; }
        public ClassType ClassType { get; set; }
        public int PreferredWindow { get; set; }
        public DeduplicationMode Deduplication { get; set; }
        
        // Receive Window Configuration
        public int? RX1DROffset { get; set; }
        public int? RX2DataRate { get; set; }
        public int? RXDelay { get; set; }
        
        // Frame Counter Configuration
        public bool? ABPRelaxMode { get; set; }
        public int? FCntUpStart { get; set; }
        public int? FCntDownStart { get; set; }
        public int? FCntResetCounter { get; set; }
        public bool? Supports32BitFCnt { get; set; }
        
        // Connection
        public int? KeepAliveTimeout { get; set; }
        public string? SensorDecoder { get; set; }
        
        // Telemetry
        public ICollection<LoRaDeviceTelemetry> Telemetry { get; set; }
    }
    ```

- `src/IoTHub.Portal.Domain/Entities/LoRaDeviceTelemetry.cs`
  - Entity storing telemetry messages from LoRa devices
  - Links to LorawanDevice through relationship

### UI Components
- `src/IoTHub.Portal.Client/Pages/Devices/DeviceDetailPage.razor` (Lines 1-41)
  - Device detail page that routes to EditDevice component
  - Accepts IsLoRa query parameter to render LoRa-specific UI
  - Authorization: device:read permission required
  - Conditional display of write and execute capabilities

- `src/IoTHub.Portal.Client/Pages/Devices/DeviceListPage.razor` (Lines 1-21375)
  - Unified device list page showing both standard and LoRaWAN devices
  - Supports filtering, searching, and pagination
  - Displays LoRa-specific columns when applicable
  - Authorization: device:read permission required

- `src/IoTHub.Portal.Client/Components/Devices/EditDevice.razor` (Lines 1-815)
  - Main device edit/create component with LoRa-specific sections
  - Features:
    - Device details form with model selection
    - LoRa-specific tabs when IsLoRa=true:
      - OTAA configuration (AppEUI, AppKey)
      - ABP configuration (DevAddr, AppSKey, NwkSKey)
      - LoRaWAN settings (Class type, preferred window, deduplication)
      - Receive window configuration (RX1/RX2 data rates, offsets, delays)
      - Frame counter settings
      - Gateway assignment
      - Sensor decoder configuration
    - Device tags and labels management
    - Device commands execution panel
    - Save, duplicate, and delete operations
    - Form validation with Fluent Validator
    - Responsive layout with MudBlazor components

### Data Transfer Objects
- `src/IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/LoRaDeviceDetails.cs` (Lines 1-174)
  - **Snippet**: Complete LoRaWAN device DTO with validation
    ```csharp
    public class LoRaDeviceDetails : LoRaDeviceBase, IDeviceDetails
    {
        [Required(ErrorMessage = "The device should have a name.")]
        public string DeviceName { get; set; } = default!;
        
        [Required(ErrorMessage = "The device should use a model.")]
        public string ModelId { get; set; } = default!;
        
        public string ModelName { get; set; } = default!;
        public string Image { get; set; } = default!;
        public bool IsConnected { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime StatusUpdatedTime { get; set; }
        public DateTime LastActivityTime { get; set; }
        
        public Dictionary<string, string> Tags { get; set; } = new();
        
        [Required(ErrorMessage = "The device should have a unique identifier.")]
        [MaxLength(ErrorMessage = "The device identifier should be up to 128 characters long.")]
        [RegularExpression("^[A-Z0-9]{16}$", ErrorMessage = "The device identifier must contain 16 hexadecimal characters (numbers from 0 to 9 and/or letters from A to F)")]
        public string DeviceID { get; set; } = default!;
        
        [DefaultValue(true)]
        public bool UseOTAA { get; set; }
        
        public string AppKey { get; set; } = default!;
        public string AppEUI { get; set; } = default!;
        public string AppSKey { get; set; } = default!;
        public string NwkSKey { get; set; } = default!;
        public string DevAddr { get; set; } = default!;
        
        public bool AlreadyLoggedInOnce { get; set; }
        public string DataRate { get; set; } = default!;
        public string TxPower { get; set; } = default!;
        public string NbRep { get; set; } = default!;
        public string ReportedRX2DataRate { get; set; } = default!;
        public string ReportedRX1DROffset { get; set; } = default!;
        public string ReportedRXDelay { get; set; } = default!;
        public string GatewayID { get; set; } = default!;
        public string? LayerId { get; set; } = default!;
        
        [DefaultValue(true)]
        public bool? Downlink { get; set; }
        
        [DefaultValue(true)]
        public static bool IsLoraWan => true;
        
        public List<LabelDto> Labels { get; set; } = new();
    }
    ```

- `src/IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/LoRaDeviceBase.cs` (Lines 1-100)
  - **Snippet**: Base class with LoRa-specific configuration properties
    ```csharp
    public abstract class LoRaDeviceBase
    {
        [DefaultValue(ClassType.A)]
        public ClassType ClassType { get; set; }
        
        [DefaultValue(1)]
        public int PreferredWindow { get; set; }
        
        [DefaultValue(DeduplicationMode.Drop)]
        public DeduplicationMode Deduplication { get; set; }
        
        [DefaultValue(0)]
        public int? RX1DROffset { get; set; }
        
        [DefaultValue(0)]
        public int? RX2DataRate { get; set; }
        
        public int? RXDelay { get; set; }
        
        [DefaultValue(true)]
        public bool? ABPRelaxMode { get; set; }
        
        [Range(0, 4294967295)]
        [DefaultValue(0)]
        public int? FCntUpStart { get; set; }
        
        [Range(0, 4294967295)]
        [DefaultValue(0)]
        public int? FCntDownStart { get; set; }
        
        [Range(0, 4294967295)]
        [DefaultValue(0)]
        public int? FCntResetCounter { get; set; }
        
        [DefaultValue(true)]
        public bool? Supports32BitFCnt { get; set; }
        
        [DefaultValue(null)]
        public int? KeepAliveTimeout { get; set; }
        
        public string SensorDecoder { get; set; } = default!;
    }
    ```

- `src/IoTHub.Portal.Shared/Models/v1.0/LoRaDeviceTelemetryDto.cs`
  - DTO for LoRa device telemetry data
  - Contains timestamp and telemetry payload

### Client Services
- `src/IoTHub.Portal.Client/Services/ILoRaWanDeviceClientService.cs` (Lines 1-22)
  - **Snippet**: Client-side service interface for HTTP API calls
    ```csharp
    public interface ILoRaWanDeviceClientService
    {
        Task<LoRaDeviceDetails> GetDevice(string deviceId);
        Task CreateDevice(LoRaDeviceDetails device);
        Task UpdateDevice(LoRaDeviceDetails device);
        Task DeleteDevice(string deviceId);
        Task ExecuteCommand(string deviceId, string commandId);
        Task<LoRaGatewayIDList> GetGatewayIdList();
        Task<IEnumerable<LoRaDeviceTelemetryDto>> GetDeviceTelemetry(string deviceId);
    }
    ```

- `src/IoTHub.Portal.Client/Services/LoRaWanDeviceClientService.cs`
  - Implementation using HttpClient for API communication
  - GET/POST/PUT/DELETE endpoints for device CRUD
  - Command execution endpoint
  - Gateway list retrieval
  - Telemetry data retrieval

### Mappers
- `src/IoTHub.Portal.Infrastructure/Mappers/LoRaDeviceTwinMapper.cs` (Lines 1-191)
  - **Snippet**: Device twin mapper for IoT Hub integration
    ```csharp
    public class LoRaDeviceTwinMapper : IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>
    {
        public LoRaDeviceDetails CreateDeviceDetails(Twin twin, IEnumerable<string> tags)
        {
            // Maps twin tags to device properties
            // Extracts desired properties for LoRa settings
            // Extracts reported properties for ADR values
            // Parses enums for ClassType and DeduplicationMode
        }
        
        public DeviceListItem CreateDeviceListItem(Twin twin)
        {
            // Creates list item from twin for device list display
        }
        
        public void UpdateTwin(Twin twin, LoRaDeviceDetails item)
        {
            // Updates twin tags and desired properties from device details
            // Sets SupportLoRaFeatures flag
        }
    }
    ```

- `src/IoTHub.Portal.Infrastructure/Mappers/LoRaDeviceMapper.cs`
  - AutoMapper profile for LorawanDevice entity to DTO mapping
  - Bidirectional mapping between entity and DTO

---

## API Endpoints

### LoRaWAN Device Management
- `GET /api/lorawan/devices` - Get paginated LoRaWAN device list
  - Query Parameters:
    - searchText (string, optional): Filter by device name/ID
    - searchStatus (bool?, optional): Filter by connection status
    - searchState (bool?, optional): Filter by enabled state
    - pageSize (int, default: 10): Number of results per page
    - pageNumber (int, default: 0): Page number (zero-indexed)
    - orderBy (string[], optional): Sort fields
    - modelId (string, optional): Filter by device model
  - Returns: PaginationResult<DeviceListItem>
  - Authorization: device:read
  - Feature Filter: LoRaFeatureActiveFilter

- `GET /api/lorawan/devices/{deviceID}` - Get specific LoRaWAN device details
  - Route Parameter: deviceID (string)
  - Returns: LoRaDeviceDetails with all LoRa-specific properties
  - Authorization: device:read
  - Feature Filter: LoRaFeatureActiveFilter

- `POST /api/lorawan/devices` - Create new LoRaWAN device
  - Body: LoRaDeviceDetails with validation
  - Returns: 200 OK on success
  - Authorization: device:write
  - Feature Filter: LoRaFeatureActiveFilter
  - Validation:
    - DeviceID: 16 hexadecimal characters
    - DeviceName: Required
    - ModelId: Required
    - OTAA or ABP settings based on UseOTAA flag

- `PUT /api/lorawan/devices` - Update existing LoRaWAN device
  - Body: LoRaDeviceDetails with DeviceID
  - Returns: 200 OK on success
  - Authorization: device:write
  - Feature Filter: LoRaFeatureActiveFilter
  - Updates device twin in IoT Hub

- `DELETE /api/lorawan/devices/{deviceID}` - Delete LoRaWAN device
  - Route Parameter: deviceID (string)
  - Returns: 200 OK on success
  - Authorization: device:write
  - Feature Filter: LoRaFeatureActiveFilter
  - Cascades delete to tags, labels, and telemetry

- `POST /api/lorawan/devices/{deviceId}/_command/{commandId}` - Execute LoRaWAN command
  - Route Parameters:
    - deviceId (string): Device identifier
    - commandId (string): Command identifier from device model
  - Returns: 200 OK on success
  - Authorization: device:execute
  - Feature Filter: LoRaFeatureActiveFilter
  - Sends C2D message through IoT Hub

- `GET /api/lorawan/devices/gateways` - Get list of available gateways
  - Returns: LoRaGatewayIDList
  - Authorization: device:read
  - Feature Filter: LoRaFeatureActiveFilter
  - Used for gateway assignment dropdown

- `GET /api/lorawan/devices/{deviceId}/telemetry` - Get device telemetry history
  - Route Parameter: deviceId (string)
  - Returns: IEnumerable<LoRaDeviceTelemetryDto>
  - Authorization: device:read
  - Feature Filter: LoRaFeatureActiveFilter

- `GET /api/lorawan/devices/available-labels` - Get all labels used by LoRaWAN devices
  - Returns: IEnumerable<LabelDto>
  - Authorization: device:read
  - Feature Filter: LoRaFeatureActiveFilter
  - Used for label filtering

---

## Authorization

### Required Permissions
- **device:read** - View LoRaWAN devices, device lists, details, telemetry, gateways, and labels
- **device:write** - Create, update, and delete LoRaWAN devices
- **device:execute** - Execute commands on LoRaWAN devices

### Authorization Implementation
- Attribute-based authorization using `[Authorize("permission")]` attributes on controller methods
- Permission strings defined in PortalPermissionsHelper
- Base authorization requirement: `[Authorize]` on LoRaWANDevicesController
- Permissions managed through role-based access control (RBAC)
- Default Administrator role includes device:read, device:write, and device:execute permissions

### Feature Filter
- `[LoRaFeatureActiveFilter]` attribute on controller
- Blocks all LoRaWAN endpoints when LoRa feature is disabled in configuration
- Returns 404 Not Found when feature is inactive
- Configuration setting: `PortalSettings.IsLoRaSupported`

### Permission Mapping
- `PortalPermissions.DeviceRead` → "device:read"
- `PortalPermissions.DeviceWrite` → "device:write"
- `PortalPermissions.DeviceExecute` → "device:execute"

---

## Dependencies

### Internal Feature Dependencies
- **Device Models** - LoRaWAN device models define available commands, properties, and decoder settings
- **Device Tag Settings** - Tags applied to LoRaWAN devices for organization
- **Label Management** - Labels for categorizing and filtering LoRaWAN devices
- **IoT Hub Integration** - Device twin synchronization for all LoRa properties
- **Role-Based Access Control** - Permissions enforced through RBAC system
- **LoRaWAN Commands** - Command execution on devices via ILoRaWANCommandService
- **Gateway Management** - LoRa concentrators/gateways for device connectivity

### Service Dependencies
- `IDeviceService<LoRaDeviceDetails>` - Core device operations
- `ILorawanDeviceRepository` - Device entity persistence
- `ILoRaDeviceTelemetryRepository` - Telemetry data persistence
- `IDeviceTagValueRepository` - Device-specific tag values
- `ILabelRepository` - Device label persistence
- `ILoRaWANCommandService` - Command execution
- `IExternalDeviceService` - IoT Hub/AWS IoT Core integration
- `IDeviceTagService` - Tag definition management
- `IDeviceModelImageManager` - Device model image handling
- `IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>` - Twin mapping
- `IUnitOfWork` - Transaction management
- `IMapper` (AutoMapper) - Entity to DTO mapping
- `LoRaGatewayIDList` - Gateway configuration

### Related Entities
- **LorawanDevice** - Primary device entity inheriting from Device
- **LoRaDeviceTelemetry** - Telemetry messages collection
- **DeviceTagValue** - Custom tag values per device
- **Label** - Device categorization labels
- **DeviceModel** - LoRaWAN-specific device models
- **Concentrator** - LoRa gateways/concentrators

### External Dependencies
- **Entity Framework Core** - Database access via PortalDbContext
- **AutoMapper** - Entity to DTO mapping
- **FluentValidation** - DTO validation
- **Azure IoT Hub SDK** - Device twin operations and C2D messages
- **AWS IoT Core SDK** - Alternative cloud provider support

### UI Dependencies
- **MudBlazor** - UI component library
  - MudForm - Form container with validation
  - MudTextField - Text input for device properties
  - MudSelect - Dropdowns for gateway, class type, deduplication mode
  - MudCheckBox - Boolean flags (UseOTAA, Downlink, etc.)
  - MudNumericField - Numeric inputs for frame counters, timeouts
  - MudAutocomplete - Model selection
  - MudTabs - Tabbed interface for LoRa settings sections
  - MudExpansionPanels - Collapsible configuration panels
  - MudTable - Device list with pagination
  - MudSnackbar - User feedback notifications

---

## Key Features & Behaviors

### LoRaWAN Authentication Modes
- **OTAA (Over-The-Air Activation)**: 
  - Uses AppEUI (Application EUI) and AppKey for secure join
  - Device negotiates session keys during join procedure
  - Recommended for production deployments
  - DevAddr assigned dynamically after join
- **ABP (Activation By Personalization)**:
  - Uses pre-configured AppSKey and NwkSKey
  - Requires DevAddr to be set manually
  - Faster initial connection but less secure
  - Frame counter management critical

### Device ID Format
- Must be exactly 16 hexadecimal characters (0-9, A-F)
- Represents the DevEUI (Device Extended Unique Identifier)
- Validated by regex: `^[A-Z0-9]{16}$`
- Immutable after device creation

### LoRa Device Classes
- **Class A**: Lowest power, bi-directional communication after uplink
- **Class B**: Scheduled receive windows for downlink
- **Class C**: Continuous receive, highest power consumption
- Configurable via ClassType property

### Receive Window Configuration
- **Preferred Window**: RX1 (1) or RX2 (2)
- **RX1DROffset**: Offset between RX and TX data rate (OTAA only)
- **RX2DataRate**: Custom data rate for RX2 window (OTAA only)
- **RXDelay**: Wait time between receive and transmit per LoRaWAN spec

### Frame Counter Management
- **FCntUpStart**: Initial uplink frame counter (default: 0)
- **FCntDownStart**: Initial downlink frame counter (default: 0)
- **FCntResetCounter**: Allows frame counter reset
- **Supports32BitFCnt**: Enable 32-bit counters (default: true)
- **ABPRelaxMode**: Relaxed frame counter validation for ABP (default: true)
- Valid range: 0 to 4,294,967,295

### Adaptive Data Rate (ADR)
- Network server automatically adjusts transmission parameters
- Reported values from device:
  - DataRate: Current data rate
  - TxPower: Current transmit power
  - NbRep: Number of repetitions
  - ReportedRX2DataRate: Actual RX2 data rate
  - ReportedRX1DROffset: Actual RX1 offset
  - ReportedRXDelay: Actual RX delay
- Optimizes battery life and network capacity

### Deduplication Modes
- **None**: No deduplication, process all messages
- **Drop**: Drop duplicate messages from multiple gateways
- **Mark**: Mark duplicates but don't drop
- Default: None
- Important for devices in range of multiple gateways

### Gateway Assignment
- GatewayID links device to specific LoRa concentrator
- Optional field for gateway-specific routing
- Gateway list retrieved via `/gateways` endpoint
- Used in multi-gateway deployments

### Sensor Decoder
- Custom decoder API URL for payload parsing
- Converts binary LoRa payload to JSON telemetry
- Decoder specific to device model capabilities
- Optional field

### Device Telemetry
- Stored in LoRaDeviceTelemetry collection
- Includes EnqueuedTime and LoRaTelemetry payload
- Processed from EventHub messages
- Limited to recent telemetry entries
- Accessible via telemetry endpoint

### Connection Management
- **KeepAliveTimeout**: Sliding expiration for IoT Hub connection
- **Downlink**: Enable/disable downlink messages (default: true)
- **AlreadyLoggedInOnce**: Tracks first successful join
- **IsConnected**: Current connection state from device twin
- **IsEnabled**: Device enabled state in IoT Hub

### Device Tags and Labels
- Tags: Custom metadata with name-value pairs
- Labels: Simple categorization markers
- Synchronized to device twin tags section
- Used for filtering and searching
- Managed through DeviceTagValue and Label entities

### Command Execution
- Execute LoRaWAN-specific commands on devices
- Commands defined in device model
- Sent as cloud-to-device (C2D) messages
- Requires device:execute permission
- Uses ILoRaWANCommandService

### CRUD Operations
- **Create**: Validates device ID format, creates device twin, stores entity
- **Read**: Retrieves from database with eager loading of tags/labels
- **Update**: Updates device twin and entity, replaces tags/labels
- **Delete**: Removes device twin, cascades to tags/labels/telemetry
- All operations unit-of-work transactional

### Device Twin Synchronization
- All LoRa settings stored in device twin desired properties
- Reported properties contain ADR values
- Tags contain device metadata (name, model, custom tags)
- SupportLoRaFeatures flag distinguishes from standard devices
- Automatic sync on create/update operations

### Pagination and Filtering
- Server-side pagination with configurable page size
- Filter by search text (name/ID)
- Filter by connection status (connected/disconnected)
- Filter by enabled state (enabled/disabled)
- Filter by device model
- Filter by custom tags
- Filter by labels
- Sorting support via orderBy parameter

### UI Edit/Create Flow
- Unified EditDevice component handles both standard and LoRa devices
- IsLoRa parameter controls LoRa-specific UI sections
- Tabbed interface for grouped settings
- Real-time validation feedback
- Save, Save and Duplicate, Save and Add New options
- Model selection with autocomplete
- Device image preview from model

### Error Handling
- ResourceNotFoundException for missing devices
- ValidationException for invalid inputs
- ProblemDetailsException handling in controllers
- DbUpdateException handling for database conflicts
- User-friendly error messages via Snackbar

---

## Notes

### Architecture Patterns
- **Generic Device Service Pattern** - IDeviceService<TDto> allows polymorphism
- **Repository Pattern** - Clean separation of data access
- **Unit of Work Pattern** - Transactional consistency
- **Device Twin Mapper Pattern** - Abstracts IoT Hub integration
- **Controller Inheritance** - DevicesControllerBase<T> for shared logic
- **DTO Pattern** - Data transfer with validation
- **Feature Filter Pattern** - Runtime feature toggling

### LoRaWAN Protocol Support
- Full LoRaWAN 1.0.x specification support
- OTAA and ABP authentication modes
- Class A/B/C device types
- Adaptive Data Rate (ADR) parameter management
- Frame counter security features
- Multi-gateway deduplication
- Bi-directional communication support

### DevEUI Validation
- 16 hexadecimal character requirement enforces LoRaWAN standard
- DevEUI is globally unique identifier per LoRaWAN specification
- Uppercase enforcement for consistency
- Prevents invalid device identifiers at API level

### OTAA vs ABP Trade-offs
- OTAA recommended for production: dynamic key negotiation, enhanced security
- ABP useful for testing: faster connection, simpler setup
- ABP requires careful frame counter management to prevent replay attacks
- OTAA requires AppEUI and AppKey; ABP requires DevAddr, AppSKey, NwkSKey

### Frame Counter Security
- Critical for preventing replay attacks in LoRaWAN
- ABPRelaxMode trades security for flexibility in testing
- 32-bit counters prevent counter exhaustion
- Reset counter allows controlled frame counter resets

### Multi-Gateway Considerations
- Deduplication modes handle messages received by multiple gateways
- Gateway assignment can force routing through specific gateway
- Telemetry processing handles duplicate messages
- Network coverage optimization through gateway distribution

### Telemetry Processing
- ProcessTelemetryEvent handles incoming messages from EventHub
- Filters by device scope (ignores non-device events)
- Deduplicates by sequence number
- Maintains limited telemetry history per device
- KeepOnlyLatestTelemetry prevents unbounded growth

### Integration with IoT Hub/AWS IoT Core
- Device twin desired properties store configuration
- Device twin reported properties store runtime values
- Device twin tags store metadata
- C2D messages for command execution
- D2C messages for telemetry
- Multi-cloud support via IExternalDeviceService abstraction

### Feature Toggle
- LoRaFeatureActiveFilter ensures LoRa endpoints only available when enabled
- Configuration-driven feature activation
- Prevents accidental LoRa operations in non-LoRa deployments
- Returns 404 when feature disabled

### Database Schema
- LorawanDevice inherits from Device (TPH - Table Per Hierarchy)
- LoRaDeviceTelemetry separate table with foreign key
- Cascade delete configured for telemetry on device delete
- DeviceTagValue and Label many-to-many relationships

### Performance Considerations
- Eager loading of Tags and Labels prevents N+1 queries
- Telemetry history limited to recent entries
- Pagination on device list endpoints
- Device twin operations cached when possible
- Efficient repository queries using Include()

### Security Considerations
- Authentication keys (AppKey, AppSKey, NwkSKey) stored securely
- Authorization required at controller level
- Permission-based access control
- Input validation prevents injection attacks
- Frame counters prevent replay attacks
- Device twin access controlled by IoT Hub policies

### Testing Coverage
- Unit tests: LoRaWanDeviceServiceTests.cs
- Controller tests: LoRaWANDevicesControllerTests.cs
- Client service tests: LoRaWanDeviceClientServiceTests.cs
- Mapper tests: LoRaDeviceTwinMapperTests.cs
- Integration tests likely cover end-to-end scenarios

### Known Limitations
- Telemetry history limited (not configurable)
- No bulk device operations (create/update multiple)
- No device provisioning service integration
- No LoRa network server direct integration (relies on IoT Hub)
- Gateway list static configuration (not dynamic discovery)
- Sensor decoder URL must be manually configured (no registry)

### Future Enhancement Opportunities
- Bulk device import/export with CSV
- Device provisioning service integration for OTAA
- LoRa network server webhook integration
- Device telemetry visualization dashboard
- Gateway health monitoring and alerts
- Sensor decoder registry with versioning
- Device firmware update management
- LoRaWAN MAC command support
- Multicast group management
- Device geolocation from gateway metadata
- Advanced ADR algorithm configuration
- Device battery level monitoring
- Historical telemetry analytics
- Device migration between gateways
- LoRaWAN compliance testing tools

### Related LoRaWAN Features
- **Concentrator Management**: LoRa gateways/concentrators that route device messages
- **LoRaWAN Device Models**: Device templates with command definitions
- **LoRaWAN Frequency Plans**: Regional frequency configurations
- **LoRaWAN Commands**: C2D command definitions and execution

### LoRa-Specific UI Sections
- **OTAA Tab**: AppEUI and AppKey configuration
- **ABP Tab**: DevAddr, AppSKey, NwkSKey configuration
- **LoRaWAN Settings Tab**: Class type, preferred window, deduplication
- **Receive Window Tab**: RX1/RX2 configuration, offsets, delays
- **Frame Counter Tab**: FCnt start values, reset counter, 32-bit support
- **General Tab**: Gateway, sensor decoder, downlink, keep-alive timeout

### Default Values
- ClassType: A (lowest power)
- PreferredWindow: 1 (RX1)
- Deduplication: None
- Downlink: true
- ABPRelaxMode: true
- Supports32BitFCnt: true
- FCntUpStart: 0
- FCntDownStart: 0
- FCntResetCounter: 0
- UseOTAA: true

### Validation Rules
- DeviceID: Required, 16 hexadecimal characters uppercase
- DeviceName: Required
- ModelId: Required
- AppKey/AppEUI: Required if UseOTAA=true (conditional validation)
- DevAddr/AppSKey/NwkSKey: Required if UseOTAA=false (conditional validation)
- Frame counters: Range 0-4294967295
- PreferredWindow: 1 or 2

### Client-Side State Management
- EditDevice component manages form state
- Device model selection triggers image update
- UseOTAA toggle controls OTAA/ABP section visibility
- Form validation tracks field-level errors
- Loading states during async operations
- Duplicate device mode for cloning

### Server-Side Processing
- Validation via data annotations and Fluent Validation
- Transaction management via Unit of Work
- Cascade deletes handled by repository layer
- Device twin updates wrapped in try-catch
- Telemetry processing idempotent

### Multi-Cloud Abstraction
- IExternalDeviceService abstracts cloud provider
- Azure IoT Hub implementation via Twin operations
- AWS IoT Core support via Thing Shadow
- Cloud-agnostic device entity model
- Provider-specific twin mapper implementations
