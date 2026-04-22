# Feature: Edge Device Model Management

**Category**: Device Model Management  
**Status**: Analyzed  

---

## Description

The Edge Device Model Management feature enables administrators to define, configure, and manage IoT Edge device models (templates) that represent different types of Azure IoT Edge or AWS IoT Greengrass devices. Edge device models serve as blueprints that define the modules, routes, system configurations, and metadata for edge devices, allowing standardized deployment and consistent configuration across edge device fleets. This feature includes:

- Creating and managing edge device model definitions with modules and routes
- Configuring system modules (edgeAgent, edgeHub) for Azure IoT Edge
- Defining custom edge modules with container images, environment variables, and twin settings
- Managing module commands for remote execution capabilities
- Defining message routing between modules and cloud
- Avatar/image management for visual model identification
- Label support for model categorization and filtering
- Multi-cloud support (Azure IoT Edge and AWS IoT Greengrass)
- Integration with Azure IoT Hub configurations and AWS IoT Core deployments
- Public module catalog for reusable edge module templates
- CRUD operations through REST API and interactive UI

This feature provides business value by enabling infrastructure-as-code approach to edge deployments, reducing manual configuration errors, ensuring consistency across device fleets, and allowing rapid deployment of standardized edge solutions. Organizations can define tested and approved edge configurations once, then deploy them to multiple devices efficiently.

---

## Code Locations

### Entry Points / Endpoints
- `src/IoTHub.Portal.Server/Controllers/v1.0/EdgeModelsController.cs` (Lines 1-126)
  - **Snippet**: Main REST API controller for edge model management
    ```csharp
    [Authorize]
    [Route("api/edge/models")]
    [ApiExplorerSettings(GroupName = "IoT Edge Devices Models")]
    [ApiController]
    public class EdgeModelsController : ControllerBase
    {
        private readonly IEdgeModelService edgeModelService;
        
        [HttpGet]
        [Authorize("edge-model:read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<IoTEdgeModelListItem>>> GetEdgeModelList(
            [FromQuery] EdgeModelFilter edgeModelFilter)
        
        [HttpGet("{edgeModelId}")]
        [Authorize("edge-model:read")]
        public async Task<ActionResult<IoTEdgeModel>> GetEdgeDeviceModel(string edgeModelId)
        
        [HttpPost]
        [Authorize("edge-model:write")]
        public async Task<IActionResult> CreateEdgeModel(IoTEdgeModel EdgeModel)
        
        [HttpPut]
        [Authorize("edge-model:write")]
        public async Task<IActionResult> UpdateEdgeModel(IoTEdgeModel EdgeModel)
        
        [HttpDelete("{edgeModelId}")]
        [Authorize("edge-model:write")]
        public async Task<IActionResult> DeleteModelAsync(string edgeModelId)
        
        [HttpGet("{edgeModelId}/avatar")]
        [Authorize("edge-model:read")]
        public virtual async Task<ActionResult<string>> GetAvatar(string edgeModelId)
        
        [HttpPost("{edgeModelId}/avatar")]
        [Authorize("edge-model:write")]
        public virtual async Task<ActionResult<string>> ChangeAvatar(
            string edgeModelId, string avatar)
        
        [HttpDelete("{edgeModelId}/avatar")]
        [Authorize("edge-model:write")]
        public virtual async Task<IActionResult> DeleteAvatar(string edgeModelId)
        
        [HttpGet("public-modules")]
        [Authorize("edge-model:read")]
        public virtual async Task<ActionResult<IEnumerable<IoTEdgeModel>>> GetPublicEdgeModules()
    }
    ```

### Business Logic
- `src/IoTHub.Portal.Application/Services/IEdgeModelService.cs` (Lines 1-25)
  - **Snippet**: Core service interface for edge model operations
    ```csharp
    public interface IEdgeModelService
    {
        Task<IEnumerable<IoTEdgeModelListItem>> GetEdgeModels(EdgeModelFilter edgeModelFilter);
        Task<IoTEdgeModel> GetEdgeModel(string modelId);
        Task CreateEdgeModel(IoTEdgeModel edgeModel);
        Task UpdateEdgeModel(IoTEdgeModel edgeModel);
        Task DeleteEdgeModel(string edgeModelId);
        Task<string> GetEdgeModelAvatar(string edgeModelId);
        Task<string> UpdateEdgeModelAvatar(string edgeModelId, string avatar);
        Task DeleteEdgeModelAvatar(string edgeModelId);
        Task<IEnumerable<IoTEdgeModule>> GetPublicEdgeModules();
    }
    ```

- `src/IoTHub.Portal.Infrastructure/Services/EdgeModelService.cs` (Lines 1-339)
  - Concrete implementation of IEdgeModelService
  - Handles edge model CRUD operations with multi-cloud support
  - Manages module commands for Azure IoT Edge
  - Integrates with IConfigService for Azure/AWS deployment configurations
  - Manages device model images through IDeviceModelImageManager
  - Implements label management and filtering
  - Handles system module (edgeAgent, edgeHub) configuration
  - Orchestrates edge configuration rollout to cloud providers

### Data Access
- `src/IoTHub.Portal.Domain/Repositories/IEdgeDeviceModelRepository.cs` (Lines 1-10)
  - Repository interface for EdgeDeviceModel entity
  - Extends IRepository<EdgeDeviceModel>
  - Provides GetByNameAsync method for name-based lookups

- `src/IoTHub.Portal.Domain/Repositories/IEdgeDeviceModelCommandRepository.cs`
  - Repository interface for EdgeDeviceModelCommand entity
  - Manages module commands associated with edge models

- `src/IoTHub.Portal.Domain/Entities/EdgeDeviceModel.cs` (Lines 1-19)
  - **Snippet**: Edge device model entity definition
    ```csharp
    public class EdgeDeviceModel : EntityBase
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? ExternalIdentifier { get; set; }  // AWS deployment ID or Azure config ID
        
        /// <summary>
        /// Labels for categorization and filtering
        /// </summary>
        public ICollection<Label> Labels { get; set; } = default!;
    }
    ```

- `src/IoTHub.Portal.Domain/Entities/EdgeDeviceModelCommand.cs`
  - Entity for module commands associated with edge models
  - Links commands to specific modules within a model
  - Azure IoT Edge specific functionality

### UI Components
- `src/IoTHub.Portal.Client/Pages/EdgeModels/EdgeModelListPage.razor` (Lines 1-159)
  - Main edge model list view with search and filtering
  - Interactive MudTable displaying model list
  - Features:
    - Search by keyword (name/description)
    - Model avatar display
    - Label display for each model
    - Quick navigation to model details
    - Delete model with confirmation dialog
    - Add new model button
    - Refresh functionality
    - Sortable columns (Name, Description)
    - Pagination with configurable page sizes
  - Accessible at route: `/edge/models`
  - Authorization: Requires `edge-model:read` permission
  - Conditional write operations based on `edge-model:write` permission

- `src/IoTHub.Portal.Client/Pages/EdgeModels/EdgeModelDetailPage.razor` (Lines 1-800+)
  - Detailed edge model view and edit page
  - Features:
    - Model avatar management (upload, delete)
    - General details (name, description)
    - System modules configuration (Azure only)
    - Custom edge modules management
    - Module environment variables
    - Module twin settings
    - Module commands
    - Edge routes configuration
    - Labels management
    - Save and delete operations
    - Tabbed interface (General, Modules, Routes)
    - Expansion panels for organized sections
    - Read-only mode for users without write permissions
  - Accessible at route: `/edge/models/{ModelID}`
  - Authorization: Requires `edge-model:read` permission
  - Write operations require `edge-model:write` permission

- `src/IoTHub.Portal.Client/Pages/EdgeModels/CreateEdgeModelsPage.razor` (Lines 1-600+)
  - Edge model creation page with full configuration
  - Similar structure to detail page but for new models
  - Features:
    - Model name and description input
    - Avatar upload
    - System modules setup (Azure)
    - Custom modules configuration
    - Routes definition
    - Labels assignment
    - Public modules catalog integration
    - Form validation
  - Accessible at route: `/edge/models/new`
  - Authorization: Requires `edge-model:write` permission

- `src/IoTHub.Portal.Client/Components/EdgeModels/EdgeModelSearch.razor`
  - Search component for filtering edge models
  - Keyword-based search functionality
  - Integrated into EdgeModelListPage

- `src/IoTHub.Portal.Client/Dialogs/EdgeModels/DeleteEdgeModelDialog.razor`
  - Confirmation dialog for edge model deletion
  - Displays model name and ID
  - Handles deletion API call
  - Provides user feedback on success/failure

### Data Transfer Objects
- `src/IoTHub.Portal.Shared/Models/v1.0/IoTEdgeModel.cs` (Lines 1-31)
  - **Snippet**: Complete edge model DTO
    ```csharp
    public class IoTEdgeModel : IoTEdgeModelListItem
    {
        /// <summary>
        /// The device model module list (custom modules)
        /// </summary>
        public List<IoTEdgeModule> EdgeModules { get; set; } = new List<IoTEdgeModule>();
        
        /// <summary>
        /// Message routing configuration
        /// </summary>
        public List<IoTEdgeRoute> EdgeRoutes { get; set; } = new List<IoTEdgeRoute>();
        
        /// <summary>
        /// System modules (edgeAgent, edgeHub) - Azure only
        /// </summary>
        public List<EdgeModelSystemModule> SystemModules { get; set; }
        
        /// <summary>
        /// Model labels for categorization
        /// </summary>
        public new List<LabelDto> Labels { get; set; } = new();
        
        public IoTEdgeModel()
        {
            SystemModules = new List<EdgeModelSystemModule>
            {
                new EdgeModelSystemModule("edgeAgent"),
                new EdgeModelSystemModule("edgeHub")
            };
        }
    }
    ```

- `src/IoTHub.Portal.Shared/Models/v1.0/IoTEdgeModelListItem.cs` (Lines 1-37)
  - **Snippet**: List view DTO for edge models
    ```csharp
    public class IoTEdgeModelListItem
    {
        public string ModelId { get; set; } = default!;
        
        [Required(ErrorMessage = "The IoT Edge device model name is required.")]
        public string Name { get; set; } = default!;
        
        public string Description { get; set; } = default!;
        public string Image { get; set; } = default!;
        
        /// <summary>
        /// The AWS deployment ID or Azure configuration ID
        /// </summary>
        public string ExternalIdentifier { get; set; } = default!;
        
        /// <summary>
        /// Gets the edge model labels
        /// </summary>
        public IEnumerable<LabelDto> Labels { get; set; } = new List<LabelDto>();
    }
    ```

- `src/IoTHub.Portal.Shared/Models/v1.0/IoTEdgeModule.cs` (Lines 1-54)
  - **Snippet**: Edge module definition DTO
    ```csharp
    public class IoTEdgeModule
    {
        public string Id { get; set; } = default!;  // AWS Greengrass module ID
        
        [Required(ErrorMessage = "The device model name is required.")]
        public string ModuleName { get; set; } = default!;
        
        [Required(ErrorMessage = "The device image uri is required.")]
        public string Image { get; set; } = default!;  // Container image URI
        
        public string ContainerCreateOptions { get; set; } = default!;
        public int StartupOrder { get; set; }
        public string Status { get; set; } = default!;
        public string Version { get; set; } = default!;
        
        public List<IoTEdgeModuleEnvironmentVariable> EnvironmentVariables { get; set; } 
            = new List<IoTEdgeModuleEnvironmentVariable>();
        
        public List<IoTEdgeModuleTwinSetting> ModuleIdentityTwinSettings { get; set; } 
            = new List<IoTEdgeModuleTwinSetting>();
        
        public List<IoTEdgeModuleCommand> Commands { get; set; } 
            = new List<IoTEdgeModuleCommand>();
    }
    ```

- `src/IoTHub.Portal.Shared/Models/v1.0/IoTEdgeRoute.cs` (Lines 1-35)
  - **Snippet**: Edge route definition DTO
    ```csharp
    public class IoTEdgeRoute
    {
        [Required(ErrorMessage = "The route should have a name.")]
        public string Name { get; set; } = default!;
        
        [RegularExpression(@"^(?i)FROM [\S]+( WHERE (NOT )?[\S]+)? INTO [\S]+$", 
            ErrorMessage = "Route should be 'FROM <source> (WHERE <condition>) INTO <sink>'")]
        [Required(ErrorMessage = "The route should have a value.")]
        public string Value { get; set; } = default!;  // e.g., "FROM /messages/* INTO $upstream"
        
        [Range(0, 9)]
        public int? Priority { get; set; } = null;
        
        [Range(0, uint.MaxValue)]
        public uint? TimeToLive { get; set; } = null;  // Seconds
    }
    ```

- `src/IoTHub.Portal.Shared/Models/v1.0/EdgeModelSystemModule.cs`
  - DTO for system modules (edgeAgent, edgeHub)
  - Azure IoT Edge specific
  - Contains module image URI and configuration

- `src/IoTHub.Portal.Shared/Models/v1.0/IoTEdgeModuleCommand.cs`
  - DTO for module commands
  - Links command to module within edge model
  - Used for remote command execution

- `src/IoTHub.Portal.Shared/Models/v1.0/IoTEdgeModuleEnvironmentVariable.cs`
  - DTO for module environment variables
  - Key-value pairs injected into module containers

- `src/IoTHub.Portal.Shared/Models/v1.0/IoTEdgeModuleTwinSetting.cs`
  - DTO for module twin desired properties
  - Configuration settings synchronized to module twin

- `src/IoTHub.Portal.Shared/Models/v1.0/Filters/EdgeModelFilter.cs`
  - Filter DTO for edge model list queries
  - Supports keyword-based filtering

### Client Services
- `src/IoTHub.Portal.Client/Services/IEdgeModelClientService.cs` (Lines 1-26)
  - Client-side service interface for HTTP API calls
  - Methods: GetIoTEdgeModelList, GetIoTEdgeModel, CreateIoTEdgeModel, 
    UpdateIoTEdgeModel, DeleteIoTEdgeModel, GetAvatar, ChangeAvatar, 
    DeleteAvatar, GetPublicEdgeModules

- `src/IoTHub.Portal.Client/Services/EdgeModelClientService.cs` (Lines 1-69)
  - Implementation using HttpClient for API communication
  - Base URL: `api/edge/models`
  - Handles JSON serialization/deserialization
  - Supports filtering via query parameters

### Validators
- `src/IoTHub.Portal.Client/Validators/EdgeModelValidator.cs`
  - FluentValidation validator for IoTEdgeModel
  - Validates model name, description, modules, routes
  - Ensures module and route configuration correctness

### Mappers
- `src/IoTHub.Portal.Application/Mappers/EdgeDeviceModelProfile.cs` (Lines 1-19)
  - **Snippet**: AutoMapper profile for edge model entity mapping
    ```csharp
    public class EdgeDeviceModelProfile : Profile
    {
        public EdgeDeviceModelProfile()
        {
            // Map IoTEdgeModel DTO to EdgeDeviceModel entity
            CreateMap<IoTEdgeModel, EdgeDeviceModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ModelId))
                .ReverseMap();
            
            // Map IoTEdgeModelListItem DTO to EdgeDeviceModel entity
            CreateMap<IoTEdgeModelListItem, EdgeDeviceModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ModelId))
                .ReverseMap();
        }
    }
    ```

- `src/IoTHub.Portal.Application/Mappers/EdgeDeviceModelCommandProfile.cs`
  - AutoMapper profile for module command mapping
  - Maps between IoTEdgeModuleCommand DTO and EdgeDeviceModelCommand entity

---

## API Endpoints

### Edge Model Management
- `GET /api/edge/models` - Get filtered list of edge models
  - Query Parameters: `Keyword` (optional) - Filter by name or description
  - Returns: `IEnumerable<IoTEdgeModelListItem>`
  - Authorization: `edge-model:read`
  - Used to populate model list page with search/filter capabilities

- `GET /api/edge/models/{edgeModelId}` - Get detailed edge model configuration
  - Path Parameter: `edgeModelId` (string) - Model identifier
  - Returns: `IoTEdgeModel` (includes modules, routes, system modules, labels)
  - Authorization: `edge-model:read`
  - Cloud-specific: Returns different structures for Azure vs AWS
  - Azure: Includes system modules, routes, and module commands
  - AWS: Includes custom modules only

- `POST /api/edge/models` - Create new edge model
  - Body: `IoTEdgeModel` (complete model configuration)
  - Returns: 200 OK
  - Authorization: `edge-model:write`
  - Validates model doesn't already exist
  - Creates database record
  - Sets default avatar image
  - Rolls out configuration to cloud provider (Azure IoT Hub or AWS IoT Core)
  - Saves module commands (Azure only)
  - Stores external identifier (deployment/configuration ID)

- `PUT /api/edge/models` - Update existing edge model
  - Body: `IoTEdgeModel` (complete model configuration)
  - Returns: 200 OK
  - Authorization: `edge-model:write`
  - Updates database record
  - Updates labels (delete old, insert new)
  - Rolls out updated configuration to cloud provider
  - Updates module commands (Azure only)

- `DELETE /api/edge/models/{edgeModelId}` - Delete edge model
  - Path Parameter: `edgeModelId` (string) - Model identifier
  - Returns: 204 No Content
  - Authorization: `edge-model:write`
  - Deletes cloud configuration (Azure IoT Hub or AWS IoT Core)
  - Deletes module commands (Azure only)
  - Deletes labels
  - Deletes database record

### Avatar Management
- `GET /api/edge/models/{edgeModelId}/avatar` - Get model avatar image URL
  - Path Parameter: `edgeModelId` (string) - Model identifier
  - Returns: `string` (image URL or base64 data)
  - Authorization: `edge-model:read`

- `POST /api/edge/models/{edgeModelId}/avatar` - Upload/update model avatar
  - Path Parameter: `edgeModelId` (string) - Model identifier
  - Body: `string` (image data)
  - Returns: `string` (new image URL)
  - Authorization: `edge-model:write`

- `DELETE /api/edge/models/{edgeModelId}/avatar` - Delete model avatar
  - Path Parameter: `edgeModelId` (string) - Model identifier
  - Returns: 204 No Content
  - Authorization: `edge-model:write`
  - Reverts to default placeholder image

### Public Modules Catalog
- `GET /api/edge/models/public-modules` - Get public edge module catalog
  - Returns: `IEnumerable<IoTEdgeModule>`
  - Authorization: `edge-model:read`
  - Provides reusable module templates from marketplace/catalog
  - Used during model creation to import pre-configured modules

---

## Authorization

### Required Permissions
- **edge-model:read** - View edge models, retrieve model details, and access avatar images
- **edge-model:write** - Create, update, and delete edge models and manage avatars

### Authorization Implementation
- Attribute-based authorization using `[Authorize("permission")]` on controller methods
- Permission strings defined in PortalPermissionsHelper
- Base authorization requirement: `[Authorize]` on EdgeModelsController and edge model pages
- Permissions managed through role-based access control (RBAC)
- Default Administrator role includes both edge-model:read and edge-model:write permissions
- UI components conditionally render write operations based on user permissions

### Permission Mapping
- `PortalPermissions.EdgeModelRead` → "edge-model:read"
- `PortalPermissions.EdgeModelWrite` → "edge-model:write"

---

## Dependencies

### Internal Feature Dependencies
- **Edge Device Management** - Edge devices reference edge models for deployment configuration
- **Device Configurations** - Edge models create Azure IoT Hub automatic device configurations
- **Label Management** - Edge models use labels for categorization and filtering
- **Role-Based Access Control** - Permissions enforced through RBAC system
- **Dashboard** - May display edge model statistics and metrics
- **AWS/Azure Integration** - Multi-cloud deployment orchestration

### Service Dependencies
- `IEdgeDeviceModelRepository` - Edge model entity persistence
- `IEdgeDeviceModelCommandRepository` - Module command persistence (Azure)
- `ILabelRepository` - Label entity persistence
- `IConfigService` - Cloud provider configuration management (Azure/AWS)
  - RollOutEdgeModelConfiguration - Deploy model to cloud
  - GetConfigModuleList - Retrieve modules from cloud configuration
  - GetModelSystemModule - Retrieve system modules (Azure)
  - GetConfigRouteList - Retrieve routes from cloud configuration
  - GetPublicEdgeModules - Access public module catalog
  - DeleteConfiguration - Remove cloud configuration
- `IDeviceModelImageManager` - Model avatar/image management
  - GetDeviceModelImageAsync - Retrieve model image
  - SetDefaultImageToModel - Set default placeholder image
  - ChangeDeviceModelImageAsync - Update model image
  - DeleteDeviceModelImageAsync - Remove model image
- `IUnitOfWork` - Transaction management for database operations
- `IMapper` (AutoMapper) - Entity to DTO mapping
- `ConfigHandler` - Application configuration (cloud provider setting)

### Related Entities
- **EdgeDevice** - References EdgeDeviceModel for deployment configuration
- **Label** - Many-to-many relationship with EdgeDeviceModel
- **EdgeDeviceModelCommand** - One-to-many relationship with EdgeDeviceModel

### External Dependencies
- **Entity Framework Core** - Database access via PortalDbContext
- **AutoMapper** - Entity to DTO mapping
- **FluentValidation** - DTO and form validation
- **Azure IoT Hub SDK** - Azure edge configuration deployment
- **AWS IoT SDK** - AWS Greengrass deployment management

### UI Dependencies
- **MudBlazor** - UI component library
  - MudTable - Data grid for model list
  - MudTextField - Text input fields
  - MudForm - Form validation
  - MudTabs - Tabbed interface for detail page
  - MudExpansionPanels - Collapsible sections
  - MudAvatar/MudImage - Image display
  - MudButton/MudIconButton - Action buttons
  - MudSnackbar - User feedback notifications
  - MudDialog - Confirmation dialogs

---

## Key Features & Behaviors

### Edge Model Definition
- **ModelId**: Unique identifier generated on creation
- **Name**: Human-readable model name (required, read-only after creation in UI)
- **Description**: Optional detailed description of the model purpose
- **Image/Avatar**: Visual identifier for quick recognition in lists
- **Labels**: Categorization tags for filtering and organization
- **ExternalIdentifier**: Cloud provider-specific deployment/configuration ID

### Multi-Cloud Support
- **Azure IoT Edge**: Full support including system modules, routes, and module commands
  - EdgeAgent and EdgeHub system module configuration
  - Message routing between modules and cloud
  - Module commands for remote execution
  - Automatic device configuration deployment
- **AWS IoT Greengrass**: Core module deployment support
  - Component-based module structure
  - Greengrass component version management
  - Deployment configuration to IoT Core

### Edge Modules Configuration
- **Custom Modules**: User-defined containerized modules
  - Module name (unique within model)
  - Container image URI (Docker registry)
  - Container create options (JSON configuration)
  - Startup order for dependency management
  - Status tracking
  - Version specification
- **Environment Variables**: Key-value pairs injected into module containers
- **Module Twin Settings**: Desired properties synchronized to module twins
- **Module Commands**: Remote execution capabilities (Azure only)
  - Command name
  - Associated module
  - Stored in database for quick access

### System Modules (Azure Only)
- **edgeAgent**: Edge runtime agent managing module lifecycle
  - Image URI configuration
  - Runtime settings
- **edgeHub**: Message routing and cloud communication hub
  - Image URI configuration
  - Routing configuration

### Message Routing (Azure Only)
- **Route Definition**: FROM <source> (WHERE <condition>) INTO <sink>
  - Source: Module output or system source
  - Optional WHERE clause for message filtering
  - Sink: Module input, cloud ($upstream), or built-in endpoints
  - Regex validation ensures correct syntax
- **Priority**: 0-9 range for route prioritization
- **Time To Live**: Message expiration in seconds

### Model Lifecycle
1. **Creation**
   - User defines model name, description, and configuration
   - System generates unique ModelId
   - Default avatar assigned
   - Configuration deployed to cloud provider
   - ExternalIdentifier stored (deployment/config ID)
   - Module commands saved (Azure)

2. **Update**
   - Model configuration modified in UI
   - Labels updated (delete old, insert new)
   - Configuration rolled out to cloud provider
   - Module commands updated (Azure)
   - Existing devices using model can be redeployed

3. **Deletion**
   - Confirmation required
   - Cloud configuration removed
   - Module commands deleted (Azure)
   - Labels cascade deleted
   - Database record removed
   - Avatar/image cleaned up

### Public Modules Catalog
- Pre-configured reusable edge modules
- Marketplace/catalog integration
- Import modules during model creation
- Accelerates development with tested components
- Reduces configuration errors

### Image/Avatar Management
- Upload custom images (JPG, JPEG, PNG)
- Display in model list for visual identification
- Default placeholder for models without custom image
- Delete to revert to default
- Stored via IDeviceModelImageManager (blob storage or file system)

### Search and Filtering
- Keyword search across model name and description
- Case-insensitive matching
- Real-time filter application
- Label-based filtering (UI capability)

### Validation Rules
- **Model Name**: Required field
- **Module Name**: Required, unique within model
- **Module Image**: Required, valid URI format
- **Route Format**: Regex validation for "FROM <source> INTO <sink>" syntax
- **Route Priority**: 0-9 range
- **Route TTL**: Non-negative integer
- **Labels**: Valid label format and existence check

### Error Handling
- `ResourceAlreadyExistsException` - Model with same ID already exists
- `ResourceNotFoundException` - Model not found during get/update/delete
- `ProblemDetailsException` - API errors with user-friendly messages
- User feedback via Snackbar notifications
- Validation errors shown inline on form fields
- Loading states during async operations

### Cloud Provider Integration
- Configuration rollout orchestrated by IConfigService
- Azure: Creates/updates IoT Hub automatic device configuration
  - Configuration targets devices with matching tag (model=ModelId)
  - Includes modules, routes, and desired properties
  - Priority-based configuration layering
- AWS: Creates/updates Greengrass deployment
  - Deployment targets thing group
  - Component versions and configurations
  - Deployment rollout strategy

---

## Notes

### Architecture Patterns
- **Repository Pattern** - Clean separation of data access concerns
- **Unit of Work Pattern** - Transactional consistency across operations
- **Service Layer** - Business logic abstraction from controllers
- **DTO Pattern** - Data transfer between layers with validation
- **Strategy Pattern** - Multi-cloud support via IConfigService implementations

### ModelId as Primary Key
- EdgeDeviceModel entity uses auto-generated GUID as Id
- ModelId in DTOs maps to entity Id
- Simplifies tracking and prevents naming conflicts
- Name can be reused after model deletion

### Module Commands Storage (Azure Only)
- Stored in EdgeDeviceModelCommand entity
- Linked to model and module name
- Enables quick command lookup without cloud API call
- Synchronized during model create/update operations
- Cascade deleted with model

### Cloud Configuration Synchronization
- Edge models stored locally in database
- Full configuration stored in cloud provider (source of truth for deployments)
- ExternalIdentifier links database record to cloud configuration
- Updates rolled out to cloud immediately
- Devices receive configuration via cloud provider's deployment mechanism

### System Module Defaults
- Azure models automatically include edgeAgent and edgeHub
- Default system module images can be overridden
- System modules required for Azure IoT Edge runtime
- Not applicable to AWS Greengrass (different architecture)

### Route Query Language (Azure)
- FROM clause: Source module output or system source
  - Module: `FROM /messages/modules/{moduleName}/outputs/{outputName}`
  - System: `FROM /messages/*` (all device-to-cloud messages)
- WHERE clause: Optional message filtering
  - Property filters: `WHERE propertyName = 'value'`
  - Nested properties: `WHERE $body.temperature > 25`
  - Logical operators: AND, OR, NOT
- INTO clause: Destination
  - Upstream: `INTO $upstream` (cloud)
  - Module input: `INTO BrokeredEndpoint("/modules/{moduleName}/inputs/{inputName}")`
  - Built-in endpoints: `INTO BrokeredEndpoint("/modules/$edgeHub/inputs/events")`

### Public Modules Catalog
- Source depends on cloud provider and configuration
- Azure: Could integrate with Azure Marketplace or custom catalog
- AWS: Could integrate with Greengrass Component Catalog
- Implementation via IConfigService.GetPublicEdgeModules()
- Accelerates model creation with tested components

### Label Usage
- Labels stored as many-to-many relationship
- Enable model categorization (e.g., "production", "testing", "v1.0")
- UI filtering by labels (implementation in progress)
- Cascade deleted with model
- Update operation deletes old labels and inserts new ones

### UI/UX Considerations
- Three-page workflow: List → Detail/Create → Success
- Tabbed interface reduces clutter in detail/create pages
- Expansion panels for logical grouping
- Inline editing for modules, routes, environment variables
- Read-only mode for users without write permissions
- Visual feedback with loading indicators
- Confirmation dialogs for destructive actions
- Avatar display for quick visual identification

### Performance Considerations
- Model list loads with pagination support
- Filtering performed server-side (database query)
- Avatar images loaded asynchronously
- Modules and routes loaded on-demand (detail page)
- Lazy loading of public modules catalog
- Efficient repository queries with Include for labels

### Security Considerations
- Authorization required at both controller and UI levels
- Permission-based access control prevents unauthorized modifications
- Input validation protects against injection attacks
- Route regex validation prevents malformed route configurations
- Cloud provider credentials managed securely via ConfigHandler
- Avatar upload restricted to image file types

### Database Design
- EdgeDeviceModel: Core model entity
- EdgeDeviceModelCommand: Module commands (Azure, one-to-many)
- Label: Categorization tags (many-to-many)
- EdgeDevice: References EdgeDeviceModel (one-to-many)

### Migration History
- Initial EdgeDeviceModel entity creation
- EdgeDeviceModelCommand entity for module commands
- Label many-to-many relationship
- ExternalIdentifier field for cloud provider integration
- RBAC permissions for edge-model:read and edge-model:write

### Testing Coverage
- Unit tests: EdgeModelServiceTest.cs
- Controller tests: EdgeModelsControllerTest.cs
- UI tests: EdgeModelsListPageTest.cs, EdgeModelDetailPageTest.cs, CreateEdgeModelsPageTest.cs
- Client service tests: EdgeModelClientServiceTest.cs
- Component tests: EdgeModelSearchTests.cs, DeleteEdgeModelDialogTest.cs

### Future Enhancement Opportunities
- Model versioning and rollback capabilities
- Model templates marketplace with community contributions
- Module dependency validation and ordering
- Configuration diff/comparison between models
- Model cloning for rapid prototyping
- Import/export model configurations (JSON/YAML)
- Module health monitoring and diagnostics integration
- Automated testing framework for model validation before deployment
- Model deployment analytics (success rate, device count)
- A/B testing capabilities for model configurations
- Conditional module deployment based on device capabilities
- Module resource requirements and validation
- Integration with CI/CD pipelines for automated deployments
- Model approval workflow for production deployments
- Historical configuration audit trail
- Model usage analytics (which models are most deployed)
- Cost estimation for edge deployments based on model
- Module compatibility matrix and recommendations

### Known Limitations
- Model name cannot be changed after creation (UI enforced read-only)
- Module commands only supported for Azure IoT Edge
- Routes only supported for Azure IoT Edge
- System modules hardcoded to edgeAgent and edgeHub
- No validation of module image URI accessibility
- No automatic module version update detection
- Labels updated via replace operation (delete all, insert all)
- No draft/unpublished state for models under development
- No model deployment scheduling or phased rollouts
- No automatic rollback on deployment failure
- Public modules catalog requires external configuration
- No container registry credential management in UI
- Module startup order limited to numeric values without dependency graph
- Route priority limited to 0-9 range
- No visual route designer or graph visualization
- No module marketplace integration (manual image URI entry)
- Limited validation of container create options JSON

### Cloud Provider Differences
**Azure IoT Edge:**
- System modules (edgeAgent, edgeHub) required and configurable
- Message routing between modules via edgeHub
- Module commands stored and synchronized
- Automatic device configuration mechanism
- Module twin desired properties
- Container-based modules

**AWS IoT Greengrass:**
- Component-based architecture (no system modules concept)
- Inter-component communication via pub/sub
- Component lifecycle managed by Greengrass core
- Deployment groups target thing groups
- Component recipes define configuration
- Support for Lambda functions and containers

### Integration Points
- **Edge Device Provisioning**: Models assigned to devices during creation
- **Device Twin Sync**: Model configuration synchronized to device twin desired properties
- **Deployment Orchestration**: Cloud provider deploys configuration to devices
- **Monitoring**: Device reports module status back to portal
- **Commands**: Module commands executed remotely via cloud provider APIs
- **Telemetry**: Module telemetry routed through configured routes
