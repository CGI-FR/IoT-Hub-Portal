# Feature: Device Tag Settings Management

**Category**: Settings & Configuration  
**Status**: Analyzed  

---

## Description

The Device Tag Settings Management feature enables administrators to define and manage custom tags that can be applied to IoT devices for enhanced organization, filtering, and searching. Device tags act as metadata fields that administrators configure globally and then apply to individual devices with specific values. This feature includes:

- Creating and managing device tag definitions (name, label, required, searchable)
- Defining which tags are required during device creation
- Configuring which tags are searchable in device list filters
- CRUD operations on tag settings through an interactive table UI
- Tag validation to prevent duplicates and enforce naming conventions
- Automatic synchronization of tags with Azure IoT Hub or AWS IoT Core device twins
- Tag values stored per-device for filtering and searching capabilities

This feature provides business value by enabling flexible device categorization and metadata management without code changes, allowing organizations to adapt their device taxonomy to their specific operational needs. Searchable tags enhance device discoverability, while required tags ensure consistent device metadata across the fleet.

---

## Code Locations

### Entry Points / Endpoints
- `src/IoTHub.Portal.Server/Controllers/v1.0/DeviceTagSettingsController.cs` (Lines 1-89)
  - **Snippet**: Main REST API controller for device tag settings
    ```csharp
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("/api/settings/device-tags")]
    [ApiExplorerSettings(GroupName = "Portal Settings")]
    public class DeviceTagSettingsController : ControllerBase
    {
        private readonly ILogger<DeviceTagSettingsController> log;
        private readonly IDeviceTagService deviceTagService;
        
        [HttpPost(Name = "POST Update the Device tags settings")]
        [Authorize("device-tag:write")]
        public async Task<IActionResult> Post(IEnumerable<DeviceTagDto> tags)
        
        [HttpGet(Name = "GET Device tags settings")]
        [Authorize("device-tag:read")]
        public ActionResult<List<DeviceTagDto>> Get()
        
        [HttpPatch(Name = "Create or update a device tag")]
        [Authorize("device-tag:write")]
        public async Task<IActionResult> CreateOrUpdateDeviceTag([FromBody] DeviceTagDto deviceTag)
        
        [HttpDelete("{deviceTagName}", Name = "Delete a device tag by name")]
        [Authorize("device-tag:write")]
        public async Task<IActionResult> DeleteDeviceTagByName([FromRoute] string deviceTagName)
    }
    ```

### Business Logic
- `src/IoTHub.Portal.Application/Services/IDeviceTagService.cs` (Lines 1-20)
  - **Snippet**: Core service interface for device tag operations
    ```csharp
    public interface IDeviceTagService
    {
        IEnumerable<DeviceTagDto> GetAllTags();
        IEnumerable<string> GetAllTagsNames();
        IEnumerable<string> GetAllSearchableTagsNames();
        Task UpdateTags(IEnumerable<DeviceTagDto> tags);
        Task CreateOrUpdateDeviceTag(DeviceTagDto deviceTag);
        Task DeleteDeviceTagByName(string deviceTagName);
    }
    ```

- `src/IoTHub.Portal.Infrastructure/Services/DeviceTagService.cs` (Lines 1-114)
  - Concrete implementation of IDeviceTagService
  - Handles database operations for device tag CRUD
  - Provides filtered queries for searchable tags
  - Implements bulk update functionality (UpdateTags)
  - Individual tag create/update/delete operations

### Data Access
- `src/IoTHub.Portal.Domain/Repositories/IDeviceTagRepository.cs` (Lines 1-9)
  - Generic repository interface for DeviceTag entity
  - Inherits from IRepository<DeviceTag>

- `src/IoTHub.Portal.Domain/Repositories/IDeviceTagValueRepository.cs` (Lines 1-9)
  - Generic repository interface for DeviceTagValue entity
  - Manages actual tag values assigned to devices

- `src/IoTHub.Portal.Domain/Entities/DeviceTag.cs` (Lines 1-16)
  - **Snippet**: Device tag definition entity
    ```csharp
    public class DeviceTag : EntityBase
    {
        [NotMapped] public string Name => Id;  // Name is the entity Id
        public string Label { get; set; } = default!;
        public bool Required { get; set; }
        public bool Searchable { get; set; }
    }
    ```

- `src/IoTHub.Portal.Domain/Entities/DeviceTagValue.cs` (Lines 1-12)
  - **Snippet**: Device-specific tag value entity
    ```csharp
    public class DeviceTagValue : EntityBase
    {
        public string Name { get; set; } = default!;  // Tag name
        public string Value { get; set; } = default!; // Tag value for specific device
    }
    ```

### UI Components
- `src/IoTHub.Portal.Client/Pages/Settings/DeviceTagsPage.razor` (Lines 1-188)
  - Main device tag settings management page
  - Interactive MudTable with inline editing
  - Features:
    - Add new tags with validation
    - Edit tag label, required, and searchable properties
    - Delete tags with confirmation
    - Individual row validation and save
    - Duplicate name detection
    - Form validation for Name (alphanumeric only) and Label (required)
    - Refresh functionality
  - Accessible at route: `/settings/device-tag`
  - Authorization: `[Authorize]` required

- `src/IoTHub.Portal.Client/Models/DeviceTagModel.cs` (Lines 1-23)
  - UI model extending DeviceTagDto
  - Adds IsNewTag property to track newly added rows
  - Used for disabling name field editing on existing tags

### Data Transfer Objects
- `src/IoTHub.Portal.Shared/Models/v1.0/DeviceTagDto.cs` (Lines 1-38)
  - **Snippet**: Complete device tag DTO with validation
    ```csharp
    public class DeviceTagDto
    {
        [Required(ErrorMessage = "The tag should have a name.")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Name may only contain alphanumeric characters")]
        public string Name { get; set; } = default!;
        
        [Required(ErrorMessage = "The tag should have a label.")]
        public string Label { get; set; } = default!;
        
        [DefaultValue(false)]
        public bool Required { get; set; }
        
        [DefaultValue(false)]
        public bool Searchable { get; set; }
    }
    ```

### Client Services
- `src/IoTHub.Portal.Client/Services/IDeviceTagSettingsClientService.cs` (Lines 1-14)
  - Client-side service interface for HTTP API calls
  - Methods: GetDeviceTags, CreateOrUpdateDeviceTag, DeleteDeviceTagByName

- `src/IoTHub.Portal.Client/Services/DeviceTagSettingsClientService.cs` (Lines 1-34)
  - Implementation using HttpClient for API communication
  - PATCH endpoint for create/update operations
  - DELETE endpoint with tag name in route
  - GET endpoint for retrieving all tags

### Mappers
- `src/IoTHub.Portal.Application/Mappers/DeviceTagProfile.cs` (Lines 1-14)
  - AutoMapper profile for DeviceTag entity to DTO mapping
  - Maps DeviceTagDto.Name to DeviceTag.Id
  - Supports reverse mapping (bidirectional)

---

## API Endpoints

### Device Tag Settings Management
- `GET /api/settings/device-tags` - Get all device tag definitions
  - Returns: List<DeviceTagDto>
  - Authorization: device-tag:read
  - Used to populate the settings page and retrieve tags for device forms

- `POST /api/settings/device-tags` - Bulk update device tag settings (replaces all)
  - Body: IEnumerable<DeviceTagDto>
  - Returns: 200 OK
  - Authorization: device-tag:write
  - Deletes all existing tags and inserts new collection
  - Note: This endpoint is available but not currently used by the UI

- `PATCH /api/settings/device-tags` - Create or update a single device tag
  - Body: DeviceTagDto
  - Returns: 200 OK
  - Authorization: device-tag:write
  - Creates new tag if name doesn't exist, updates if it does
  - Used by UI for individual tag save operations

- `DELETE /api/settings/device-tags/{deviceTagName}` - Delete device tag by name
  - Route Parameter: deviceTagName (string)
  - Returns: 200 OK
  - Authorization: device-tag:write
  - Removes tag definition from system

---

## Authorization

### Required Permissions
- **device-tag:read** - View device tag settings and retrieve tag definitions
- **device-tag:write** - Create, update, and delete device tag settings

### Authorization Implementation
- Attribute-based authorization using `[Authorize("permission")]` attributes on controller methods
- Permission strings defined in PortalPermissionsHelper
- Base authorization requirement: `[Authorize]` on DeviceTagSettingsController and DeviceTagsPage
- Permissions managed through role-based access control (RBAC)
- Default Administrator role includes both device-tag:read and device-tag:write permissions

### Permission Mapping
- `PortalPermissions.DeviceTagRead` → "device-tag:read"
- `PortalPermissions.DeviceTagWrite` → "device-tag:write"

---

## Dependencies

### Internal Feature Dependencies
- **Device Management** - Device entities reference DeviceTagValue collection for storing actual tag values
- **Device Models** - Device model forms may use device tags for metadata collection
- **IoT Hub Integration** - Device tags synchronized to device twin tags in Azure IoT Hub or AWS IoT Core
- **Role-Based Access Control** - Permissions enforced through RBAC system

### Service Dependencies
- `IDeviceTagRepository` - Device tag definition persistence
- `IDeviceTagValueRepository` - Device-specific tag value persistence
- `IUnitOfWork` - Transaction management for database operations
- `IMapper` (AutoMapper) - Entity to DTO mapping

### Related Entities
- **Device** - Contains collection of DeviceTagValue objects
- **EdgeDevice** - Contains collection of DeviceTagValue objects
- **LorawanDevice** - Inherits device tags through Device base class

### External Dependencies
- **Entity Framework Core** - Database access via PortalDbContext
- **AutoMapper** - Entity to DTO mapping
- **FluentValidation** - DTO validation (via data annotations)

### UI Dependencies
- **MudBlazor** - UI component library
  - MudTable - Data grid with toolbar and inline editing
  - MudTextField - Text input with validation
  - MudCheckBox - Boolean input for Required/Searchable
  - MudIconButton - Action buttons (refresh, delete, save)
  - MudForm - Form validation container
  - MudSnackbar - User feedback notifications

---

## Key Features & Behaviors

### Tag Definition Management
- **Name**: Alphanumeric identifier stored in device twin (immutable after creation in UI)
- **Label**: Human-readable display name shown to users (can be edited)
- **Required**: Flag indicating whether tag must be provided during device creation
- **Searchable**: Flag indicating whether tag appears as filter option in device list

### Inline Table Editing
- Add new rows with "Add a new Tag" button
- Edit existing tags directly in table cells
- Individual row save operation with validation
- Delete button per row with immediate confirmation
- Name field disabled for existing tags (only editable when IsNewTag=true)
- Prevents adding new row if last row is incomplete

### Validation Rules
- **Name**: Required, alphanumeric only (regex: ^[a-zA-Z0-9]*$)
- **Label**: Required field
- **Duplicate Names**: Prevented through validation check before save
- **Form Validation**: Both Name and Label forms must be valid before save
- **Empty Values**: Cannot save tag with null/empty name or label

### Tag Usage in Device Management
- Searchable tags appear as filter options in device list search panel
- Required tags must be filled during device creation
- Tag values stored in DeviceTagValue collection per device
- Tags synchronized to IoT Hub device twin tags section
- Device queries can filter by tag name and value

### Database Operations
- **Create/Update**: Upsert operation based on tag name
- **Delete**: Cascade delete to DeviceTagValue when tag definition removed
- **Bulk Update**: Replace all tags operation available via POST endpoint
- **Query**: Filtered retrieval of searchable tags vs. all tags

### Error Handling
- ProblemDetailsException handling for API errors
- User-friendly error messages via Snackbar notifications
- Validation errors shown inline on form fields
- Loading state management during async operations

---

## Notes

### Architecture Patterns
- **Repository Pattern** - Clean separation of data access concerns
- **Unit of Work Pattern** - Transactional consistency across operations
- **Service Layer** - Business logic abstraction from controllers
- **DTO Pattern** - Data transfer between layers with validation

### Tag Name as Primary Key
- DeviceTag entity uses Name as Id (mapped via [NotMapped] property)
- Simplifies lookups and prevents duplicate tag names at database level
- Name immutability enforced through UI (disabled field for existing tags)

### Tag Value Storage
- DeviceTagValue is a separate entity from DeviceTag
- Enables many-to-many relationship: Device ↔ DeviceTagValue ↔ DeviceTag
- Cascade delete configured: deleting device removes associated DeviceTagValue records
- Tag values synchronized to IoT Hub device twin tags

### Integration with Device Twin
- Tags defined here appear in Azure IoT Hub device twin's tags section
- Searchable tags enable efficient device queries in IoT Hub
- Tag synchronization handled by device service implementations
- Supports both Azure IoT Hub and AWS IoT Core (via IExternalDeviceService)

### Multi-Cloud Support
- Tag definitions cloud-agnostic
- Tag values synchronized to cloud provider's device metadata
- Azure: Device Twin Tags
- AWS: Thing Attributes

### UI/UX Considerations
- Inline editing reduces clicks and page loads
- Individual row save allows partial updates
- Validation feedback immediate and contextual
- Refresh button to reload tags from server
- Loading indicators for async operations

### Performance Considerations
- Tags loaded once on page initialization
- No server-side pagination (reasonable tag count expected)
- Lightweight DTO for data transfer
- Efficient repository queries using GetAll()

### Security Considerations
- Authorization required at both controller and UI levels
- Permission-based access control prevents unauthorized modifications
- Input validation protects against injection attacks
- Name regex prevents special characters in tag names

### Database Migrations
- Initial DeviceTag entity: Migration 20220911135612
- DeviceTagValue entity: Migration 20220929174657
- Cascade delete configuration: Migration 20240503170119
- RBAC permissions: Migration 20260107100522

### Testing Coverage
- Unit tests: DeviceTagServiceTests.cs
- Controller tests: Not explicitly found but likely covered in integration tests
- UI tests: DeviceTagsPageTests.cs
- Client service tests: DeviceTagSettingsClientServiceTests.cs

### Future Enhancement Opportunities
- Tag value type support (string, number, boolean, enum)
- Tag value validation rules (regex, range, options)
- Tag groups/categories for better organization
- Tag templates for common device types
- Bulk tag assignment to multiple devices
- Tag usage analytics (which tags are most used)
- Tag value suggestions based on existing values
- Import/export tag definitions
- Tag versioning and audit trail
- Conditional tag visibility based on device model

### Known Limitations
- Tag names cannot be renamed after creation (must delete and recreate)
- No tag value constraints or validation beyond string type
- No tag dependencies or hierarchies
- Bulk POST endpoint exists but not used by UI
- No pagination on tags list (assumes reasonable count)
- Deleting tag definition doesn't prompt about affected devices
