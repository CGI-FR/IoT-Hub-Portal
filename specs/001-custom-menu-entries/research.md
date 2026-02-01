# Research: Custom Menu Entries - User Story 2

**Feature**: Custom Menu Entries - Edit and Delete Operations  
**Date**: 2026-02-01  
**Researcher**: Implementation Planning Agent

## Executive Summary

User Story 2 extends the Custom Menu Entries feature with edit and delete capabilities. Research shows that **no new technical decisions are required** - US2 leverages the existing infrastructure from US1 (MenuEntry entity, repository pattern, service layer, API, client services, and UI components). The implementation focuses on enhancing existing methods with proper validation, error handling, and user experience improvements (delete confirmation dialogs).

**Key Finding**: The US1 implementation already includes update and delete methods in the service and controller layers. US2 work is primarily about:
1. Ensuring robust validation in update operations
2. Implementing delete confirmation UI patterns
3. Comprehensive error handling and user feedback
4. Testing coverage for edit/delete scenarios

---

## 1. Technology Stack (Inherited from US1)

### Backend
- **ASP.NET Core 8.0**: Web API framework
- **Entity Framework Core 8.0**: ORM for database access
- **AutoMapper**: Entity-to-DTO mapping
- **PostgreSQL 13+ / MySQL 8.0+**: Dual database support

### Frontend
- **Blazor WebAssembly**: Client-side SPA framework
- **MudBlazor 6.x**: Material Design component library
- **System.Net.Http.Json**: HTTP client with JSON support

### Testing
- **xUnit**: Unit testing framework
- **Moq**: Mocking framework for dependencies
- **bUnit**: Blazor component testing
- **FluentAssertions**: Readable assertion library

**Decision**: No changes to technology stack. All tools required for US2 are already in place from US1.

---

## 2. Validation Requirements

### 2.1 Update Operation Validation

Research into ASP.NET Core validation best practices and US1 implementation reveals:

**Validation Rules** (same as CREATE):
- **Name**: Required, max 100 characters, unique within tenant
- **URL**: Required, valid HTTP/HTTPS or relative path format
- **Duplicate Check**: Must exclude current entry when checking for duplicate names

**Implementation Pattern**:
```csharp
// MenuEntryService.UpdateMenuEntryAsync()
1. Validate name (required, max length)
2. Validate URL format
3. Check for duplicate name (GetByNameAsync, exclude current ID)
4. Auto-detect IsExternal based on URL
5. Set UpdatedAt timestamp
6. Call repository.Update() and unitOfWork.SaveAsync()
7. Throw ResourceNotFoundException if entry doesn't exist
8. Throw ResourceAlreadyExistsException if duplicate name found
```

**Rationale**: Consistency with CREATE operation ensures predictable behavior. Users shouldn't encounter different validation rules when editing vs. creating entries.

**Alternatives Considered**:
- ❌ **Skip duplicate check on update**: Could lead to duplicate entries, violates FR-016
- ❌ **Different validation rules for update**: Confusing for users, inconsistent UX
- ✅ **Reuse validation logic**: Simplest, most consistent approach

**Best Practice Reference**: [Microsoft Docs - Model validation in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation)

### 2.2 Delete Operation Validation

Research into REST API design patterns for delete operations:

**Validation Requirements**:
- Entry must exist (throw `ResourceNotFoundException` if not)
- No cascade delete concerns (MenuEntry is a leaf entity)
- No soft delete required (no audit requirements for menu entries)

**HTTP Status Codes** (REST best practices):
- **204 No Content**: Successful deletion (no response body)
- **404 Not Found**: Entry doesn't exist
- **401 Unauthorized**: User lacks `menuentry:write` permission

**Rationale**: Menu entries are configuration data, not business-critical records requiring audit trails. Hard delete simplifies implementation and matches user expectations.

**Alternatives Considered**:
- ❌ **Soft delete with IsDeleted flag**: Adds complexity, no audit requirement
- ❌ **Cascade delete checks**: Not needed (no relationships)
- ✅ **Simple hard delete**: Matches requirement, simplest implementation

**Best Practice Reference**: [REST API Tutorial - DELETE Method](https://restfulapi.net/http-methods/#delete)

---

## 3. Error Handling Patterns

### 3.1 Backend Error Handling

Research into IoT Hub Portal's existing error handling architecture (from constitution.md):

**Exception Hierarchy** (already defined):
```
BaseException
├── ResourceNotFoundException → 404 Not Found
├── ResourceAlreadyExistsException → 400 Bad Request  
├── InternalServerErrorException → 500 Internal Server Error
└── InvalidCloudProviderException → 400 Bad Request
```

**ProblemDetails Format** (RFC 7807):
```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Resource not found",
  "status": 404,
  "detail": "The menu entry with id {id} doesn't exist"
}
```

**Application to US2**:
- Update operations throw `ResourceNotFoundException` when entry doesn't exist
- Update operations throw `ResourceAlreadyExistsException` for duplicate names
- Delete operations throw `ResourceNotFoundException` when entry doesn't exist
- Controller catches these exceptions and returns appropriate HTTP status codes

**Decision**: Use existing exception types and ProblemDetails format. No new exceptions needed.

**Best Practice Reference**: [RFC 7807 - Problem Details for HTTP APIs](https://tools.ietf.org/html/rfc7807)

### 3.2 Frontend Error Handling

Research into MudBlazor best practices for error handling:

**Pattern**: Try-catch blocks with Snackbar notifications

```csharp
try
{
    await ClientService.DeleteMenuEntryAsync(id);
    Snackbar.Add("Menu entry deleted successfully", Severity.Success);
    await LoadData(); // Refresh list
}
catch (Exception ex)
{
    Snackbar.Add($"Failed to delete menu entry: {ex.Message}", Severity.Error);
    // Keep entry visible in list
}
```

**User Feedback Mechanisms**:
- **Success**: Green snackbar with confirmation message
- **Error**: Red snackbar with error details
- **Loading**: Progress indicator during API calls
- **Validation**: Inline form validation messages

**Decision**: Use MudBlazor Snackbar for toast notifications (consistent with existing portal patterns).

**Alternatives Considered**:
- ❌ **Modal error dialogs**: Too intrusive for simple operations
- ❌ **Inline error messages only**: Easy to miss for async operations
- ✅ **Snackbar notifications**: Standard pattern in IoT Hub Portal

**Best Practice Reference**: [MudBlazor Snackbar Documentation](https://mudblazor.com/components/snackbar)

---

## 4. User Experience Patterns

### 4.1 Delete Confirmation Dialog

Research into UX best practices for destructive actions:

**Pattern**: Two-step confirmation for delete operations
1. User clicks Delete button
2. Confirmation dialog appears with clear message
3. User explicitly confirms or cancels
4. API call only made after confirmation

**Dialog Message Template**:
```
Title: Confirm Delete
Message: Are you sure you want to delete '{entry.Name}'? This action cannot be undone.
Actions: [Cancel] [Delete]
```

**Rationale**: 
- Prevents accidental deletions
- Provides clear context (shows entry name)
- Standard pattern users expect for destructive actions
- No backend confirmation needed (frontend responsibility)

**Implementation**: MudBlazor `MudDialog` component

```razor
<MudDialog>
    <TitleContent>
        <MudText Typo="Typo.h6">Confirm Delete</MudText>
    </TitleContent>
    <DialogContent>
        <MudText>Are you sure you want to delete '@entryName'?</MudText>
        <MudText>This action cannot be undone.</MudText>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Error" OnClick="ConfirmDelete">Delete</MudButton>
    </DialogActions>
</MudDialog>
```

**Best Practice Reference**: 
- [Material Design - Confirmation Dialogs](https://material.io/components/dialogs#confirmation-dialog)
- [Nielsen Norman Group - Preventing User Errors](https://www.nngroup.com/articles/slips/)

### 4.2 Edit Form Validation Feedback

Research into real-time validation patterns:

**Validation Strategy** (hybrid approach):
- **Character counter**: Real-time (shows "45/100" as user types)
- **URL format**: On blur or submit (validates format)
- **Duplicate name**: On submit only (requires API call)

**Rationale**:
- Character counter provides immediate feedback without being intrusive
- URL validation on blur gives feedback without constant error messages while typing
- Duplicate check deferred to submit to avoid excessive API calls

**MudBlazor Implementation**:
```razor
<MudTextField @bind-Value="model.Name" 
              Label="Name" 
              Required 
              MaxLength="100"
              Counter="100"
              Validation="@(new Func<string, string>(ValidateName))" />

<MudTextField @bind-Value="model.Url" 
              Label="URL" 
              Required
              Validation="@(new Func<string, string>(ValidateUrl))"
              OnBlur="ValidateUrlFormat" />
```

**Best Practice Reference**: [Luke Wroblewski - Inline Validation in Web Forms](https://alistapart.com/article/inline-validation-in-web-forms/)

---

## 5. Authorization & Security

### 5.1 Permission Model (Inherited from US1)

Research confirms existing RBAC model is sufficient:

**Permissions Required**:
- `menuentry:read` - GET operations (retrieve single entry for edit)
- `menuentry:write` - PUT/DELETE operations (update, delete)

**Enforcement Points**:
- **Controller Level**: `[Authorize("menuentry:write")]` attributes
- **UI Level**: Show/hide Edit/Delete buttons based on user permissions
- **Client Service**: No authorization logic (server enforces)

**Security Checklist for US2**:
- [x] PUT endpoint has `[Authorize("menuentry:write")]`
- [x] DELETE endpoint has `[Authorize("menuentry:write")]`
- [x] GET by ID endpoint has `[Authorize("menuentry:read")]`
- [x] URL validation prevents script injection (validated by DataAnnotations)
- [x] No SQL injection risk (EF Core parameterized queries)

**Decision**: Use existing permission model. No new permissions needed.

### 5.2 Input Sanitization

Research into ASP.NET Core input sanitization:

**Built-in Protections**:
- DataAnnotations validation (automatic in controllers)
- Model binding with type safety
- EF Core parameterized queries (SQL injection prevention)
- Blazor automatic HTML encoding (XSS prevention)

**URL Validation**:
- `[Url]` attribute validates format
- No JavaScript URLs allowed (`javascript:` scheme rejected by validation)
- Relative paths validated (must start with `/`)

**Decision**: Built-in protections are sufficient. No additional sanitization needed.

**Best Practice Reference**: [OWASP - Input Validation Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Input_Validation_Cheat_Sheet.html)

---

## 6. Testing Strategy

### 6.1 Unit Testing Approach

Research into ASP.NET Core unit testing best practices:

**Test Structure** (AAA pattern):
- **Arrange**: Set up mocks and test data
- **Act**: Call method under test
- **Assert**: Verify expected outcome

**Mocking Strategy**:
- Mock `IMenuEntryRepository` for data access
- Mock `IUnitOfWork` for transaction management
- Mock `ILogger` for logging (optional, can use NullLogger)
- Use `It.IsAny<T>()` for flexible mock matching

**Test Coverage Targets**:
- **Service Layer**: Business logic, validation, exception throwing
- **Controller Layer**: HTTP status codes, authorization, model validation
- **Client Layer**: UI logic, event handling, dialog flows

**Example Test (Update with Duplicate Name)**:
```csharp
[Fact]
public async Task UpdateMenuEntry_DuplicateName_ThrowsResourceAlreadyExistsException()
{
    // Arrange
    var existingEntry = new MenuEntry { Id = "id1", Name = "Existing" };
    var updateDto = new MenuEntryDto { Id = "id2", Name = "Existing", Url = "https://example.com" };
    
    _mockRepository.Setup(r => r.GetByIdAsync("id2")).ReturnsAsync(new MenuEntry { Id = "id2" });
    _mockRepository.Setup(r => r.GetByNameAsync("Existing")).ReturnsAsync(existingEntry);
    
    // Act & Assert
    await Assert.ThrowsAsync<ResourceAlreadyExistsException>(
        () => _service.UpdateMenuEntryAsync(updateDto)
    );
}
```

**Decision**: Follow TDD approach - write tests first, ensure they fail, implement functionality, verify tests pass.

**Best Practice Reference**: [Microsoft Docs - Unit testing C# in .NET](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)

### 6.2 Blazor Component Testing

Research into bUnit testing patterns:

**Component Test Structure**:
```csharp
[Fact]
public void DeleteButton_Click_ShowsConfirmationDialog()
{
    // Arrange
    using var ctx = new TestContext();
    var mockService = new Mock<IMenuEntryClientService>();
    var mockDialogService = new Mock<IDialogService>();
    ctx.Services.AddSingleton(mockService.Object);
    ctx.Services.AddSingleton(mockDialogService.Object);
    
    var component = ctx.RenderComponent<MenuEntryListPage>();
    
    // Act
    var deleteButton = component.Find("button.delete-button");
    deleteButton.Click();
    
    // Assert
    mockDialogService.Verify(d => d.Show<ConfirmDeleteDialog>(It.IsAny<string>()), Times.Once);
}
```

**Decision**: Use bUnit for component testing with mocked services.

**Best Practice Reference**: [bUnit Documentation](https://bunit.dev/)

---

## 7. Performance Considerations

### 7.1 API Performance

Research into typical REST API performance:

**Expected Performance** (from spec.md):
- API operations: < 200ms for typical menu management tasks
- UI updates: < 100ms response time

**Optimization Strategies** (not needed for US2, but documented):
- Database indexes on frequently queried fields (Name for duplicate checks)
- Caching for read-heavy operations (GetAll for navigation menu)
- Async/await throughout the stack (already implemented)

**Decision**: No performance optimizations needed for US2. Expected load is low (infrequent edit/delete operations).

### 7.2 UI Performance

Research into Blazor performance best practices:

**Rendering Optimization**:
- Use `@key` directive in loops to prevent unnecessary re-renders
- Call `StateHasChanged()` only when needed
- Avoid inline lambda expressions in event handlers (causes re-renders)

**Already Implemented in US1**:
- Async data loading with progress indicators
- Debouncing for real-time validation (if applicable)

**Decision**: Follow existing patterns from US1. No new optimizations needed.

---

## 8. Integration Points

### 8.1 NavMenu.razor Integration

Research into how menu deletions affect navigation:

**Current Behavior** (US1):
- NavMenu loads entries in `OnInitializedAsync()`
- Entries displayed as navigation links
- Refreshes on page navigation

**Required Enhancement for US2**:
- Graceful error handling if entry deleted while menu is displayed
- Skip entries that fail to load (defensive programming)
- Reload entries on navigation to reflect deletions

**Implementation Pattern**:
```csharp
protected override async Task OnInitializedAsync()
{
    try
    {
        var entries = await ClientService.GetMenuEntriesAsync();
        menuEntries = entries.Where(e => e.IsEnabled).ToList();
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Failed to load menu entries");
        // Continue rendering other menu sections
    }
}
```

**Decision**: Add try-catch in NavMenu to handle missing entries gracefully. No real-time synchronization needed (requirement states "no real-time synchronization required").

### 8.2 List Page Refresh Strategy

Research into data refresh patterns:

**Refresh Triggers**:
1. After successful edit (dialog close)
2. After successful delete (confirmation)
3. On page navigation (OnInitializedAsync)

**Implementation Pattern**:
```csharp
private async Task HandleEditSuccess()
{
    await LoadData(); // Reload list from API
    Snackbar.Add("Menu entry updated successfully", Severity.Success);
}

private async Task HandleDeleteSuccess()
{
    await LoadData(); // Reload list from API
    Snackbar.Add("Menu entry deleted successfully", Severity.Success);
}
```

**Decision**: Reload full list after edit/delete operations (simple, reliable, sufficient for low-frequency operations).

**Alternatives Considered**:
- ❌ **Update list item in place**: Complex, error-prone
- ❌ **Real-time updates via SignalR**: Overkill for infrequent changes
- ✅ **Full list reload**: Simplest, most reliable

---

## 9. Documentation Requirements

### 9.1 API Documentation

Research into OpenAPI/Swagger documentation standards:

**Required Documentation** (XML comments):
```csharp
/// <summary>
/// Updates an existing menu entry
/// </summary>
/// <param name="id">Menu entry unique identifier</param>
/// <param name="menuEntryDto">Updated menu entry data</param>
/// <returns>Updated menu entry</returns>
/// <response code="200">Menu entry updated successfully</response>
/// <response code="400">Validation failed or duplicate name</response>
/// <response code="404">Menu entry not found</response>
/// <response code="401">User lacks menuentry:write permission</response>
[HttpPut("{id}")]
[Authorize("menuentry:write")]
[ProducesResponseType(typeof(MenuEntryDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public async Task<ActionResult<MenuEntryDto>> UpdateMenuEntry(string id, MenuEntryDto menuEntryDto)
```

**Decision**: Add comprehensive XML documentation to all controller methods.

### 9.2 User Documentation

**Required User Guide Sections**:
1. How to edit a menu entry
2. How to delete a menu entry
3. What validation rules apply
4. What to do if errors occur

**Decision**: User guide to be created as part of quickstart.md in Phase 1.

---

## 10. Risk Assessment

### 10.1 Technical Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| Duplicate name check misses edge cases | Medium | Comprehensive unit tests, case-insensitive comparison |
| Delete operation fails silently | Medium | Proper error handling, user feedback via Snackbar |
| NavMenu breaks if entry deleted | Low | Defensive error handling, graceful degradation |
| Concurrent updates cause data loss | Low | Entity Framework concurrency tokens (if needed) |

**Decision**: All risks have acceptable mitigation strategies. No blocking issues.

### 10.2 User Experience Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| User accidentally deletes entry | High | Confirmation dialog with clear message |
| User doesn't see validation errors | Medium | Inline validation, Snackbar notifications |
| Edit form doesn't pre-fill data | Medium | GET by ID endpoint provides data for dialog |
| Delete doesn't refresh navigation | Medium | Reload menu entries after delete |

**Decision**: All UX risks addressed by design (confirmation dialog, validation feedback, data reload).

---

## 11. Dependencies

### 11.1 External Dependencies

**NuGet Packages** (already in project, no new dependencies):
- Microsoft.AspNetCore.Authorization
- Microsoft.EntityFrameworkCore
- AutoMapper
- MudBlazor

**Decision**: No new dependencies required for US2.

### 11.2 Internal Dependencies

**Code Dependencies** (already implemented in US1):
- MenuEntry entity ✅
- IMenuEntryRepository ✅
- MenuEntryService ✅
- MenuEntriesController ✅
- MenuEntryClientService ✅
- MenuEntryListPage ✅
- EditMenuEntryDialog ✅

**Decision**: US2 builds on existing US1 infrastructure. No new foundational components needed.

---

## 12. Implementation Checklist

### Backend
- [ ] Enhance MenuEntryService.UpdateMenuEntryAsync with validation
- [ ] Enhance MenuEntryService.DeleteMenuEntryAsync with existence check
- [ ] Verify MenuEntriesController PUT endpoint error handling
- [ ] Verify MenuEntriesController DELETE endpoint returns 204 No Content
- [ ] Add XML documentation to all controller methods

### Frontend
- [ ] Implement delete confirmation dialog
- [ ] Enhance EditMenuEntryDialog with validation feedback
- [ ] Add Edit button to MenuEntryListPage
- [ ] Add Delete button to MenuEntryListPage
- [ ] Implement list refresh after edit/delete
- [ ] Add error handling to NavMenu.razor

### Testing
- [ ] Write service layer unit tests (update + delete)
- [ ] Write controller layer unit tests (PUT + DELETE endpoints)
- [ ] Write client layer unit tests (EditMenuEntryDialog)
- [ ] Update MenuEntryListPageTests with edit/delete scenarios
- [ ] Manual integration testing (all acceptance scenarios)

### Documentation
- [ ] Update API documentation (XML comments)
- [ ] Create quickstart guide (Phase 1 output)
- [ ] Update OpenAPI spec with US2 endpoints (Phase 1 output)

---

## 13. Conclusion

Research confirms that **User Story 2 is a straightforward extension of User Story 1** with minimal technical complexity:

**Key Findings**:
1. ✅ **No new architecture patterns needed** - US1 provides complete foundation
2. ✅ **No new dependencies required** - All tools already in place
3. ✅ **No breaking changes** - US1 functionality remains intact
4. ✅ **Clear implementation path** - Enhance existing methods with validation and error handling

**Implementation Complexity**: **LOW to MEDIUM**
- Backend: Enhance existing service methods (1-2 days)
- Frontend: Add confirmation dialog and refresh logic (1 day)
- Testing: Comprehensive unit and integration tests (1 day)

**Total Estimated Effort**: 3-4 days for complete implementation and testing

**Recommended Approach**: 
1. Write tests first (TDD)
2. Implement backend enhancements (service + controller)
3. Implement frontend enhancements (dialog + list page)
4. Manual testing of all acceptance scenarios
5. Deploy to staging for final validation

**Next Steps**: Proceed to Phase 1 (data model, contracts, quickstart generation)

---

## References

1. [ASP.NET Core Validation](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation)
2. [REST API Tutorial - HTTP Methods](https://restfulapi.net/http-methods/)
3. [RFC 7807 - Problem Details](https://tools.ietf.org/html/rfc7807)
4. [MudBlazor Documentation](https://mudblazor.com/)
5. [Material Design - Dialogs](https://material.io/components/dialogs)
6. [OWASP Input Validation](https://cheatsheetseries.owasp.org/cheatsheets/Input_Validation_Cheat_Sheet.html)
7. [bUnit Testing](https://bunit.dev/)
8. [Microsoft Unit Testing Guide](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)

---

*Research completed: 2026-02-01*
*Ready for Phase 1: Data Model & Contracts*
