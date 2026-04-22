# Feature: Role Management

**Category**: Security & Access Control  
**Status**: Analyzed  

---

## Description

The Role Management feature provides a comprehensive role-based access control (RBAC) system that enables administrators to define custom security roles with granular permission assignments. This feature serves as the foundation for the application's authorization infrastructure, allowing organizations to implement the principle of least privilege by creating roles that align with their organizational structure and security policies.

Key capabilities include:

- Creating and managing security roles with name, description, and color coding
- Assigning fine-grained permissions (actions) to roles from a predefined set of portal capabilities
- Filtering and searching permissions by resource type (device, edge-device, role, etc.) and action type (read, write, execute)
- Paginated role browsing with search functionality and sorting
- Visual role representation with customizable avatar colors
- Role detail viewing with complete permission listings
- Role editing with permission synchronization (add/remove/update)
- Role deletion with cascading action cleanup
- Integration with Azure AD/Entra ID for user principal management
- Permission validation throughout the application via authorization policies

This feature provides critical business value by:
- Enforcing security boundaries and access control policies
- Enabling flexible authorization models without code changes
- Supporting multi-tenancy and organizational hierarchy
- Audit trail through role-permission associations
- Simplifying compliance with security standards (ISO 27001, SOC 2)
- Reducing security risks through granular permission control

The RBAC system uses a two-level model: Roles contain Actions (permissions), and Users are assigned Roles through an access control service. Permissions are expressed as resource:action pairs (e.g., "device:read", "role:write") and mapped to ASP.NET Core authorization policies.

---

## Code Locations

### Entry Points / Endpoints
- `src/IoTHub.Portal.Server/Controllers/v1.0/RolesController.cs` (Lines 1-199)
  - **Snippet**: Main REST API controller for role management
    ```csharp
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/roles")]
    [ApiExplorerSettings(GroupName = "Role Management")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly ILogger<RolesController> logger;
        private readonly IRoleManagementService roleManagementService;
        
        [HttpGet(Name = "GetRoles")]
        [Authorize("role:read")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginationResult<RoleModel>))]
        public async Task<PaginationResult<RoleModel>> Get(
            [FromQuery] string searchKeyword = null,
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 0,
            [FromQuery] string[] orderBy = null)
        
        [HttpGet("{id}", Name = "GetRole")]
        [Authorize("role:read")]
        public async Task<IActionResult> GetRoleDetails(string id)
        
        [HttpPost(Name = "POST Create a Role")]
        [Authorize("role:write")]
        public async Task<IActionResult> CreateRoleAsync(RoleDetailsModel role)
        
        [HttpPut("{id}", Name = "PUT Edit Role")]
        [Authorize("role:write")]
        public async Task<IActionResult> EditRoleAsync(string id, RoleDetailsModel roleDetails)
        
        [HttpDelete("{id}", Name = "DELETE Role")]
        [Authorize("role:write")]
        public async Task<IActionResult> DeleteRole(string id)
    }
    ```

- `src/IoTHub.Portal.Server/Controllers/v1.0/PermissionsController.cs` (Lines 1-83)
  - **Snippet**: Controller for retrieving available permissions
    ```csharp
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/permissions")]
    [ApiExplorerSettings(GroupName = "Role Management")]
    public class PermissionsController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<PortalPermissions[]> Get()
        // Returns all available portal permissions for role assignment
        
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<PortalPermissions[]>> GetMyPermissions()
        // Returns permissions for the current authenticated user
    }
    ```

### Business Logic
- `src/IoTHub.Portal.Application/Services/IRoleManagementService.cs` (Lines 1-21)
  - **Snippet**: Core service interface for role operations
    ```csharp
    public interface IRoleManagementService
    {
        Task<PaginatedResult<RoleModel>> GetRolePage(
            string? searchKeyword = null,
            int pageSize = 10,
            int pageNumber = 0,
            string[] orderBy = null);
        Task<RoleDetailsModel> GetRoleDetailsAsync(string id);
        Task<RoleDetailsModel> CreateRole(RoleDetailsModel role);
        Task<RoleDetailsModel?> UpdateRole(string id, RoleDetailsModel role);
        Task<bool> DeleteRole(string id);
    }
    ```

- `src/IoTHub.Portal.Application/Services/RoleService.cs` (Lines 1-181)
  - Concrete implementation of IRoleManagementService
  - Handles database operations for role CRUD operations
  - Implements role search with keyword filtering (name, description)
  - Provides paginated role retrieval with sorting support
  - Manages role-action associations during create/update operations
  - Implements cascade delete for role and associated actions
  - Validates role name uniqueness during create and update
  - Uses predicate builder for flexible query construction
  - Key methods:
    - `GetRolePage`: Paginated retrieval with search and sort
    - `GetRoleDetailsAsync`: Single role with actions included
    - `CreateRole`: Create role with permission validation
    - `UpdateRole`: Update role and sync actions (add/remove)
    - `DeleteRole`: Remove role and cascade delete actions

### Data Access
- `src/IoTHub.Portal.Domain/Repositories/IRoleRepository.cs` (Lines 1-12)
  - Repository interface for Role entity operations
  - Extends generic IRepository<Role>
  - Custom method: `GetByNameAsync` for name-based lookup

- `src/IoTHub.Portal.Domain/Repositories/IActionRepository.cs` (Lines 1-10)
  - Repository interface for Action entity operations
  - Extends generic IRepository<Action>
  - Standard repository pattern methods

- `src/IoTHub.Portal.Domain/Entities/Role.cs` (Lines 1-19)
  - **Snippet**: Role entity definition
    ```csharp
    public class Role : EntityBase
    {
        [Required]
        public string Name { get; set; } = default!;
        public string Color { get; set; } = default!;  // Hex color code for UI
        public string? Description { get; set; } = default!;
        public virtual ICollection<Action> Actions { get; set; } = new Collection<Action>();
    }
    ```

- `src/IoTHub.Portal.Domain/Entities/Action.cs` (Lines 1-14)
  - **Snippet**: Action (permission) entity definition
    ```csharp
    public class Action : EntityBase
    {
        [Required]
        public string Name { get; set; } = default!;  // Permission string (e.g., "device:read")
    }
    ```

### UI Components
- `src/IoTHub.Portal.Client/Pages/RBAC/RolesListPage.razor` (Lines 1-146)
  - Main role listing page with server-side pagination
  - Features:
    - MudTable with sortable columns (Name, Description)
    - Color-coded role avatars showing first letter of role name
    - Refresh button to reload data
    - Add new role button (navigation to create page)
    - View details button per role
    - Delete button per role (conditional on write permission)
    - Delete confirmation dialog integration
    - Server-side pagination with configurable page sizes (3, 5, 10)
  - Accessible at route: `/roles`
  - Authorization: `[Authorize]` with PortalPermissions.RoleRead

- `src/IoTHub.Portal.Client/Pages/RBAC/RoleDetailPage.razor` (Lines 1-41)
  - Role detail and edit page
  - Features:
    - Displays role information and permissions
    - Reuses EditRole component for editing functionality
    - Conditional edit based on RoleWrite permission
    - Success notification on save
    - Navigation back to list on save
  - Accessible at route: `/roles/{RoleId}`
  - Authorization: `[Authorize]` with PortalPermissions.RoleRead

- `src/IoTHub.Portal.Client/Pages/RBAC/CreateRolePage.razor` (Lines 1-248)
  - Role creation page with comprehensive permission selection
  - Features:
    - Form with Name, Description, and Color picker
    - Dynamic avatar preview with role color
    - Permission selection with advanced filtering:
      - Filter by resource type (device, edge-device, role, etc.)
      - Filter by action type (read, write, execute)
      - Clear filters button
      - Select all (filtered) button
    - Permission counter showing selected/total
    - Grid layout for permission checkboxes (responsive)
    - Multi-selection text display
    - Loading states during permission fetch and save
    - Authentication state subscription for permission loading
  - Accessible at route: `/roles/new`
  - Authorization: `[Authorize]` with PortalPermissions.RoleWrite

- `src/IoTHub.Portal.Client/Components/Roles/EditRole.razor` (Lines 1-219)
  - Reusable component for role editing
  - Features:
    - Form with Name, Description, Color picker
    - Same permission filtering UI as create page
    - Pre-populates selected permissions from role
    - Dual mode: create or edit based on IsEdit parameter
    - Form validation before save
    - EventCallback for save completion notification
  - Used by: RoleDetailPage

### Data Transfer Objects
- `src/IoTHub.Portal.Shared/Models/v1.0/RoleModel.cs` (Lines 1-14)
  - **Snippet**: Lightweight role DTO for list view
    ```csharp
    public class RoleModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }  // Hex color code
        public string Description { get; set; }
    }
    ```

- `src/IoTHub.Portal.Shared/Models/v1.0/RoleDetailsModel.cs` (Lines 1-15)
  - **Snippet**: Complete role DTO with permissions
    ```csharp
    public class RoleDetailsModel
    {
        public string? Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Description { get; set; }
        public ICollection<string> Actions { get; set; } = new List<string>();
    }
    ```

### Client Services
- `src/IoTHub.Portal.Client/Services/IRoleClientService.cs` (Lines 1-24)
  - Client-side service interface for HTTP API calls
  - Methods:
    - GetRoles: Retrieve paginated role list
    - GetRole: Retrieve single role details
    - CreateRole: Create new role
    - UpdateRole: Update existing role
    - DeleteRole: Delete role by ID
    - GetPermissions: Retrieve all available permissions

- `src/IoTHub.Portal.Client/Services/RoleClientService.cs` (Lines 1-42)
  - Implementation using HttpClient for API communication
  - Uses named "api" HttpClient with auth header and error handling
  - Endpoints:
    - GET with continuation URI for pagination
    - GET `api/roles/{id}` for details
    - POST `api/roles` for creation
    - PUT `api/roles/{id}` for updates
    - DELETE `api/roles/{id}` for deletion
    - GET `api/permissions` for permission list

### Mappers
- `src/IoTHub.Portal.Application/Mappers/RoleProfile.cs` (Lines 1-43)
  - AutoMapper profile for Role entity and DTO mappings
  - Mappings:
    - Role ↔ RoleModel (bidirectional)
    - Role ↔ RoleDetailsModel (bidirectional)
    - Special handling for Actions collection:
      - Entity to DTO: Maps Action.Name to string collection
      - DTO to Entity: Creates Action entities from string collection
      - Update mapping: Ignores Id to preserve entity identity

### Security
- `src/IoTHub.Portal.Shared/Security/PortalPermissions.cs` (Lines 1-44)
  - Enum defining all available permissions in the portal
  - 42 permissions across 14 resource types
  - Permissions include: RoleRead, RoleWrite, UserRead, UserWrite, DeviceRead, DeviceWrite, etc.
  - Naming convention: {Resource}{Action} (e.g., DeviceRead, EdgeModelWrite)
  - Used throughout application for authorization checks

---

## API Endpoints

### Role Management
- `GET /api/roles` - Get paginated list of roles
  - Query Parameters:
    - searchKeyword (string, optional): Filter by role name or description
    - pageSize (int, default: 10): Number of roles per page
    - pageNumber (int, default: 0): Page number (zero-based)
    - orderBy (string[], optional): Sort criteria
  - Returns: PaginationResult<RoleModel> with Items, TotalItems, NextPage
  - Authorization: role:read
  - Features: Server-side search, pagination, sorting

- `GET /api/roles/{id}` - Get role details with permissions
  - Route Parameter: id (string) - Role unique identifier
  - Returns: RoleDetailsModel with Id, Name, Color, Description, Actions
  - Authorization: role:read
  - Returns: 200 OK with role details, 404 NotFound if role doesn't exist

- `POST /api/roles` - Create new role
  - Body: RoleDetailsModel (Name, Color, Description, Actions)
  - Returns: 201 Created with created role details
  - Authorization: role:write
  - Validation: Role name must be unique
  - Returns: 400 BadRequest on validation failure or duplicate name

- `PUT /api/roles/{id}` - Update existing role
  - Route Parameter: id (string) - Role unique identifier
  - Body: RoleDetailsModel (Name, Color, Description, Actions)
  - Returns: 200 OK with updated role details
  - Authorization: role:write
  - Features:
    - Updates scalar properties (name, color, description)
    - Synchronizes actions: removes absent ones, adds new ones
    - Allows keeping same name while changing permissions
    - Validates name uniqueness against other roles
  - Returns: 404 NotFound if role doesn't exist, 400 BadRequest on name conflict

- `DELETE /api/roles/{id}` - Delete role by ID
  - Route Parameter: id (string) - Role unique identifier
  - Returns: 200 OK on success
  - Authorization: role:write
  - Features: Cascade deletes associated actions
  - Returns: 404 NotFound if role doesn't exist

### Permission Management
- `GET /api/permissions` - Get all available permissions
  - Returns: PortalPermissions[] (array of enum values)
  - Authorization: [AllowAnonymous] - Public endpoint for UI initialization
  - Returns: All 42 portal permissions defined in PortalPermissions enum

- `GET /api/permissions/me` - Get current user's permissions
  - Returns: PortalPermissions[] (array of enum values)
  - Authorization: [Authorize] - Requires authenticated user
  - Features:
    - Retrieves user from Azure AD by email claim
    - Checks permission assignments through access control service
    - Returns filtered list of permissions user has access to
  - Returns: 401 Unauthorized if email claim missing

---

## Authorization

### Required Permissions
- **role:read** (PortalPermissions.RoleRead) - View roles, role details, and role lists
- **role:write** (PortalPermissions.RoleWrite) - Create, update, and delete roles

### Authorization Implementation
- Attribute-based authorization using `[Authorize("permission")]` attributes on controller methods
- Permission strings map to ASP.NET Core authorization policies
- Policies configured in startup to check user claims against role assignments
- Base authorization requirement: `[Authorize]` on RolesController and role pages
- Permissions managed through role-action associations in database
- UI components conditionally render based on permission checks:
  - Delete button hidden if user lacks RoleWrite permission
  - Edit functionality disabled if user lacks RoleWrite permission
  - Create page only accessible with RoleWrite permission

### Permission Mapping
- Enum values converted to policy strings via extension methods
- Format: lowercase resource:action (e.g., "role:read", "role:write")
- PortalPermissionsHelper provides conversion utilities
- Permission validation occurs at multiple layers:
  - Controller layer: via [Authorize("permission")] attributes
  - Service layer: implicit through controller validation
  - UI layer: via AuthorizedComponentBase.HasPermissionAsync()

### Access Control Flow
1. User authenticates via Azure AD/Entra ID
2. AccessControlService retrieves user's assigned roles
3. Roles loaded with associated Action entities (permissions)
4. Permission claims added to user's ClaimsPrincipal
5. Authorization middleware validates claims against policy requirements
6. Controller action executes if authorized, returns 403 Forbidden otherwise

---

## Dependencies

### Internal Feature Dependencies
- **User Management** - Roles assigned to users through access control service
- **Access Control Management** - Manages user-role associations and permission checks
- **Authentication** - Azure AD/Entra ID integration for user identity
- **Authorization Policies** - ASP.NET Core policy-based authorization

### Service Dependencies
- `IRoleRepository` - Role entity persistence and queries
- `IActionRepository` - Action entity persistence and deletion
- `IUnitOfWork` - Transaction management for database operations
- `IMapper` (AutoMapper) - Entity to DTO mapping
- `IAccessControlManagementService` - User permission validation
- `IUserManagementService` - User retrieval and creation

### Related Entities
- **User** - Users are assigned roles through access control
- **AccessControl** - Junction entity linking users to roles
- **Action** - Permissions owned by roles
- **Role** - Parent entity containing actions

### External Dependencies
- **Entity Framework Core** - Database access via PortalDbContext
- **AutoMapper** - Entity to DTO mapping
- **Azure AD/Entra ID** - Authentication and user directory
- **ASP.NET Core Authorization** - Policy-based authorization framework

### UI Dependencies
- **MudBlazor** - UI component library
  - MudTable - Data grid with server-side pagination and sorting
  - MudAvatar - Role visual representation with color coding
  - MudTextField - Text input for name and description
  - MudColorPicker - Color selection for role avatars
  - MudCheckBox - Permission selection
  - MudSelect - Dropdown filters for resource and action types
  - MudChip - Permission counter display
  - MudButton - Action buttons (save, refresh, delete)
  - MudSnackbar - User feedback notifications
  - MudForm - Form validation container
  - MudExpansionPanel - Collapsible sections
  - MudTabs - Tabbed interface
- **AuthorizedComponentBase** - Base class for permission checks in UI

---

## Key Features & Behaviors

### Role Definition
- **Name**: Required, unique identifier for the role
- **Color**: Hex color code for visual differentiation in UI
- **Description**: Optional text describing role purpose
- **Actions**: Collection of permission strings assigned to the role

### Permission Model
- Permissions expressed as resource:action pairs (e.g., "device:read")
- 14 resource types: device, edge-device, role, user, model, edge-model, device-tag, planning, schedule, layer, concentrator, access-control, dashboard, idea, setting, device-configuration, group
- 3 action types: read, write, execute (execute only for devices)
- Special permissions: DeviceImport, DeviceExport, IdeaWrite
- Total 42 permissions available for assignment

### Role Creation Workflow
1. User navigates to create page (/roles/new)
2. System loads available permissions from API
3. User enters role name, description, and selects color
4. User selects permissions via checkbox grid with optional filters
5. System validates form (name and permission selection required)
6. API validates role name uniqueness
7. Service creates Role entity with associated Action entities
8. Transaction commits and returns created role
9. UI navigates to role list with success notification

### Role Update Workflow
1. User navigates to role detail page (/roles/{id})
2. System loads role details including current permissions
3. EditRole component pre-populates form with current values
4. User modifies name, description, color, or permissions
5. System validates form and checks name uniqueness (excluding self)
6. Service synchronizes actions:
   - Removes actions not in new set
   - Adds new actions not previously present
   - Preserves unchanged actions
7. Transaction commits and returns updated role
8. UI navigates to role list with success notification

### Role Deletion Workflow
1. User clicks delete button on role list
2. System displays confirmation dialog with role ID
3. On confirmation, API deletes role entity
4. Service cascade deletes associated action entities
5. Transaction commits
6. UI refreshes table to reflect deletion
7. Returns: 404 NotFound if role already deleted/doesn't exist

### Pagination Implementation
- Server-side pagination via PagedList pattern
- Client specifies page number and page size
- Server returns: Items, TotalItems, NextPage URL
- NextPage URL includes all query parameters for continuation
- Supports sorting via orderBy parameter (array of field names)
- Default page size: 10, configurable in UI (3, 5, 10)

### Search and Filter
- **Role Search**: Keyword-based search on role name and description (case-insensitive)
- **Permission Filter**: Client-side filtering by:
  - Resource type: Extracted from permission string before colon
  - Action type: Extracted from permission string after colon
  - Combined filters: Both resource AND action must match
- Search predicate built using PredicateBuilder pattern
- Filters reset via Clear Filters button
- Select All applies only to filtered permissions

### Permission Selection UI
- Grid layout showing all permissions as checkboxes
- Resource and action dropdown filters for permission subset
- Permission counter chip showing selected/total count
- Select All button to check all filtered permissions
- Real-time multi-selection text updates
- Loading indicator during permission fetch
- Authentication state subscription for delayed auth scenarios
- Empty state message when filters match no permissions

### Database Operations
- **Create**: Insert role with associated action entities in single transaction
- **Update**: Update role and sync actions (add/remove) in single transaction
- **Delete**: Cascade delete role and all associated actions
- **Query**: 
  - Paginated with search and sort via GetPaginatedListAsync
  - Single with includes via GetByIdAsync(id, r => r.Actions)
  - Name lookup via GetByNameAsync for uniqueness checks
- All operations use Unit of Work pattern for transactional consistency

### Error Handling
- ProblemDetailsException handling for API errors
- User-friendly error messages via Snackbar notifications
- Form validation errors shown inline
- Loading state management during async operations
- Try-catch blocks in service methods with logging
- 404 responses for non-existent roles
- 400 responses for validation failures (duplicate name, etc.)
- Exception rethrowing to preserve stack traces

### Validation Rules
- **Name**: Required, must be unique across all roles
- **Description**: Optional
- **Color**: Required (default value likely set in UI)
- **Actions**: Optional (role can exist without permissions)
- **Update Name Uniqueness**: Excludes current role from uniqueness check

---

## Notes

### Architecture Patterns
- **Repository Pattern** - Clean separation of data access concerns
- **Unit of Work Pattern** - Transactional consistency across operations
- **Service Layer** - Business logic abstraction from controllers
- **DTO Pattern** - Data transfer between layers with validation
- **Authorization Policy Pattern** - Centralized permission management
- **Component Composition** - EditRole component reused for create and edit

### Role-Permission Model Design
- Two-level hierarchy: Role → Action (permission)
- Actions stored as separate entities (not embedded collection)
- Enables:
  - Audit trail of permission changes
  - Flexible permission queries
  - Role-permission history tracking
  - Potential for permission descriptions/metadata
- Alternative design consideration: Embedded string collection would simplify queries but reduce flexibility

### Permission String Format
- Convention: lowercase resource:action (e.g., "device:read")
- Benefits:
  - Human-readable and intuitive
  - Easy parsing for UI filtering
  - Consistent with RESTful conventions
  - Supports wildcard patterns (future enhancement)
- Stored as strings in Action.Name field
- Mapped to PortalPermissions enum for type safety in code

### Name Uniqueness Enforcement
- Database-level: Unique constraint on Role.Name (likely)
- Application-level: GetByNameAsync check before create
- Update-level: Exclude self from uniqueness check (id != currentId)
- Prevents duplicate roles and ensures role names serve as secondary key

### Action Synchronization Strategy
- On update, service compares new action list with existing
- Removes: Actions in DB but not in new list
- Adds: Actions in new list but not in DB
- Preserves: Actions in both lists (no duplicate insert)
- Uses LINQ queries: Where, Any, FirstOrDefault
- Cascade delete handled via actionRepository.Delete

### Color Coding for Roles
- Hex color codes stored in database
- Used for:
  - Role avatar background in list view
  - Role name text color in list view
  - Avatar preview in create/edit forms
- Enhances visual recognition and user experience
- MudColorPicker provides color selection UI

### Pagination Performance
- Server-side pagination reduces client memory usage
- Essential for large role catalogs
- Continuation URI pattern for stateless pagination
- NextPage URL includes all query parameters
- Client can bookmark/share specific pages

### Permission Loading Strategy
- Permissions loaded once on page initialization
- Cached in component state for session duration
- AllowAnonymous on permissions endpoint enables pre-auth loading
- Authentication state subscription handles delayed auth scenarios
- Prevents multiple redundant API calls during permission selection

### Integration with Access Control
- Roles assigned to users via AccessControl entity
- AccessControlService checks user permissions at runtime
- Permission claims populated during authentication
- Supports multiple roles per user (union of permissions)
- Role changes propagate to users on next auth token refresh

### Security Considerations
- Authorization required at controller and UI levels
- Permission-based access prevents unauthorized modifications
- Role names stored as plain text (no sensitive data)
- Action names (permissions) are system-defined (not user input)
- Cascade delete prevents orphaned actions
- Role deletion doesn't check for assigned users (consider adding warning)

### UI/UX Considerations
- Server-side pagination for scalability
- Color-coded avatars for visual differentiation
- Sortable columns for role discovery
- Search across name and description for flexibility
- Permission filtering reduces cognitive load during selection
- Inline delete confirmation prevents accidental deletion
- Success notifications provide feedback
- Loading indicators for async operations
- Responsive grid layout for permission selection

### Performance Considerations
- Paginated role queries prevent full table scans
- Lazy loading not used (explicit includes for actions)
- Predicate building enables efficient WHERE clauses
- Indexed searches on name and description (database level)
- Client-side permission filtering avoids server round trips
- Role list loads without actions (lightweight RoleModel)
- Role details loads with actions (RoleDetailsModel with includes)

### Multi-Tenancy Considerations
- Current implementation: Single-tenant (shared role catalog)
- All users see same roles (permissions filter access)
- Future enhancement: Organization-level role isolation
- Role names unique globally (not per-organization)

### Database Migrations
- Role entity: Migration date unknown (likely early in project)
- Action entity: Separate migration for permission system
- RBAC schema: Migration 20260107100522 (recent)
- Cascade delete configuration: Likely in entity configuration

### Testing Coverage
- Unit tests: RoleServiceTests.cs (likely exists)
- Controller tests: Integration tests for API endpoints
- UI tests: RolesListPageTests.cs, RoleDetailPageTests.cs (likely)
- Client service tests: RoleClientServiceTests.cs (likely)
- Permission tests: Authorization policy tests

### Known Limitations
- Role names cannot be easily renamed if already assigned to users (consider user impact)
- No role templates or quick-start roles
- No role cloning/duplication feature
- No bulk role operations
- No role hierarchy or inheritance
- No role-level metadata (created date, modified date, created by)
- Deleting role doesn't warn about assigned users
- No role assignment preview or impact analysis
- Permission descriptions not shown in UI (only names)
- No permission grouping or categorization in UI

### Future Enhancement Opportunities
- Role templates (Admin, Editor, Viewer, etc.)
- Role cloning to create similar roles quickly
- Role assignment impact analysis (show users with role before delete)
- Role audit trail (track permission changes over time)
- Role hierarchy with permission inheritance
- Role metadata (created date, modified date, created by)
- Permission descriptions and help text in UI
- Permission grouping/categories for better organization
- Bulk permission operations (assign multiple, revoke multiple)
- Role usage analytics (most/least used roles)
- Permission usage analytics (most/least used permissions)
- Import/export roles for migration scenarios
- Role versioning for rollback capability
- Conditional permissions (e.g., device:write only for own devices)
- Time-based role assignments (temporary elevated access)
- Role approval workflow for sensitive permissions
- Permission delegation (allow role owners to assign sub-permissions)
- Role recommendations based on user behavior
- Integration with external identity providers (Okta, Auth0)
- Custom permission definitions (beyond predefined set)
- Dynamic permission evaluation (context-aware permissions)

### Design Decisions and Trade-offs
- **Actions as Entities vs. Embedded Collection**:
  - Chosen: Separate Action entities with foreign key to Role
  - Benefit: Audit trail, extensibility, normalized schema
  - Trade-off: Additional table, more complex queries, cascade delete complexity
  
- **Server-side vs. Client-side Pagination**:
  - Chosen: Server-side pagination for roles
  - Benefit: Scalability, reduced payload, better performance
  - Trade-off: More complex implementation, requires continuation URI pattern
  
- **AllowAnonymous on Permissions Endpoint**:
  - Chosen: Allow unauthenticated access to permission list
  - Benefit: UI can load permissions before auth completes
  - Trade-off: Exposes permission names (low security risk, permissions are policies not secrets)
  
- **Permission Name Format (enum vs. string)**:
  - Chosen: Enum in code, string in database
  - Benefit: Type safety in code, flexibility in database, easy parsing
  - Trade-off: Manual synchronization between enum and permission strings
  
- **Role Name as Unique Identifier**:
  - Chosen: Separate ID field, unique constraint on name
  - Benefit: Allows name changes without breaking references
  - Trade-off: Additional uniqueness check during create/update

### Localization Notes
- UI text in French in some components (RoleDetailPage, EditRole)
- English in RolesListPage and CreateRolePage
- Inconsistent localization suggests internationalization in progress
- Localization key opportunities:
  - Permission names and descriptions
  - Error messages and validation text
  - Button labels and UI text
  - Snackbar notifications

### Accessibility Considerations
- MudBlazor components provide ARIA attributes
- Color-coded avatars include text (first letter) for color-blind users
- Tooltips on icon buttons provide context
- Form labels properly associated with inputs
- Loading states announced via ARIA live regions
- Consider: Keyboard navigation for permission checkboxes
- Consider: Screen reader announcements for permission counts
