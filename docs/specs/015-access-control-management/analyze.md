# Feature: Access Control Management

**Category**: Security & Access Control  
**Status**: Analyzed  

---

## Description

The Access Control Management feature provides a comprehensive authorization bridge that maps users (principals) and roles to specific resource scopes within the IoT Hub Portal. This feature serves as the critical junction between the authentication system (Azure AD/Entra ID), role definitions, and actual resource access, enabling fine-grained, scope-based permission management across the application.

Key capabilities include:

- Creating access control entries that link principals (users) to roles with specific scopes
- Managing user-role associations through a principal-based model
- Supporting wildcard scopes ("*") for organization-wide access or specific resource scopes
- Paginated access control browsing with search and filtering by principal
- Direct permission validation for authorization decisions
- Integration with Azure AD/Entra ID principals for user identity
- Cascading permission inheritance through role-action associations
- Real-time access control management through user detail pages
- Flexible scope-based authorization supporting multi-tenancy patterns

This feature provides critical business value by:
- Enforcing granular access control at the principal-role-scope level
- Enabling dynamic permission assignment without code deployments
- Supporting organizational hierarchies and resource isolation
- Facilitating compliance with access control requirements (GDPR, HIPAA, SOC 2)
- Providing audit trail through principal-role-scope associations
- Enabling flexible authorization models (role-based, scope-based, hybrid)
- Reducing security risks through explicit permission grants
- Supporting temporary and permanent role assignments
- Enabling self-service role management through user interfaces

The access control system uses a three-level model: Principals (users/identities) are assigned Roles with specific Scopes. Roles contain Actions (permissions) defined elsewhere. At runtime, the system checks if a principal has the required permission by traversing: Principal → AccessControl → Role → Actions. Scopes enable resource-level isolation, allowing the same role to apply to different resource boundaries.

---

## Code Locations

### Entry Points / Endpoints
- `src/IoTHub.Portal.Server/Controllers/v1.0/AccessControlController.cs` (Lines 1-179)
  - **Snippet**: Main REST API controller for access control management
    ```csharp
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/access-controls")]
    [ApiExplorerSettings(GroupName = "Access Control Management")]
    [ApiController]
    public class AccessControlController : ControllerBase
    {
        private readonly ILogger<AccessControlController> logger;
        private readonly IAccessControlManagementService service;
        
        [HttpGet(Name = "Get Pagined Access Control")]
        [Authorize("access-control:read")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginationResult<AccessControlModel>))]
        public async Task<PaginationResult<AccessControlModel>> Get(
            string searchKeyword = null,
            int pageSize = 10,
            int pageNumber = 0,
            [FromQuery] string[] orderBy = null,
            [FromQuery] string? principalId = null)
        
        [HttpGet("{id}", Name = "Get AccessControl By Id")]
        [Authorize("access-control:read")]
        public async Task<IActionResult> GetACById(string id)
        
        [HttpPost(Name = "POST Create a AccessControl")]
        [Authorize("access-control:write")]
        public async Task<IActionResult> CreateAccessControlAsync(AccessControlModel accessControl)
        
        [HttpPut("{id}", Name = "PUT Edit AccessControl")]
        [Authorize("access-control:write")]
        public async Task<IActionResult> EditAccessControlAsync(string id, AccessControlModel accessControl)
        
        [HttpDelete("{id}")]
        [Authorize("access-control:write")]
        public async Task<IActionResult> DeleteAccessControl(string id)
    }
    ```

### Business Logic
- `src/IoTHub.Portal.Application/Services/IAccessControlManagementService.cs` (Lines 1-25)
  - **Snippet**: Core service interface for access control operations
    ```csharp
    public interface IAccessControlManagementService
    {
        Task<PaginatedResult<AccessControlModel>> GetAccessControlPage(
            string? searchKeyword = null,
            int pageSize = 10,
            int pageNumber = 0,
            string[] orderBy = null,
            string? principalId = null);
        Task<AccessControlModel> GetAccessControlAsync(string Id);
        Task<AccessControlModel> CreateAccessControl(AccessControlModel role);
        Task<AccessControlModel?> UpdateAccessControl(string id, AccessControlModel accessControl);
        Task<bool> DeleteAccessControl(string id);
        Task<bool> UserHasPermissionAsync(string principalId, string permission);
    }
    ```

- `src/IoTHub.Portal.Application/Services/AccessControlService.cs` (Lines 1-189)
  - Concrete implementation of IAccessControlManagementService
  - Handles database operations for access control CRUD operations
  - Implements access control search with keyword filtering on scope and role name
  - Provides paginated access control retrieval with sorting and principal filtering
  - Validates principal and role existence during create/update operations
  - Implements permission checking through role-action traversal
  - Key methods:
    - `GetAccessControlPage`: Paginated retrieval with search, sort, and principal filter
    - `GetAccessControlAsync`: Single access control with role included
    - `CreateAccessControl`: Create access control with principal/role validation
    - `UpdateAccessControl`: Update principal, role, or scope assignments
    - `DeleteAccessControl`: Remove access control entry
    - `UserHasPermissionAsync`: Runtime permission validation for authorization

### Data Access
- `src/IoTHub.Portal.Domain/Repositories/IAccessControlRepository.cs` (Lines 1-11)
  - Repository interface for AccessControl entity operations
  - Extends generic IRepository<AccessControl>
  - Standard repository pattern methods (no custom methods)

- `src/IoTHub.Portal.Domain/Repositories/IPrincipalRepository.cs` (Lines 1-11)
  - Repository interface for Principal entity operations
  - Extends generic IRepository<Principal>
  - Used for principal existence validation

- `src/IoTHub.Portal.Domain/Entities/AccessControl.cs` (Lines 1-16)
  - **Snippet**: AccessControl entity definition
    ```csharp
    public class AccessControl : EntityBase
    {
        public string Scope { get; set; } = default!;
        public string RoleId { get; set; } = default!;
        public virtual Role Role { get; set; }
        public string PrincipalId { get; set; }
        public virtual Principal Principal { get; set; }
    }
    ```

- `src/IoTHub.Portal.Domain/Entities/Principal.cs` (Lines 1-18)
  - **Snippet**: Principal entity definition
    ```csharp
    public class Principal : EntityBase
    {
        [Required]
        public virtual ICollection<AccessControl> AccessControls { get; set; } = new Collection<AccessControl>();
        public virtual User? User { get; set; }
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
- `src/IoTHub.Portal.Client/Pages/RBAC/UserDetailPage.razor` (Lines 1-183)
  - User detail page with embedded access control management
  - Features:
    - Displays user information (given name, family name, email)
    - Shows access controls (role assignments) in table format
    - Add role dropdown with scope field (defaults to "*" - all scope)
    - Role assignment with real-time table updates
    - Remove role button per access control entry
    - Conditional UI based on user:write permission
    - Loading states and error handling
    - Server-side data loading with safe API calls
  - Accessible at route: `/users/{UserId}`
  - Authorization: `[Authorize]` with PortalPermissions.UserRead
  - Write operations require: PortalPermissions.UserWrite

### Data Transfer Objects
- `src/IoTHub.Portal.Shared/Models/v1.0/AccessControlModel.cs` (Lines 1-15)
  - **Snippet**: Access control DTO
    ```csharp
    public class AccessControlModel
    {
        public string Id { get; set; }
        public string PrincipalId { get; set; }
        public string Scope { get; set; } = default!;
        public RoleModel Role { get; set; } = default!;
    }
    ```

- `src/IoTHub.Portal.Shared/Models/v1.0/Filters/AccessControlFilter.cs` (Lines 1-12)
  - **Snippet**: Filter DTO for access control pagination
    ```csharp
    public class AccessControlFilter : PaginationFilter
    {
        public string Keyword { get; set; } = default!;
    }
    ```

### Client Services
- `src/IoTHub.Portal.Client/Services/IAccessControlClientService.cs` (Lines 1-15)
  - Client-side service interface for HTTP API calls
  - Methods:
    - GetAccessControls: Retrieve paginated access control list
    - Create: Create new access control
    - Delete: Delete access control by ID

- `src/IoTHub.Portal.Client/Services/AccessControlClientService.cs` (Lines 1-33)
  - Implementation using HttpClient for API communication
  - Uses named "api" HttpClient with auth header and error handling
  - Endpoints:
    - GET with continuation URI for pagination
    - POST `api/access-controls` for creation
    - DELETE `api/access-controls/{id}` for deletion

### Mappers
- `src/IoTHub.Portal.Application/Mappers/AccessControlProfile.cs` (Lines 1-34)
  - AutoMapper profile for AccessControl entity and DTO mappings
  - Mappings:
    - AccessControl → AccessControlModel (entity to DTO)
      - Maps Id, PrincipalId, Scope directly
      - Maps Role navigation property to RoleModel (Id, Name only)
      - Handles null Role gracefully
    - AccessControlModel → AccessControl (DTO to entity)
      - Ignores Id (generated by database)
      - Maps PrincipalId and Scope directly
      - Maps Role.Id to RoleId (foreign key)
      - Ignores Role navigation property (loaded separately)

---

## API Endpoints

### Access Control Management
- `GET /api/access-controls` - Get paginated list of access controls
  - Query Parameters:
    - searchKeyword (string, optional): Filter by scope or role name (case-insensitive)
    - pageSize (int, default: 10): Number of access controls per page
    - pageNumber (int, default: 0): Page number (zero-based)
    - orderBy (string[], optional): Sort criteria
    - principalId (string, optional): Filter by specific principal
  - Returns: PaginationResult<AccessControlModel> with Items, TotalItems, NextPage
  - Authorization: access-control:read
  - Features: Server-side search, pagination, sorting, principal filtering

- `GET /api/access-controls/{id}` - Get access control details
  - Route Parameter: id (string) - Access control unique identifier
  - Returns: AccessControlModel with Id, PrincipalId, Scope, Role
  - Authorization: access-control:read
  - Includes: Role navigation property for display
  - Returns: 200 OK with details, 404 NotFound if doesn't exist

- `POST /api/access-controls` - Create new access control
  - Body: AccessControlModel (PrincipalId, Role, Scope)
  - Returns: 200 OK with created access control details
  - Authorization: access-control:write
  - Validation:
    - Principal must exist in database
    - Role must exist in database
    - Scope is required (typically "*" for all access)
  - Returns: 400 BadRequest on validation failure

- `PUT /api/access-controls/{id}` - Update existing access control
  - Route Parameter: id (string) - Access control unique identifier
  - Body: AccessControlModel (PrincipalId, Role, Scope)
  - Returns: 200 OK with updated access control details
  - Authorization: access-control:write
  - Features:
    - Updates principal assignment (re-assign to different user)
    - Updates role assignment (change role)
    - Updates scope (change resource boundary)
    - Validates principal and role existence
  - Returns: 404 NotFound if access control doesn't exist, 400 BadRequest on validation failure

- `DELETE /api/access-controls/{id}` - Delete access control by ID
  - Route Parameter: id (string) - Access control unique identifier
  - Returns: 200 OK on success
  - Authorization: access-control:write
  - Features: Removes user's role assignment for specified scope
  - Returns: 404 NotFound if access control doesn't exist

---

## Authorization

### Required Permissions
- **access-control:read** (PortalPermissions.AccessControlRead) - View access controls and assignments
- **access-control:write** (PortalPermissions.AccessControlWrite) - Create, update, and delete access controls
- **user:read** (PortalPermissions.UserRead) - View user detail page (where access controls are managed)
- **user:write** (PortalPermissions.UserWrite) - Modify user role assignments

### Authorization Implementation
- Attribute-based authorization using `[Authorize("permission")]` attributes on controller methods
- Permission strings map to ASP.NET Core authorization policies
- Policies configured in startup to check user claims against access control assignments
- Base authorization requirement: `[Authorize]` on AccessControlController
- UI components conditionally render based on permission checks:
  - Add role UI disabled if user lacks UserWrite permission
  - Remove role button hidden if user lacks UserWrite permission
  - Access control management only accessible through authorized user pages

### Permission Validation Flow
- Runtime permission checking via `UserHasPermissionAsync` method
- Process:
  1. Retrieve all access controls for the principal (user)
  2. Include Role and Role.Actions navigation properties
  3. Iterate through each access control's role
  4. Check if any role's actions contain the requested permission (case-insensitive)
  5. Return true if permission found, false otherwise
- Used by authorization middleware and explicit permission checks
- Supports scope-based filtering (future enhancement)

### Access Control Flow
1. User authenticates via Azure AD/Entra ID (obtains principal identity)
2. Principal entity created or retrieved from database
3. AccessControl entities link principal to roles with scopes
4. Roles loaded with associated Action entities (permissions)
5. Authorization middleware invokes UserHasPermissionAsync
6. Permission validation traverses: Principal → AccessControl → Role → Actions
7. Controller action executes if authorized, returns 403 Forbidden otherwise
8. Scope currently not evaluated in permission check (all scopes treated equally)

---

## Dependencies

### Internal Feature Dependencies
- **Role Management** - Defines roles and their associated permissions (actions)
- **User Management** - Manages user entities linked to principals
- **Authentication** - Azure AD/Entra ID integration for principal identity
- **Authorization Policies** - ASP.NET Core policy-based authorization
- **Principal Management** - Manages principal entities representing identities

### Service Dependencies
- `IAccessControlRepository` - Access control entity persistence and queries
- `IPrincipalRepository` - Principal entity validation and retrieval
- `IRoleRepository` - Role entity validation and retrieval
- `IUnitOfWork` - Transaction management for database operations
- `IMapper` (AutoMapper) - Entity to DTO mapping
- `IUserManagementService` - User retrieval for UI display

### Related Entities
- **Principal** - Identity entity linking to access controls and users
- **User** - Portal user entity linked to principal
- **Role** - Security role containing permissions
- **Action** - Individual permission within a role
- **AccessControl** - Junction entity linking principal, role, and scope

### External Dependencies
- **Entity Framework Core** - Database access via PortalDbContext
- **AutoMapper** - Entity to DTO mapping
- **Azure AD/Entra ID** - Authentication and principal identity management
- **ASP.NET Core Authorization** - Policy-based authorization framework

### UI Dependencies
- **MudBlazor** - UI component library
  - MudTextField - Text input for user details and scope
  - MudSelect - Dropdown for role selection
  - MudTable - Data grid for access control list
  - MudButton - Action buttons (add, delete)
  - MudIconButton - Icon-based delete action
  - MudStack - Layout for role assignment UI
  - MudDivider - Visual separation
  - MudPaper - Container component
  - MudSnackbar - User feedback notifications
- **AuthorizedComponentBase** - Base class for permission checks in UI

---

## Key Features & Behaviors

### Access Control Model
- **PrincipalId**: Unique identifier for the user/identity (from Azure AD)
- **RoleId**: Foreign key to Role entity defining permissions
- **Scope**: Resource boundary filter (e.g., "*" for all, specific resource IDs)
- **Role**: Navigation property to Role entity with actions

### Scope-Based Authorization
- Scopes define resource boundaries for role application
- Wildcard scope ("*"): Role applies to all resources organization-wide
- Specific scope: Role applies only to specified resource (future implementation)
- Current implementation: Scope stored but not enforced in permission checks
- Future enhancement: Scope-aware permission validation
- Use cases:
  - Organization-wide admin: scope = "*"
  - Department admin: scope = "department-123"
  - Device group manager: scope = "device-group-456"
  - Single device operator: scope = "device-789"

### Access Control Creation Workflow
1. User navigates to user detail page (/users/{userId})
2. System loads user details and existing access controls
3. System loads available roles from API (all roles in system)
4. User selects role from dropdown
5. Scope defaults to "*" (all access) - currently not editable in UI
6. User clicks Add button
7. API validates principal existence (linked to user)
8. API validates role existence
9. Service creates AccessControl entity linking principal, role, and scope
10. Transaction commits and returns created access control
11. UI refreshes access control table with new entry
12. Success feedback via loading state (no explicit notification)

### Access Control Deletion Workflow
1. User views access control list on user detail page
2. User clicks delete icon button for specific access control
3. API deletes access control entity by ID
4. Transaction commits
5. UI removes entry from table
6. Success feedback via loading state
7. Returns: 404 NotFound if access control already deleted

### Permission Validation Workflow
1. User attempts to access protected resource or action
2. Authorization middleware extracts principal ID from claims
3. System calls UserHasPermissionAsync with principalId and permission
4. Service queries all access controls for principal
5. Service includes Role and Role.Actions in query (eager loading)
6. Service iterates through each access control:
   - Checks if Role is not null
   - Checks if Role.Actions is not null
   - Searches for action matching permission (case-insensitive)
7. Returns true if any access control grants permission
8. Authorization middleware allows/denies request based on result

### Pagination Implementation
- Server-side pagination via PagedList pattern
- Client specifies page number and page size
- Server returns: Items, TotalItems, NextPage URL
- NextPage URL includes all query parameters for continuation
- Supports sorting via orderBy parameter (array of field names)
- Default page size: 10
- Principal filtering enables user-specific access control views

### Search and Filter
- **Keyword Search**: Searches scope and role name (case-insensitive)
- **Principal Filter**: Filters access controls by specific principal ID
- **Combined Filters**: Keyword AND principal filters can be combined
- Search predicate built using PredicateBuilder pattern
- Filters applied at database level (not client-side)
- Used primarily for user-specific views (filter by user's principal)

### Database Operations
- **Create**: Insert access control with principal/role validation
- **Update**: Update principal, role, or scope assignments
- **Delete**: Remove access control entry (no cascade effects)
- **Query**:
  - Paginated with search and sort via GetPaginatedListAsync
  - Single with includes via GetByIdAsync(id, ac => ac.Role)
  - Principal-filtered queries for user-specific views
  - Permission checks with deep includes (Role, Role.Actions)
- All operations use Unit of Work pattern for transactional consistency

### Principal-User Relationship
- Principal: Abstract identity entity (can represent user, service, group)
- User: Concrete portal user entity with profile information
- Relationship: One-to-one optional (Principal.User can be null)
- Access controls attached to Principal (not directly to User)
- Benefits:
  - Supports non-user identities (service accounts, API keys)
  - Separates authentication identity from user profile
  - Enables identity federation (multiple auth providers)
- UI limitation: Access controls currently managed only through User pages

### Error Handling
- ProblemDetailsException handling for API errors
- User-friendly error messages via Snackbar notifications
- Loading state management during async operations
- Try-catch blocks in service methods with logging
- ResourceNotFoundException for missing principals or roles
- 404 responses for non-existent access controls
- 400 responses for validation failures
- Safe API call pattern in UI prevents unhandled exceptions

### Validation Rules
- **PrincipalId**: Required, must exist in database
- **RoleId**: Required, must exist in database
- **Scope**: Required, defaults to "*" in UI
- **Uniqueness**: No explicit uniqueness constraint (multiple roles per principal allowed)
- **Update Validation**: Principal and role must exist when updating

---

## Notes

### Architecture Patterns
- **Repository Pattern** - Clean separation of data access concerns
- **Unit of Work Pattern** - Transactional consistency across operations
- **Service Layer** - Business logic abstraction from controllers
- **DTO Pattern** - Data transfer between layers
- **Authorization Policy Pattern** - Centralized permission management
- **Eager Loading** - Navigation properties loaded explicitly with includes
- **Predicate Builder Pattern** - Dynamic query construction

### Access Control Model Design
- Three-level hierarchy: Principal → AccessControl → Role → Action
- AccessControl as junction entity with additional scope attribute
- Benefits:
  - Many-to-many relationship between principals and roles
  - Scope attribute enables resource-level isolation
  - Flexible permission model supporting various authorization patterns
  - Clear separation between identity (principal), assignment (access control), and permissions (role/actions)
- Alternative designs considered:
  - Direct principal-role many-to-many: Simpler but no scope support
  - Embedded role collection in principal: Less normalized, harder to query
  - Separate permission entities: More granular but more complex

### Scope Attribute Design
- Current implementation: Scope stored in database but not enforced
- Wildcard scope "*": Convention for organization-wide access
- Specific scopes: Future enhancement for resource-level permissions
- Scope evaluation strategy (future):
  - Check if requested resource matches access control scope
  - Support wildcard matching and scope hierarchies
  - Enable scope inheritance patterns
- Use case examples:
  - Device management: scope = device ID or "*"
  - Multi-tenant: scope = organization ID or "*"
  - Department-based: scope = department ID or "*"

### Principal vs User Distinction
- **Principal**: Authentication identity (Azure AD object ID, email, etc.)
- **User**: Portal-specific user profile (name, preferences, metadata)
- Design rationale:
  - Supports multiple authentication providers (Azure AD, SAML, OAuth)
  - Enables service principals and API key authentication
  - Separates concerns: identity vs. profile
  - Future-proofs for federated identity scenarios
- Current limitation: UI only manages access through User pages
- Future enhancement: Support principal management independent of users

### Permission Checking Performance
- Current implementation: Loads all access controls + roles + actions per check
- Optimization considerations:
  - Cache permission results per request/session
  - Materialize principal permissions table
  - Use claims-based approach (load permissions into claims at login)
  - Index optimization on AccessControl (PrincipalId) and Action (Name)
- Trade-offs:
  - Caching: Faster checks but delayed permission propagation
  - Materialization: Faster queries but additional storage and sync
  - Claims: Fastest but requires token refresh for permission changes

### Role Assignment Model
- Multiple roles per principal: Supported (union of permissions)
- Multiple scopes per role: Requires multiple access controls
- Permission conflict resolution: Union (any role grants permission)
- Scope conflict resolution: Not yet implemented (currently ignored)
- Future considerations:
  - Explicit permission denials (deny rules)
  - Priority-based conflict resolution
  - Scope-specific permission overrides

### UI/UX Considerations
- Access control management embedded in user detail page
- No standalone access control management UI
- Scope field disabled (always "*") indicating incomplete implementation
- Role dropdown shows all roles (no filtering or search)
- No bulk role assignment (one at a time)
- Real-time table updates (no page reload required)
- Minimal feedback (loading states, no success notifications)
- Delete action has no confirmation dialog (immediate deletion)

### Security Considerations
- Authorization required at controller level
- Permission-based access prevents unauthorized modifications
- Principal validation prevents assignments to non-existent identities
- Role validation ensures only valid roles are assigned
- Scope currently not validated (any string accepted)
- No audit trail for access control changes (consider adding)
- Deletion doesn't warn about active user sessions (consider impact)
- No rate limiting on role assignment (consider abuse prevention)

### Multi-Tenancy Considerations
- Scope attribute designed to support multi-tenancy
- Current implementation: Single-tenant (scope not enforced)
- Future multi-tenant pattern:
  - Organization-level scope: scope = "org:{organizationId}"
  - Cross-organization admin: scope = "*"
  - Per-tenant role isolation or shared role catalog
- Scope hierarchy considerations:
  - Support nested scopes (organization > department > team)
  - Scope inheritance (permissions flow down hierarchy)
  - Scope wildcards ("org:123:*" for all departments in org 123)

### Performance Considerations
- Paginated access control queries prevent full table scans
- Principal filtering enables efficient user-specific queries
- Eager loading reduces N+1 queries (includes for Role)
- Permission check loads all access controls + roles + actions (expensive)
- Optimization opportunities:
  - Index on PrincipalId, RoleId, Scope
  - Composite index on (PrincipalId, RoleId) for uniqueness
  - Cached permission lookups
  - Materialized permission view
  - Claims-based permission storage

### Database Schema Considerations
- AccessControl entity likely has foreign keys to Principal and Role
- Cascade delete considerations:
  - Delete principal: Should cascade delete access controls (orphan cleanup)
  - Delete role: Should prevent or cascade delete access controls (data integrity)
  - Delete user: Should handle principal cleanup (soft delete vs. hard delete)
- Unique constraint considerations:
  - Currently: No uniqueness constraint (duplicate role assignments possible)
  - Future: Unique constraint on (PrincipalId, RoleId, Scope) prevents duplicates
- Migration considerations:
  - Schema added in RBAC migration (recent)
  - Backward compatibility with existing authorization system

### Testing Coverage
- Unit tests: AccessControlServiceTests.cs (likely exists)
- Controller tests: Integration tests for API endpoints
- UI tests: UserDetailPageTests.cs with access control scenarios
- Client service tests: AccessControlClientServiceTests.cs
- Authorization tests: Permission validation scenarios
- Test scenarios:
  - Create access control with valid/invalid principal/role
  - Update access control assignments
  - Delete access control
  - Permission checking with multiple roles
  - Permission checking with no roles
  - Pagination and filtering
  - Scope variations (though not enforced yet)

### Known Limitations
- Scope attribute stored but not enforced in permission checks
- No standalone access control management UI
- No bulk role assignment or revocation
- No access control history or audit trail
- No role assignment expiration or time-based access
- Scope field not editable in UI (always "*")
- No confirmation dialog for access control deletion
- No validation for scope format or values
- No duplicate detection (same principal-role-scope)
- No access control approval workflow
- No role assignment impact analysis
- Limited role search/filtering in UI (shows all roles)
- No resource-based access control (RBAC) enforcement
- Permission check doesn't consider scope (treats all scopes equally)

### Future Enhancement Opportunities
- **Scope Enforcement**: Implement scope-aware permission validation
- **Standalone UI**: Dedicated access control management interface
- **Bulk Operations**: Assign/revoke multiple roles at once
- **Audit Trail**: Track all access control changes with timestamps and actors
- **Time-Based Access**: Temporary role assignments with expiration
- **Scope Editor**: UI for editing and validating scope values
- **Duplicate Prevention**: Unique constraint and UI validation
- **Approval Workflow**: Require approval for sensitive role assignments
- **Impact Analysis**: Show permission changes before applying
- **Role Search**: Filter and search roles in assignment dropdown
- **Resource Picker**: Select specific resources for scope
- **Scope Templates**: Predefined scope patterns (org-wide, dept-specific, etc.)
- **Permission Preview**: Show what permissions user will gain/lose
- **Access Reviews**: Periodic review and recertification of role assignments
- **Delegation**: Allow role owners to assign sub-permissions
- **Service Principals**: UI support for non-user principal management
- **Batch Import/Export**: Bulk access control management via CSV/JSON
- **Scope Hierarchy**: Parent-child scope relationships with inheritance
- **Conditional Access**: Context-aware permissions (time, location, device)
- **Permission Analytics**: Usage tracking and anomaly detection

### Design Decisions and Trade-offs
- **Scope in AccessControl vs. Separate Scope Entity**:
  - Chosen: Scope as string attribute in AccessControl
  - Benefit: Simplicity, flexibility, no additional tables
  - Trade-off: No scope validation, no scope metadata, harder to query scope hierarchies
  
- **Principal Entity vs. Direct User-Role Association**:
  - Chosen: Separate Principal entity as identity abstraction
  - Benefit: Supports non-user identities, separates auth from profile, enables federation
  - Trade-off: Additional entity, more complex queries, indirection in UI
  
- **Embedded vs. Separate Access Control Management**:
  - Chosen: Embedded in user detail page
  - Benefit: Contextual role assignment, simpler navigation
  - Trade-off: No global access control view, harder to manage cross-user assignments
  
- **Eager vs. Lazy Loading for Permission Checks**:
  - Chosen: Eager loading with explicit includes
  - Benefit: Predictable query performance, avoids N+1 problems
  - Trade-off: Always loads full object graph even when not needed
  
- **Union vs. Priority-Based Permission Resolution**:
  - Chosen: Union (any role grants permission)
  - Benefit: Simpler logic, easier to reason about, no conflicts
  - Trade-off: No way to deny permissions, no role precedence

### Integration Points
- **Authentication System**: Receives principal ID from Azure AD claims
- **Role Management**: References roles defined in role system
- **User Management**: Access controls managed through user detail pages
- **Authorization Middleware**: Calls UserHasPermissionAsync for permission checks
- **API Gateway**: May need access control information for API-level authorization
- **Audit System**: Should log access control changes (future)

### Localization Notes
- UI text in English (UserDetailPage)
- Error messages in English
- Localization opportunities:
  - "Roles" header and labels
  - "Add" button and "Actions" column
  - Error messages and validation text
  - Scope display value ("ALL")
  - Empty state message ("No roles assigned.")

### Accessibility Considerations
- MudBlazor components provide ARIA attributes
- Role dropdown properly labeled
- Delete icon button needs accessible label
- Loading states should announce changes
- Consider: Keyboard navigation for role assignment
- Consider: Screen reader announcements for table updates
- Consider: Focus management after add/delete operations
- Disabled states properly communicated via ARIA

### Migration and Deployment Notes
- AccessControl table likely added in recent RBAC migration
- Data migration considerations:
  - Migrate existing user-role associations to access controls
  - Create principals for existing users
  - Set default scope ("*") for existing assignments
- Rollback strategy:
  - Keep old authorization system in parallel during transition
  - Feature flag for new access control system
  - Data sync between old and new systems
- Performance impact:
  - Additional joins for permission checks
  - Potential for slow queries if not properly indexed
  - Consider database-level optimization (indexed views, etc.)
