# Implementation Plan: Custom Menu Entries - User Story 2 (Edit and Delete)

**Branch**: `001-custom-menu-entries` | **Date**: 2026-02-01 | **Spec**: [spec.md](./spec.md)  
**Input**: Feature specification from `/specs/001-custom-menu-entries/spec.md`

## Summary

This plan covers **User Story 2: Edit and Delete Menu Entries** of the Custom Menu Entries feature. Users can modify existing menu entries (change name, URL, or other properties) and delete entries that are no longer needed. This enables long-term maintenance of the menu structure as tools and resources evolve.

**Technical Approach**: Build upon the existing US1 foundation (entity, repository, service, controller, client service, and UI already implemented). US2 requires implementing the edit/delete operations that were scaffolded but need proper validation, error handling, authorization checks, and user interface enhancements including delete confirmation dialogs.

## Technical Context

**Language/Version**: C# / .NET 8.0  
**Primary Dependencies**: ASP.NET Core 8.0, Blazor WebAssembly, Entity Framework Core 8.0, MudBlazor 6.x, AutoMapper  
**Storage**: PostgreSQL 13+ OR MySQL 8.0+ (dual database support via separate migration projects)  
**Testing**: xUnit, Moq, bUnit for Blazor component tests  
**Target Platform**: Web (Server-side API + Client-side Blazor WASM)  
**Project Type**: Web application with Clean Architecture (Domain → Application → Infrastructure → Server + Client)  
**Performance Goals**: 
- API operations complete within 200ms for typical menu management tasks
- UI updates reflect immediately (under 100ms response time)
- Support up to 50 custom menu entries without performance degradation

**Constraints**: 
- Must maintain backward compatibility with US1 (no breaking changes to existing APIs or data model)
- Edit and delete must work independently (not dependent on other unimplemented user stories)
- Full RBAC enforcement with `menuentry:write` permission
- Validation consistent with US1 (name required, max 100 chars, valid URL format, duplicate name check)

**Scale/Scope**: 
- Single feature with 2 core operations (edit, delete)
- 21 tasks total (T036-T056 from tasks.md)
- Extends existing 27 unit tests with 4 new service/controller tests + 2 client tests
- Estimated complexity: 3-4 days for full implementation and testing

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### ✅ Architecture Compliance

| Requirement | Status | Evidence |
|-------------|--------|----------|
| Clean Architecture layers | ✅ PASS | Domain → Application → Infrastructure → Server + Client pattern maintained |
| Repository Pattern | ✅ PASS | IMenuEntryRepository already defines Update/Delete methods |
| Unit of Work | ✅ PASS | MenuEntryService uses IUnitOfWork for transactions |
| Dependency Injection | ✅ PASS | All services registered in ServiceCollectionExtension.cs |
| Authorization Policies | ✅ PASS | Controllers use `[Authorize("menuentry:write")]` for mutating operations |

### ✅ Security Requirements

| Requirement | Status | Implementation |
|-------------|--------|----------------|
| Permission-based access | ✅ PASS | `menuentry:write` required for PUT/DELETE endpoints |
| Input validation | ✅ PASS | Name max 100 chars, URL format, duplicate check (reuse US1 logic) |
| Authorization enforcement | ✅ PASS | Controller-level `[Authorize]` attributes |
| Error handling | ✅ PASS | Custom exceptions (ResourceNotFoundException) for 404 responses |

### ✅ Code Quality Standards

| Requirement | Status | Implementation |
|-------------|--------|----------------|
| File headers | ✅ PASS | CGI France copyright required (enforced by .editorconfig) |
| Naming conventions | ✅ PASS | PascalCase classes, camelCase parameters, Async suffix |
| Testing requirements | ✅ PASS | Unit tests for service, controller, and client layers |
| XML documentation | ✅ PASS | Controller endpoints require XML docs for Swagger |

### 🔄 Post-Design Re-Check

*To be completed after Phase 1*

- [ ] All new methods follow async patterns (`UpdateAsync`, `DeleteAsync`)
- [ ] DTOs have proper validation attributes
- [ ] Error responses use ProblemDetails format
- [ ] Tests follow naming convention: `{MethodName}_{Scenario}_{ExpectedBehavior}`

## Project Structure

### Documentation (this feature)

```text
specs/001-custom-menu-entries/
├── spec.md              # Feature specification (USER PROVIDED)
├── tasks.md             # Task list with US2 tasks T036-T056 (USER PROVIDED)
├── plan.md              # This file (CREATED BY THIS COMMAND)
├── research.md          # Phase 0 output (CREATED IN PHASE 0 BELOW)
├── data-model.md        # Phase 1 output (CREATED IN PHASE 1 BELOW)
├── quickstart.md        # Phase 1 output (CREATED IN PHASE 1 BELOW)
└── contracts/           # Phase 1 output (CREATED IN PHASE 1 BELOW)
    └── menu-entries-api.yaml  # OpenAPI spec with US2 endpoints
```

### Source Code (repository root)

```text
# IoT Hub Portal - Clean Architecture Web Application

src/
├── IoTHub.Portal.Domain/                    # Domain layer
│   ├── Entities/
│   │   └── MenuEntry.cs                     # ✅ EXISTS (US1) - no changes needed
│   ├── Repositories/
│   │   └── IMenuEntryRepository.cs          # ✅ EXISTS (US1) - has Update/Delete methods
│   └── Exceptions/
│       ├── BaseException.cs                 # ✅ EXISTS - base for custom exceptions
│       └── ResourceNotFoundException.cs     # ✅ EXISTS - used for 404 responses
│
├── IoTHub.Portal.Application/               # Application layer
│   ├── Services/
│   │   └── IMenuEntryService.cs             # ✅ EXISTS (US1) - has Update/Delete signatures
│   └── Mappers/
│       └── MenuEntryMapper.cs               # ✅ EXISTS (US1) - no changes needed
│
├── IoTHub.Portal.Infrastructure/            # Infrastructure layer
│   ├── Repositories/
│   │   └── MenuEntryRepository.cs           # ✅ EXISTS (US1) - implements Update/Delete
│   ├── Services/
│   │   └── MenuEntryService.cs              # 🔧 UPDATE - enhance Update/Delete with validation
│   └── Startup/
│       └── ServiceCollectionExtension.cs    # ✅ EXISTS - services already registered
│
├── IoTHub.Portal.Shared/                    # Shared layer
│   ├── Models/v1.0/
│   │   └── MenuEntryDto.cs                  # ✅ EXISTS (US1) - has validation attributes
│   └── Security/
│       └── PortalPermissions.cs             # ✅ EXISTS - MenuEntryRead/Write defined
│
├── IoTHub.Portal.Server/                    # Server layer (API)
│   └── Controllers/v1.0/
│       └── MenuEntriesController.cs         # 🔧 UPDATE - enhance PUT/DELETE with error handling
│
└── IoTHub.Portal.Client/                    # Client layer (Blazor WASM)
    ├── Services/
    │   ├── IMenuEntryClientService.cs       # ✅ EXISTS (US1) - has Update/Delete methods
    │   └── MenuEntryClientService.cs        # 🔧 UPDATE - ensure proper error propagation
    ├── Pages/MenuEntries/
    │   └── MenuEntryListPage.razor          # 🔧 UPDATE - add Edit/Delete buttons + confirmation
    ├── Dialogs/MenuEntries/
    │   ├── CreateMenuEntryDialog.razor      # ✅ EXISTS (US1)
    │   └── EditMenuEntryDialog.razor        # ✅ EXISTS (US1) - needs validation enhancement
    └── Shared/
        └── NavMenu.razor                    # 🔧 UPDATE - graceful handling of deleted entries

tests/
├── IoTHub.Portal.Tests.Unit/
│   ├── Infrastructure/Services/
│   │   └── MenuEntryServiceTests.cs         # ➕ ADD US2 tests (T036, T037)
│   ├── Server/Controllers/v1.0/
│   │   └── MenuEntriesControllerTests.cs    # ➕ ADD US2 tests (T038, T039)
│   └── Client/
│       ├── Services/
│       │   └── MenuEntryClientServiceTests.cs  # 🔧 UPDATE with delete tests (T056)
│       ├── Pages/MenuEntries/
│       │   └── MenuEntryListPageTests.cs       # 🔧 UPDATE with edit/delete tests (T056)
│       └── Dialogs/MenuEntries/
│           └── EditMenuEntryDialogTests.cs     # ➕ ADD (T055)
```

**Structure Decision**: The project follows Clean Architecture with clear separation between Domain (entities, interfaces), Application (service contracts), Infrastructure (implementations), Server (API), and Client (UI). This structure is **already established** and US2 builds upon it without requiring structural changes. The key principle is dependency flow toward the Domain core, with outer layers depending on inner layers via interfaces.

## Complexity Tracking

> **No violations** - US2 fully complies with constitution requirements. All complexity is inherited from existing architecture decisions made in US1 (repository pattern, clean architecture, dual database support). No additional complexity introduced.

---

# Phase 0: Research & Decision Documentation

**Goal**: Document research findings and technical decisions for US2 implementation.

## 0.1 Known Constraints (From US1 Implementation)

Based on the existing codebase analysis:

### Validation Rules (Must maintain consistency)
- **Name**: Required, max 100 characters, duplicate check within tenant
- **URL**: Valid HTTP/HTTPS or relative path starting with `/`
- **Order**: Integer for sorting (handled by US4, not US2)
- **IsEnabled**: Boolean flag (UI toggle, not part of US2 scope)
- **IsExternal**: Auto-detected based on URL (external if http/https, internal if relative)

### Error Handling Patterns
- `ResourceNotFoundException` → 404 Not Found (when menu entry doesn't exist)
- `ResourceAlreadyExistsException` → 400 Bad Request (duplicate name during update)
- Validation errors → 400 Bad Request with ProblemDetails format
- Authorization failures → 401 Unauthorized (automatic via `[Authorize]`)

### Authorization Model
- **Permission**: `menuentry:write` required for POST/PUT/DELETE operations
- **Permission**: `menuentry:read` required for GET operations
- **Enforcement**: Controller-level `[Authorize("permission")]` attributes

## 0.2 Research Questions & Answers

### Q1: What validation should apply to UPDATE operations?
**Answer**: Same validation as CREATE operations:
- Name required, max 100 chars, duplicate check (excluding current entry ID)
- URL format validation (HTTP/HTTPS or relative path `/`)
- Auto-detect IsExternal based on URL
- Update `UpdatedAt` timestamp automatically

**Rationale**: Consistency with CREATE operation ensures predictable behavior. Users shouldn't encounter different validation rules when editing vs. creating.

**Implementation**: Reuse existing validation logic in `MenuEntryService.ValidateMenuEntry()` method.

### Q2: Should DELETE be soft delete or hard delete?
**Answer**: **Hard delete** (physical removal from database)

**Rationale**: 
- Menu entries don't have complex relationships or audit requirements
- No cascade delete concerns (they're leaf entities)
- Simplifies implementation and testing
- Matches user expectation ("delete" means "gone")

**Implementation**: `_repository.Delete(entity)` followed by `_unitOfWork.SaveAsync()`

### Q3: Should delete confirmation happen in backend or frontend?
**Answer**: **Frontend only** (backend just executes the delete)

**Rationale**:
- Better UX (no extra API call for confirmation)
- Backend should be stateless
- User can't accidentally delete via API without intentional API call
- Standard pattern in web applications

**Implementation**: Use MudBlazor `MudDialog` with confirmation message before calling delete API

### Q4: What happens if a user edits a menu entry to have a duplicate name?
**Answer**: Return 400 Bad Request with error message "A menu entry with name '{name}' already exists"

**Rationale**: Same behavior as CREATE operation for consistency

**Implementation**: `MenuEntryService.UpdateMenuEntryAsync()` calls `GetByNameAsync()` and throws `ResourceAlreadyExistsException` if duplicate found (excluding current entry)

### Q5: What HTTP status codes should UPDATE/DELETE return?
**Answer**: 
- **PUT** (Update): 200 OK with updated MenuEntryDto in response body
- **DELETE**: 204 No Content (successful deletion, no response body)

**Rationale**: REST best practices
- 200 OK for successful updates (client needs updated data for UI refresh)
- 204 No Content for successful deletes (nothing to return)

### Q6: Should NavMenu.razor poll for changes or refresh on navigation?
**Answer**: **Refresh on navigation/page load only** (no real-time updates)

**Rationale**:
- Requirement states "no real-time synchronization required"
- Simpler implementation
- Menu entries change infrequently
- Polling adds unnecessary complexity and server load

**Implementation**: NavMenu loads entries in `OnInitializedAsync()`, refreshes on page navigation

### Q7: How should the UI handle delete errors (e.g., network failure)?
**Answer**: Show error toast notification with user-friendly message, keep entry in list

**Rationale**:
- User needs feedback on failure
- Entry should remain visible until successfully deleted
- Consistent with MudBlazor snackbar pattern used elsewhere in portal

**Implementation**: Try-catch in `MenuEntryListPage.razor` with `Snackbar.Add(errorMessage, Severity.Error)`

### Q8: Should edit validation be real-time or on submit?
**Answer**: **Hybrid approach**
- Character counter shows live (max 100 chars for name)
- URL format validation on blur or submit
- Duplicate name check on submit only (requires API call)

**Rationale**:
- Real-time character counter improves UX (user sees limit approaching)
- URL validation on blur gives feedback without being intrusive
- Duplicate check requires API call, too expensive for real-time

**Implementation**: MudBlazor form validation with `Validation` parameter

## 0.3 Technology Best Practices

### ASP.NET Core Controller Patterns
- Use `[HttpPut("{id}")]` for updates (idempotent)
- Use `[HttpDelete("{id}")]` for deletes
- Return `ActionResult<T>` for flexible response types
- Use `ModelState.IsValid` for automatic DTO validation
- Return `NotFound()` when entity doesn't exist
- Return `BadRequest()` for validation errors

### Blazor Component Patterns
- Use `MudDialog` for confirmation dialogs
- Use `MudForm` with validation for edit forms
- Use `StateHasChanged()` after async operations if needed
- Inject services via `@inject` directive
- Use try-catch for graceful error handling
- Show loading indicators during async operations

### Unit Testing Patterns
- One test class per tested class
- Test method naming: `{MethodName}_{Scenario}_{ExpectedBehavior}`
- Use Moq for mocking dependencies
- Use FluentAssertions for readable assertions
- Test happy path + error conditions
- Mock `IUnitOfWork`, `IMenuEntryRepository`, `ILogger`

---

# Phase 1: Data Model & Contracts

**Goal**: Document the data model (no changes from US1) and API contracts for US2.

## 1.1 Data Model

### Entity: MenuEntry (NO CHANGES from US1)

**Location**: `src/IoTHub.Portal.Domain/Entities/MenuEntry.cs`

```csharp
public class MenuEntry : EntityBase
{
    // Inherited from EntityBase
    // public string Id { get; set; }
    
    public string Name { get; set; }          // Display name (max 100 chars)
    public string Url { get; set; }           // Target URL (HTTP/HTTPS or relative)
    public int Order { get; set; }            // Sort order (used by US4, default 0)
    public bool IsEnabled { get; set; }       // Show/hide toggle (default true)
    public bool IsExternal { get; set; }      // Auto-detected from URL
    public DateTime CreatedAt { get; set; }   // Creation timestamp
    public DateTime UpdatedAt { get; set; }   // Last update timestamp
}
```

**Relationships**: None (standalone entity)

**Indexes**: 
- Primary key on `Id` (inherited from EntityBase)
- Potential future index on `Name` for duplicate checks (optimization, not required)

### DTO: MenuEntryDto (NO CHANGES from US1)

**Location**: `src/IoTHub.Portal.Shared/Models/v1.0/MenuEntryDto.cs`

```csharp
public class MenuEntryDto
{
    public string Id { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "URL is required")]
    [Url(ErrorMessage = "URL must be a valid format")]
    public string Url { get; set; }
    
    public int Order { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsExternal { get; set; }
}
```

**Validation Attributes**: DataAnnotations for automatic controller validation

## 1.2 API Contracts

### US2-Relevant Endpoints

API contracts are defined in OpenAPI/Swagger format. See `contracts/menu-entries-api.yaml` below.

#### 1. Get Single Menu Entry (supports edit pre-fill)

```
GET /api/menu-entries/{id}
Authorization: Bearer token (requires menuentry:read permission)

Response 200 OK:
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Monitoring Dashboard",
  "url": "https://monitoring.example.com",
  "order": 0,
  "isEnabled": true,
  "isExternal": true
}

Response 404 Not Found:
{
  "type": "https://httpstatuses.com/404",
  "title": "Resource not found",
  "status": 404,
  "detail": "The menu entry with id {id} doesn't exist"
}
```

#### 2. Update Menu Entry

```
PUT /api/menu-entries/{id}
Authorization: Bearer token (requires menuentry:write permission)
Content-Type: application/json

Request Body:
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Updated Dashboard Name",
  "url": "https://new-url.example.com",
  "order": 0,
  "isEnabled": true
}

Response 200 OK:
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Updated Dashboard Name",
  "url": "https://new-url.example.com",
  "order": 0,
  "isEnabled": true,
  "isExternal": true
}

Response 400 Bad Request (duplicate name):
{
  "type": "https://httpstatuses.com/400",
  "title": "Resource already exists",
  "status": 400,
  "detail": "A menu entry with name 'Updated Dashboard Name' already exists"
}

Response 400 Bad Request (validation):
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "Name": ["Name cannot exceed 100 characters"]
  }
}

Response 404 Not Found:
{
  "type": "https://httpstatuses.com/404",
  "title": "Resource not found",
  "status": 404,
  "detail": "The menu entry with id {id} doesn't exist"
}
```

#### 3. Delete Menu Entry

```
DELETE /api/menu-entries/{id}
Authorization: Bearer token (requires menuentry:write permission)

Response 204 No Content
(Empty response body)

Response 404 Not Found:
{
  "type": "https://httpstatuses.com/404",
  "title": "Resource not found",
  "status": 404,
  "detail": "The menu entry with id {id} doesn't exist"
}
```

## 1.3 Validation Rules Summary

| Field | Create | Update | Notes |
|-------|--------|--------|-------|
| **Name** | Required, max 100 chars, unique | Same | Duplicate check excludes current entry on update |
| **URL** | Required, valid HTTP/HTTPS or relative path | Same | Auto-detects IsExternal |
| **Order** | Optional (default 0) | Optional | Used by US4 ordering feature |
| **IsEnabled** | Optional (default true) | Optional | UI toggle, not core to US2 |
| **IsExternal** | Auto-calculated (read-only) | Auto-calculated | Based on URL format |
| **Id** | Auto-generated | Must match URL parameter | GUID format |

---

# Phase 2: Implementation Guide

## Step-by-Step Implementation Plan

### ✅ Prerequisites (Already Complete from US1)
- Domain entity `MenuEntry` ✅
- Repository interface `IMenuEntryRepository` with Update/Delete ✅
- Repository implementation `MenuEntryRepository` ✅
- Service interface `IMenuEntryService` with UpdateAsync/DeleteAsync ✅
- DTO `MenuEntryDto` with validation ✅
- Controller `MenuEntriesController` with PUT/DELETE endpoints ✅
- Client service interfaces ✅
- UI components (MenuEntryListPage, EditMenuEntryDialog) ✅

### 🔧 Required Enhancements for US2

#### Backend Enhancements

1. **MenuEntryService.cs** (T041, T042)
   - Enhance `UpdateMenuEntryAsync()`:
     - Validate name (required, max 100 chars)
     - Check for duplicate name (exclude current entry)
     - Validate URL format
     - Auto-detect IsExternal
     - Update UpdatedAt timestamp
     - Throw `ResourceNotFoundException` if entry doesn't exist
     - Throw `ResourceAlreadyExistsException` if duplicate name
   
   - Enhance `DeleteMenuEntryAsync()`:
     - Check if entry exists
     - Throw `ResourceNotFoundException` if not found
     - Call `_repository.Delete(entity)` and `_unitOfWork.SaveAsync()`

2. **MenuEntriesController.cs** (T043, T044, T045, T046)
   - **GET by ID** (T043): Already exists, ensure proper error handling
   - **PUT** (T044): 
     - Verify `[Authorize("menuentry:write")]` attribute
     - Ensure ModelState validation
     - Return 200 OK with updated DTO
     - Handle 404 (ResourceNotFoundException)
     - Handle 400 (validation errors, duplicate name)
   - **DELETE** (T045):
     - Verify `[Authorize("menuentry:write")]` attribute
     - Return 204 No Content on success
     - Handle 404 (ResourceNotFoundException)
   - **Error Handling** (T046):
     - Ensure ProblemDetails format for all errors
     - Map custom exceptions to appropriate status codes

#### Frontend Enhancements

3. **MenuEntryClientService.cs** (T048)
   - Ensure proper error propagation
   - Parse error responses and throw meaningful exceptions
   - Handle network errors gracefully

4. **EditMenuEntryDialog.razor** (T050)
   - Add/enhance form validation:
     - Real-time character counter for name
     - URL validation on blur
     - Submit disabled if form invalid
   - Show loading indicator during save
   - Show error messages from API
   - Close dialog on success

5. **MenuEntryListPage.razor** (T051, T052, T053)
   - **Edit Button** (T051):
     - Add Edit button to each row
     - Open EditMenuEntryDialog with selected entry
     - Pass entry data to dialog
   - **Delete Button** (T052):
     - Add Delete button to each row
     - Show confirmation dialog before delete
     - Confirmation message: "Are you sure you want to delete '{entry.Name}'? This action cannot be undone."
     - Call delete API only after confirmation
   - **Refresh Logic** (T053):
     - Refresh list after edit (on dialog close with success)
     - Refresh list after delete (on successful delete)
     - Show success/error toast notifications

6. **NavMenu.razor** (T054)
   - Add error handling for missing entries
   - Gracefully handle menu entry deletions:
     - Catch exceptions during menu loading
     - Skip entries that fail to load
     - Log errors but don't break menu rendering
   - Reload menu entries on navigation (OnInitializedAsync)

### 🧪 Testing Strategy

#### Unit Tests (Write FIRST, ensure they FAIL)

7. **MenuEntryServiceTests.cs** (T036, T037)
   - **UpdateMenuEntry Tests** (T036):
     - `UpdateMenuEntry_ValidData_UpdatesSuccessfully`
     - `UpdateMenuEntry_DuplicateName_ThrowsResourceAlreadyExistsException`
     - `UpdateMenuEntry_InvalidUrl_ThrowsArgumentException`
     - `UpdateMenuEntry_NonExistentId_ThrowsResourceNotFoundException`
     - `UpdateMenuEntry_NameTooLong_ThrowsArgumentException`
   
   - **DeleteMenuEntry Tests** (T037):
     - `DeleteMenuEntry_ExistingId_DeletesSuccessfully`
     - `DeleteMenuEntry_NonExistentId_ThrowsResourceNotFoundException`

8. **MenuEntriesControllerTests.cs** (T038, T039)
   - **PUT Endpoint Tests** (T038):
     - `Put_ValidData_Returns200Ok`
     - `Put_DuplicateName_Returns400BadRequest`
     - `Put_InvalidModelState_Returns400BadRequest`
     - `Put_NonExistentId_Returns404NotFound`
   
   - **DELETE Endpoint Tests** (T039):
     - `Delete_ExistingId_Returns204NoContent`
     - `Delete_NonExistentId_Returns404NotFound`

9. **EditMenuEntryDialogTests.cs** (T055)
   - `Dialog_ValidData_SubmitsSuccessfully`
   - `Dialog_InvalidUrl_ShowsValidationError`
   - `Dialog_NameTooLong_ShowsValidationError`
   - `Dialog_ClosesOnSuccess`

10. **MenuEntryListPageTests.cs** (T056 - update existing)
    - `EditButton_Click_OpensDialogWithCorrectData`
    - `DeleteButton_Click_ShowsConfirmationDialog`
    - `DeleteConfirmed_CallsApiAndRefreshesList`
    - `DeleteCancelled_DoesNotCallApi`

#### Integration Testing (Manual - before demo)
- Create menu entry via UI (US1)
- Edit the entry (change name and URL)
- Verify updated entry appears in navigation menu
- Delete the entry
- Verify entry removed from navigation menu
- Attempt to edit with duplicate name → see error
- Attempt to delete non-existent entry → see 404 error

---

# Phase 3: Testing Strategy

## Test Coverage Requirements

### Unit Test Layers
1. **Service Layer** (MenuEntryService)
   - Mock: IMenuEntryRepository, IUnitOfWork, ILogger
   - Focus: Business logic, validation rules, exception throwing

2. **Controller Layer** (MenuEntriesController)
   - Mock: IMenuEntryService
   - Focus: HTTP status codes, authorization, ModelState validation

3. **Client Layer** (Blazor components)
   - Mock: IMenuEntryClientService, IDialogService, ISnackbar
   - Focus: UI logic, event handling, dialog flows

### Test Data
- Valid entry: `{ id: "valid-guid", name: "Test Entry", url: "https://example.com" }`
- Invalid URL: `{ name: "Test", url: "invalid-url" }`
- Name too long: `{ name: "A".repeat(101), url: "https://example.com" }`
- Duplicate name: Entry with name that already exists in mock repository

### Test Execution Order
1. Write tests first (TDD approach)
2. Verify tests FAIL (red phase)
3. Implement functionality
4. Verify tests PASS (green phase)
5. Refactor if needed

---

# Phase 4: Rollout Plan

## Deployment Steps

1. **Pre-Deployment Checklist**
   - [ ] All unit tests pass (100% success rate)
   - [ ] Code review completed
   - [ ] Security review (authorization checks verified)
   - [ ] Manual testing completed (all acceptance scenarios)
   - [ ] Documentation updated (API docs, user guide)

2. **Deployment Sequence**
   - Backend changes (service + controller) deployed first
   - Database migrations (none required for US2)
   - Frontend changes (client service + UI components)

3. **Post-Deployment Validation**
   - [ ] Edit existing menu entry via UI → verify success
   - [ ] Delete menu entry via UI → verify removal from navigation
   - [ ] Attempt edit with duplicate name → verify error message
   - [ ] Attempt edit with invalid URL → verify validation error
   - [ ] Check authorization (non-admin user should see 401)

4. **Rollback Plan**
   - If critical issues found:
     - Revert frontend deployment (UI changes)
     - Backend remains compatible (GET operations still work)
   - No database rollback needed (no schema changes)

## Feature Flag (Optional)
Not required for US2 - edit/delete are natural extensions of US1 create/list functionality.

---

# Appendix A: Task Checklist (from tasks.md)

## User Story 2 Tasks (T036-T056)

### Tests (Write First)
- [ ] T036 [P] [US2] Unit test for IMenuEntryService.UpdateMenuEntry
- [ ] T037 [P] [US2] Unit test for IMenuEntryService.DeleteMenuEntry  
- [ ] T038 [P] [US2] Unit test for MenuEntriesController PUT endpoint
- [ ] T039 [P] [US2] Unit test for MenuEntriesController DELETE endpoint

### Backend Implementation
- [ ] T040 [US2] Add UpdateMenuEntry and DeleteMenuEntry methods to IMenuEntryService interface
- [ ] T041 [US2] Implement UpdateMenuEntry in MenuEntryService with validation
- [ ] T042 [US2] Implement DeleteMenuEntry in MenuEntryService with existence check
- [ ] T043 [US2] Add GET by ID endpoint to MenuEntriesController
- [ ] T044 [US2] Add PUT endpoint to MenuEntriesController
- [ ] T045 [US2] Add DELETE endpoint to MenuEntriesController  
- [ ] T046 [US2] Add error handling for 404 Not Found

### Frontend Implementation
- [ ] T047 [P] [US2] Add GetMenuEntryById, UpdateMenuEntry, DeleteMenuEntry to IMenuEntryClientService
- [ ] T048 [P] [US2] Implement new methods in MenuEntryClientService
- [ ] T049 [P] [US2] Create EditMenuEntryDialog.razor
- [ ] T050 [US2] Add validation to EditMenuEntryDialog
- [ ] T051 [US2] Add Edit button to MenuEntryListPage
- [ ] T052 [US2] Add Delete button with confirmation to MenuEntryListPage
- [ ] T053 [US2] Update MenuEntryListPage to refresh after edit/delete
- [ ] T054 [US2] Update NavMenu.razor to handle deletions gracefully

### Client Tests
- [ ] T055 [P] [US2] Unit test for EditMenuEntryDialog
- [ ] T056 [P] [US2] Update MenuEntryListPageTests for edit/delete

---

# Appendix B: Acceptance Criteria

## User Story 2 Acceptance Scenarios

### Scenario 1: Edit entry name
**Given** a custom menu entry exists  
**When** a user edits the entry's name from "Old Docs" to "New Docs"  
**Then** the updated name appears in the navigation menu

**Verification**:
1. Navigate to Menu Entry management page
2. Click Edit button for "Old Docs" entry
3. Change name to "New Docs"
4. Submit form
5. Verify "New Docs" appears in list
6. Verify navigation menu shows "New Docs" link

### Scenario 2: Delete entry
**Given** a custom menu entry exists  
**When** a user deletes the entry  
**Then** it is removed from the navigation menu and no longer accessible

**Verification**:
1. Navigate to Menu Entry management page
2. Click Delete button for entry
3. Confirm deletion in dialog
4. Verify entry removed from list
5. Verify navigation menu no longer shows the link

### Scenario 3: Edit with invalid URL
**Given** a user is editing a menu entry  
**When** they change the URL to an invalid format  
**Then** validation prevents saving and displays an error message

**Verification**:
1. Navigate to Menu Entry management page
2. Click Edit button
3. Change URL to "invalid-url-format"
4. Attempt to submit
5. Verify error message appears
6. Verify form does not submit
7. Verify original entry unchanged

### Scenario 4: Delete confirmation
**Given** a menu entry is in use  
**When** a user attempts to delete it  
**Then** the system confirms deletion and removes the entry

**Verification**:
1. Navigate to Menu Entry management page
2. Click Delete button
3. Verify confirmation dialog appears
4. Click Cancel → verify entry still exists
5. Click Delete button again
6. Click Confirm → verify entry removed

---

# Appendix C: OpenAPI Contract

Generated OpenAPI specification will be created in Phase 1 output directory.

**File**: `specs/001-custom-menu-entries/contracts/menu-entries-api.yaml`

Key endpoints for US2:
- `GET /api/menu-entries/{id}` - Retrieve single entry
- `PUT /api/menu-entries/{id}` - Update entry
- `DELETE /api/menu-entries/{id}` - Delete entry

(Full OpenAPI spec to be generated in Phase 1.4 below)

---

# Summary

This plan provides a comprehensive roadmap for implementing User Story 2 (Edit and Delete Menu Entries). The implementation builds upon the solid foundation of US1 with minimal changes required:

**Key Changes**:
- Enhance validation in MenuEntryService (update/delete methods)
- Improve error handling in MenuEntriesController  
- Add delete confirmation dialog in UI
- Enhance EditMenuEntryDialog with proper validation feedback
- Add edit/delete buttons to MenuEntryListPage
- Ensure graceful error handling in NavMenu

**No Breaking Changes**: All US1 functionality remains intact. US2 is a pure enhancement.

**Estimated Timeline**: 3-4 days for full implementation and testing

**Next Steps**: Proceed to Phase 0 (research.md generation) → Phase 1 (contracts, data model, quickstart) → Phase 2 (tasks.md already exists)
