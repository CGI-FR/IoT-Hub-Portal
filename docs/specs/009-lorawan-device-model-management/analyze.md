# Feature: LoRaWAN Device Model Management

**Category**: Device Management / LoRaWAN  
**Status**: Analyzed  

---

## Description

The LoRaWAN Device Model Management feature provides comprehensive CRUD (Create, Read, Update, Delete) operations for LoRaWAN-specific device model templates in the portal. This feature extends the standard device model management with LoRaWAN protocol-specific configurations and capabilities. The feature includes:

- Paginated LoRaWAN device model listing with search and filtering capabilities (by name and description)
- Device model creation with LoRaWAN-specific protocol settings (OTAA/ABP, class types, deduplication, etc.)
- Device model editing with LoRaWAN properties, receive window configuration, and frame counter management
- Device model deletion with validation to prevent deletion if models are in use by devices
- Device model avatar/image management (upload, change, delete)
- LoRaWAN-specific command management for device models (confirmed/unconfirmed commands with ports and frames)
- Label management for device model categorization
- Integration with Azure IoT Hub or AWS IoT Core for enrollment group and configuration management
- Support for built-in models that cannot be edited or deleted
- LoRaWAN protocol configuration including:
  - OTAA (Over-The-Air Activation) and ABP (Activation By Personalization) authentication modes
  - Device class types (A, B, C) for different communication patterns
  - Receive window settings (RX1, RX2) for downlink communication
  - Frame counter management (FCntUpStart, FCntDownStart, FCntResetCounter)
  - Message deduplication strategies
  - Sensor decoder URL configuration for payload processing
  - Connection timeout and keep-alive settings

This feature serves as the foundation for LoRaWAN device lifecycle management, providing business value through:
- Standardized LoRaWAN device templates for consistent device provisioning
- Reusable LoRaWAN-specific configurations across multiple devices
- Centralized LoRaWAN protocol parameter management
- Simplified LoRaWAN device onboarding through model-based configuration
- Support for both OTAA and ABP authentication strategies
- Command management for remote device control

---

## Code Locations

### Entry Points / Endpoints
- `src/IoTHub.Portal.Server/Controllers/v1.0/LoRaWAN/LoRaWANDeviceModelsController.cs` (Lines 1-138)
  - **Snippet**: Main REST API controller inheriting from DeviceModelsControllerBase
    ```csharp
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/lorawan/models")]
    [ApiExplorerSettings(GroupName = "LoRa WAN")]
    [LoRaFeatureActiveFilter]
    public class LoRaWANDeviceModelsController : DeviceModelsControllerBase<DeviceModelDto, LoRaDeviceModelDto>
    {
        [HttpGet(Name = "GET LoRaWAN device model list")]
        [Authorize("model:read")]
        public override async Task<ActionResult<PaginationResult<DeviceModelDto>>> GetItems([FromQuery] DeviceModelFilter deviceModelFilter)
        // Returns only models with SupportLoRaFeatures = true
        
        [HttpGet("{id}", Name = "GET LoRaWAN device model")]
        [Authorize("model:read")]
        public override Task<ActionResult<LoRaDeviceModelDto>> GetItem(string id)
        
        [HttpGet("{id}/avatar", Name = "GET LoRaWAN device model avatar URL")]
        [Authorize("model:read")]
        public override Task<ActionResult<string>> GetAvatar(string id)
        
        [HttpPost("{id}/avatar", Name = "POST Update the LoRaWAN device model avatar")]
        [Authorize("model:write")]
        public override Task<ActionResult<string>> ChangeAvatar(string id, string avatar)
        
        [HttpDelete("{id}/avatar", Name = "DELETE Remove the LoRaWAN device model avatar")]
        [Authorize("model:write")]
        public override Task<IActionResult> DeleteAvatar(string id)
        
        [HttpPost(Name = "POST Create a new LoRaWAN device model")]
        [Authorize("model:write")]
        public override Task<IActionResult> Post(LoRaDeviceModelDto deviceModelDto)
        
        [HttpPut(Name = "PUT Update the LoRaWAN device model")]
        [Authorize("model:write")]
        public override Task<IActionResult> Put(LoRaDeviceModelDto deviceModelDto)
        
        [HttpDelete("{id}", Name = "DELETE Remove the LoRaWAN device model")]
        [Authorize("model:write")]
        public override Task<IActionResult> Delete(string id)
    }
    ```

- `src/IoTHub.Portal.Server/Controllers/v1.0/LoRaWAN/LoRaWANCommandsController.cs` (Lines 1-65)
  - **Snippet**: Controller for LoRaWAN device model command management
    ```csharp
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/lorawan/models/{id}/commands")]
    [ApiExplorerSettings(GroupName = "LoRa WAN")]
    [LoRaFeatureActiveFilter]
    public class LoRaWANCommandsController : ControllerBase
    {
        [HttpGet(Name = "GET Device model commands")]
        [Authorize("model:read")]
        public async Task<ActionResult<DeviceModelCommandDto[]>> Get(string id)
        
        [HttpPost(Name = "POST Set device model commands")]
        [Authorize("model:write")]
        public async Task<IActionResult> Post(string id, DeviceModelCommandDto[] commands)
    }
    ```

- `src/IoTHub.Portal.Server/Controllers/v1.0/DeviceModelControllerBase.cs` (Lines 1-138)
  - Base controller implementation with shared logic for device model operations
  - Generic controller supporting both standard and LoRaWAN models

### Business Logic
- `src/IoTHub.Portal.Application/Services/IDeviceModelService.cs` (Lines 1-26)
  - **Snippet**: Core service interface
    ```csharp
    public interface IDeviceModelService<TListItem, TModel>
        where TListItem : class, IDeviceModel
        where TModel : class, IDeviceModel
    {
        Task<PaginatedResult<DeviceModelDto>> GetDeviceModels(DeviceModelFilter deviceModelFilter);
        Task<TModel> GetDeviceModel(string deviceModelId);
        Task<TModel> CreateDeviceModel(TModel deviceModel);
        Task UpdateDeviceModel(TModel deviceModel);
        Task DeleteDeviceModel(string deviceModelId);
        Task<string> GetDeviceModelAvatar(string deviceModelId);
        Task<string> UpdateDeviceModelAvatar(string deviceModelId, string avatar);
        Task DeleteDeviceModelAvatar(string deviceModelId);
    }
    ```

- `src/IoTHub.Portal.Server/Services/DeviceModelService.cs` (Lines 1-190)
  - Concrete generic implementation for device models
  - Handles database operations for device model CRUD
  - Manages device model labels and images
  - Creates enrollment groups and configurations in IoT Hub/AWS
  - Validates device model deletion against existing devices
  - Supports both standard and LoRaWAN device models through generic type parameters

- `src/IoTHub.Portal.Application/Services/ILoRaWANCommandService.cs`
  - Service interface for LoRaWAN command management
  - Methods: GetDeviceModelCommandsFromModel, PostDeviceModelCommands

### Data Mappers
- `src/IoTHub.Portal.Infrastructure/Mappers/IDeviceModelMapper.cs` (Lines 1-16)
  - **Snippet**: Generic device model mapper interface
    ```csharp
    public interface IDeviceModelMapper<TListItem, TModel>
    {
        public TListItem CreateDeviceModelListItem(TableEntity entity);
        public TModel CreateDeviceModel(TableEntity entity);
        public Dictionary<string, object> BuildDeviceModelDesiredProperties(TModel model);
    }
    ```

- `src/IoTHub.Portal.Infrastructure/Mappers/LoRaDeviceModelMapper.cs` (Lines 1-79)
  - Concrete implementation for LoRaWAN device models
  - Maps TableEntity to LoRaDeviceModelDto with all LoRaWAN-specific properties
  - Builds device twin desired properties from LoRaWAN model configuration
  - Handles all LoRaWAN protocol parameters (OTAA, ClassType, Deduplication, etc.)

### Data Access
- `src/IoTHub.Portal.Domain/Repositories/IDeviceModelRepository.cs` (Lines 1-10)
  - Generic repository interface for DeviceModel entity
  - Methods: GetByNameAsync for model lookup by name

- `src/IoTHub.Portal.Domain/Repositories/IDeviceModelCommandRepository.cs`
  - Repository interface for DeviceModelCommand entity
  - Handles LoRaWAN command persistence

- `src/IoTHub.Portal.Domain/Entities/DeviceModel.cs` (Lines 1-41)
  - **Snippet**: Core device model entity with LoRaWAN support
    ```csharp
    public class DeviceModel : EntityBase
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsBuiltin { get; set; }
        public bool SupportLoRaFeatures { get; set; }
        
        // LoRaWAN-specific properties
        public bool? UseOTAA { get; set; }
        public int? PreferredWindow { get; set; }
        public DeduplicationMode? Deduplication { get; set; }
        public ClassType ClassType { get; set; }
        public bool? ABPRelaxMode { get; set; }
        public bool? Downlink { get; set; }
        public int? KeepAliveTimeout { get; set; }
        public int? RXDelay { get; set; }
        public string? SensorDecoder { get; set; }
        public string? AppEUI { get; set; }
        
        public ICollection<Label> Labels { get; set; }
    }
    ```

- `src/IoTHub.Portal.Domain/Entities/DeviceModelCommand.cs` (Lines 1-20)
  - **Snippet**: LoRaWAN command entity
    ```csharp
    public class DeviceModelCommand : EntityBase
    {
        public string Name { get; set; }
        public string Frame { get; set; }
        public bool Confirmed { get; set; }
        public int Port { get; set; } = 1;
        public bool IsBuiltin { get; set; }
        public string DeviceModelId { get; set; }
    }
    ```

### UI Components
- `src/IoTHub.Portal.Client/Pages/DeviceModels/DeviceModelListPage.razor` (Lines 1-150+)
  - Main device model listing page shared by both standard and LoRaWAN models
  - Server-side paginated table with sorting
  - Implements search by name and description
  - Conditional rendering based on permissions (model:read, model:write)
  - Shows label badges for each model
  - Prevents deletion of built-in models

- `src/IoTHub.Portal.Client/Components/DeviceModels/LoRaWAN/CreateLoraDeviceModel.razor` (Lines 1-50+)
  - LoRaWAN-specific device model creation component
  - Contains expansion panels for LoRaWAN protocol configuration
  - Fields for OTAA/ABP settings, class type, deduplication, sensor decoder
  - Connection timeout configuration

- `src/IoTHub.Portal.Client/Components/DeviceModels/LoRaWAN/EditLoraDeviceModel.razor` (Lines 1-100+)
  - LoRaWAN-specific device model editing component
  - Comprehensive LoRaWAN configuration UI:
    - OTAA/ABP toggle with ABP relax mode option
    - Class type selection (A, B, C)
    - Message deduplication mode
    - Sensor decoder URL configuration
    - Device connection timeout
    - Receive windows configuration (downlink support, preferred window, RX delays)
    - RX1 datarate offset and RX2 datarate settings
  - Uses MudBlazor expansion panels for organized configuration sections

- `src/IoTHub.Portal.Client/Components/DeviceModels/DeviceModelSearch.razor`
  - Search component for filtering device models by name/description

### Data Transfer Objects
- `src/IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/LoRaDeviceModelDto.cs` (Lines 1-89)
  - **Snippet**: Complete LoRaWAN device model DTO
    ```csharp
    public class LoRaDeviceModelDto : LoRaDeviceBase, IDeviceModel
    {
        public string ModelId { get; set; }
        public string Image { get; set; }
        [Required(ErrorMessage = "The device model name is required.")]
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsBuiltin { get; set; }
        public bool SupportLoRaFeatures { get; } = true;
        
        [DefaultValue(true)]
        public bool UseOTAA { get; set; }
        
        [DefaultValue(true)]
        public bool? Downlink { get; set; }
        
        public List<LabelDto> Labels { get; set; }
        
        // Inherits from LoRaDeviceBase:
        // ClassType, PreferredWindow, Deduplication, RX1DROffset, RX2DataRate,
        // RXDelay, ABPRelaxMode, FCntUpStart, FCntDownStart, FCntResetCounter,
        // Supports32BitFCnt, KeepAliveTimeout, SensorDecoder
    }
    ```

- `src/IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/LoRaDeviceBase.cs` (Lines 1-100)
  - Abstract base class containing common LoRaWAN properties
  - Properties with defaults: ClassType (A), PreferredWindow (1), Deduplication (Drop), ABPRelaxMode (true)
  - Optional properties: RX1DROffset, RX2DataRate, RXDelay, FCntUpStart, FCntDownStart, FCntResetCounter, Supports32BitFCnt, KeepAliveTimeout
  - SensorDecoder URL for payload processing

- `src/IoTHub.Portal.Shared/Models/v1.0/DeviceModelDto.cs` (Lines 1-54)
  - Standard device model DTO
  - Has SupportLoRaFeatures flag that determines LoRaWAN capability

- `src/IoTHub.Portal.Shared/Models/v1.0/LoRaWAN/DeviceModelCommandDto.cs`
  - DTO for LoRaWAN device commands
  - Properties: Id, Name, Frame, Confirmed, Port, IsBuiltin, DeviceModelId

### Client Services
- `src/IoTHub.Portal.Client/Services/IDeviceModelsClientService.cs` (Lines 1-26)
  - Client-side service interface for HTTP API calls
  - Methods: GetDeviceModelsAsync, GetDeviceModel, CreateDeviceModelAsync, UpdateDeviceModel, DeleteDeviceModel, GetDeviceModelModelPropertiesAsync, SetDeviceModelModelProperties, GetAvatar, ChangeAvatarAsync

- `src/IoTHub.Portal.Client/Services/DeviceModelsClientService.cs`
  - Concrete implementation of client service
  - Makes HTTP requests to device model endpoints
  - Handles avatar upload/download

---

## API Endpoints

### Device Model Management
- `GET /api/lorawan/models` - Get paginated LoRaWAN device model list
  - Query parameters: searchText, pageSize, pageNumber, orderBy[]
  - Returns: PaginationResult<DeviceModelDto> filtered to SupportLoRaFeatures = true
  - Authorization: model:read
  - Filter: LoRaFeatureActiveFilter (requires LoRaWAN feature to be enabled)

- `GET /api/lorawan/models/{id}` - Get LoRaWAN device model details
  - Returns: LoRaDeviceModelDto with all LoRaWAN-specific properties
  - Authorization: model:read
  - Filter: LoRaFeatureActiveFilter

- `POST /api/lorawan/models` - Create new LoRaWAN device model
  - Body: LoRaDeviceModelDto
  - Returns: LoRaDeviceModelDto
  - Authorization: model:write
  - Filter: LoRaFeatureActiveFilter
  - Creates device model in database
  - Creates enrollment group in IoT Hub/AWS
  - Rolls out device model configuration with LoRaWAN desired properties
  - Sets default model image

- `PUT /api/lorawan/models` - Update LoRaWAN device model
  - Body: LoRaDeviceModelDto
  - Returns: 200 OK
  - Authorization: model:write
  - Filter: LoRaFeatureActiveFilter
  - Updates device model in database
  - Recreates enrollment group and configuration with updated properties

- `DELETE /api/lorawan/models/{id}` - Delete LoRaWAN device model
  - Returns: 204 No Content
  - Authorization: model:write
  - Filter: LoRaFeatureActiveFilter
  - Validates no devices are using this model
  - Deletes enrollment group from IoT Hub/AWS
  - Deletes associated configurations
  - Deletes associated commands
  - Deletes model image
  - Deletes labels

### Device Model Avatar Management
- `GET /api/lorawan/models/{id}/avatar` - Get LoRaWAN device model avatar URL
  - Returns: string (image URL)
  - Authorization: model:read
  - Filter: LoRaFeatureActiveFilter

- `POST /api/lorawan/models/{id}/avatar` - Update LoRaWAN device model avatar
  - Body: string (base64 encoded image or URL)
  - Returns: string (new image URL)
  - Authorization: model:write
  - Filter: LoRaFeatureActiveFilter

- `DELETE /api/lorawan/models/{id}/avatar` - Remove LoRaWAN device model avatar
  - Returns: 204 No Content
  - Authorization: model:write
  - Filter: LoRaFeatureActiveFilter
  - Resets to default model image

### LoRaWAN Command Management
- `GET /api/lorawan/models/{id}/commands` - Get device model commands
  - Returns: DeviceModelCommandDto[] (array of commands)
  - Authorization: model:read
  - Filter: LoRaFeatureActiveFilter

- `POST /api/lorawan/models/{id}/commands` - Set device model commands
  - Body: DeviceModelCommandDto[] (array of commands)
  - Returns: 200 OK
  - Authorization: model:write
  - Filter: LoRaFeatureActiveFilter
  - Replaces all existing commands for the model

---

## Authorization

### Required Permissions
- **model:read** - View LoRaWAN device model list, details, properties, commands, and avatars
- **model:write** - Create, update, delete LoRaWAN device models, modify properties/commands, and manage avatars

### Authorization Implementation
- Attribute-based authorization using `[Authorize("permission")]` attributes on controller methods
- Permission checks in UI components using `HasPermissionAsync(PortalPermissions.*)` for conditional rendering
- Base authorization requirement: `[Authorize]` on LoRaWANDeviceModelsController
- Feature gate: `[LoRaFeatureActiveFilter]` ensures LoRaWAN functionality is enabled in portal settings

---

## Dependencies

### Internal Feature Dependencies
- **Device Model Management (Standard)** - Inherits from base device model controller and service
- **Label Management** - Device model categorization and filtering system
- **LoRaWAN Device Management** - LoRaWAN devices must be associated with a LoRaWAN device model
- **Device Model Properties Management** - Shared property management for device models

### Service Dependencies
- `IDeviceModelService<TListItem, TModel>` - Generic device model service implementation
- `ILoRaWANCommandService` - LoRaWAN command management
- `IDeviceModelImageManager` - Device model image/avatar management
- `IDeviceModelMapper<TListItem, TModel>` - Mapping device models to/from storage and building desired properties
- `IExternalDeviceService` - IoT Hub/AWS device operations for validating device usage
- `IDeviceRegistryProvider` - Creates/deletes enrollment groups for LoRaWAN models
- `IConfigService` - Rolls out device model configurations with LoRaWAN properties
- `IDeviceModelCommandRepository` - LoRaWAN command persistence
- `ILabelRepository` - Label persistence

### External Dependencies
- **Azure IoT Hub** or **AWS IoT Core** - Cloud IoT device management service
  - Enrollment group management for OTAA/ABP provisioning
  - Device configuration management with LoRaWAN desired properties
- **Entity Framework Core** - Database access via PortalDbContext
- **AutoMapper** - Entity to DTO mapping

### UI Dependencies
- **MudBlazor** - UI component library (MudTable, MudForm, MudTextField, MudSwitch, MudSelect, MudExpansionPanel, etc.)
- Device Model Search component
- Label display component

---

## Key Features & Behaviors

### LoRaWAN Protocol Configuration
- **OTAA (Over-The-Air Activation)** - Secure device activation using network/app keys
  - Default authentication mode (UseOTAA = true)
  - Supports frame counter management
  - Supports RX window configuration
  
- **ABP (Activation By Personalization)** - Pre-provisioned device activation
  - ABP relax mode option (enabled by default)
  - Frame counter initialization (FCntUpStart, FCntDownStart)
  - Frame counter reset support

- **Device Class Types**
  - Class A: Bi-directional end-devices (default)
  - Class B: Bi-directional end-devices with scheduled receive slots
  - Class C: Bi-directional end-devices with maximal receive slots

- **Message Deduplication**
  - None: No deduplication
  - Drop: Drop duplicate messages
  - Mark: Mark duplicate messages
  - Controls handling of messages received by multiple gateways

### Receive Window Configuration
- **Downlink Support** - Enable/disable cloud-to-device messages (default: enabled)
- **Preferred Window** - RX1 (1) or RX2 (2) for downlink (default: 1)
- **RX Delays** - Custom wait time between receive and transmit
- **RX1 Datarate Offset** - Offset between received and retransmit datarate (OTAA only)
- **RX2 Datarate** - Custom datarate for second receive window (OTAA only)

### Frame Counter Management
- **FCntUpStart** - Uplink frame counter start value (0-4294967295, default: 0)
- **FCntDownStart** - Downlink frame counter start value (0-4294967295, default: 0)
- **FCntResetCounter** - Reset frame counters to start values (0-4294967295, default: 0)
- **Supports32BitFCnt** - Enable 32-bit frame counters (default: true)

### Sensor Decoder
- URL configuration for custom payload decoder
- Allows processing of LoRaWAN device telemetry before storage
- Decoder receives raw frame and returns decoded JSON

### Connection Management
- **KeepAliveTimeout** - Sliding expiration for device connection to IoT/Edge Hub
- Optional configuration (default: none, connection not dropped)

### Command Management
- Define custom commands for LoRaWAN devices
- Command properties:
  - Name: Command identifier
  - Frame: Payload frame (hex-encoded)
  - Confirmed: Whether device must acknowledge command
  - Port: LoRaWAN port for command transmission (default: 1)
  - IsBuiltin: Whether command is system-defined and protected
- Commands stored per device model
- Commands inherited by all devices using the model

### Search and Filtering
- Full-text search by device model name or description
- Filter to show only models with SupportLoRaFeatures = true
- Sorting support on multiple columns
- Label-based filtering

### Pagination
- Server-side pagination with configurable page size
- Uses cursor-based navigation with nextPage URL

### Validation
- Device model name required
- OTAA/ABP setting immutable after creation (toggle disabled in edit UI)
- Built-in models cannot be deleted or edited
- Prevents deletion if model is in use by any device
- Frame counter range validation (0-4294967295)

### Image Management
- Upload custom avatar/image for device model
- Change existing avatar
- Delete avatar (resets to default)
- Images stored and served via IDeviceModelImageManager

---

## Notes

### Architecture Patterns
- **Generic Controller Base** - DeviceModelsControllerBase<TListItem, TModel> allows polymorphic handling of standard and LoRaWAN models
- **Generic Service Layer** - IDeviceModelService<TListItem, TModel> enables type-safe model management
- **Mapper Pattern** - IDeviceModelMapper<TListItem, TModel> abstracts model-to-storage and model-to-twin conversions
- **Repository Pattern** - Clean separation of data access concerns
- **Feature Filter** - LoRaFeatureActiveFilter ensures LoRaWAN endpoints only available when feature is enabled

### LoRaWAN Specifics
- LoRaDeviceModelDto inherits from LoRaDeviceBase to share common properties with LoRaWAN devices
- SupportLoRaFeatures flag differentiates LoRaWAN models from standard models
- Device model configurations are translated to IoT Hub device twin desired properties
- Enrollment groups enable automatic provisioning of LoRaWAN devices using the model
- All LoRaWAN-specific properties are optional with sensible defaults

### Multi-Cloud Support
- Supports both Azure IoT Hub and AWS IoT Core
- Cloud provider selection via PortalSettings.CloudProvider
- Provider-specific implementations of IDeviceRegistryProvider and IConfigService
- Enrollment groups work differently but are abstracted by the provider interface

### Performance Considerations
- Server-side pagination reduces data transfer
- Lazy loading with Include() for related entities (Labels)
- Image URLs retrieved asynchronously
- Query optimization with predicate building
- LoRaWAN models filtered at database level using SupportLoRaFeatures flag

### Security Considerations
- Comprehensive authorization checks at controller and UI levels
- Feature gate prevents unauthorized access to LoRaWAN functionality
- Built-in model protection prevents accidental deletion/modification
- Device usage validation prevents orphaned device references
- Input validation on all user-submitted data
- XSS protection through Blazor's automatic encoding

### Testing Coverage
- Unit tests exist for controllers, services, mappers, and UI components
- Test files: LoRaWANDeviceModelsControllerTest.cs, DeviceModelServiceTests.cs, LoRaDeviceModelMapperTests.cs, etc.
- LoRaWAN command controller tests: LoRaWANCommandsControllerTests.cs

### Future Enhancement Opportunities
- Visual command builder for creating LoRaWAN frames
- Command templates/library for common device types
- Validation of LoRaWAN frame format (hex encoding)
- Import/export of device models with commands
- Device model versioning and migration
- Real-time validation of LoRaWAN parameters against regional specifications
- Multi-region support with frequency plan integration
- Device model cloning functionality
- Command scheduling and automation
- Integration with LoRaWAN network server for advanced features
- Support for LoRaWAN 1.1 features (separate network/application session keys)
