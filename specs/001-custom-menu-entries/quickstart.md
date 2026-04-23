# Quickstart Guide: Custom Menu Entries - User Story 2

**Feature**: Edit and Delete Menu Entries  
**Audience**: Developers implementing User Story 2  
**Prerequisites**: User Story 1 (Create and List) must be complete

---

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites Check](#prerequisites-check)
3. [Implementation Steps](#implementation-steps)
4. [Testing Guide](#testing-guide)
5. [Common Pitfalls](#common-pitfalls)
6. [Troubleshooting](#troubleshooting)

---

## Overview

This guide provides step-by-step instructions for implementing **User Story 2: Edit and Delete Menu Entries**. You'll enhance the existing Custom Menu Entries feature with edit and delete capabilities.

### What You'll Build

- **Edit Functionality**: Users can modify menu entry names, URLs, and properties
- **Delete Functionality**: Users can permanently remove menu entries (with confirmation)
- **Validation**: Same rules as create (name required, max 100 chars, valid URL, duplicate check)
- **Error Handling**: Graceful handling of missing entries, validation failures, and authorization errors

### Time Estimate

- Backend implementation: 1-2 days
- Frontend implementation: 1-2 days
- Testing: 0.5-1 day
- **Total**: 3-4 days

---

## Prerequisites Check

Before starting, verify these US1 components exist:

### ✅ Domain Layer
- [ ] `src/IoTHub.Portal.Domain/Entities/MenuEntry.cs` exists
- [ ] `src/IoTHub.Portal.Domain/Repositories/IMenuEntryRepository.cs` has `Update` and `Delete` methods
- [ ] `src/IoTHub.Portal.Domain/Exceptions/ResourceNotFoundException.cs` exists

### ✅ Application Layer
- [ ] `src/IoTHub.Portal.Application/Services/IMenuEntryService.cs` has method signatures for UpdateAsync/DeleteAsync
- [ ] `src/IoTHub.Portal.Application/Mappers/MenuEntryMapper.cs` exists

### ✅ Infrastructure Layer
- [ ] `src/IoTHub.Portal.Infrastructure/Repositories/MenuEntryRepository.cs` implements Update/Delete
- [ ] `src/IoTHub.Portal.Infrastructure/Services/MenuEntryService.cs` exists

### ✅ Server Layer
- [ ] `src/IoTHub.Portal.Server/Controllers/v1.0/MenuEntriesController.cs` exists
- [ ] Controller has GET, POST endpoints from US1

### ✅ Shared Layer
- [ ] `src/IoTHub.Portal.Shared/Models/v1.0/MenuEntryDto.cs` has validation attributes
- [ ] `src/IoTHub.Portal.Shared/Security/PortalPermissions.cs` has MenuEntryRead/MenuEntryWrite

### ✅ Client Layer
- [ ] `src/IoTHub.Portal.Client/Services/MenuEntryClientService.cs` exists
- [ ] `src/IoTHub.Portal.Client/Pages/MenuEntries/MenuEntryListPage.razor` exists
- [ ] `src/IoTHub.Portal.Client/Dialogs/MenuEntries/EditMenuEntryDialog.razor` exists (may need enhancement)

---

## Implementation Steps

### Phase 1: Write Tests First (TDD Approach)

#### Step 1.1: Service Layer Tests

**File**: `src/IoTHub.Portal.Tests.Unit/Infrastructure/Services/MenuEntryServiceTests.cs`

Add these test methods (ensure they FAIL before implementing):

```csharp
[Fact]
public async Task UpdateMenuEntry_ValidData_UpdatesSuccessfully()
{
    // Arrange
    var existingEntry = new MenuEntry 
    { 
        Id = "id1", 
        Name = "Old Name", 
        Url = "https://old.com",
        CreatedAt = DateTime.UtcNow.AddDays(-1)
    };
    
    var updateDto = new MenuEntryDto 
    { 
        Id = "id1", 
        Name = "New Name", 
        Url = "https://new.com" 
    };
    
    _mockRepository.Setup(r => r.GetByIdAsync("id1")).ReturnsAsync(existingEntry);
    _mockRepository.Setup(r => r.GetByNameAsync("New Name")).ReturnsAsync((MenuEntry)null);
    _mockRepository.Setup(r => r.Update(It.IsAny<MenuEntry>())).Returns(Task.CompletedTask);
    _mockUnitOfWork.Setup(u => u.SaveAsync()).Returns(Task.CompletedTask);
    
    // Act
    var result = await _service.UpdateMenuEntryAsync(updateDto);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("New Name", result.Name);
    Assert.Equal("https://new.com", result.Url);
    Assert.True(result.IsExternal);
    _mockRepository.Verify(r => r.Update(It.IsAny<MenuEntry>()), Times.Once);
}

[Fact]
public async Task UpdateMenuEntry_DuplicateName_ThrowsResourceAlreadyExistsException()
{
    // Arrange
    var existingEntry = new MenuEntry { Id = "id1", Name = "Name1" };
    var duplicateEntry = new MenuEntry { Id = "id2", Name = "Name2" };
    var updateDto = new MenuEntryDto { Id = "id1", Name = "Name2", Url = "https://example.com" };
    
    _mockRepository.Setup(r => r.GetByIdAsync("id1")).ReturnsAsync(existingEntry);
    _mockRepository.Setup(r => r.GetByNameAsync("Name2")).ReturnsAsync(duplicateEntry);
    
    // Act & Assert
    await Assert.ThrowsAsync<ResourceAlreadyExistsException>(
        () => _service.UpdateMenuEntryAsync(updateDto)
    );
}

[Fact]
public async Task UpdateMenuEntry_NonExistentId_ThrowsResourceNotFoundException()
{
    // Arrange
    var updateDto = new MenuEntryDto { Id = "nonexistent", Name = "Name", Url = "https://example.com" };
    _mockRepository.Setup(r => r.GetByIdAsync("nonexistent")).ReturnsAsync((MenuEntry)null);
    
    // Act & Assert
    await Assert.ThrowsAsync<ResourceNotFoundException>(
        () => _service.UpdateMenuEntryAsync(updateDto)
    );
}

[Fact]
public async Task DeleteMenuEntry_ExistingId_DeletesSuccessfully()
{
    // Arrange
    var existingEntry = new MenuEntry { Id = "id1", Name = "Test" };
    _mockRepository.Setup(r => r.GetByIdAsync("id1")).ReturnsAsync(existingEntry);
    _mockRepository.Setup(r => r.Delete(existingEntry)).Returns(Task.CompletedTask);
    _mockUnitOfWork.Setup(u => u.SaveAsync()).Returns(Task.CompletedTask);
    
    // Act
    await _service.DeleteMenuEntryAsync("id1");
    
    // Assert
    _mockRepository.Verify(r => r.Delete(existingEntry), Times.Once);
    _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
}

[Fact]
public async Task DeleteMenuEntry_NonExistentId_ThrowsResourceNotFoundException()
{
    // Arrange
    _mockRepository.Setup(r => r.GetByIdAsync("nonexistent")).ReturnsAsync((MenuEntry)null);
    
    // Act & Assert
    await Assert.ThrowsAsync<ResourceNotFoundException>(
        () => _service.DeleteMenuEntryAsync("nonexistent")
    );
}
```

**Run tests**: `dotnet test` → Should see 5 new failures ✅

#### Step 1.2: Controller Layer Tests

**File**: `src/IoTHub.Portal.Tests.Unit/Server/Controllers/v1.0/MenuEntriesControllerTests.cs`

Add these test methods:

```csharp
[Fact]
public async Task Put_ValidData_Returns200Ok()
{
    // Arrange
    var id = "id1";
    var dto = new MenuEntryDto { Id = id, Name = "Updated", Url = "https://example.com" };
    _mockService.Setup(s => s.UpdateMenuEntryAsync(dto)).ReturnsAsync(dto);
    
    // Act
    var result = await _controller.Put(id, dto);
    
    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var returnedDto = Assert.IsType<MenuEntryDto>(okResult.Value);
    Assert.Equal("Updated", returnedDto.Name);
}

[Fact]
public async Task Put_NonExistentId_Returns404NotFound()
{
    // Arrange
    var id = "nonexistent";
    var dto = new MenuEntryDto { Id = id, Name = "Test", Url = "https://example.com" };
    _mockService.Setup(s => s.UpdateMenuEntryAsync(dto))
        .ThrowsAsync(new ResourceNotFoundException($"Menu entry with id {id} doesn't exist"));
    
    // Act & Assert
    await Assert.ThrowsAsync<ResourceNotFoundException>(() => _controller.Put(id, dto));
}

[Fact]
public async Task Put_InvalidModelState_Returns400BadRequest()
{
    // Arrange
    var id = "id1";
    var dto = new MenuEntryDto { Id = id, Name = "", Url = "https://example.com" };
    _controller.ModelState.AddModelError("Name", "Name is required");
    
    // Act
    var result = await _controller.Put(id, dto);
    
    // Assert
    Assert.IsType<BadRequestObjectResult>(result.Result);
}

[Fact]
public async Task Delete_ExistingId_Returns204NoContent()
{
    // Arrange
    var id = "id1";
    _mockService.Setup(s => s.DeleteMenuEntryAsync(id)).Returns(Task.CompletedTask);
    
    // Act
    var result = await _controller.Delete(id);
    
    // Assert
    Assert.IsType<NoContentResult>(result);
}

[Fact]
public async Task Delete_NonExistentId_Returns404NotFound()
{
    // Arrange
    var id = "nonexistent";
    _mockService.Setup(s => s.DeleteMenuEntryAsync(id))
        .ThrowsAsync(new ResourceNotFoundException($"Menu entry with id {id} doesn't exist"));
    
    // Act & Assert
    await Assert.ThrowsAsync<ResourceNotFoundException>(() => _controller.Delete(id));
}
```

**Run tests**: Should see 5 more failures ✅

---

### Phase 2: Backend Implementation

#### Step 2.1: Enhance MenuEntryService

**File**: `src/IoTHub.Portal.Infrastructure/Services/MenuEntryService.cs`

Implement the UpdateMenuEntryAsync method:

```csharp
public async Task<MenuEntryDto> UpdateMenuEntryAsync(MenuEntryDto dto)
{
    ArgumentNullException.ThrowIfNull(dto);
    
    // 1. Check if entry exists
    var existingEntry = await _repository.GetByIdAsync(dto.Id);
    if (existingEntry == null)
    {
        throw new ResourceNotFoundException($"The menu entry with id {dto.Id} doesn't exist");
    }
    
    // 2. Validate name
    if (string.IsNullOrWhiteSpace(dto.Name))
    {
        throw new ArgumentException("Name is required");
    }
    
    if (dto.Name.Length > 100)
    {
        throw new ArgumentException("Name cannot exceed 100 characters");
    }
    
    // 3. Check for duplicate name (excluding current entry)
    var duplicateEntry = await _repository.GetByNameAsync(dto.Name);
    if (duplicateEntry != null && duplicateEntry.Id != dto.Id)
    {
        throw new ResourceAlreadyExistsException($"A menu entry with name '{dto.Name}' already exists");
    }
    
    // 4. Validate URL format
    bool isExternalUrl = dto.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) 
                      || dto.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    bool isRelativePath = dto.Url.StartsWith("/");
    
    if (!isExternalUrl && !isRelativePath)
    {
        throw new ArgumentException("URL must be a valid HTTP/HTTPS URL or a relative path starting with /");
    }
    
    // 5. Update entity properties
    existingEntry.Name = dto.Name;
    existingEntry.Url = dto.Url;
    existingEntry.Order = dto.Order;
    existingEntry.IsEnabled = dto.IsEnabled;
    existingEntry.IsExternal = isExternalUrl;
    existingEntry.UpdatedAt = DateTime.UtcNow;
    // CreatedAt is preserved (not updated)
    
    // 6. Save changes
    await _repository.Update(existingEntry);
    await _unitOfWork.SaveAsync();
    
    // 7. Return updated DTO
    return _mapper.Map<MenuEntryDto>(existingEntry);
}
```

Implement the DeleteMenuEntryAsync method:

```csharp
public async Task DeleteMenuEntryAsync(string id)
{
    ArgumentException.ThrowIfNullOrEmpty(id);
    
    // 1. Check if entry exists
    var existingEntry = await _repository.GetByIdAsync(id);
    if (existingEntry == null)
    {
        throw new ResourceNotFoundException($"The menu entry with id {id} doesn't exist");
    }
    
    // 2. Delete entry
    await _repository.Delete(existingEntry);
    await _unitOfWork.SaveAsync();
    
    _logger.LogInformation("Menu entry {Id} ({Name}) deleted successfully", id, existingEntry.Name);
}
```

**Run tests**: Service tests should now PASS ✅

#### Step 2.2: Enhance MenuEntriesController

**File**: `src/IoTHub.Portal.Server/Controllers/v1.0/MenuEntriesController.cs`

Add/enhance these methods:

```csharp
/// <summary>
/// Gets a single menu entry by ID
/// </summary>
/// <param name="id">Menu entry unique identifier</param>
/// <returns>Menu entry details</returns>
/// <response code="200">Menu entry retrieved successfully</response>
/// <response code="404">Menu entry not found</response>
/// <response code="401">User lacks menuentry:read permission</response>
[HttpGet("{id}")]
[Authorize("menuentry:read")]
[ProducesResponseType(typeof(MenuEntryDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public async Task<ActionResult<MenuEntryDto>> Get(string id)
{
    try
    {
        var entry = await _menuEntryService.GetByIdAsync(id);
        return Ok(entry);
    }
    catch (ResourceNotFoundException ex)
    {
        return NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource not found",
            Detail = ex.Message
        });
    }
}

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
public async Task<ActionResult<MenuEntryDto>> Put(string id, MenuEntryDto menuEntryDto)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }
    
    if (id != menuEntryDto.Id)
    {
        return BadRequest(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "ID mismatch",
            Detail = "The ID in the URL must match the ID in the request body"
        });
    }
    
    try
    {
        var updatedEntry = await _menuEntryService.UpdateMenuEntryAsync(menuEntryDto);
        return Ok(updatedEntry);
    }
    catch (ResourceNotFoundException ex)
    {
        return NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource not found",
            Detail = ex.Message
        });
    }
    catch (ResourceAlreadyExistsException ex)
    {
        return BadRequest(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Resource already exists",
            Detail = ex.Message
        });
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Detail = ex.Message
        });
    }
}

/// <summary>
/// Deletes a menu entry
/// </summary>
/// <param name="id">Menu entry unique identifier</param>
/// <returns>No content on success</returns>
/// <response code="204">Menu entry deleted successfully</response>
/// <response code="404">Menu entry not found</response>
/// <response code="401">User lacks menuentry:write permission</response>
[HttpDelete("{id}")]
[Authorize("menuentry:write")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public async Task<IActionResult> Delete(string id)
{
    try
    {
        await _menuEntryService.DeleteMenuEntryAsync(id);
        return NoContent();
    }
    catch (ResourceNotFoundException ex)
    {
        return NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource not found",
            Detail = ex.Message
        });
    }
}
```

**Run tests**: Controller tests should now PASS ✅

---

### Phase 3: Frontend Implementation

#### Step 3.1: Enhance MenuEntryClientService

**File**: `src/IoTHub.Portal.Client/Services/MenuEntryClientService.cs`

Ensure these methods exist (may already be implemented):

```csharp
public async Task<MenuEntryDto> GetByIdAsync(string id)
{
    var response = await _httpClient.GetAsync($"/api/menu-entries/{id}");
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<MenuEntryDto>();
}

public async Task<MenuEntryDto> UpdateAsync(MenuEntryDto dto)
{
    var response = await _httpClient.PutAsJsonAsync($"/api/menu-entries/{dto.Id}", dto);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<MenuEntryDto>();
}

public async Task DeleteAsync(string id)
{
    var response = await _httpClient.DeleteAsync($"/api/menu-entries/{id}");
    response.EnsureSuccessStatusCode();
}
```

#### Step 3.2: Enhance EditMenuEntryDialog

**File**: `src/IoTHub.Portal.Client/Dialogs/MenuEntries/EditMenuEntryDialog.razor`

```razor
@inject IMenuEntryClientService ClientService
@inject ISnackbar Snackbar

<MudDialog>
    <TitleContent>
        <MudText Typo="Typo.h6">Edit Menu Entry</MudText>
    </TitleContent>
    
    <DialogContent>
        <MudForm @ref="_form" Model="@Model" Validation="@(_validator.ValidateValue)">
            <MudTextField @bind-Value="Model.Name" 
                          Label="Name" 
                          Required 
                          MaxLength="100"
                          Counter="100"
                          Immediate="true"
                          Validation="@(new Func<string, string>(ValidateName))" />
            
            <MudTextField @bind-Value="Model.Url" 
                          Label="URL" 
                          Required
                          Validation="@(new Func<string, string>(ValidateUrl))"
                          HelperText="Enter HTTP/HTTPS URL or relative path (/docs)" />
            
            <MudSwitch @bind-Checked="Model.IsEnabled" 
                       Label="Enabled" 
                       Color="Color.Primary" />
            
            @if (!string.IsNullOrEmpty(Model.Url))
            {
                <MudChip Color="@(IsExternalUrl(Model.Url) ? Color.Info : Color.Success)" 
                         Size="Size.Small">
                    @(IsExternalUrl(Model.Url) ? "External" : "Internal")
                </MudChip>
            }
        </MudForm>
    </DialogContent>
    
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Primary" 
                   OnClick="Submit" 
                   Disabled="@(_isSaving || !_form.IsValid)">
            @if (_isSaving)
            {
                <MudProgressCircular Size="Size.Small" Indeterminate="true" />
                <MudText Class="ml-2">Saving...</MudText>
            }
            else
            {
                <MudText>Save</MudText>
            }
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public MenuEntryDto Model { get; set; }
    
    private MudForm _form;
    private FluentValidationValidator _validator = new();
    private bool _isSaving = false;
    
    private string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Name is required";
        if (name.Length > 100)
            return "Name cannot exceed 100 characters";
        return null;
    }
    
    private string ValidateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return "URL is required";
        
        bool isValidHttp = url.StartsWith("http://") || url.StartsWith("https://");
        bool isValidRelative = url.StartsWith("/");
        
        if (!isValidHttp && !isValidRelative)
            return "URL must be HTTP/HTTPS or relative path starting with /";
        
        return null;
    }
    
    private bool IsExternalUrl(string url) => 
        url?.StartsWith("http://") == true || url?.StartsWith("https://") == true;
    
    private async Task Submit()
    {
        await _form.Validate();
        if (!_form.IsValid) return;
        
        _isSaving = true;
        
        try
        {
            var updated = await ClientService.UpdateAsync(Model);
            Snackbar.Add("Menu entry updated successfully", Severity.Success);
            MudDialog.Close(DialogResult.Ok(updated));
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to update menu entry: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isSaving = false;
        }
    }
    
    private void Cancel() => MudDialog.Cancel();
}
```

#### Step 3.3: Enhance MenuEntryListPage

**File**: `src/IoTHub.Portal.Client/Pages/MenuEntries/MenuEntryListPage.razor`

Add Edit and Delete buttons to the table:

```razor
<MudTable Items="@_menuEntries" Dense="true" Hover="true">
    <HeaderContent>
        <MudTh>Name</MudTh>
        <MudTh>URL</MudTh>
        <MudTh>Type</MudTh>
        <MudTh>Order</MudTh>
        <MudTh>Enabled</MudTh>
        <MudTh>Actions</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd DataLabel="Name">@context.Name</MudTd>
        <MudTd DataLabel="URL">
            <MudLink Href="@context.Url" Target="@(context.IsExternal ? "_blank" : "_self")">
                @context.Url
            </MudLink>
        </MudTd>
        <MudTd DataLabel="Type">
            <MudChip Color="@(context.IsExternal ? Color.Info : Color.Success)" Size="Size.Small">
                @(context.IsExternal ? "External" : "Internal")
            </MudChip>
        </MudTd>
        <MudTd DataLabel="Order">@context.Order</MudTd>
        <MudTd DataLabel="Enabled">
            <MudIcon Icon="@(context.IsEnabled ? Icons.Material.Filled.CheckCircle : Icons.Material.Filled.Cancel)"
                     Color="@(context.IsEnabled ? Color.Success : Color.Default)" />
        </MudTd>
        <MudTd DataLabel="Actions">
            <MudIconButton Icon="@Icons.Material.Filled.Edit" 
                           Color="Color.Primary" 
                           OnClick="@(() => OpenEditDialog(context))"
                           Title="Edit" />
            <MudIconButton Icon="@Icons.Material.Filled.Delete" 
                           Color="Color.Error" 
                           OnClick="@(() => OpenDeleteConfirmation(context))"
                           Title="Delete" />
        </MudTd>
    </RowTemplate>
</MudTable>

@code {
    private async Task OpenEditDialog(MenuEntryDto entry)
    {
        var parameters = new DialogParameters { ["Model"] = entry };
        var dialog = DialogService.Show<EditMenuEntryDialog>("Edit Menu Entry", parameters);
        var result = await dialog.Result;
        
        if (!result.Cancelled)
        {
            await LoadData(); // Refresh list
        }
    }
    
    private async Task OpenDeleteConfirmation(MenuEntryDto entry)
    {
        bool? confirmed = await DialogService.ShowMessageBox(
            "Confirm Delete",
            $"Are you sure you want to delete '{entry.Name}'? This action cannot be undone.",
            yesText: "Delete",
            cancelText: "Cancel"
        );
        
        if (confirmed == true)
        {
            await DeleteEntry(entry.Id);
        }
    }
    
    private async Task DeleteEntry(string id)
    {
        try
        {
            await ClientService.DeleteAsync(id);
            Snackbar.Add("Menu entry deleted successfully", Severity.Success);
            await LoadData(); // Refresh list
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to delete menu entry: {ex.Message}", Severity.Error);
        }
    }
}
```

#### Step 3.4: Update NavMenu.razor

**File**: `src/IoTHub.Portal.Client/Shared/NavMenu.razor`

Add error handling for deleted entries:

```razor
@code {
    protected override async Task OnInitializedAsync()
    {
        try
        {
            var entries = await ClientService.GetMenuEntriesAsync();
            _menuEntries = entries.Where(e => e.IsEnabled).ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load menu entries");
            _menuEntries = new List<MenuEntryDto>(); // Fallback to empty list
            // Continue rendering other menu sections
        }
    }
}
```

---

### Phase 4: Run All Tests

```bash
# Run all unit tests
dotnet test

# Expected results:
# - All MenuEntryServiceTests pass (including new US2 tests)
# - All MenuEntriesControllerTests pass (including new US2 tests)
# - All client tests pass

# Should see output like:
# Passed!  - Failed:     0, Passed:    XX, Skipped:     0, Total:    XX
```

---

## Testing Guide

### Manual Testing Checklist

#### Test 1: Edit Menu Entry Name
1. Navigate to Menu Entry management page
2. Click Edit button for an existing entry
3. Change the name from "Old Name" to "New Name"
4. Click Save
5. ✅ Verify success message appears
6. ✅ Verify "New Name" appears in the list
7. ✅ Verify navigation menu shows "New Name"

#### Test 2: Edit Menu Entry URL
1. Click Edit button for an entry
2. Change URL from `https://old.com` to `https://new.com`
3. Click Save
4. ✅ Verify success message
5. ✅ Verify clicking the link in navigation goes to new URL

#### Test 3: Delete Menu Entry
1. Click Delete button for an entry
2. ✅ Verify confirmation dialog appears with entry name
3. Click Cancel → ✅ Verify entry still exists
4. Click Delete button again
5. Click Delete in confirmation dialog
6. ✅ Verify success message
7. ✅ Verify entry removed from list
8. ✅ Verify entry removed from navigation menu

#### Test 4: Edit with Duplicate Name
1. Create two entries: "Entry A" and "Entry B"
2. Click Edit on "Entry A"
3. Change name to "Entry B" (duplicate)
4. Click Save
5. ✅ Verify error message: "A menu entry with name 'Entry B' already exists"
6. ✅ Verify entry NOT updated

#### Test 5: Edit with Invalid URL
1. Click Edit on an entry
2. Change URL to "invalid-url" (no http:// or /)
3. Try to save
4. ✅ Verify inline validation error appears
5. ✅ Verify Save button is disabled

#### Test 6: Authorization Check
1. Log out and log in as user WITHOUT `menuentry:write` permission
2. Navigate to Menu Entry management page
3. ✅ Verify Edit and Delete buttons are hidden OR disabled
4. (Optional) Try to call API directly → ✅ Verify 401 Unauthorized

---

## Common Pitfalls

### ❌ Pitfall 1: Forgetting to Check for Duplicate Name
**Problem**: Update allows duplicate names  
**Solution**: Call `GetByNameAsync()` and exclude current entry ID in check

```csharp
// ❌ Wrong
var duplicate = await _repository.GetByNameAsync(dto.Name);
if (duplicate != null) throw new ResourceAlreadyExistsException(...);

// ✅ Correct
var duplicate = await _repository.GetByNameAsync(dto.Name);
if (duplicate != null && duplicate.Id != dto.Id) 
    throw new ResourceAlreadyExistsException(...);
```

### ❌ Pitfall 2: Not Preserving CreatedAt Timestamp
**Problem**: Update overwrites CreatedAt  
**Solution**: Only update specific fields, leave CreatedAt unchanged

```csharp
// ❌ Wrong - using AutoMapper.Map will overwrite CreatedAt
existingEntity = _mapper.Map<MenuEntry>(dto);

// ✅ Correct - manual property updates
existingEntity.Name = dto.Name;
existingEntity.Url = dto.Url;
existingEntry.UpdatedAt = DateTime.UtcNow;
// CreatedAt not touched
```

### ❌ Pitfall 3: Not Refreshing List After Edit/Delete
**Problem**: UI shows stale data  
**Solution**: Call `LoadData()` after successful operation

```csharp
// ✅ Correct
var result = await dialog.Result;
if (!result.Cancelled)
{
    await LoadData(); // Refresh list
}
```

### ❌ Pitfall 4: Missing Delete Confirmation
**Problem**: Users accidentally delete entries  
**Solution**: Always show confirmation dialog before delete

```csharp
// ✅ Correct
bool? confirmed = await DialogService.ShowMessageBox(
    "Confirm Delete",
    $"Are you sure you want to delete '{entry.Name}'? This action cannot be undone.",
    yesText: "Delete",
    cancelText: "Cancel"
);

if (confirmed == true)
{
    await DeleteEntry(entry.Id);
}
```

---

## Troubleshooting

### Issue: Tests Fail with "ResourceNotFoundException not found"
**Cause**: Custom exception not imported  
**Solution**: Add using statement:
```csharp
using IoTHub.Portal.Domain.Exceptions;
```

### Issue: PUT Endpoint Returns 400 with "ID mismatch"
**Cause**: ID in URL doesn't match ID in body  
**Solution**: Ensure client sends same ID in both places:
```csharp
await _httpClient.PutAsJsonAsync($"/api/menu-entries/{dto.Id}", dto);
```

### Issue: Delete Doesn't Refresh Navigation Menu
**Cause**: NavMenu doesn't reload after delete  
**Solution**: NavMenu reloads on navigation (OnInitializedAsync), user must navigate to see change

### Issue: Edit Dialog Doesn't Show Current Values
**Cause**: Not calling GET by ID before opening dialog  
**Solution**: Pass existing entry to dialog:
```csharp
var parameters = new DialogParameters { ["Model"] = entry };
```

### Issue: Authorization Errors in Tests
**Cause**: Test setup missing authorization mocks  
**Solution**: Mock authorization in controller tests:
```csharp
var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
{
    new Claim(ClaimTypes.Name, "testuser"),
    new Claim("permission", "menuentry:write")
}, "mock"));

_controller.ControllerContext = new ControllerContext
{
    HttpContext = new DefaultHttpContext { User = user }
};
```

---

## Next Steps

After completing US2:

1. ✅ **Verify all acceptance scenarios** (see spec.md)
2. ✅ **Run full test suite** (unit + integration)
3. ✅ **Manual testing** (all scenarios above)
4. ✅ **Code review** (security, validation, error handling)
5. ✅ **Update documentation** (user guide, API docs)
6. ✅ **Deploy to staging** for stakeholder testing
7. 🚀 **Prepare for US3** (API enhancements) or **US4** (ordering feature)

---

## Resources

- [ASP.NET Core Validation](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation)
- [MudBlazor Dialogs](https://mudblazor.com/components/dialog)
- [xUnit Testing](https://xunit.net/docs/getting-started)
- [Moq Documentation](https://github.com/moq/moq4)
- [REST API Best Practices](https://restfulapi.net/)

---

*Last Updated: 2026-02-01*  
*For questions or issues, contact the IoT Hub Portal development team*
