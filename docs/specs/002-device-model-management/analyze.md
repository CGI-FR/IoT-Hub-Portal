# Feature: Device Model Management

**Category**: Device Management  
**Status**: Analyzed  

---

## Description

The Device Model Management feature provides comprehensive CRUD (Create, Read, Update, Delete) operations for device model templates in the portal. Device models serve as blueprints for IoT devices, defining their properties, behaviors, and metadata. The feature includes:

- Paginated device model listing with search and filtering capabilities (by name and description)
- Device model creation with support for both standard and LoRaWAN device types
- Device model editing with real-time property and command management
- Device model deletion with validation to prevent deletion if models are in use by devices
- Device model avatar/image management (upload, change, delete)
- Device model property management (define properties with types, display names, order, and writability)
- Device model command management for LoRaWAN devices
- Label management for device model categorization
- Integration with Azure IoT Hub or AWS IoT Core for enrollment group and configuration management
- Support for built-in models that cannot be edited or deleted
- Automatic enrollment group creation in IoT Hub/AWS when creating device models

This feature serves as the foundation for device lifecycle management, providing business value through:
- Standardized device templates for consistent device provisioning
- Reusable device configurations across multiple devices
- Centralized property definition and management
- Simplified device onboarding through model-based configuration
- Support for both standard IoT and LoRaWAN device types

---

## Code Locations

### Entry Points / Endpoints
- `src/IoTHub.Portal.Server/Controllers/v1.0/DeviceModelsController.cs` (Lines 1-133)
  - **Snippet**: Main REST API controller inheriting from DeviceModelsControllerBase
    ```csharp
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/models")]
    [ApiExplorerSettings(GroupName = "Device Models")]
    public class DeviceModelsController : DeviceModelsControllerBase<DeviceModelDto, DeviceModelDto>
    {
        [HttpGet(Name = "GET Device model list")]
        [Authorize("model:read")]
        public override async Task<ActionResult<PaginationResult<DeviceModelDto>>> GetItems([FromQuery] DeviceModelFilter deviceModelFilter)
        
        [HttpGet("{id}", Name = "GET Device model")]
        [Authorize("model:read")]
        public override Task<ActionResult<DeviceModelDto>> GetItem(string id)
        
        [HttpGet("{id}/avatar", Name = "GET Device model avatar URL")]
        [Authorize("model:read")]
        public override Task<ActionResult<string>> GetAvatar(string id)
        
        [HttpPost("{id}/avatar", Name = "POST Update the device model avatar")]
        [Authorize("model:write")]
        public override async Task<ActionResult<string>> ChangeAvatar(string id, string avatar)
        
        [HttpDelete("{id}/avatar", Name = "DELETE Remove the device model avatar")]
        [Authorize("model:write")]
        public override Task<IActionResult> DeleteAvatar(string id)
        
        [HttpPost(Name = "POST Create a new device model")]
        [Authorize("model:write")]
        public override Task<IActionResult> Post(DeviceModelDto deviceModel)
        
        [HttpPut(Name = "PUT Update the device model")]
        [Authorize("model:write")]
        public override Task<IActionResult> Put(DeviceModelDto deviceModel)
        
        [HttpDelete("{id}", Name = "DELETE Remove the device model")]
        [Authorize("model:write")]
        public override Task<IActionResult> Delete(string id)
    }
    ```

- `src/IoTHub.Portal.Server/Controllers/v1.0/DeviceModelControllerBase.cs` (Lines 1-138)
  - Base controller implementation with shared logic for device model operations
  - Generic controller supporting both standard and LoRaWAN models

- `src/IoTHub.Portal.Server/Controllers/v1.0/DeviceModelPropertiesController.cs` (Lines 1-52)
  - **Snippet**: Controller for device model property management
    ```csharp
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/models/{id}/properties")]
    [ApiExplorerSettings(GroupName = "Device Models")]
    public class DeviceModelPropertiesController : DeviceModelPropertiesControllerBase
    {
        [HttpGet(Name = "GET Device model properties")]
        [Authorize("model:read")]
        public override async Task<ActionResult<IEnumerable<DeviceProperty>>> GetProperties(string id)
        
        [HttpPost(Name = "POST Device model properties")]
        [Authorize("model:write")]
        public override async Task<ActionResult> SetProperties(string id, IEnumerable<DeviceProperty> properties)
    }
    ```

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
  - Concrete implementation for device model operations
  - Handles database operations for device model CRUD
  - Manages device model properties, commands, and labels
  - Coordinates with IoT Hub for enrollment group creation
  - Validates device model usage before deletion
  - **Key Methods**:
    - `GetDeviceModels`: Retrieves paginated list with search filtering
    - `GetDeviceModel`: Gets single model with labels
    - `CreateDeviceModel`: Creates model, sets default image, creates IoT Hub configuration
    - `UpdateDeviceModel`: Updates model and labels, recreates IoT Hub configuration
    - `DeleteDeviceModel`: Validates no devices use model, deletes enrollment groups, configurations, commands, labels, and image
    - Avatar management methods

- `src/IoTHub.Portal.Application/Services/IDeviceModelPropertiesService.cs` (Lines 1-13)
  - Service interface for device model property management
  - Methods: GetModelProperties, SavePropertiesForModel, GetAllPropertiesNames

- `src/IoTHub.Portal.Infrastructure/Services/DeviceModelPropertiesService.cs`
  - Manages device model properties stored in portal database
  - Handles property CRUD operations

### Data Access
- `src/IoTHub.Portal.Domain/Repositories/IDeviceModelRepository.cs` (Lines 1-10)
  - Generic repository interface for DeviceModel entity
  - Method: GetByNameAsync for name-based lookup

- `src/IoTHub.Portal.Domain/Entities/DeviceModel.cs` (Lines 1-41)
  - **Snippet**: Core device model entity
    ```csharp
    public class DeviceModel : EntityBase
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsBuiltin { get; set; }
        public bool SupportLoRaFeatures { get; set; }
        
        // LoRaWAN specific properties
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
        
        // Navigation properties
        public ICollection<Label> Labels { get; set; } = default!;
    }
    ```

- `src/IoTHub.Portal.Domain/Entities/DeviceModelProperty.cs` (Lines 1-45)
  - **Snippet**: Device model property entity
    ```csharp
    public class DeviceModelProperty : EntityBase
    {
        [Required]
        public string Name { get; set; } = default!;
        
        public string DisplayName { get; set; } = default!;
        
        [Required]
        public bool IsWritable { get; set; }
        
        [Required]
        public int Order { get; set; }
        
        [Required]
        public DevicePropertyType PropertyType { get; set; }
        
        [Required]
        public string ModelId { get; set; } = default!;
    }
    ```

- `src/IoTHub.Portal.Domain/Entities/DeviceModelCommand.cs` (Lines 1-20)
  - Device model command entity for LoRaWAN devices
  - Properties: Name, Frame, Confirmed, Port, IsBuiltin, DeviceModelId

### UI Components
- `src/IoTHub.Portal.Client/Pages/DeviceModels/DeviceModelListPage.razor` (Lines 1-211)
  - Main device model listing page with search panel
  - Server-side paginated table with sorting
  - Implements search by name and description
  - Displays model image, name, description, and labels
  - Conditional rendering based on permissions (model:read, model:write)
  - Built-in models cannot be deleted
  - Table columns: Image, Name, Description, Details button, Delete button

- `src/IoTHub.Portal.Client/Pages/DeviceModels/DeviceModelDetailPage.razor` (Lines 1-411)
  - Device model detail view and editor
  - Supports both standard and LoRaWAN device models
  - Tabs: General (details, properties, labels), LoRaWAN (LoRa-specific configuration)
  - Avatar upload/delete functionality with image preview
  - Property management: add, edit, remove properties with validation
  - Command management for LoRaWAN models
  - Label editor integration
  - Built-in models have edit restrictions
  - Save and delete operations with validation
  - Permission checks for model:read, model:write

- `src/IoTHub.Portal.Client/Pages/DeviceModels/CreateDeviceModelPage.razor` (Lines 1-357)
  - Device model creation page
  - Similar structure to detail page but for new models
  - Toggle for LoRaWAN support (when portal has LoRa enabled)
  - Property and command management
  - Avatar upload functionality
  - Form validation before submission
  - Requires model:write permission

- `src/IoTHub.Portal.Client/Pages/DeviceModels/DeleteDeviceModelPage.razor` (Lines 1-44)
  - Confirmation dialog for device model deletion
  - Displays warning about irreversibility
  - Simple modal with cancel and delete actions

- `src/IoTHub.Portal.Client/Components/DeviceModels/DeviceModelSearch.razor` (Lines 1-39)
  - Search panel component with expansion panel
  - Single text field for searching name or description
  - Search and reset buttons

### Data Transfer Objects
- `src/IoTHub.Portal.Shared/Models/v1.0/DeviceModelDto.cs` (Lines 1-54)
  - Complete device model DTO with validation attributes
  - **Snippet**:
    ```csharp
    public class DeviceModelDto : IDeviceModel
    {
        public string ModelId { get; set; } = default!;
        public string Image { get; set; } = default!;
        
        [Required(ErrorMessage = "The device model name is required.")]
        public string Name { get; set; } = default!;
        
        public string Description { get; set; } = default!;
        public bool IsBuiltin { get; set; }
        public bool SupportLoRaFeatures { get; set; }
        public List<LabelDto> Labels { get; set; } = new();
    }
    ```

- `src/IoTHub.Portal.Shared/Models/v1.0/DeviceProperty.cs` (Lines 1-45)
  - Device property DTO for model properties
  - **Snippet**:
    ```csharp
    public class DeviceProperty
    {
        [Required(ErrorMessage = "The property name is required.")]
        [RegularExpression(@"^([\w]+\.)+[\w]+|[\w]+$", ErrorMessage = "Property name must be formed by a word or words separated by a dot")]
        public string Name { get; set; } = default!;
        
        [Required(ErrorMessage = "The display name is required.")]
        public string DisplayName { get; set; } = default!;
        
        [Required(ErrorMessage = "The property should indicate whether it's writable or not.")]
        public bool IsWritable { get; set; }
        
        [Required(ErrorMessage = "The property should indicate whether it's writable or not.")]
        public int Order { get; set; }
        
        [Required(ErrorMessage = "The property should define the expected type.")]
        public DevicePropertyType PropertyType { get; set; }
    }
    ```

- `src/IoTHub.Portal.Shared/Models/v1.0/Filters/DeviceModelFilter.cs` (Lines 1-11)
  - Filter DTO for device model search
  - Properties: SearchText (name or description), plus inherited pagination properties

### Client Services
- `src/IoTHub.Portal.Client/Services/IDeviceModelsClientService.cs` (Lines 1-26)
  - Client-side service interface for HTTP API calls
  - Methods: GetDeviceModelsAsync, GetDeviceModel, CreateDeviceModelAsync, UpdateDeviceModel, DeleteDeviceModel, GetDeviceModelModelPropertiesAsync, SetDeviceModelModelProperties, GetAvatar, ChangeAvatarAsync

### Validators
- `src/IoTHub.Portal.Client/Validators/DeviceModelValidator.cs` (Lines 1-15)
  - FluentValidation validator for DeviceModelDto
  - Validates: Name is required

- `src/IoTHub.Portal.Client/Validators/DevicePropertyValidator.cs`
  - FluentValidation validator for DeviceProperty
  - Validates property fields according to data annotations

---

## API Endpoints

### Device Model Management
- `GET /api/models` - Get paginated device model list
  - Query parameters: searchText, pageSize, pageNumber, orderBy[]
  - Returns: PaginationResult<DeviceModelDto>
  - Authorization: model:read
  - Searches in name and description fields

- `GET /api/models/{id}` - Get device model details
  - Returns: DeviceModelDto (or LoRaDeviceModelDto)
  - Authorization: model:read
  - Includes labels

- `POST /api/models` - Create new device model
  - Body: DeviceModelDto
  - Returns: DeviceModelDto with ModelId
  - Authorization: model:write
  - Creates model in database, sets default image, creates IoT Hub enrollment group and configuration

- `PUT /api/models` - Update device model
  - Body: DeviceModelDto
  - Returns: 200 OK
  - Authorization: model:write
  - Updates model in database, recreates IoT Hub configuration

- `DELETE /api/models/{id}` - Delete device model
  - Returns: 204 No Content
  - Authorization: model:write
  - Validates model is not in use by devices
  - Removes enrollment group, configurations, commands, labels, and image

### Device Model Avatar Management
- `GET /api/models/{id}/avatar` - Get device model avatar URL
  - Returns: string (image URL)
  - Authorization: model:read

- `POST /api/models/{id}/avatar` - Update device model avatar
  - Body: string (base64 image data)
  - Returns: string (new image URL)
  - Authorization: model:write
  - Accepts JPG, JPEG, PNG formats
  - Resizes to 200x200

- `DELETE /api/models/{id}/avatar` - Remove device model avatar
  - Returns: 204 No Content
  - Authorization: model:write
  - Resets to default avatar

### Device Model Properties
- `GET /api/models/{id}/properties` - Get device model properties
  - Returns: IEnumerable<DeviceProperty>
  - Authorization: model:read
  - Returns properties ordered by Order field

- `POST /api/models/{id}/properties` - Set device model properties
  - Body: IEnumerable<DeviceProperty>
  - Returns: 200 OK
  - Authorization: model:write
  - Replaces all properties for the model

---

## Authorization

### Required Permissions
- **model:read** - View device model list, details, properties, and avatars
- **model:write** - Create, update, delete device models, manage properties and avatars

### Authorization Implementation
- Attribute-based authorization using `[Authorize("permission")]` attributes on controller methods
- Permission checks in UI components using `HasPermissionAsync(PortalPermissions.*)` for conditional rendering
- Base authorization requirement: `[Authorize]` on DeviceModelsController
- Built-in models have additional UI restrictions preventing edit/delete even with write permissions

---

## Dependencies

### Internal Feature Dependencies
- **Label Management** - Device models can be tagged with labels for categorization
- **Device Management** - Device models are referenced by devices; models cannot be deleted if in use
- **LoRaWAN Management** - LoRaWAN device models have additional properties and commands
- **IoT Hub Integration** - External device service for enrollment group and configuration management

### Service Dependencies
- `IDeviceModelRepository` - Device model persistence
- `IDeviceModelCommandRepository` - Device model command persistence (LoRaWAN)
- `ILabelRepository` - Label persistence
- `IDeviceRegistryProvider` - IoT Hub/AWS enrollment group management
- `IConfigService` - IoT Hub/AWS configuration management
- `IDeviceModelImageManager` - Device model image storage and retrieval
- `IDeviceModelMapper` - Mapping between DTOs and entities
- `IExternalDeviceService` - Query devices to validate model usage
- `IUnitOfWork` - Transaction management
- `IMapper` (AutoMapper) - Entity to DTO mapping

### External Dependencies
- **Azure IoT Hub** or **AWS IoT Core** - Cloud IoT device enrollment and configuration service
- **Entity Framework Core** - Database access via PortalDbContext
- **AutoMapper** - Entity to DTO mapping
- **Azure Blob Storage** or **AWS S3** - Image storage (via IDeviceModelImageManager)

### UI Dependencies
- **MudBlazor** - UI component library (MudTable, MudForm, MudTextField, MudExpansionPanel, etc.)
- **FluentValidation** - Form validation
- LabelsEditor component - Label management
- EditLoraDeviceModel / CreateLoraDeviceModel components - LoRaWAN-specific UI

---

## Key Features & Behaviors

### Search and Filtering
- Full-text search by device model name or description (case-insensitive)
- Searches across both Name and Description fields simultaneously
- Server-side filtering for performance

### Pagination
- Server-side pagination with configurable page size
- Default page size: 10 items
- Uses cursor-based navigation with nextPage URL
- Sorting support on Name and Description columns

### Property Management
- Define custom properties for device models
- Property types: String, Integer, Double, Float, Long, Boolean, DateTime
- Properties can be writable (desired properties) or read-only (reported properties)
- Property order controls display sequence in device editor
- Properties are mapped to device twin properties in IoT Hub
- Validation: name must be word or dot-separated words, display name required, order required

### Avatar Management
- Upload custom images for device models (JPG, JPEG, PNG)
- Automatic image resizing to 200x200 pixels
- Default avatar assigned when no image uploaded
- Delete avatar to reset to default
- Images stored in blob storage (Azure or AWS)

### Device Model Types
- **Standard Device Models**: Basic IoT devices with properties
- **LoRaWAN Device Models**: LoRaWAN devices with additional configuration (OTAA, class type, deduplication, etc.) and commands
- Type selection during creation (if LoRa is enabled in portal settings)

### IoT Hub Integration
- Automatic enrollment group creation when model is created
- Configuration rollout to all devices of the model type
- Enrollment group deletion when model is deleted
- Desired properties from model properties are pushed to device configurations

### Built-in Models
- System-defined models that cannot be edited or deleted
- Flagged with IsBuiltin property
- Edit/delete buttons disabled in UI
- Useful for default or template models

### Validation
- Device model name required
- Property name format validation (word or dot-separated words)
- Property display name required
- Cannot delete model if in use by any device
- Duplicate property names prevented
- LoRaWAN command validation (if applicable)

### Label Management
- Device models can have multiple labels
- Labels provide categorization and filtering
- Labels are displayed in list view
- Label editor integrated in create/edit forms

---

## Notes

### Architecture Patterns
- **Generic Controller Base** - DeviceModelsControllerBase<TListItem, TModel> allows reuse for different model types (standard, LoRaWAN)
- **Generic Service Interface** - IDeviceModelService<TListItem, TModel> enables polymorphic model handling
- **Repository Pattern** - Clean separation of data access concerns
- **Service Layer Abstraction** - Business logic isolated from controllers
- **DTO Pattern** - Clear separation between domain entities and API contracts

### Multi-Cloud Support
- Supports both Azure IoT Hub and AWS IoT Core
- Cloud provider selection via PortalSettings.CloudProvider
- Provider-specific implementations handle enrollment groups and configurations
- AWS provider has some restrictions (e.g., Name field disabled)

### Performance Considerations
- Server-side pagination reduces data transfer
- Lazy loading with Include() for related entities (Labels)
- Image URLs retrieved asynchronously
- Query optimization with predicate building (PredicateBuilder)
- Parallel image retrieval during list loading

### Security Considerations
- Comprehensive authorization checks at controller and UI levels
- Built-in models protected from modification
- Input validation on all user-submitted data
- Device usage validation prevents orphaning devices
- XSS protection through Blazor's automatic encoding
- Image upload restricted to specific formats

### Testing Coverage
- Unit tests exist for controllers, services, validators, and UI components
- Test files: DeviceModelServiceTests.cs, DeviceModelsControllerTests.cs, DeviceModelValidatorTests.cs, DeviceModelPropertiesServiceTests.cs

### Integration Points
- Device models are the foundation for device provisioning
- Properties defined in models are used in device twin synchronization
- Commands defined in models (LoRaWAN) are available for device execution
- Labels provide cross-cutting categorization
- Images stored in blob storage with model association

### Future Enhancement Opportunities
- Import/export device model templates
- Device model versioning
- Model inheritance or composition
- Bulk model operations
- Model usage statistics and reporting
- Property templates or presets
- Model validation rules (constraints on property values)
- Model search by labels
- Model duplication/cloning feature
