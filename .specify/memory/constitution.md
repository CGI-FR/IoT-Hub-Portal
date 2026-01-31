# Project Constitution

> This document defines the authoritative standards, patterns, and guidelines for the IoT Hub Portal project.
> It is derived from actual codebase analysis and should be maintained alongside code changes.

**Last Updated**: January 31, 2026  
**Source**: Generated from `docs/analyze.md` and deep codebase analysis

---

## 1. Project Overview

### Purpose

IoT Hub Portal is an open-source solution for managing IoT devices through a web-based interface. It provides:
- Device and device model lifecycle management
- Edge device management
- LoRaWAN device connectivity and management
- Cloud-to-Device (C2D) methods and commands
- Role-Based Access Control (RBAC) for secure multi-tenant operations

The portal supports multiple cloud providers (Azure IoT Hub and AWS IoT Core) and multiple databases (PostgreSQL, MySQL).

### Modules

| Module | Purpose | Location |
|--------|---------|----------|
| **IoTHub.Portal.Server** | API Controllers, Web Host, Authentication | `src/IoTHub.Portal.Server/` |
| **IoTHub.Portal.Client** | Blazor WebAssembly UI components | `src/IoTHub.Portal.Client/` |
| **IoTHub.Portal.Application** | Business logic services and managers | `src/IoTHub.Portal.Application/` |
| **IoTHub.Portal.Domain** | Domain entities, repository interfaces, exceptions | `src/IoTHub.Portal.Domain/` |
| **IoTHub.Portal.Infrastructure** | Data access, external services, background jobs | `src/IoTHub.Portal.Infrastructure/` |
| **IoTHub.Portal.Shared** | DTOs, models, constants, security definitions | `src/IoTHub.Portal.Shared/` |
| **IoTHub.Portal.Crosscutting** | Cross-cutting utilities and helpers | `src/IoTHub.Portal.Crosscutting/` |
| **IoTHub.Portal.MySql** | MySQL-specific EF Core configurations | `src/IoTHub.Portal.MySql/` |
| **IoTHub.Portal.Postgres** | PostgreSQL-specific EF Core configurations | `src/IoTHub.Portal.Postgres/` |
| **IoTHub.Portal.Tests.Unit** | Unit test project | `src/IoTHub.Portal.Tests.Unit/` |
| **IoTHub.Portal.Tests.E2E** | End-to-end test project | `src/IoTHub.Portal.Tests.E2E/` |

---

## 2. Architecture Principles

### Clean / Layered Architecture

The project follows Clean Architecture principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                    IoTHub.Portal.Server                     │
│            (Controllers, Filters, Authentication)           │
├─────────────────────────────────────────────────────────────┤
│                  IoTHub.Portal.Application                  │
│           (Services, Managers, Mappers, Providers)          │
├─────────────────────────────────────────────────────────────┤
│                    IoTHub.Portal.Domain                     │
│     (Entities, Repository Interfaces, Exceptions, Base)     │
├─────────────────────────────────────────────────────────────┤
│                 IoTHub.Portal.Infrastructure                │
│   (Repository Implementations, DbContext, Jobs, External)   │
└─────────────────────────────────────────────────────────────┘
         ↑                                    ↑
┌─────────────────┐                ┌─────────────────────────┐
│ IoTHub.Portal   │                │   IoTHub.Portal.Shared  │
│    .Client      │                │   (DTOs, Constants)     │
│ (Blazor WASM)   │                └─────────────────────────┘
└─────────────────┘
```

### Dependency Direction

Dependencies flow **inward** toward the Domain layer:
- **Server** → Application → Domain
- **Infrastructure** → Domain (implements repository interfaces)
- **Shared** is referenced by multiple layers for DTOs

### Key Patterns

- **Repository Pattern**: All data access goes through `IRepository<T>` interfaces defined in Domain, implemented in Infrastructure
- **Unit of Work**: `IUnitOfWork` coordinates database transactions across repositories
- **Dependency Injection**: All services are registered through `Startup.cs` files in each layer
- **AutoMapper Profiles**: Entity-to-DTO mappings defined in `Application/Mappers/` folder
- **Background Jobs**: Scheduled tasks using Quartz.NET with `IJob` implementations in `Infrastructure/Jobs/`
- **Generic Repository**: `GenericRepository<T>` base class for common CRUD operations

---

## 3. Coding Standards

### File Organization

```
[Project]/
├── GlobalUsings.cs          # Global using statements
├── [Project].csproj         # Project file
├── Startup.cs               # DI registration (if applicable)
├── Controllers/             # API controllers (Server only)
│   └── v1.0/               # API versioned controllers
├── Services/                # Service implementations
├── Managers/                # Manager implementations
├── Mappers/                 # AutoMapper profiles
├── Repositories/            # Repository implementations (Infrastructure)
├── Entities/                # Domain entities (Domain)
├── Exceptions/              # Custom exceptions (Domain)
├── Models/                  # DTOs (Shared)
│   └── v1.0/               # API version DTOs
├── Helpers/                 # Static helper classes
└── Jobs/                    # Background job implementations
```

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Files | PascalCase matching class name | `DevicesController.cs` |
| Classes | PascalCase | `DeviceService`, `UserRepository` |
| Interfaces | `I` prefix + PascalCase | `IDeviceService`, `IRepository<T>` |
| Public Methods | PascalCase | `GetDeviceAsync`, `CreateUser` |
| Private Fields | camelCase | `deviceService`, `unitOfWork` |
| Private Constants | PascalCase | `ModelId`, `DefaultPageSize` |
| Parameters | camelCase | `deviceId`, `searchText` |
| Async Methods | `Async` suffix | `GetByIdAsync`, `SaveAsync` |
| DTOs | Model suffix | `DeviceModel`, `UserDetailsModel` |
| Entities | No suffix | `Device`, `User`, `Role` |
| Controllers | `Controller` suffix | `DevicesController`, `UsersController` |
| Repositories | `Repository` suffix | `DeviceRepository`, `UserRepository` |
| Services | `Service` suffix | `DeviceService`, `UserService` |
| Mappers | `Profile` suffix | `DeviceProfile`, `UserProfile` |
| Jobs | `Job` suffix | `SyncDevicesJob`, `DeviceMetricLoaderJob` |

### Code Structure Within Files

1. **File Header**: Required copyright notice (enforced by `.editorconfig`)
   ```csharp
   // Copyright (c) CGI France. All rights reserved.
   // Licensed under the MIT license. See LICENSE file in the project root for full license information.
   ```

2. **Namespace Declaration**: Using statements inside namespace (block-scoped)
   ```csharp
   namespace IoTHub.Portal.Application.Services
   {
       using AutoMapper;
       using IoTHub.Portal.Domain;
       // ...
   }
   ```

3. **Class Organization**:
   - Private fields first
   - Constructor(s)
   - Public methods
   - Private methods

### Formatting Rules (from `.editorconfig`)

| Rule | Setting |
|------|---------|
| Indentation | 4 spaces |
| Line endings | CRLF |
| Final newline | Required |
| Trailing whitespace | Trimmed |
| Braces | `when_multiline` |
| Using placement | Inside namespace |
| `var` usage | When type is apparent |
| `this.` qualifier | Required for fields, not for properties/methods |

---

## 4. Security & Authorization

### Permission Model

Permissions are defined as static enumerations in `IoTHub.Portal.Shared/Security/PortalPermissions.cs`:

```csharp
public enum PortalPermissions
{
    // Access Control
    AccessControlRead, AccessControlWrite,
    
    // Devices
    DeviceRead, DeviceWrite, DeviceExecute,
    DeviceImport, DeviceExport,
    
    // Device Models
    ModelRead, ModelWrite,
    
    // Edge Devices
    EdgeDeviceRead, EdgeDeviceWrite, EdgeDeviceExecute,
    EdgeModelRead, EdgeModelWrite,
    
    // LoRaWAN
    ConcentratorRead, ConcentratorWrite,
    
    // RBAC
    UserRead, UserWrite,
    RoleRead, RoleWrite,
    
    // Planning & Scheduling
    PlanningRead, PlanningWrite,
    ScheduleRead, ScheduleWrite,
    
    // Other
    LayerRead, LayerWrite,
    DashboardRead, SettingRead, IdeaWrite
}
```

### Permission Format

Permissions follow the pattern: `{resource}:{action}`
- `device:read`, `device:write`
- `user:read`, `user:write`
- `role:read`, `role:write`
- `edge-device:read`, `edge-device:write`
- `access-control:read`, `access-control:write`

### Authorization Enforcement

Controllers use policy-based authorization:

```csharp
[Authorize]                    // Class-level: requires authentication
[Authorize("device:read")]     // Method-level: requires specific permission
public async Task<DeviceDetails> GetItem(string deviceID)
```

### RBAC Architecture

```
Principal (User) ──────┐
                       ├── AccessControl ──── Role ──── Actions (Permissions)
                       │        │
                       │        └── Scope (e.g., "*" for all, or specific resource)
                       │
                       └── via PrincipalId on User entity
```

- **Principal**: Identity entity linking to access controls
- **AccessControl**: Maps Principal to Role with a Scope
- **Role**: Named collection of Actions (permissions)
- **Action**: Individual permission identifier

### Authentication Flow

1. OAuth2/OIDC authentication with external providers (Azure AD B2C, etc.)
2. Email claim extracted from authenticated user
3. User auto-provisioned on first login
4. First user automatically becomes administrator
5. Permissions checked via `IAccessControlManagementService.UserHasPermissionAsync()`

---

## 5. Error Handling

### Exception Hierarchy

```
Exception (System)
    └── BaseException (Domain)
            ├── ResourceNotFoundException    → 404 Not Found
            ├── ResourceAlreadyExistsException → 400 Bad Request
            ├── InternalServerErrorException  → 500 Internal Server Error
            └── InvalidCloudProviderException → 400 Bad Request
```

### BaseException Structure

```csharp
public abstract class BaseException : Exception
{
    public string Title { get; set; }   // Error type identifier
    public string Detail { get; set; }  // Detailed error message
}
```

### Exception to HTTP Mapping (Startup.cs)

| Exception Type | HTTP Status | Title |
|---------------|-------------|-------|
| `ResourceNotFoundException` | 404 | From exception |
| `InternalServerErrorException` | 500 | From exception |
| `BaseException` (other) | 400 | From exception |
| `ArgumentNullException` | 400 | "Null Argument" |
| `UniqueConstraintException` | 500 | "Unique Violation" |
| `CannotInsertNullException` | 500 | "Not Null Violation" |
| `MaxLengthExceededException` | 500 | "String Data Right Truncation" |

### Error Response Format (ProblemDetails)

```json
{
    "type": "https://httpstatuses.com/404",
    "title": "Resource not found",
    "status": 404,
    "detail": "The device with id xyz doesn't exist"
}
```

### Throwing Exceptions

```csharp
// Resource not found
if (user == null)
    throw new ResourceNotFoundException($"The user with the id {id} doesn't exist");

// Duplicate resource
if (existingName != null)
    throw new ResourceAlreadyExistsException($"The User with the name {user.Name} already exists!");
```

---

## 6. Configuration Management

### Configuration Hierarchy

```
ConfigHandler (Abstract - Domain)
    └── ConfigHandlerBase (Abstract - Infrastructure)
            ├── DevelopmentConfigHandler (Infrastructure)
            ├── ProductionAzureConfigHandler (Infrastructure)
            └── ProductionAWSConfigHandler (Infrastructure)
```

### Configuration Sources

| Source | Purpose |
|--------|---------|
| `appsettings.json` | Default configuration |
| `appsettings.Production.json` | Production overrides |
| Environment Variables | Runtime configuration |
| Azure Key Vault (Production) | Secrets management |

### Configuration Categories

| Category | Key Examples |
|----------|-------------|
| **Portal** | `PortalName`, `PortalUrl` |
| **Database** | `PostgreSQLConnectionString`, `MySQLConnectionString`, `DbProvider` |
| **Azure IoT Hub** | `AzureIoTHubConnectionString`, `AzureDPSConnectionString` |
| **AWS IoT Core** | `AWSAccess`, `AWSAccessSecret`, `AWSRegion` |
| **Authentication** | `OIDCAuthority`, `OIDCClientId`, `OIDCScope` |
| **LoRaWAN** | `IsLoRaEnabled`, `AzureLoRaKeyManagementUrl` |
| **Jobs** | `SyncDatabaseJobRefreshIntervalInMinutes`, `MetricExporterRefreshIntervalInSeconds` |

### Cloud Provider Selection

The `CloudProvider` setting determines which configuration handler is used:
- `Azure`: Uses Azure IoT Hub services
- `AWS`: Uses AWS IoT Core services

### Database Provider Selection

The `DbProvider` setting determines the database:
- `PostgreSQL`: Uses PostgreSQL with corresponding migrations
- `MySQL`: Uses MySQL with corresponding migrations

---

## 7. Documentation Requirements

### Feature Specifications

Location: `specs/{feature-folder}/`

Each feature should contain:
- `analyze.md`: Comprehensive feature analysis including:
  - Description and business value
  - Code locations (controllers, services, entities, DTOs, UI components)
  - API endpoints with authorization requirements
  - Data flow and dependencies
  - Business rules and edge cases

### Specification Template

```markdown
# Feature: [Feature Name]

**Category**: [Category]  
**Status**: Analyzed  

## Description
[Detailed feature description and business value]

## Code Locations

### Entry Points / Endpoints
- `path/to/Controller.cs` (Lines X-Y)

### Business Logic
- `path/to/Service.cs` (Lines X-Y)

### Data Access
- `path/to/Repository.cs` (Lines X-Y)

### UI Components
- `path/to/Page.razor` (Lines X-Y)
```

### Code Documentation

- XML documentation for public APIs
- Summary comments on public methods
- Parameter descriptions for complex methods

---

## 8. Development Workflow

### Adding New Features

1. **Create Feature Branch**: `{issue-number}-feature-name`

2. **Domain Layer First**:
   - Add entity in `Domain/Entities/`
   - Add repository interface in `Domain/Repositories/`
   - Add exceptions if needed in `Domain/Exceptions/`

3. **Infrastructure Layer**:
   - Implement repository in `Infrastructure/Repositories/`
   - Add DbContext configuration
   - Create migrations for both PostgreSQL and MySQL

4. **Application Layer**:
   - Add service interface and implementation in `Application/Services/`
   - Create AutoMapper profile in `Application/Mappers/`
   - Add DTOs in `Shared/Models/v1.0/`

5. **Server Layer**:
   - Add controller in `Server/Controllers/v1.0/`
   - Apply `[Authorize("{resource}:{action}")]` attributes
   - Register services in `Startup.cs`

6. **Client Layer**:
   - Add Blazor pages in `Client/Pages/`
   - Add client services in `Client/Services/`

7. **Testing**:
   - Add unit tests in `Tests.Unit/`
   - Tests should mock dependencies

### API Versioning

- Current version: `1.0`
- Controllers use `[ApiVersion("1.0")]`
- Routes: `/api/{resource}`
- DTOs versioned in `Shared/Models/v1.0/`

### Testing Requirements

| Type | Project | Purpose |
|------|---------|---------|
| Unit Tests | `IoTHub.Portal.Tests.Unit` | Test individual components in isolation |
| E2E Tests | `IoTHub.Portal.Tests.E2E` | Test complete user workflows |

Test naming: `{MethodName}_{Scenario}_{ExpectedBehavior}`

---

## 9. Anti-Patterns to Avoid

❌ **Direct Database Access**: Never access `PortalDbContext` directly from controllers or services outside Infrastructure layer

❌ **Bypassing Repository Pattern**: Always use repository interfaces for data access

❌ **Hard-coded Configuration**: Use `ConfigHandler` abstraction, not direct `IConfiguration` access

❌ **Skipping Authorization**: Every controller action must have appropriate `[Authorize]` attributes

❌ **Mixing Concerns**: Keep business logic in Application layer, not in Controllers

❌ **Ignoring Unit of Work**: Always call `unitOfWork.SaveAsync()` after repository modifications

❌ **Throwing Generic Exceptions**: Use domain-specific exceptions (`ResourceNotFoundException`, etc.)

❌ **Missing File Headers**: All `.cs` files must have the CGI France copyright header

❌ **Inconsistent Naming**: Follow established naming conventions strictly

❌ **Synchronous Database Calls**: Use async methods (`GetByIdAsync`, not `GetById`)

---

## 10. Quick Reference

### Common Locations

| What | Where |
|------|-------|
| API Controllers | `src/IoTHub.Portal.Server/Controllers/v1.0/` |
| Services | `src/IoTHub.Portal.Application/Services/` |
| Entities | `src/IoTHub.Portal.Domain/Entities/` |
| Repositories (Interface) | `src/IoTHub.Portal.Domain/Repositories/` |
| Repositories (Implementation) | `src/IoTHub.Portal.Infrastructure/Repositories/` |
| DTOs | `src/IoTHub.Portal.Shared/Models/v1.0/` |
| Blazor Pages | `src/IoTHub.Portal.Client/Pages/` |
| Background Jobs | `src/IoTHub.Portal.Infrastructure/Jobs/` |
| AutoMapper Profiles | `src/IoTHub.Portal.Application/Mappers/` |
| Permissions | `src/IoTHub.Portal.Shared/Security/PortalPermissions.cs` |
| Feature Specs | `specs/` |

### Key Files

| File | Purpose |
|------|---------|
| `src/.editorconfig` | Coding style rules and formatting |
| `src/Directory.Build.props` | Shared MSBuild properties |
| `src/IoTHub.Portal.sln` | Solution file |
| `src/IoTHub.Portal.Server/Startup.cs` | DI configuration and middleware |
| `src/IoTHub.Portal.Server/appsettings.json` | Application configuration |
| `src/IoTHub.Portal.Domain/ConfigHandler.cs` | Configuration abstraction |
| `src/IoTHub.Portal.Infrastructure/PortalDbContext.cs` | EF Core DbContext |
| `CONTRIBUTING.md` | Contribution guidelines |
| `README.md` | Project documentation |

---

## Appendix: Feature Categories

| Category | Feature Count | Description |
|----------|---------------|-------------|
| Device Management | 5 | Standard device CRUD, models, properties, tags, configurations |
| Edge Devices | 2 | IoT Edge device and model management |
| LoRaWAN | 5 | LoRaWAN devices, models, concentrators, commands, frequency plans |
| RBAC & Security | 4 | Users, roles, access controls, permissions |
| Planning & Scheduling | 2 | Planning and schedule management for devices |
| Building Management | 1 | Layer/building management |
| Monitoring & Analytics | 1 | Dashboard metrics |
| Admin Operations | 1 | Device import/export |
| Community Features | 1 | Ideas submission |
| Configuration | 1 | Portal settings |
| Background Jobs | 3 | Device sync, metrics collection, planning commands |

---

## Appendix: Technology Stack

| Category | Technology |
|----------|------------|
| **Backend Framework** | ASP.NET Core 8.0 |
| **Frontend Framework** | Blazor WebAssembly |
| **Language** | C# |
| **ORM** | Entity Framework Core |
| **Database** | PostgreSQL, MySQL |
| **Cloud Providers** | Azure IoT Hub, AWS IoT Core |
| **Authentication** | OAuth2/OIDC (Azure AD B2C) |
| **Object Mapping** | AutoMapper |
| **UI Components** | MudBlazor |
| **Background Jobs** | Quartz.NET |
| **API Documentation** | Swagger/OpenAPI |
| **Testing** | xUnit, bUnit, Moq |

---

*This constitution should be updated whenever significant architectural changes occur or new patterns are established.*
