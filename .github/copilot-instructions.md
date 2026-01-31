# Copilot Instructions for IoT Hub Portal

> **REQUIRED**: Read [.github/constitution.md](constitution.md) for authoritative project standards.

## Quick Context

This is an **ASP.NET Core + Blazor WebAssembly** IoT device management portal supporting Azure IoT Hub and AWS IoT Core with PostgreSQL/MySQL backends.

## Architecture (Clean Architecture)

```
Server (Controllers) → Application (Services) → Domain (Entities) ← Infrastructure (Repositories)
```

- **Domain-first**: Start new features in `Domain/Entities/` and `Domain/Repositories/`
- **DTOs in Shared**: All models in `Shared/Models/v1.0/`
- **Repository Pattern**: Never access `PortalDbContext` directly outside Infrastructure

## Essential Patterns

### API Controllers
```csharp
[Authorize]                        // Required on class
[Authorize("device:read")]         // Required on methods - format: {resource}:{action}
[ApiVersion("1.0")]
[Route("api/{resource}")]
```

### Exceptions
- `ResourceNotFoundException` → 404
- `ResourceAlreadyExistsException` → 400
- Always include descriptive messages

### Data Access
```csharp
// Always use repositories + unit of work
await repository.InsertAsync(entity);
await unitOfWork.SaveAsync();  // Required to commit
```

## File Header (Required)
```csharp
// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
```

## Key Conventions

| Item | Convention |
|------|------------|
| Async methods | `{Name}Async` suffix |
| Private fields | camelCase, `this.` prefix |
| Using directives | Inside namespace block |
| DTOs | `{Entity}Model` or `{Entity}DetailsModel` |
| Permissions | Enum in `Shared/Security/PortalPermissions.cs` |

## Don't

- ❌ Skip authorization attributes on controller actions
- ❌ Use `IConfiguration` directly (use `ConfigHandler` abstraction)
- ❌ Put business logic in controllers
- ❌ Forget `unitOfWork.SaveAsync()` after repository changes
- ❌ Throw generic `Exception` (use domain exceptions)

## Testing

- Unit tests: `src/IoTHub.Portal.Tests.Unit/`
- Mock all dependencies
- Name: `{Method}_{Scenario}_{Expected}`

## Feature Specs

New features documented in `specs/{feature-folder}/analyze.md`
