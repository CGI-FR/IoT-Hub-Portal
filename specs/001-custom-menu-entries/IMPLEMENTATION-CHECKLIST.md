# User Story 2: Edit and Delete - Implementation Checklist

**Feature**: Custom Menu Entries  
**User Story**: US2 - Edit and Delete Menu Entries  
**Tasks**: T036-T056 (21 tasks total)

---

## 📋 Pre-Implementation Checklist

- [x] Plan.md reviewed and understood
- [x] Quickstart.md guide ready
- [x] API contracts reviewed (contracts/menu-entries-api.yaml)
- [x] Data model understood (data-model.md)
- [x] Research findings reviewed (research.md)
- [x] Tasks.md reviewed (T036-T056)
- [ ] Development environment ready
- [ ] User Story 1 (US1) implementation verified

---

## 🧪 Phase 1: Write Tests (TDD - Tests MUST Fail First)

### Service Layer Tests (Tasks T036, T037)

**File**: `src/IoTHub.Portal.Tests.Unit/Infrastructure/Services/MenuEntryServiceTests.cs`

- [ ] T036: `UpdateMenuEntry_ValidData_UpdatesSuccessfully`
- [ ] T036: `UpdateMenuEntry_DuplicateName_ThrowsResourceAlreadyExistsException`
- [ ] T036: `UpdateMenuEntry_InvalidUrl_ThrowsArgumentException`
- [ ] T036: `UpdateMenuEntry_NonExistentId_ThrowsResourceNotFoundException`
- [ ] T036: `UpdateMenuEntry_NameTooLong_ThrowsArgumentException`
- [ ] T037: `DeleteMenuEntry_ExistingId_DeletesSuccessfully`
- [ ] T037: `DeleteMenuEntry_NonExistentId_ThrowsResourceNotFoundException`

**Verification**: Run `dotnet test` → Should see 7 FAILURES ✅

### Controller Layer Tests (Tasks T038, T039)

**File**: `src/IoTHub.Portal.Tests.Unit/Server/Controllers/v1.0/MenuEntriesControllerTests.cs`

- [ ] T038: `Put_ValidData_Returns200Ok`
- [ ] T038: `Put_DuplicateName_Returns400BadRequest`
- [ ] T038: `Put_InvalidModelState_Returns400BadRequest`
- [ ] T038: `Put_NonExistentId_Returns404NotFound`
- [ ] T039: `Delete_ExistingId_Returns204NoContent`
- [ ] T039: `Delete_NonExistentId_Returns404NotFound`

**Verification**: Run `dotnet test` → Should see 6 MORE FAILURES ✅

---

## 🔧 Phase 2: Backend Implementation

### Service Interface (Task T040)

**File**: `src/IoTHub.Portal.Application/Services/IMenuEntryService.cs`

- [ ] T040: Verify `UpdateMenuEntryAsync(MenuEntryDto dto)` method signature exists
- [ ] T040: Verify `DeleteMenuEntryAsync(string id)` method signature exists

### Service Implementation (Tasks T041, T042)

**File**: `src/IoTHub.Portal.Infrastructure/Services/MenuEntryService.cs`

- [ ] T041: Implement `UpdateMenuEntryAsync()` with:
  - [ ] Existence check (throw ResourceNotFoundException if not found)
  - [ ] Name validation (required, max 100 chars)
  - [ ] Duplicate name check (exclude current ID)
  - [ ] URL format validation (HTTP/HTTPS or relative path)
  - [ ] Auto-detect IsExternal
  - [ ] Update UpdatedAt timestamp
  - [ ] Preserve CreatedAt
  - [ ] Call repository.Update() and unitOfWork.SaveAsync()

- [ ] T042: Implement `DeleteMenuEntryAsync()` with:
  - [ ] Existence check (throw ResourceNotFoundException if not found)
  - [ ] Call repository.Delete() and unitOfWork.SaveAsync()
  - [ ] Add logging

**Verification**: Run `dotnet test` → Service tests should PASS ✅

### Controller Implementation (Tasks T043, T044, T045, T046)

**File**: `src/IoTHub.Portal.Server/Controllers/v1.0/MenuEntriesController.cs`

- [ ] T043: Verify GET by ID endpoint exists with `[Authorize("menuentry:read")]`
- [ ] T044: Implement/enhance PUT endpoint with:
  - [ ] `[HttpPut("{id}")]` attribute
  - [ ] `[Authorize("menuentry:write")]` attribute
  - [ ] ModelState validation check
  - [ ] ID mismatch check (URL id vs body id)
  - [ ] Return 200 OK with updated DTO
  - [ ] Proper XML documentation

- [ ] T045: Implement/enhance DELETE endpoint with:
  - [ ] `[HttpDelete("{id}")]` attribute
  - [ ] `[Authorize("menuentry:write")]` attribute
  - [ ] Return 204 No Content on success
  - [ ] Proper XML documentation

- [ ] T046: Enhance error handling for all endpoints:
  - [ ] 404 Not Found for ResourceNotFoundException
  - [ ] 400 Bad Request for validation errors
  - [ ] 400 Bad Request for ResourceAlreadyExistsException
  - [ ] ProblemDetails format for all errors

**Verification**: Run `dotnet test` → Controller tests should PASS ✅

---

## 🎨 Phase 3: Frontend Implementation

### Client Service Interface (Task T047)

**File**: `src/IoTHub.Portal.Client/Services/IMenuEntryClientService.cs`

- [ ] T047: Verify `GetByIdAsync(string id)` method exists
- [ ] T047: Verify `UpdateAsync(MenuEntryDto dto)` method exists
- [ ] T047: Verify `DeleteAsync(string id)` method exists

### Client Service Implementation (Task T048)

**File**: `src/IoTHub.Portal.Client/Services/MenuEntryClientService.cs`

- [ ] T048: Implement `GetByIdAsync()` with GET request to `/api/menu-entries/{id}`
- [ ] T048: Implement `UpdateAsync()` with PUT request to `/api/menu-entries/{id}`
- [ ] T048: Implement `DeleteAsync()` with DELETE request to `/api/menu-entries/{id}`
- [ ] T048: Ensure proper error propagation (EnsureSuccessStatusCode)

### Edit Dialog (Tasks T049, T050)

**File**: `src/IoTHub.Portal.Client/Dialogs/MenuEntries/EditMenuEntryDialog.razor`

- [ ] T049: Verify dialog component exists (may be from US1)
- [ ] T050: Add/enhance form validation:
  - [ ] Real-time character counter for Name (max 100)
  - [ ] URL format validation on blur
  - [ ] External/Internal indicator chip
  - [ ] Disabled Save button when form invalid
  - [ ] Loading indicator during save
  - [ ] Error/success Snackbar notifications

### List Page Enhancements (Tasks T051, T052, T053)

**File**: `src/IoTHub.Portal.Client/Pages/MenuEntries/MenuEntryListPage.razor`

- [ ] T051: Add Edit button to each table row
- [ ] T051: Implement `OpenEditDialog(MenuEntryDto entry)` method
- [ ] T051: Refresh list on dialog close with success

- [ ] T052: Add Delete button to each table row
- [ ] T052: Implement `OpenDeleteConfirmation(MenuEntryDto entry)` method
- [ ] T052: Show confirmation dialog with entry name
- [ ] T052: Implement `DeleteEntry(string id)` method
- [ ] T052: Call delete API only after confirmation

- [ ] T053: Implement list refresh logic:
  - [ ] Refresh after successful edit
  - [ ] Refresh after successful delete
  - [ ] Show success Snackbar on operations
  - [ ] Show error Snackbar on failures

### Navigation Menu (Task T054)

**File**: `src/IoTHub.Portal.Client/Shared/NavMenu.razor`

- [ ] T054: Add try-catch in `OnInitializedAsync()`
- [ ] T054: Gracefully handle missing entries (log error, continue rendering)
- [ ] T054: Reload menu entries on navigation

**Verification**: Manual testing - all UI flows should work ✅

---

## 🧪 Phase 4: Client Tests

### Edit Dialog Tests (Task T055)

**File**: `src/IoTHub.Portal.Tests.Unit/Client/Dialogs/MenuEntries/EditMenuEntryDialogTests.cs`

- [ ] T055: `Dialog_ValidData_SubmitsSuccessfully`
- [ ] T055: `Dialog_InvalidUrl_ShowsValidationError`
- [ ] T055: `Dialog_NameTooLong_ShowsValidationError`
- [ ] T055: `Dialog_ClosesOnSuccess`

### List Page Tests (Task T056)

**File**: `src/IoTHub.Portal.Tests.Unit/Client/Pages/MenuEntries/MenuEntryListPageTests.cs`

- [ ] T056: `EditButton_Click_OpensDialogWithCorrectData`
- [ ] T056: `DeleteButton_Click_ShowsConfirmationDialog`
- [ ] T056: `DeleteConfirmed_CallsApiAndRefreshesList`
- [ ] T056: `DeleteCancelled_DoesNotCallApi`

**Verification**: Run `dotnet test` → All tests (including client) should PASS ✅

---

## ✅ Phase 5: Manual Testing & Validation

### Manual Testing Scenarios

- [ ] **Test 1: Edit menu entry name**
  - [ ] Navigate to Menu Entry management page
  - [ ] Click Edit button for an existing entry
  - [ ] Change name from "Old Name" to "New Name"
  - [ ] Click Save
  - [ ] Verify success message appears
  - [ ] Verify "New Name" appears in list
  - [ ] Verify navigation menu shows "New Name"

- [ ] **Test 2: Edit menu entry URL**
  - [ ] Click Edit button for an entry
  - [ ] Change URL from old to new
  - [ ] Click Save
  - [ ] Verify success message
  - [ ] Verify clicking link goes to new URL

- [ ] **Test 3: Delete menu entry**
  - [ ] Click Delete button
  - [ ] Verify confirmation dialog appears with entry name
  - [ ] Click Cancel → verify entry still exists
  - [ ] Click Delete button again
  - [ ] Click Delete in confirmation → verify success message
  - [ ] Verify entry removed from list
  - [ ] Verify entry removed from navigation menu

- [ ] **Test 4: Edit with duplicate name**
  - [ ] Create two entries: "Entry A" and "Entry B"
  - [ ] Edit "Entry A" to have name "Entry B"
  - [ ] Verify error message about duplicate name
  - [ ] Verify entry NOT updated

- [ ] **Test 5: Edit with invalid URL**
  - [ ] Click Edit on entry
  - [ ] Change URL to "invalid-url"
  - [ ] Verify inline validation error
  - [ ] Verify Save button disabled

- [ ] **Test 6: Authorization check**
  - [ ] Log in as user without `menuentry:write` permission
  - [ ] Verify Edit/Delete buttons hidden or disabled
  - [ ] (Optional) Try API call → verify 401 Unauthorized

### Acceptance Criteria Validation

- [ ] **AC1: Edit entry name** - updated name appears in navigation
- [ ] **AC2: Delete entry** - removed from navigation and storage
- [ ] **AC3: Edit with invalid URL** - validation prevents saving
- [ ] **AC4: Delete confirmation** - system confirms before removal

---

## 📊 Phase 6: Code Quality & Review

### Code Quality Checks

- [ ] All unit tests pass (100% success rate)
- [ ] No compiler warnings
- [ ] All methods have proper XML documentation
- [ ] Code follows IoT Hub Portal naming conventions
- [ ] Authorization attributes present on all endpoints
- [ ] Error responses use ProblemDetails format
- [ ] No hardcoded strings (use constants)
- [ ] Proper async/await patterns used

### Security Review

- [ ] PUT endpoint has `[Authorize("menuentry:write")]`
- [ ] DELETE endpoint has `[Authorize("menuentry:write")]`
- [ ] GET by ID has `[Authorize("menuentry:read")]`
- [ ] URL validation prevents script injection
- [ ] No SQL injection risk (EF Core parameterized queries)
- [ ] Input sanitization via DataAnnotations

### Performance Review

- [ ] No N+1 query issues
- [ ] Async methods used throughout
- [ ] List refresh doesn't cause UI lag
- [ ] API operations complete within 200ms

---

## 🚀 Phase 7: Deployment Preparation

### Pre-Deployment

- [ ] All tests passing
- [ ] Manual testing complete
- [ ] Code review approved
- [ ] Documentation updated
- [ ] No breaking changes confirmed
- [ ] Staging environment ready

### Deployment Steps

- [ ] Deploy backend changes (service + controller)
- [ ] Deploy frontend changes (client service + UI)
- [ ] Verify staging deployment
- [ ] Smoke test on staging
- [ ] Get stakeholder approval
- [ ] Deploy to production

### Post-Deployment Validation

- [ ] Edit existing entry → success
- [ ] Delete entry → success + confirmation
- [ ] Navigation menu reflects changes
- [ ] Authorization working correctly
- [ ] No errors in application logs

---

## 📈 Completion Metrics

### Task Completion

- **Total Tasks**: 21 (T036-T056)
- **Tests Written**: 21 new unit tests
- **Files Modified**: ~10 files
- **Files Created**: ~2 test files
- **Time Spent**: ___ days

### Quality Metrics

- **Test Pass Rate**: ___% (target: 100%)
- **Code Coverage**: ___% (existing coverage maintained)
- **Bugs Found**: ___
- **Bugs Fixed**: ___

---

## ✅ Final Sign-Off

- [ ] All tasks complete (T036-T056)
- [ ] All tests passing
- [ ] All acceptance criteria met
- [ ] Manual testing complete
- [ ] Code review approved
- [ ] Documentation complete
- [ ] Deployed to staging
- [ ] Stakeholder approval received
- [ ] Ready for production deployment

---

**Implementation Status**: 🚧 IN PROGRESS  
**Started**: ___________  
**Completed**: ___________  
**Total Time**: ___________

---

*Use this checklist to track progress through User Story 2 implementation*
