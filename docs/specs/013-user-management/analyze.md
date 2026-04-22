# Feature: User Management

**Category**: Security & Administration  
**Status**: Analyzed  

---

## Description

The User Management feature provides comprehensive capabilities for managing user accounts within the IoT Hub Portal, enabling administrators to control access to the system through a robust user lifecycle management system. This feature integrates deeply with the Role-Based Access Control (RBAC) system to provide fine-grained authorization. Key capabilities include:

- Creating and managing user accounts with profile information (name, email, avatar)
- Viewing paginated user lists with search and filtering capabilities
- Managing user details including role assignments through access controls
- Automatic user provisioning on first login via OAuth/OIDC authentication
- Automatic administrator assignment for the first user in the system
- Principal-based identity management linking users to access control policies
- Integration with external authentication providers (OAuth2/OIDC)
- User deletion with cascade cleanup of associated principals and access controls
- Server-side pagination, sorting, and filtering for large user bases
- RESTful API with full CRUD operations on user entities

This feature provides essential business value by enabling secure, centralized user account management that integrates seamlessly with external authentication systems while maintaining local user profiles and role assignments. The automatic first-user-as-admin feature simplifies initial system setup, while the principal-based architecture enables flexible multi-tenant access control patterns.

---

## Code Locations

### Entry Points / Endpoints
- `src/IoTHub.Portal.Server/Controllers/v1.0/UsersController.cs` (Lines 1-178)
  - **Snippet**: Main REST API controller for user management
    ```csharp
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/users")]
    [ApiExplorerSettings(GroupName = "User Management")]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService userManagementService;
        private readonly IAccessControlManagementService accessControlService;
        private readonly ILogger<UsersController> logger;
        
        [HttpGet(Name = "Get Users")]
        [Authorize("user:read")]
        public async Task<PaginationResult<UserModel>> Get(
            string searchName, string searchEmail, int pageSize, int pageNumber, string[] orderBy)
        
        [HttpGet("{id}")]
        [Authorize("user:read")]
        public async Task<IActionResult> GetUserDetails(string id)
        
        [HttpPost(Name = "POST Create an User")]
        [Authorize("user:write")]
        public async Task<IActionResult> CreateUser([FromBody] UserDetailsModel user)
        
        [HttpPut("{id}", Name = "PUT Edit User")]
        [Authorize("user:write")]
        public async Task<IActionResult> EditUserAsync(string id, UserDetailsModel user)
        
        [HttpDelete("{id}")]
        [Authorize("user:write")]
        public async Task<IActionResult> DeleteUser(string id)
    }
    ```

### Business Logic
- `src/IoTHub.Portal.Application/Services/IUserManagementService.cs` (Lines 1-24)
  - **Snippet**: Core service interface for user operations
    ```csharp
    public interface IUserManagementService
    {
        Task<PaginatedResult<UserModel>> GetUserPage(
            string? searchName, string? searchEmail, 
            int pageSize, int pageNumber, string[] orderBy);
        Task<UserDetailsModel> GetUserDetailsAsync(string userId);
        Task<UserDetailsModel> CreateUserAsync(UserDetailsModel userCreateModel);
        Task<UserDetailsModel?> UpdateUser(string id, UserDetailsModel user);
        Task<bool> DeleteUser(string userId);
        Task<UserDetailsModel> GetOrCreateUserByEmailAsync(string email, ClaimsPrincipal principal);
    }
    ```

- `src/IoTHub.Portal.Application/Services/UserService.cs` (Lines 1-206)
  - Concrete implementation of IUserManagementService
  - Handles database operations for user CRUD with validation
  - Implements paginated queries with search filters
  - Manages user-principal relationships
  - Automatic first-user administrator provisioning logic
  - OAuth/OIDC claims mapping to user entity (Lines 144-204)
  - Duplicate name validation during create/update
  - Cascade deletion of principal when user is deleted

### Data Access
- `src/IoTHub.Portal.Domain/Repositories/IUserRepository.cs` (Lines 1-14)
  - **Snippet**: User repository interface with custom queries
    ```csharp
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByNameAsync(string userName, 
            params Expression<Func<User, object>>[] includeProperties);
    }
    ```

- `src/IoTHub.Portal.Domain/Repositories/IPrincipalRepository.cs` (Lines 1-11)
  - Generic repository interface for Principal entity
  - Inherits from IRepository<Principal>
  - Used for managing principal lifecycle alongside users

- `src/IoTHub.Portal.Domain/Entities/User.cs` (Lines 1-21)
  - **Snippet**: User entity definition
    ```csharp
    public class User : EntityBase
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string GivenName { get; set; }
        public string? Name { get; set; }       // Full name
        public string? FamilyName { get; set; }
        public string? Avatar { get; set; }     // Avatar URL/data
        public string PrincipalId { get; set; }  // Links to RBAC Principal
    }
    ```

- `src/IoTHub.Portal.Domain/Entities/Principal.cs` (Lines 1-18)
  - **Snippet**: Principal entity for RBAC integration
    ```csharp
    public class Principal : EntityBase
    {
        [Required]
        public virtual ICollection<AccessControl> AccessControls { get; set; }
        public virtual User? User { get; set; }
    }
    ```

### UI Components
- `src/IoTHub.Portal.Client/Pages/RBAC/UsersListPage.razor` (Lines 1-84)
  - Main user list page with search and pagination
  - Features:
    - Server-side paginated MudTable
    - Search by name or email with immediate filtering
    - Configurable page sizes (5, 10, 20)
    - Navigation to user detail pages
    - Refresh functionality
    - Displays: Name (GivenName), Email, Principal ID
  - Accessible at route: `/users`
  - Authorization: `[Authorize]` with `PortalPermissions.UserRead` required

- `src/IoTHub.Portal.Client/Pages/RBAC/UserDetailPage.razor` (Lines 1-182)
  - User detail page with role assignment management
  - Features:
    - Read-only user profile display (GivenName, FamilyName, Email)
    - Role assignment interface (add/remove roles)
    - Access control list display with role names and scopes
    - Permission-based UI element visibility (write permissions)
    - Loading and error state handling
    - Snackbar notifications for operations
    - Scope display (currently hardcoded to "ALL")
  - Accessible at route: `/users/{UserId}`
  - Authorization: `PortalPermissions.UserRead` required, `UserWrite` for modifications
  - Integrates with AccessControlClientService for role assignments

### Data Transfer Objects
- `src/IoTHub.Portal.Shared/Models/v1.0/UserModel.cs` (Lines 1-14)
  - **Snippet**: Lightweight user model for list display
    ```csharp
    public class UserModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string GivenName { get; set; }
        public string PrincipalId { get; set; }
    }
    ```

- `src/IoTHub.Portal.Shared/Models/v1.0/UserDetailsModel.cs` (Lines 1-18)
  - **Snippet**: Complete user details with full profile
    ```csharp
    public class UserDetailsModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string GivenName { get; set; }
        public string Name { get; set; }
        public string FamilyName { get; set; }
        public string Avatar { get; set; }
        public string PrincipalId { get; set; }
        // Note: AccessControls collection commented out in current implementation
    }
    ```

- `src/IoTHub.Portal.Shared/Models/v1.0/Filters/UserFilter.cs` (Lines 1-12)
  - Filter model for user queries
  - Properties: SearchName, SearchEmail
  - Inherits pagination parameters from PaginationFilter

### Client Services
- `src/IoTHub.Portal.Client/Services/IUserClientService.cs` (Lines 1-17)
  - Client-side service interface for HTTP API calls
  - Methods: GetUsers, GetUser, CreateUser, UpdateUser, DeleteUser
  - Uses continuation URI pattern for pagination

- `src/IoTHub.Portal.Client/Services/UserClientService.cs` (Lines 1-45)
  - Implementation using HttpClient for API communication
  - GET endpoints for list (with continuation URI) and details
  - POST endpoint for user creation (returns 201 Created)
  - PUT endpoint for updates with user ID in route
  - DELETE endpoint with user ID in route
  - Graceful 404 handling for GetUser method

### Mappers
- `src/IoTHub.Portal.Application/Mappers/UserProfile.cs` (Lines 1-44)
  - AutoMapper profile for User entity and DTO mappings
  - Bidirectional mappings between User ↔ UserModel
  - Bidirectional mappings between User ↔ UserDetailsModel
  - ID field ignored on reverse mapping (auto-generated)
  - All profile fields mapped explicitly

---

## API Endpoints

### User Management
- `GET /api/users` - Get paginated list of users
  - Query Parameters: 
    - searchName (optional) - Filters by Name, FamilyName, or GivenName (case-insensitive)
    - searchEmail (optional) - Filters by email
    - pageSize (default: 10) - Number of results per page
    - pageNumber (default: 0) - Zero-based page index
    - orderBy (optional) - Array of field names for sorting
  - Returns: PaginationResult<UserModel> with Items, TotalItems, and NextPage link
  - Authorization: user:read
  - Used to populate the users list page

- `GET /api/users/{id}` - Get detailed user information
  - Route Parameter: id (string) - User identifier
  - Returns: UserDetailsModel with full user profile
  - Authorization: user:read
  - Returns 404 if user not found
  - Used by user detail page for display

- `POST /api/users` - Create a new user
  - Body: UserDetailsModel
  - Returns: 201 Created with UserDetailsModel and Location header
  - Authorization: user:write
  - Validates unique username
  - Generates new PrincipalId
  - Returns 400 BadRequest on validation failure

- `PUT /api/users/{id}` - Update existing user
  - Route Parameter: id (string) - User identifier
  - Body: UserDetailsModel
  - Returns: 200 OK with updated UserDetailsModel
  - Authorization: user:write
  - Validates unique username (excluding current user)
  - Updates profile fields: Email, GivenName, Name, FamilyName, Avatar
  - Throws ResourceNotFoundException if user doesn't exist

- `DELETE /api/users/{id}` - Delete user by ID
  - Route Parameter: id (string) - User identifier
  - Returns: 200 OK with boolean result, 404 if not found
  - Authorization: user:write
  - Cascade deletes associated Principal entity
  - Throws ResourceNotFoundException if user doesn't exist

---

## Authorization

### Required Permissions
- **user:read** - View users list and retrieve user details
- **user:write** - Create, update, and delete users

### Authorization Implementation
- Attribute-based authorization using `[Authorize("permission")]` attributes on controller methods
- Permission strings defined in PortalPermissions enum (UserRead, UserWrite)
- Base authorization requirement: `[Authorize]` on UsersController and all user pages
- Permissions mapped through PermissionsExtension.AsString() method
- Managed through role-based access control (RBAC) via AccessControl entities

### Permission Mapping
- `PortalPermissions.UserRead` → "user:read"
- `PortalPermissions.UserWrite` → "user:write"

### Permission-Based UI
- UserDetailPage conditionally shows role management controls based on user:write permission
- Add role button and delete role buttons disabled when canWrite = false
- Authorization checks performed in OnInitializedAsync via HasPermissionAsync

---

## Dependencies

### Internal Feature Dependencies
- **Role-Based Access Control (RBAC)** - Users are linked to roles via Principal → AccessControl → Role
- **Access Control Management** - Manages role assignments for users via AccessControl entities
- **Authentication System** - OAuth/OIDC integration for user provisioning via GetOrCreateUserByEmailAsync
- **Principal Management** - Each user has a corresponding Principal entity for access control
- **Role Management** - Roles displayed and assigned through user detail page

### Service Dependencies
- `IUserRepository` - User entity persistence and queries
- `IPrincipalRepository` - Principal entity lifecycle management
- `IAccessControlRepository` - Access control assignment operations
- `IRoleRepository` - Role lookup for first-user admin assignment
- `IUnitOfWork` - Transaction management for database operations
- `IMapper` (AutoMapper) - Entity to DTO mapping
- `IAccessControlManagementService` - Role assignment operations (dependency in controller)

### Related Entities
- **Principal** - One-to-one relationship with User via PrincipalId
- **AccessControl** - Many-to-one relationship with Principal (via collection)
- **Role** - Many-to-many relationship with User through AccessControl → Principal → User chain
- **EntityBase** - Base class providing Id and common entity properties

### External Dependencies
- **Entity Framework Core** - Database access via PortalDbContext
- **AutoMapper** - Entity to DTO mapping
- **System.Security.Claims** - ClaimsPrincipal for OAuth/OIDC user data extraction
- **Microsoft.AspNetCore.Authorization** - Authorization attribute support

### UI Dependencies
- **MudBlazor** - UI component library
  - MudTable - Server-side paginated data grid
  - MudTextField - Text input for search and display
  - MudSelect/MudSelectItem - Role selection dropdown
  - MudButton - Action buttons (add role)
  - MudIconButton - Icon action buttons (refresh, view, delete)
  - MudPaper - Layout container
  - MudSnackbar - Notification system
  - MudProgressCircular - Loading indicators
  - MudTooltip - Contextual help for buttons

---

## Key Features & Behaviors

### User Profile Management
- **Email**: Required field, used for login and user lookup (case-insensitive)
- **GivenName**: Required field, displayed as primary name in lists and forms
- **Name**: Optional full name field
- **FamilyName**: Optional surname/family name field
- **Avatar**: Optional field for avatar URL or data URI
- **PrincipalId**: System-generated GUID linking user to RBAC principal

### OAuth/OIDC User Provisioning
- Automatic user creation on first login via GetOrCreateUserByEmailAsync
- Claims mapping:
  - "name" claim → User.Name
  - "preferred_username" claim → User.GivenName
  - "family_name" claim → User.FamilyName
  - Email from parameter → User.Email
- Case-insensitive email lookup to prevent duplicates
- Generates new Principal with unique GUID for RBAC integration

### First User Administrator Assignment
- System checks if user being created is the first user (CountAsync = 0)
- First user automatically assigned to "Administrators" role
- Creates AccessControl entry linking user's Principal to admin role
- Scope set to empty string (ALL scope, system-wide access)
- Throws ResourceNotFoundException if Administrators role doesn't exist

### Pagination & Search
- Server-side pagination with configurable page sizes
- Search across multiple fields: Name, FamilyName, GivenName, Email
- Case-insensitive search using ToLower() comparison
- Predicate builder pattern for composable filter queries
- NextPage link generation for hypermedia navigation
- Default page size: 10 users per page

### Validation Rules
- **Email**: Required field via [Required] attribute
- **GivenName**: Required field via [Required] attribute
- **Name Uniqueness**: Validated during create/update operations
  - Checks for existing user with same Name
  - Throws ResourceAlreadyExistsException on duplicate
  - Name uniqueness check excludes current user during updates
- **User Existence**: Validated on update/delete operations
  - Throws ResourceNotFoundException if user doesn't exist

### User-Principal Relationship
- One-to-one relationship between User and Principal
- PrincipalId stored in User entity
- Principal entity contains User navigation property
- Principal cascade deleted when user is deleted
- Principal provides collection of AccessControl entries for role assignments

### Role Assignment Integration
- User detail page displays assigned roles via AccessControl entities
- Role assignment performed through AccessControlClientService
- Scope currently hardcoded to "*" (ALL) for all role assignments
- Can add multiple roles to a single user
- Can remove role assignments with confirmation
- Role management requires user:write permission

### Error Handling
- ResourceNotFoundException thrown when user/resource doesn't exist
- ResourceAlreadyExistsException thrown on duplicate name
- ArgumentNullException thrown for null parameter validation
- Graceful 404 handling in client service for missing users
- User-friendly error messages via Snackbar notifications
- Structured logging throughout controller and service methods

### Database Operations
- **Create**: Insert user with new Principal, validate uniqueness
- **Read**: Single user by ID, paginated list with filters
- **Update**: Update profile fields, preserve ID and PrincipalId
- **Delete**: Cascade delete Principal, then User entity
- **Query**: Predicate-based filtering with pagination
- **Transaction**: UnitOfWork pattern for consistency

---

## Notes

### Architecture Patterns
- **Repository Pattern** - Clean separation of data access concerns
- **Unit of Work Pattern** - Transactional consistency across operations
- **Service Layer** - Business logic abstraction from controllers
- **DTO Pattern** - Data transfer between layers
- **Predicate Builder Pattern** - Composable query filters
- **Claims-Based Identity** - OAuth/OIDC integration via ClaimsPrincipal

### Principal-Based Identity Architecture
- Users don't directly have roles; instead, users have a Principal
- Principal is the RBAC subject (identity entity for access control)
- AccessControl entries link Principal to Roles with optional Scopes
- This architecture enables future multi-tenant and scoped access patterns
- Allows single identity (Principal) to have different roles in different scopes

### Authentication vs. Authorization Separation
- Authentication handled by external OAuth/OIDC providers (Azure AD, Keycloak, etc.)
- User provisioning occurs on first successful authentication
- Local user entity stores profile information and links to authorization system
- Authorization managed internally via Principal → AccessControl → Role chain
- No password storage; authentication delegated to identity providers

### First User Bootstrap Logic
- Ensures system is never locked out after initial deployment
- First user automatically gets full administrator access
- Subsequent users require explicit role assignment by administrators
- Prevents chicken-and-egg problem: "who assigns the first admin?"
- Hardcoded dependency on "Administrators" role existing in database

### Commented Code in UserDetailsModel
- AccessControls collection property is commented out in current implementation
- UserDetailPage loads access controls separately via API call
- Design decision: Separate API call provides better performance for large AC lists
- Could be re-enabled for simpler single-request user details retrieval

### Search Implementation
- Search performed server-side for scalability
- Single search term applied to multiple fields (Name, FamilyName, GivenName)
- Email search is separate parameter but currently not used distinctly in UI
- Case-insensitive search via ToLower() - could be optimized with database collation
- Immediate search triggering on keyup may generate many API calls

### Navigation Patterns
- UsersListPage → UserDetailPage navigation via user ID
- Detail page is read-only for user profile (no inline editing)
- Role management performed through separate AccessControl operations
- No direct edit page for user profile (PUT endpoint exists but no UI)

### Pagination Strategy
- Uses continuation URI pattern for hypermedia navigation
- NextPage link includes all current filter parameters
- Zero-based page indexing (pageNumber starts at 0)
- TotalItems count provided for UI pagination controls
- Page size configurable per request (default 10, options 5/10/20)

### Logging Strategy
- Structured logging at controller level for all operations
- Success operations log informational messages with user IDs
- Error operations log errors with exception details and context
- User count logged on successful list retrieval
- Consistent log message patterns aid debugging and monitoring

### Multi-Tenancy Considerations
- Scope field in AccessControl enables future multi-tenant access patterns
- Currently all scopes default to "*" (global/ALL access)
- Principal architecture designed to support scoped role assignments
- User entities are global; scoping applied at access control level

### Security Considerations
- Authorization required at both controller and UI levels
- Permission-based access control prevents unauthorized operations
- Email used as primary identifier (not username) for better uniqueness
- No sensitive data stored in user entity (passwords managed externally)
- Cascade deletion ensures no orphaned principals
- Principal-based design enables audit trail of all role assignments

### Database Migrations
- User entity migrations: Track user table creation and schema changes
- Principal entity migrations: Track RBAC principal infrastructure
- AccessControl relationship migrations: Track role assignment schema
- Foreign key constraints enforce referential integrity
- Cascade delete configuration ensures cleanup

### Testing Coverage
- Unit tests expected for UserService (business logic)
- Controller tests for API endpoint validation
- UI tests for UsersListPage and UserDetailPage interactions
- Client service tests for HTTP communication
- Integration tests for OAuth/OIDC user provisioning flow

### Known Limitations
- No user profile editing UI (API exists but no corresponding page)
- Scope field not fully utilized (hardcoded to "*" or empty string)
- Search triggers on every keyup (could add debouncing for performance)
- No bulk user operations (import/export, bulk role assignment)
- No user status management (active/inactive/locked accounts)
- No password reset or email verification flows (delegated to external IdP)
- Avatar field exists but no upload/management UI
- Name uniqueness validation has typo in error message ("tis" should be "with")

### Future Enhancement Opportunities
- User profile editing page with inline form validation
- User status management (active, inactive, locked, pending)
- User activity audit log (login history, role changes)
- Bulk user import/export (CSV, Excel)
- User group management for easier role assignments
- Invitation workflow for new user onboarding
- User avatar upload and management
- Email notification on role changes
- Advanced search with filters (by role, by status, by last login)
- User impersonation for administrators (for support scenarios)
- Self-service user profile editing
- User preferences and settings storage
- Integration with external user directories (LDAP sync)
- Multi-factor authentication status display
- Session management (view/revoke active sessions)
- Scoped role assignments with UI support
- User deletion soft-delete option (deactivation instead of hard delete)
- User merge functionality for duplicate accounts
- Batch role assignment operations
- User type classification (admin, operator, viewer)

### Performance Considerations
- Pagination limits result set size for large user bases
- Search queries use indexed Email and Name fields (assumed)
- Single database query for user list (no N+1 problems)
- Separate API calls for user details and access controls
- Loading states prevent UI blocking during async operations
- AutoMapper provides efficient entity-to-DTO conversions

### Internationalization Considerations
- User names stored as Unicode strings (supports international characters)
- Email addresses follow international email standards
- No hardcoded locale-specific validation or formatting
- UI strings currently not localized (future enhancement opportunity)

### Compliance & Privacy
- User email addresses are PII (Personal Identifiable Information)
- User deletion removes all associated data (GDPR right to be forgotten)
- No password storage (authentication delegated to external providers)
- Audit trail via structured logging for compliance reporting
- Consider data retention policies for deleted users
- May require additional privacy controls for GDPR/CCPA compliance
