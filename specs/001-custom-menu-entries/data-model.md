# Data Model: Custom Menu Entries - User Story 2

**Feature**: Custom Menu Entries - Edit and Delete Operations  
**Date**: 2026-02-01  
**Status**: Phase 1 Complete

## Overview

This document describes the data model for User Story 2 (Edit and Delete Menu Entries). **No changes to the data model are required** - US2 uses the exact same entity and DTO structures defined in User Story 1. This document serves as a reference for developers implementing US2.

---

## Entity Model

### MenuEntry Entity

**Location**: `src/IoTHub.Portal.Domain/Entities/MenuEntry.cs`

**Purpose**: Domain entity representing a custom menu entry in the portal's navigation system.

```csharp
namespace IoTHub.Portal.Domain.Entities
{
    /// <summary>
    /// Represents a custom menu entry that can link to internal or external resources
    /// </summary>
    public class MenuEntry : EntityBase
    {
        // Inherited from EntityBase:
        // public string Id { get; set; }  // GUID primary key
        
        /// <summary>
        /// Display name shown in the navigation menu (max 100 characters)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        /// <summary>
        /// Target URL - can be HTTP/HTTPS (external) or relative path (internal)
        /// </summary>
        [Required]
        public string Url { get; set; }
        
        /// <summary>
        /// Sort order for menu entries (lower values appear first)
        /// Used by User Story 4 (ordering feature)
        /// </summary>
        public int Order { get; set; } = 0;
        
        /// <summary>
        /// Whether this entry is enabled and visible in the menu
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// Auto-detected based on URL format
        /// True if URL starts with http:// or https://
        /// False if URL is a relative path starting with /
        /// </summary>
        public bool IsExternal { get; set; }
        
        /// <summary>
        /// Timestamp when entry was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Timestamp when entry was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
```

### Entity Properties

| Property | Type | Required | Default | Validation | US2 Relevance |
|----------|------|----------|---------|------------|---------------|
| **Id** | string (GUID) | Yes (auto) | Generated | Valid GUID | Used in PUT/DELETE endpoints |
| **Name** | string | Yes | - | Max 100 chars, unique | Updated in edit operation |
| **Url** | string | Yes | - | Valid URL format | Updated in edit operation |
| **Order** | int | No | 0 | Positive integer | Not used in US2 (US4 feature) |
| **IsEnabled** | bool | No | true | - | Can be toggled in edit |
| **IsExternal** | bool | No | Auto | Read-only | Auto-detected on update |
| **CreatedAt** | DateTime | Yes (auto) | UTC Now | - | Preserved on update |
| **UpdatedAt** | DateTime | Yes (auto) | UTC Now | - | Set to current time on update |

### Entity Base Class

```csharp
public abstract class EntityBase
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
}
```

**Inherited Properties**:
- `Id`: Unique identifier (GUID string format)

---

## Data Transfer Object (DTO)

### MenuEntryDto

**Location**: `src/IoTHub.Portal.Shared/Models/v1.0/MenuEntryDto.cs`

**Purpose**: Data contract for API requests and responses. Includes validation attributes for automatic model validation in controllers.

```csharp
namespace IoTHub.Portal.Shared.Models.v1.0
{
    /// <summary>
    /// Data transfer object for menu entry operations
    /// </summary>
    public class MenuEntryDto
    {
        /// <summary>
        /// Unique identifier (GUID format)
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Display name for the menu entry
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }
        
        /// <summary>
        /// Target URL (HTTP/HTTPS or relative path)
        /// </summary>
        [Required(ErrorMessage = "URL is required")]
        [Url(ErrorMessage = "URL must be a valid format")]
        public string Url { get; set; }
        
        /// <summary>
        /// Sort order (lower numbers appear first)
        /// </summary>
        public int Order { get; set; } = 0;
        
        /// <summary>
        /// Whether the entry is visible in the menu
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// Whether the URL points to an external resource
        /// Auto-detected based on URL format (read-only for clients)
        /// </summary>
        public bool IsExternal { get; set; }
    }
}
```

### DTO Properties vs Entity Properties

| Property | In DTO? | In Entity? | Notes |
|----------|---------|------------|-------|
| Id | ✅ | ✅ | Same GUID value |
| Name | ✅ | ✅ | With validation attributes |
| Url | ✅ | ✅ | With validation attributes |
| Order | ✅ | ✅ | Default value 0 |
| IsEnabled | ✅ | ✅ | Default value true |
| IsExternal | ✅ | ✅ | Auto-calculated |
| CreatedAt | ❌ | ✅ | Not exposed in API |
| UpdatedAt | ❌ | ✅ | Not exposed in API |

**Rationale for Omitting Timestamps from DTO**:
- CreatedAt and UpdatedAt are server-managed
- Clients should not set or modify timestamps
- Reduces payload size
- Prevents timestamp manipulation

---

## Validation Rules

### Field-Level Validation

Enforced via DataAnnotations attributes on DTO properties:

```csharp
// Name validation
[Required(ErrorMessage = "Name is required")]
[MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
public string Name { get; set; }

// URL validation  
[Required(ErrorMessage = "URL is required")]
[Url(ErrorMessage = "URL must be a valid format")]
public string Url { get; set; }
```

**Validation Execution**:
- Automatic in ASP.NET Core controllers (ModelState.IsValid)
- Manual in Blazor forms (MudForm validation)

### Business Logic Validation

Enforced in `MenuEntryService`:

**1. Duplicate Name Check**
```csharp
// For UPDATE operations
var existingEntry = await _repository.GetByNameAsync(dto.Name);
if (existingEntry != null && existingEntry.Id != dto.Id)
{
    throw new ResourceAlreadyExistsException(
        $"A menu entry with name '{dto.Name}' already exists"
    );
}
```

**2. URL Format Validation**
```csharp
// Validate URL is HTTP/HTTPS or relative path
bool isExternalUrl = dto.Url.StartsWith("http://") || dto.Url.StartsWith("https://");
bool isRelativePath = dto.Url.StartsWith("/");

if (!isExternalUrl && !isRelativePath)
{
    throw new ArgumentException("URL must be a valid HTTP/HTTPS URL or a relative path starting with /");
}

// Auto-detect IsExternal
dto.IsExternal = isExternalUrl;
```

**3. Entity Existence Check (Update/Delete)**
```csharp
var existingEntity = await _repository.GetByIdAsync(id);
if (existingEntity == null)
{
    throw new ResourceNotFoundException(
        $"The menu entry with id {id} doesn't exist"
    );
}
```

---

## Database Schema

### Table: MenuEntries

**PostgreSQL Schema**:
```sql
CREATE TABLE "MenuEntries" (
    "Id" VARCHAR(36) PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Url" TEXT NOT NULL,
    "Order" INTEGER NOT NULL DEFAULT 0,
    "IsEnabled" BOOLEAN NOT NULL DEFAULT TRUE,
    "IsExternal" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP NOT NULL
);

-- Optional index for duplicate name checks (performance optimization)
CREATE INDEX "IX_MenuEntries_Name" ON "MenuEntries" ("Name");
```

**MySQL Schema**:
```sql
CREATE TABLE `MenuEntries` (
    `Id` VARCHAR(36) PRIMARY KEY,
    `Name` VARCHAR(100) NOT NULL,
    `Url` TEXT NOT NULL,
    `Order` INT NOT NULL DEFAULT 0,
    `IsEnabled` BOOLEAN NOT NULL DEFAULT TRUE,
    `IsExternal` BOOLEAN NOT NULL DEFAULT FALSE,
    `CreatedAt` DATETIME NOT NULL,
    `UpdatedAt` DATETIME NOT NULL,
    INDEX `IX_MenuEntries_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### Constraints

| Constraint | Type | Description |
|------------|------|-------------|
| Primary Key | Id | Unique identifier (GUID) |
| Not Null | Name, Url, Order, IsEnabled, IsExternal | Required fields |
| Max Length | Name (100) | Enforced at application layer |
| Default Value | Order (0), IsEnabled (true) | Set on insert |

**No Foreign Keys**: MenuEntry is a standalone entity with no relationships.

---

## Entity Lifecycle

### Create Lifecycle (User Story 1)

```
1. User submits create form
   ↓
2. DTO validation (DataAnnotations)
   ↓
3. Service validation (duplicate check, URL format)
   ↓
4. Entity created with:
   - Id = new GUID
   - CreatedAt = UTC now
   - UpdatedAt = UTC now
   - IsExternal = auto-detected
   ↓
5. Repository.Add() + UnitOfWork.SaveAsync()
   ↓
6. Return created entity (mapped to DTO)
```

### Update Lifecycle (User Story 2)

```
1. User submits edit form with entry Id
   ↓
2. DTO validation (DataAnnotations)
   ↓
3. Service retrieves existing entity by Id
   ↓
4. Service validation:
   - Entity exists? (else throw ResourceNotFoundException)
   - Duplicate name? (else throw ResourceAlreadyExistsException)
   - URL format valid?
   ↓
5. Update entity properties:
   - Name = dto.Name
   - Url = dto.Url
   - IsEnabled = dto.IsEnabled
   - Order = dto.Order
   - IsExternal = auto-detected
   - UpdatedAt = UTC now
   - CreatedAt = UNCHANGED
   ↓
6. Repository.Update() + UnitOfWork.SaveAsync()
   ↓
7. Return updated entity (mapped to DTO)
```

### Delete Lifecycle (User Story 2)

```
1. User confirms delete (with entry Id)
   ↓
2. Service retrieves existing entity by Id
   ↓
3. Service validation:
   - Entity exists? (else throw ResourceNotFoundException)
   ↓
4. Repository.Delete(entity)
   ↓
5. UnitOfWork.SaveAsync()
   ↓
6. Return success (204 No Content)
```

---

## Mapping Configuration

### AutoMapper Profile

**Location**: `src/IoTHub.Portal.Application/Mappers/MenuEntryMapper.cs`

```csharp
public class MenuEntryProfile : Profile
{
    public MenuEntryProfile()
    {
        // Entity → DTO (for API responses)
        CreateMap<MenuEntry, MenuEntryDto>()
            .ReverseMap();  // DTO → Entity (for create/update)
    }
}
```

**Mapping Behavior**:
- **Entity → DTO**: All properties mapped (except CreatedAt/UpdatedAt not in DTO)
- **DTO → Entity**: All properties mapped, timestamps set by service layer

### Manual Mapping for Update

```csharp
// In MenuEntryService.UpdateMenuEntryAsync()
var existingEntity = await _repository.GetByIdAsync(dto.Id);

// Update only editable fields
existingEntity.Name = dto.Name;
existingEntity.Url = dto.Url;
existingEntity.Order = dto.Order;
existingEntity.IsEnabled = dto.IsEnabled;
existingEntity.IsExternal = dto.Url.StartsWith("http://") || dto.Url.StartsWith("https://");
existingEntity.UpdatedAt = DateTime.UtcNow;
// CreatedAt is preserved (not updated)

await _repository.Update(existingEntity);
await _unitOfWork.SaveAsync();
```

---

## State Transitions

### Menu Entry States

Menu entries have a simple state model:

```
┌─────────┐  Create   ┌────────┐  Update   ┌────────┐
│ Non-    ├──────────>│ Active ├──────────>│ Active │
│ Existent│           │(Enabled)│           │(Enabled)│
└─────────┘           └────┬───┘           └────┬───┘
                           │                    │
                           │ Disable            │ Delete
                           ↓                    ↓
                      ┌────────┐           ┌─────────┐
                      │ Inactive├─────────>│ Deleted │
                      │(Disabled)│  Delete  │(Gone)   │
                      └────────┘           └─────────┘
                           ↑                    ↑
                           │ Enable             │
                           └────────────────────┘
```

**State Definitions**:
- **Non-Existent**: Entry does not exist in database
- **Active (Enabled)**: Entry visible in navigation menu (IsEnabled = true)
- **Inactive (Disabled)**: Entry hidden from navigation (IsEnabled = false)
- **Deleted**: Entry removed from database (hard delete)

**US2 Operations**:
- **Update**: Active ↔ Active or Active ↔ Inactive (change IsEnabled)
- **Delete**: Any state → Deleted

---

## Concurrency Handling

### Optimistic Concurrency (Optional)

**Current Implementation**: No concurrency tokens

**Behavior**: Last write wins
- If two users update the same entry simultaneously, the second update overwrites the first
- Acceptable for menu entries (low-frequency updates, non-critical data)

**Future Enhancement** (if needed):
```csharp
public class MenuEntry : EntityBase
{
    [Timestamp]
    public byte[] RowVersion { get; set; }  // EF Core concurrency token
}
```

This would enable optimistic concurrency:
- EF Core checks RowVersion on update
- Throws `DbUpdateConcurrencyException` if changed
- Controller returns 409 Conflict

**Decision**: Not implemented for US2 (not required by specification).

---

## Data Model Summary

### Key Characteristics

| Aspect | Description |
|--------|-------------|
| **Complexity** | Simple - single entity, no relationships |
| **Validation** | Multi-layered (DataAnnotations + Service logic) |
| **Timestamps** | Auto-managed (CreatedAt, UpdatedAt) |
| **Soft Delete** | No - hard delete used |
| **Concurrency** | Last write wins (no tokens) |
| **Indexing** | Primary key (Id), optional index on Name |

### Changes from US1

**None** - Data model is identical. US2 uses the same entity and DTO structures.

### Future Extensions

Potential extensions for future user stories (US4, US5):

**User Story 4 (Ordering)**:
- Order property already present (no schema changes needed)
- Add batch update endpoint for reordering

**User Story 5 (Section Positioning)**:
- New entity: `MenuSectionConfiguration`
- Properties: Position (enum), AfterSection (string)
- Separate migration required

---

## Related Documentation

- **API Contracts**: See [contracts/menu-entries-api.yaml](./contracts/menu-entries-api.yaml)
- **Quickstart Guide**: See [quickstart.md](./quickstart.md)
- **Implementation Plan**: See [plan.md](./plan.md)
- **Research Findings**: See [research.md](./research.md)

---

*Data model documentation completed: 2026-02-01*  
*Next: API contracts generation*
