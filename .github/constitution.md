# Project Constitution

> This document defines the authoritative standards, patterns, and guidelines for the IoT Hub Portal project.
> It is derived from actual codebase analysis and should be maintained alongside code changes.

**Last Updated**: 2026-01-30  
**Source**: Generated from `docs/analyze.md`, feature analyses, and deep codebase analysis

---

## 1. Project Overview

### Purpose
IoT Hub Portal is a comprehensive web-based solution for managing IoT devices across multi-cloud environments. It provides centralized device lifecycle management, configuration, monitoring, and access control for IoT devices connected through Azure IoT Hub or AWS IoT Core. The portal supports standard IoT devices, Azure IoT Edge devices, and LoRaWAN devices with concentrators.

### Modules

| Module | Purpose | Location |
|--------|---------|----------|
| IoTHub.Portal.Server | REST API controllers, authentication, and web host | `src/IoTHub.Portal.Server` |
| IoTHub.Portal.Client | Blazor WebAssembly UI components | `src/IoTHub.Portal.Client` |
| IoTHub.Portal.Application | Business logic services and managers | `src/IoTHub.Portal.Application` |
| IoTHub.Portal.Domain | Domain entities, repository interfaces, and exceptions | `src/IoTHub.Portal.Domain` |
| IoTHub.Portal.Infrastructure | Data access, external services, and background jobs | `src/IoTHub.Portal.Infrastructure` |
| IoTHub.Portal.Shared | DTOs, models, and shared constants | `src/IoTHub.Portal.Shared` |
| IoTHub.Portal.Crosscutting | Cross-cutting concerns | `src/IoTHub.Portal.Crosscutting` |
| IoTHub.Portal.MySql | MySQL-specific database implementations | `src/IoTHub.Portal.MySql` |
| IoTHub.Portal.Postgres | PostgreSQL-specific database implementations | `src/IoTHub.Portal.Postgres` |

---

## 2. Architecture Principles

### Layered Architecture

The project implements Clean Architecture with clear separation of concerns:

```
┌─────────────────────────────────────────────────────┐
│  Presentation Layer                                 │
│  - IoTHub.Portal.Client (Blazor WebAssembly)       │
│  - IoTHub.Portal.Server (API Controllers)          │
└─────────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────┐
│  Application Layer                                  │
│  - IoTHub.Portal.Application (Services, Managers)  │
└─────────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────┐
│  Domain Layer                                       │
│  - IoTHub.Portal.Domain (Entities, Interfaces)     │
└─────────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────┐
│  Infrastructure Layer                               │
│  - IoTHub.Portal.Infrastructure (Repositories,     │
│    External Services, Background Jobs)              │
└─────────────────────────────────────────────────────┘
```

**Dependency Direction**: Outer layers depend on inner layers. Domain is the core with no external dependencies.

### Component Relationships

- **Controllers** → **Services**: Controllers call service interfaces defined in Application layer
- **Services** → **Repositories**: Services use repository interfaces defined in Domain layer
- **Services** → **External Services**: Services use abstracted external service interfaces (IExternalDeviceService)
- **Infrastructure** → **Domain**: Infrastructure implements interfaces defined in Domain
- **All Layers** → **Shared**: DTOs and shared models are used across all layers

### Key Patterns

- **Repository Pattern**: All data access goes through repository interfaces (IRepository<T>, IUnitOfWork)
- **Generic Base Classes**: Controllers and services use generic base classes for code reuse (e.g., DevicesControllerBase<TDto>, DeviceServiceBase)
- **DTO Pattern**: Data transfer between layers uses dedicated DTO classes, never domain entities
- **Unit of Work Pattern**: Database transactions managed through IUnitOfWork for consistency
- **AutoMapper**: Entity-to-DTO mapping handled by AutoMapper with explicit mapping profiles
- **Dependency Injection**: All dependencies injected through constructor injection
- **External Service Abstraction**: Cloud provider differences abstracted behind IExternalDeviceService
- **Policy-Based Authorization**: Fine-grained permissions using ASP.NET Core policy-based authorization
- **Background Jobs**: Scheduled tasks using Quartz.NET with [DisallowConcurrentExecution] attribute

---

## 3. Coding Standards

### File Organization

- **Namespace inside files**: Using directives placed inside namespace declarations (`csharp_using_directive_placement = inside_namespace`)
- **One class per file**: Each class/interface in its own file matching the type name
- **Folder structure mirrors namespace**: Physical folders match namespace structure (with IDE0130 as suggestion)
- **Versioned API structure**: Controllers organized in `v1.0` folders (e.g., `Controllers/v1.0/`)

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Files | PascalCase, match type name | `DevicesController.cs`, `IDeviceRepository.cs` |
| Classes | PascalCase | `Device`, `DeviceService` |
| Interfaces | PascalCase with `I` prefix | `IDeviceService`, `IRepository<T>` |
| Private Fields | camelCase | `deviceService`, `mockUserRepository` |
| Private Constants | PascalCase | `DefaultPageSize` |
| Private Static Readonly | PascalCase | `AdminRoleName` |
| Public Properties | PascalCase | `DeviceId`, `IsConnected` |
| Methods | PascalCase | `GetDevice`, `CreateDeviceAsync` |
| Parameters | camelCase | `deviceId`, `pageSize` |
| Local Variables | camelCase with `var` preferred | `var result`, `var expectedUsers` |
| Async Methods | Suffix with `Async` | `CreateDeviceAsync`, `GetUserDetailsAsync` |
| Boolean Properties | Prefix with `Is`, `Has`, `Can` | `IsConnected`, `HasPermission`, `CanWrite` |

### Code Structure

#### File Header
All source files must include the copyright header:
```csharp
// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
```

#### Using Directives
- System namespaces first, sorted alphabetically
- Using directives placed inside namespace (after namespace declaration)
- Use GlobalUsings.cs for commonly used namespaces across the project

#### Class Organization
1. Private fields
2. Constructors
3. Public properties
4. Public methods
5. Private/protected methods

#### Method Bodies
- Prefer block bodies for methods and constructors (not expression bodies)
- Use expression bodies for properties, operators, and lambdas
- Braces required for multi-line statements

### Formatting

- **Indentation**: 4 spaces (no tabs)
- **Line Endings**: CRLF
- **Trim trailing whitespace**: Required
- **Final newline**: Required in all files
- **Braces**: `when_multiline` - required for multi-line blocks
- **Spacing**: Space before and after binary operators
- **Labels**: Indent one less than current

### Documentation

- **XML Documentation**: Required for public APIs (controllers, services)
- **Summary tags**: Describe purpose and behavior
- **Param tags**: Document parameters with meaningful descriptions
- **Returns tags**: Document return values

Example:
```csharp
/// <summary>
/// Gets the device list.
/// </summary>
/// <param name="searchText">Search text for device ID or name</param>
/// <param name="pageSize">Number of items per page</param>
/// <returns>Paginated result of device list items</returns>
```

---

## 4. Security & Authorization

### Role Model

The project uses a three-tier RBAC (Role-Based Access Control) system:

```
User → Principal → AccessControl → Role → Permissions
```

- **User**: Represents an authenticated user with profile information (email, name, avatar)
- **Principal**: RBAC subject entity (one-to-one with User), enables scoped access control
- **AccessControl**: Links a Principal to a Role with an optional Scope (for multi-tenancy)
- **Role**: Named collection of permissions (e.g., "Administrators", "Operators")
- **Permissions**: Fine-grained actions defined in PortalPermissions enum

### Permission Model

Permissions follow a `resource:action` naming convention:

| Permission String | Enum Value | Description |
|-------------------|------------|-------------|
| `device:read` | PortalPermissions.DeviceRead | View device list and details |
| `device:write` | PortalPermissions.DeviceWrite | Create, update, delete devices |
| `device:execute` | PortalPermissions.DeviceExecute | Execute device commands |
| `device:import` | PortalPermissions.DeviceImport | Import device lists |
| `device:export` | PortalPermissions.DeviceExport | Export device lists |
| `user:read` | PortalPermissions.UserRead | View user information |
| `user:write` | PortalPermissions.UserWrite | Manage users |
| `role:read` | PortalPermissions.RoleRead | View roles |
| `role:write` | PortalPermissions.RoleWrite | Manage roles |

### Permission Enforcement

#### API Controllers
Use `[Authorize("permission:string")]` attributes on controller methods:
```csharp
[Authorize("device:read")]
public Task<DeviceDetails> GetItem(string deviceID)
```

#### Blazor Components
- Page-level: `@attribute [Authorize]` on Razor pages
- Permission checks: `await HasPermissionAsync(PortalPermissions.DeviceRead)`
- Conditional rendering: Show/hide UI elements based on permissions

#### First User Bootstrap
- System automatically assigns first user to "Administrators" role
- Prevents lockout on initial deployment
- Subsequent users require explicit role assignment

### Security Boundaries

- **All API endpoints**: Protected with `[Authorize]` attribute at minimum
- **Fine-grained operations**: Additional permission-specific authorization
- **Sensitive operations**: Require specific write/execute permissions
- **Cross-cutting concerns**: Authentication handled by external OAuth/OIDC providers

---

## 5. Error Handling

### Exception Hierarchy

All custom exceptions inherit from `BaseException`:

```csharp
BaseException
├── ResourceNotFoundException (404)
├── ResourceAlreadyExistsException (400)
├── InternalServerErrorException (500)
└── InvalidCloudProviderException
```

### Exception Patterns

#### Throwing Exceptions
```csharp
// Not found
throw new ResourceNotFoundException("User not found", $"User with ID {userId} does not exist");

// Already exists
throw new ResourceAlreadyExistsException("User already exists", $"A user with name {userName} already exists");

// Internal error
throw new InternalServerErrorException("Device sync failed", "Failed to synchronize device twin", innerException);
```

#### Exception Properties
- **Title**: Human-readable short description
- **Detail**: Detailed error message with context
- **InnerException**: Optional chained exception for troubleshooting

### Error Response Format

API returns ProblemDetails format mapped in Startup.cs:
```csharp
{
  "title": "Resource not found",
  "detail": "User with ID abc123 does not exist",
  "status": 404
}
```

### Validation Patterns

- **Data Annotations**: Use `[Required]`, `[MaxLength]`, etc. on DTOs
- **FluentValidation**: For complex validation rules
- **Model State**: Controller validates ModelState automatically
- **Business Rules**: Validate in service layer before persistence

### Logging Patterns

Use structured logging with ILogger:

```csharp
// Information level
this.logger.LogInformation("Getting device list, found {Count} devices", devices.Count);

// Warning level
this.logger.LogWarning("User authenticated but email claim missing");

// Error level
this.logger.LogError(exception, "Failed to delete user {UserId}", userId);
```

**Logging Guidelines:**
- Log at Information level for successful operations with key metrics
- Log at Warning level for unexpected but recoverable conditions
- Log at Error level with exception for failures requiring attention
- Include contextual data (IDs, counts) for troubleshooting
- Use structured logging with named parameters, not string interpolation

---

## 6. Configuration Management

### Configuration Sources

| Source | Purpose | Example Settings |
|--------|---------|------------------|
| `appsettings.json` | Base configuration | Logging, AllowedHosts, Authentication type |
| `appsettings.Production.json` | Production overrides | Production-specific settings |
| Environment Variables | Runtime configuration | Connection strings, cloud credentials |
| ConfigHandler | Abstracted config access | CloudProvider, IoT Hub settings |

### Environment Handling

- **Development**: Uses DevelopmentConfigHandler with development-specific settings
- **Production**: Uses ProductionAzureConfigHandler or ProductionAWSConfigHandler based on CloudProvider
- **Config Factory**: ConfigHandlerFactory.Create() selects appropriate handler

### Configuration Pattern

```csharp
// Access configuration through injected ConfigHandler
public class MyService
{
    private readonly ConfigHandler configuration;
    
    public MyService(ConfigHandler configuration)
    {
        this.configuration = configuration;
    }
    
    public void DoWork()
    {
        var cloudProvider = this.configuration.CloudProvider;
        // Use configuration
    }
}
```

### Secrets Management

- **No secrets in source code**: All sensitive data from environment variables or secure vaults
- **Connection strings**: Stored in environment variables, not in appsettings.json
- **API keys**: Injected at runtime from secure sources
- **Cloud credentials**: Managed by cloud provider IAM (Azure Managed Identity, AWS IAM)

### Feature Flags

- **CloudProvider**: Switch between Azure IoT Hub and AWS IoT Core (`CloudProviders.Azure` or `CloudProviders.AWS`)
- **Ideas Feature**: Configurable external integration for feature suggestions
- **Authentication Type**: Configurable OIDC provider (AzureAdB2C, custom)

---

## 7. Documentation Requirements

### Required Documentation

#### For New Features
1. **Analysis Document**: `specs/{feature-number}-{feature-name}/analyze.md`
   - Feature description and business value
   - Code locations (controllers, services, entities, UI)
   - API endpoints with parameters and responses
   - Authorization requirements
   - Dependencies (internal and external)
   - Key features and behaviors

2. **API Documentation**: XML comments on all public controller methods
   - Summary of endpoint purpose
   - Parameter descriptions
   - Return type documentation

3. **README Updates**: Add feature to main README if externally visible

#### For Services and Repositories
- Interface documentation with XML comments
- Complex business logic documented inline
- Edge cases and assumptions documented

### Documentation Location

| Type | Location | Format |
|------|----------|--------|
| Feature Analysis | `specs/{feature-folder}/analyze.md` | Markdown |
| Project Overview | `docs/analyze.md` | Markdown |
| API Documentation | XML comments in controllers | XML Doc Comments |
| Architecture Docs | GitHub Pages site | Markdown |
| Code Comments | Inline in source files | C# comments |
| Constitution | `.github/constitution.md` | Markdown |

### Documentation Format

- **Markdown**: Use GitHub Flavored Markdown for all .md files
- **Code Blocks**: Include language identifier for syntax highlighting
- **Headers**: Use ATX-style headers (# ## ###)
- **Lists**: Use `-` for unordered, `1.` for ordered
- **Tables**: Use pipe tables with aligned columns
- **Links**: Use reference-style links for repeated URLs

---

## 8. Development Workflow

### Adding New Features

1. **Analyze Requirements**
   - Create feature analysis document in `specs/{number}-{name}/analyze.md`
   - Identify affected modules and components
   - Determine authorization requirements

2. **Create Domain Entities** (if needed)
   - Add entity to `IoTHub.Portal.Domain/Entities/`
   - Inherit from `EntityBase` for ID and common properties
   - Add XML documentation for properties

3. **Create Repository Interfaces**
   - Add interface to `IoTHub.Portal.Domain/Repositories/`
   - Inherit from `IRepository<TEntity>`
   - Define custom query methods if needed

4. **Implement Repository**
   - Add implementation to `IoTHub.Portal.Infrastructure/Repositories/`
   - Implement using Entity Framework Core DbContext

5. **Create DTOs**
   - Add to `IoTHub.Portal.Shared/Models/v1.0/`
   - Use data annotations for validation
   - Separate list DTOs (lightweight) from detail DTOs (complete)

6. **Create AutoMapper Profile**
   - Add to `IoTHub.Portal.Application/Mappers/`
   - Map entity ↔ DTO bidirectionally
   - Ignore ID on reverse mappings (auto-generated)

7. **Create Service Interface and Implementation**
   - Interface in `IoTHub.Portal.Application/Services/`
   - Implementation in `IoTHub.Portal.Infrastructure/Services/` or `IoTHub.Portal.Server/Services/`
   - Inject repositories and other dependencies
   - Implement business logic with validation

8. **Create API Controller**
   - Add to `IoTHub.Portal.Server/Controllers/v1.0/`
   - Inherit from `ControllerBase` or appropriate base controller
   - Add `[ApiVersion("1.0")]` and `[Route("api/{resource}")]`
   - Add `[Authorize]` and permission-specific attributes
   - Add XML documentation comments

9. **Create Client Service** (for Blazor)
   - Interface in `IoTHub.Portal.Client/Services/`
   - Implementation with HttpClient calls
   - Handle API responses and errors

10. **Create UI Components**
    - List page in `IoTHub.Portal.Client/Pages/{Feature}/`
    - Detail/edit components as needed
    - Use MudBlazor components for consistency
    - Add `@attribute [Authorize]` and permission checks
    - Inherit from `AuthorizedComponentBase` if needed

11. **Write Tests**
    - Unit tests in `IoTHub.Portal.Tests.Unit/`
    - Inherit from `BackendUnitTest` for backend tests
    - Use Moq for mocking dependencies
    - Use AutoFixture for test data generation
    - Follow Arrange-Act-Assert pattern

12. **Register Services**
    - Add service registrations to appropriate Startup extension method
    - Use appropriate lifetime (Transient, Scoped, Singleton)

13. **Update Documentation**
    - Complete feature analysis document
    - Update `docs/analyze.md` feature list
    - Add any architectural decision records

### Modifying Existing Features

1. **Locate Feature Components**
   - Check `specs/{feature}/analyze.md` for code locations
   - Identify controller, service, repository, and UI files

2. **Update Layer by Layer**
   - Domain: Update entities and interfaces
   - Infrastructure: Update repositories and implementations
   - Application: Update services and DTOs
   - Presentation: Update controllers and UI components

3. **Maintain Backward Compatibility**
   - Don't break existing API contracts
   - Add new endpoints rather than changing existing ones
   - Use API versioning for breaking changes

4. **Update Tests**
   - Modify existing tests to match new behavior
   - Add tests for new functionality
   - Ensure all tests pass

5. **Update Documentation**
   - Update feature analysis document
   - Update XML comments if API changed
   - Document breaking changes in release notes

### Testing Requirements

- **Unit Tests**: Required for all services and business logic
- **Controller Tests**: Required for all API endpoints
- **UI Tests**: Required for critical user workflows
- **Integration Tests**: Required for external service integrations
- **Test Naming**: `{MethodName}Should{ExpectedBehavior}`
- **Test Pattern**: Arrange-Act-Assert with clear section comments

Example:
```csharp
[Test]
public async Task GetUserShouldReturnAList()
{
    // Arrange
    var expectedUsers = Fixture.CreateMany<UserModel>(3).ToList();
    // ... setup mocks
    
    // Act
    var result = await this.userService.GetUserPage();
    
    // Assert
    _ = result.Data.Should().BeEquivalentTo(expectedUsers);
    MockRepository.VerifyAll();
}
```

---

## 9. Anti-Patterns to Avoid

Based on code analysis and project conventions:

- ❌ **Exposing Domain Entities in API**: Never return or accept domain entities directly; always use DTOs
- ❌ **Direct DbContext Access in Controllers**: Controllers must call services, not repositories or DbContext
- ❌ **String-Based Permissions**: Use PortalPermissions enum and AsString() extension method
- ❌ **Catching Generic Exceptions**: Catch specific exceptions; let BaseException subtypes propagate
- ❌ **Hardcoded Configuration**: Use ConfigHandler for all environment-specific settings
- ❌ **Missing Authorization Attributes**: All endpoints must have [Authorize] at minimum
- ❌ **N+1 Query Problems**: Use Include() for related entities in repository queries
- ❌ **Async Without Await**: Don't create async methods that don't await anything
- ❌ **Missing XML Documentation**: Public APIs must have XML doc comments
- ❌ **Repository Logic in Services**: Keep repository methods simple; complex queries belong in repositories
- ❌ **UI Business Logic**: Business rules belong in services, not Razor components
- ❌ **Missing this. Qualifier for Fields**: Private fields must use `this.` qualifier (dotnet_style_qualification_for_field=true)
- ❌ **Unused Using Directives**: Remove unused imports (IDE0005)
- ❌ **Constructor Injection of >5 Dependencies**: If a class needs many dependencies, consider refactoring

---

## 10. Quick Reference

### Common Locations

| What | Where |
|------|-------|
| API Controllers | `src/IoTHub.Portal.Server/Controllers/v1.0/` |
| Service Interfaces | `src/IoTHub.Portal.Application/Services/` |
| Service Implementations | `src/IoTHub.Portal.Infrastructure/Services/` or `src/IoTHub.Portal.Server/Services/` |
| Domain Entities | `src/IoTHub.Portal.Domain/Entities/` |
| Repository Interfaces | `src/IoTHub.Portal.Domain/Repositories/` |
| Repository Implementations | `src/IoTHub.Portal.Infrastructure/Repositories/` |
| DTOs | `src/IoTHub.Portal.Shared/Models/v1.0/` |
| AutoMapper Profiles | `src/IoTHub.Portal.Application/Mappers/` |
| Blazor Pages | `src/IoTHub.Portal.Client/Pages/` |
| Blazor Components | `src/IoTHub.Portal.Client/Components/` |
| Client Services | `src/IoTHub.Portal.Client/Services/` |
| Background Jobs | `src/IoTHub.Portal.Infrastructure/Jobs/` |
| Custom Exceptions | `src/IoTHub.Portal.Domain/Exceptions/` |
| Feature Analysis Docs | `specs/{feature-number}-{feature-name}/analyze.md` |
| Unit Tests | `src/IoTHub.Portal.Tests.Unit/` |

### Key Files

| File | Purpose |
|------|---------|
| `Startup.cs` | Service registration and middleware configuration |
| `Program.cs` | Application entry point |
| `PortalDbContext.cs` | Entity Framework DbContext |
| `ConfigHandler.cs` | Configuration abstraction interface |
| `GlobalUsings.cs` | Global using directives per project |
| `PortalPermissions.cs` | Permission enumeration |
| `PermissionsExtension.cs` | Permission string conversion |
| `EntityBase.cs` | Base class for all entities |
| `IRepository.cs` | Generic repository interface |
| `IUnitOfWork.cs` | Transaction management interface |
| `.editorconfig` | Code style and formatting rules |

---

## Appendix: Feature Categories

| Category | Feature Count | Description |
|----------|---------------|-------------|
| Device Management | 5 | CRUD operations for standard IoT devices, models, properties, tags, and configurations |
| Edge Devices | 2 | Management of Azure IoT Edge devices and edge device models |
| LoRaWAN | 5 | LoRaWAN device/model management, concentrators, commands, and frequency plans |
| RBAC & Security | 4 | User, role, access control, and permissions management |
| Planning & Scheduling | 2 | Device planning and schedule management |
| Building Management | 1 | Hierarchical layer management for device organization |
| Monitoring & Analytics | 1 | Dashboard with device and concentrator metrics |
| Admin Operations | 1 | Device import/export functionality |
| Community Features | 1 | External ideas submission integration |
| Configuration | 1 | Portal settings management |
| Background Jobs | 3 | Device synchronization, metrics collection, and planning command jobs |

**Total Features**: 26 (as of 2026-01-30)

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-01-30 | Initial constitution generated from codebase analysis |

---

## Contributing

When contributing to this project:

1. **Read this constitution thoroughly** - Understand the established patterns
2. **Follow the architecture** - Maintain clean separation of concerns
3. **Match existing conventions** - Consistency is critical for maintainability
4. **Document your changes** - Update feature analysis and this constitution if introducing new patterns
5. **Write tests** - All new code must have corresponding unit tests
6. **Review the code** - Ensure your changes align with security and quality standards

For questions or clarifications about these standards, open a discussion in the GitHub repository.
