# Feature: Device Properties Management

**Category**: Device Management  
**Status**: Analyzed  

---

## Description

The Device Properties Management feature enables the definition and management of properties for IoT device models and device instances. This feature provides a flexible property system that allows administrators to:

- Define property templates at the device model level that specify the schema and behavior of device properties
- Configure property types (Boolean, Double, Float, Integer, Long, String), display names, and ordering
- Specify whether properties are writable (desired properties) or read-only (reported properties)
- Retrieve and update device instance properties based on their model templates
- Synchronize properties with Azure IoT Hub device twins or AWS IoT device shadows
- Support dot notation for hierarchical property names (e.g., "config.interval")

The feature distinguishes between two key concepts:
- **Device Model Properties**: Template definitions stored in the portal database that define what properties a device model supports
- **Device Instance Properties**: Actual property values stored in the cloud IoT service (Azure IoT Hub twin or AWS IoT shadow) for individual devices

This enables model-driven device configuration where all devices of the same model share a common property schema, ensuring consistency and simplifying device management at scale. Properties support both desired state (commands sent to devices) and reported state (telemetry from devices).

---

## Code Locations

### Entry Points / Endpoints

- `src/IoTHub.Portal.Server/Controllers/v1.0/DeviceModelPropertiesController.cs` (Lines 1-53)
  - **Snippet**: REST API controller for device model properties
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

- `src/IoTHub.Portal.Server/Controllers/v1.0/DeviceModelPropertiesControllerBase.cs` (Lines 1-88)
  - Base controller with shared property management logic
  - Maps between DTOs and domain entities
  - Handles validation and error responses

- `src/IoTHub.Portal.Server/Controllers/v1.0/DevicesController.cs` (Lines 111-126)
  - **Snippet**: Device instance property endpoints
    ```csharp
    [HttpGet("{deviceID}/properties", Name = "GET Device Properties")]
    [Authorize("device:read")]
    public async Task<IEnumerable<DevicePropertyValue>> GetProperties(string deviceID)
    
    [HttpPost("{deviceID}/properties", Name = "POST Device Properties")]
    [Authorize("device:write")]
    public async Task<ActionResult<IEnumerable<DevicePropertyValue>>> SetProperties(
        string deviceID, IEnumerable<DevicePropertyValue> values)
    ```

### Business Logic

- `src/IoTHub.Portal.Application/Services/IDeviceModelPropertiesService.cs` (Lines 1-13)
  - **Snippet**: Service interface for model property templates
    ```csharp
    public interface IDeviceModelPropertiesService
    {
        Task<IEnumerable<DeviceModelProperty>> GetModelProperties(string modelId);
        Task SavePropertiesForModel(string modelId, IEnumerable<DeviceModelProperty> items);
        IEnumerable<string> GetAllPropertiesNames();
    }
    ```

- `src/IoTHub.Portal.Infrastructure/Services/DeviceModelPropertiesService.cs` (Lines 1-64)
  - Concrete implementation for device model property management
  - Validates model existence before operations
  - Retrieves distinct property names across all models
  - Coordinates with repository for CRUD operations

- `src/IoTHub.Portal.Application/Services/IDevicePropertyService.cs` (Lines 1-12)
  - **Snippet**: Service interface for device instance properties
    ```csharp
    public interface IDevicePropertyService
    {
        Task<IEnumerable<DevicePropertyValue>> GetProperties(string deviceId);
        Task SetProperties(string deviceId, IEnumerable<DevicePropertyValue> devicePropertyValues);
    }
    ```

- `src/IoTHub.Portal.Server/Services/DevicePropertyService.cs` (Lines 1-129)
  - Azure IoT Hub implementation for device instance properties
  - Retrieves device twin from Azure IoT Hub
  - Maps model properties to twin desired/reported properties
  - Reads writable properties from desired properties, read-only from reported properties
  - Supports dot notation property paths with JSON path selection

- `src/IoTHub.Portal.Infrastructure/Services/AWS/AWSDevicePropertyService.cs` (Lines 1-130+)
  - AWS IoT Core implementation for device instance properties
  - Retrieves device shadow from AWS IoT Data service
  - Applies same property mapping logic as Azure implementation
  - Handles AWS-specific shadow document structure

### Data Access

- `src/IoTHub.Portal.Domain/Repositories/IDeviceModelPropertiesRepository.cs` (Lines 1-11)
  - **Snippet**: Repository interface
    ```csharp
    public interface IDeviceModelPropertiesRepository : IRepository<DeviceModelProperty>
    {
        Task<IEnumerable<DeviceModelProperty>> GetModelProperties(string modelId);
        Task SavePropertiesForModel(string modelId, IEnumerable<DeviceModelProperty> items);
    }
    ```

- `src/IoTHub.Portal.Infrastructure/Repositories/DeviceModelPropertiesRepository.cs` (Lines 1-49)
  - Concrete repository implementation
  - Filters properties by model ID
  - Implements upsert logic: updates existing, adds new, removes deleted properties
  - Uses Entity Framework Core for database operations

- `src/IoTHub.Portal.Domain/Entities/DeviceModelProperty.cs` (Lines 1-44)
  - **Snippet**: Domain entity for property templates
    ```csharp
    public class DeviceModelProperty : EntityBase
    {
        [Required]
        public string Name { get; set; }              // Property name (supports dot notation)
        
        public string DisplayName { get; set; }       // Human-readable label
        
        [Required]
        public bool IsWritable { get; set; }          // Desired vs reported property
        
        [Required]
        public int Order { get; set; }                // Display order in UI
        
        [Required]
        public DevicePropertyType PropertyType { get; set; }  // Data type
        
        [Required]
        public string ModelId { get; set; }           // Associated device model
    }
    ```

### UI Components

- `src/IoTHub.Portal.Client/Pages/DeviceModels/DeviceModelDetailPage.razor` (Lines 78-141)
  - Device model properties editor panel
  - Add/remove properties for a model
  - Configure property name, display name, type, order, and writability
  - Dynamic form with validation
  - Requires model:write permission

- `src/IoTHub.Portal.Client/Components/Devices/EditDevice.razor` (Lines 252-317)
  - Device instance properties editor panel
  - Displays properties defined by device model
  - Type-specific input controls (checkbox for Boolean, text fields with validation for numeric types)
  - Read-only properties not editable (reported properties from device)
  - Writable properties can be updated (desired properties to device)
  - Only shown for non-LoRaWAN devices

### Data Transfer Objects

- `src/IoTHub.Portal.Shared/Models/v1.0/DeviceProperty.cs` (Lines 1-44)
  - **Snippet**: DTO for property templates
    ```csharp
    public class DeviceProperty
    {
        [Required(ErrorMessage = "The property name is required.")]
        [RegularExpression(@"^([\w]+\.)+[\w]+|[\w]+$", 
            ErrorMessage = "Property name must be formed by a word or words separated by a dot")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "The display name is required.")]
        public string DisplayName { get; set; }
        
        [Required(ErrorMessage = "The property should indicate whether it's writable or not.")]
        public bool IsWritable { get; set; }
        
        [Required]
        public int Order { get; set; }
        
        [Required(ErrorMessage = "The property should define the expected type.")]
        public DevicePropertyType PropertyType { get; set; }
    }
    ```

- `src/IoTHub.Portal.Shared/Models/v1.0/DevicePropertyValue.cs` (Lines 1-16)
  - **Snippet**: DTO for property values
    ```csharp
    public class DevicePropertyValue : DeviceProperty
    {
        public string Value { get; set; }  // Current property value as string
    }
    ```

- `src/IoTHub.Portal.Shared/Models/DevicePropertyType.cs` (Lines 1-39)
  - Enum defining supported property types: Boolean, Double, Float, Integer, Long, String
  - JSON serialized as string enum

### Client Services

- Client-side HTTP calls handled through:
  - Device model management pages call model properties endpoints
  - Device detail pages call device properties endpoints
  - Uses standard HTTP client services (IDeviceModelsClientService, IDeviceClientService)

---

## API Endpoints

### Device Model Property Management

- `GET /api/models/{id}/properties` - Get property templates for a device model
  - Returns: IEnumerable<DeviceProperty>
  - Authorization: model:read
  - Returns empty collection if model has no properties

- `POST /api/models/{id}/properties` - Set property templates for a device model
  - Body: IEnumerable<DeviceProperty>
  - Returns: 200 OK
  - Authorization: model:write
  - Performs upsert: adds new, updates existing, removes properties not in request
  - Returns 404 if model not found
  - Returns 422 if validation fails

### Device Instance Property Management

- `GET /api/devices/{deviceID}/properties` - Get current property values for a device
  - Returns: IEnumerable<DevicePropertyValue>
  - Authorization: device:read
  - Retrieves model properties and merges with device twin data
  - Writable properties read from desired properties, read-only from reported properties
  - Returns empty values for properties not yet set

- `POST /api/devices/{deviceID}/properties` - Update writable properties for a device
  - Body: IEnumerable<DevicePropertyValue>
  - Returns: 200 OK
  - Authorization: device:write
  - Only updates writable properties (desired properties)
  - Ignores read-only properties in request
  - Updates device twin in cloud IoT service

---

## Authorization

### Required Permissions

- **model:read** - View device model property templates
- **model:write** - Create, update, delete device model property templates
- **device:read** - View device instance property values
- **device:write** - Update device instance property values

### Authorization Implementation

- Attribute-based authorization using `[Authorize("permission")]` on controller methods
- Permission checks in UI components for conditional rendering
- Base authorization: `[Authorize]` on all controllers

---

## Dependencies

### Internal Feature Dependencies

- **Device Model Management** - Properties are always associated with a device model; model must exist before properties can be defined
- **Standard Device Management** - Device instance properties require device to exist and have a model assigned
- **Azure IoT Hub / AWS IoT Core Integration** - Device instance property values stored in and retrieved from cloud device twins/shadows

### Service Dependencies

- `IExternalDeviceService` - Retrieves and updates device twins in Azure IoT Hub (or AWS equivalent)
- `IDeviceRepository` - Retrieves device records from database (AWS implementation)
- `IDeviceModelRepository` - Validates device model existence
- `IDeviceModelPropertiesRepository` - Persists property templates
- `IMapper` (AutoMapper) - Maps between DTOs and entities

### External Dependencies

- **Azure IoT Hub** - Stores device twin desired and reported properties for Azure deployments
- **AWS IoT Core** - Stores device shadow desired and reported state for AWS deployments
- **Entity Framework Core** - Persists property templates in portal database
- **Newtonsoft.Json** - JSON parsing and JPath queries for property paths

### UI Dependencies

- **MudBlazor** - UI components (MudTextField, MudSelect, MudCheckBox, MudExpansionPanel)
- Device Models (property templates define device capabilities)

---

## Key Features & Behaviors

### Property Types

Supports six property types with UI validation:
- **Boolean**: Rendered as checkbox with tri-state support
- **Double**: Text field with double parsing validation
- **Float**: Text field with float parsing validation
- **Integer**: Text field with integer parsing validation
- **Long**: Text field with long parsing validation
- **String**: Multi-line text field without type validation

### Desired vs Reported Properties

- **Writable (IsWritable=true)**: Stored in device twin desired properties
  - Represents configuration sent from portal to device
  - Device should read and apply these settings
  - Editable in portal UI
  
- **Read-only (IsWritable=false)**: Stored in device twin reported properties
  - Represents telemetry or status reported by device
  - Portal displays but cannot edit
  - Device updates these values

### Dot Notation Support

Property names support hierarchical paths using dot notation:
- Example: `config.telemetryInterval` maps to `{ "config": { "telemetryInterval": 30 } }`
- Uses JSON path selection (JObject.SelectToken) to navigate nested properties
- Validation ensures format: single word or dot-separated words

### Property Ordering

- Order property determines display sequence in UI
- Lower numbers appear first
- UI sorts properties by Order before rendering
- Allows logical grouping of related properties

### Model-Based Templates

- Property definitions stored at model level act as templates
- All devices of same model share same property schema
- Ensures consistency across device fleet
- Simplifies bulk device management

### Validation

- Property name must match regex pattern for single word or dot-separated words
- Display name required
- Property type required
- Writability flag required
- Order value required (numeric, minimum 0)
- Model state validated before API calls

### Synchronization Behavior

- Properties retrieved on-demand from cloud service (not cached in portal database)
- Updates immediately written to device twin/shadow
- Portal acts as thin management layer over cloud IoT services
- No offline capability - requires cloud service availability

---

## Integration Points

### Device Configuration Export/Import

- `src/IoTHub.Portal.Server/Managers/ExportManager.cs` uses properties service
- Device exports include property values
- Device imports can set property values
- Enables bulk device configuration management

### Device Configurations

- `src/IoTHub.Portal.Server/Services/DeviceConfigurationsService.cs` integrates with properties
- Device configurations can reference and set device model properties
- Enables automated deployment of property values to device fleets

---

## Notes

### Architecture Patterns

- **Provider Pattern**: Separate implementations for Azure (DevicePropertyService) and AWS (AWSDevicePropertyService)
- **Template Pattern**: DeviceModelPropertiesControllerBase provides shared controller logic
- **Repository Pattern**: Clean separation of data access concerns
- **Service Layer**: Business logic isolated from controllers and data access

### Multi-Cloud Support

- Azure implementation uses device twin (desired/reported properties)
- AWS implementation uses device shadow (desired/reported state)
- Same business logic and API for both providers
- Provider-specific property retrieval and update logic

### Property Storage Strategy

- **Model Properties**: Stored in portal database (PortalDbContext)
  - Enables offline access to property schema
  - Allows custom property definitions beyond IoT service capabilities
  
- **Device Properties**: Stored in cloud IoT service only
  - No duplication in portal database
  - Single source of truth in device twin/shadow
  - Reduces synchronization complexity

### Limitations

- Properties limited to simple types (no complex objects or arrays as first-class types)
- Dot notation provides workaround for hierarchical properties but UI treats as flat
- No property constraints (min/max values, regex patterns) beyond type
- No property validation at device level (portal accepts any value matching type)
- LoRaWAN devices do not support custom properties feature

### Performance Considerations

- Property retrieval requires API call to cloud service (Azure IoT Hub or AWS IoT)
- Each device property operation incurs cloud service latency
- No caching of property values in portal
- Batch operations not supported (each device updated individually)

### Security Considerations

- Property values not encrypted in portal (relies on cloud service encryption)
- Sensitive configuration should use device-level secrets, not properties
- Authorization enforced at both model and device levels
- Property changes auditable through cloud service logs

### Testing Coverage

- Unit tests exist for controllers, services, repositories, and validators
- Test files: DeviceModelPropertiesControllerTests.cs, DevicePropertyServiceTests.cs, AWSDevicePropertyServiceTests.cs, DevicePropertyValidatorTests.cs

### Future Enhancement Opportunities

- Property validation rules (min/max, regex, enum values)
- Property grouping and sections in UI
- Property change history and auditing in portal
- Bulk property updates across multiple devices
- Property templates/presets for common configurations
- Complex property types (arrays, nested objects with UI support)
- Property documentation and help text
- Property units and formatting hints
- Property value suggestions/autocomplete
- Real-time property value updates via SignalR
