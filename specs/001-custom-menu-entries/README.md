# Custom Menu Entries - User Story 2: Edit and Delete

**Feature ID**: 001-custom-menu-entries  
**User Story**: US2 - Edit and Delete Menu Entries  
**Status**: Planning Complete ✅ | Implementation Ready 🚀  
**Branch**: 001-custom-menu-entries

---

## 📋 Quick Navigation

| Document | Purpose | When to Use |
|----------|---------|-------------|
| **[plan.md](./plan.md)** | Master implementation plan | Start here - overview of entire US2 implementation |
| **[quickstart.md](./quickstart.md)** | Developer guide with code examples | During implementation - step-by-step instructions |
| **[research.md](./research.md)** | Technical research and decisions | Understanding "why" behind technical choices |
| **[data-model.md](./data-model.md)** | Entity and DTO documentation | Reference for data structures and validation |
| **[contracts/menu-entries-api.yaml](./contracts/menu-entries-api.yaml)** | OpenAPI specification | API contract reference for PUT/DELETE endpoints |
| **[tasks.md](./tasks.md)** | Task breakdown (T036-T056) | Task tracking and dependency management |
| **[spec.md](./spec.md)** | Feature specification | Original requirements and acceptance criteria |

---

## 🎯 Implementation Workflow

### Phase 0: Research ✅ COMPLETE
**Output**: `research.md`  
All technical decisions documented, alternatives evaluated, best practices identified.

### Phase 1: Data Model & Contracts ✅ COMPLETE
**Outputs**: `data-model.md`, `contracts/menu-entries-api.yaml`, `quickstart.md`  
Data model confirmed (no changes), API contracts defined, implementation guide created.

### Phase 2: Implementation ⏭️ READY TO START
**Guide**: `quickstart.md`  
**Tasks**: T036-T056 in `tasks.md`

Follow this sequence:
1. **Write Tests First** (TDD approach)
   - Service tests: `MenuEntryServiceTests.cs`
   - Controller tests: `MenuEntriesControllerTests.cs`
   - Client tests: `EditMenuEntryDialog`, `MenuEntryListPage`
   - Run tests → Should FAIL ✅

2. **Backend Implementation**
   - Enhance `MenuEntryService.UpdateMenuEntryAsync()`
   - Enhance `MenuEntryService.DeleteMenuEntryAsync()`
   - Enhance `MenuEntriesController` PUT/DELETE endpoints
   - Run tests → Should PASS ✅

3. **Frontend Implementation**
   - Add delete confirmation dialog
   - Enhance `EditMenuEntryDialog` with validation
   - Add Edit/Delete buttons to `MenuEntryListPage`
   - Add error handling to `NavMenu`
   - Manual testing → Should work ✅

4. **Final Validation**
   - All unit tests pass
   - Manual testing checklist complete
   - All acceptance scenarios verified

---

## 🔑 Key Information

### What's Being Built
User Story 2 adds edit and delete capabilities to the Custom Menu Entries feature:
- **Edit**: Modify existing menu entries (change name, URL, properties)
- **Delete**: Permanently remove menu entries (with confirmation)
- **Validation**: Same rules as create (name required, max 100 chars, valid URL)
- **Authorization**: Requires `menuentry:write` permission

### What Already Exists (from US1)
- ✅ MenuEntry entity and MenuEntryDto
- ✅ Repository and service interfaces with Update/Delete methods
- ✅ Controller with basic endpoints
- ✅ Client services and UI components
- ✅ 27 existing unit tests

### What Needs Enhancement
- 🔧 Service validation logic (duplicate check, error handling)
- 🔧 Controller error responses (404, 400, proper ProblemDetails)
- 🔧 Delete confirmation dialog
- 🔧 Edit dialog validation feedback
- 🔧 List page refresh after operations
- 🔧 21 new unit tests

### Technical Decisions Summary
| Decision | Rationale |
|----------|-----------|
| Hard delete (not soft) | No audit requirements, simpler implementation |
| Frontend confirmation | Better UX than backend confirmation API |
| Same validation as CREATE | Consistency for users |
| Last-write-wins concurrency | Acceptable for low-frequency updates |
| Reload list after edit/delete | Simple, reliable, sufficient |
| No new dependencies | US1 provides everything needed |

---

## 📊 Metrics

- **Planning Documents**: 5 files, 115+ KB
- **Implementation Tasks**: 21 tasks (T036-T056)
- **New Unit Tests**: ~21 tests
- **Time Estimate**: 3-4 days
- **Risk Level**: LOW (building on US1 foundation)
- **Breaking Changes**: NONE

---

## ✅ Acceptance Criteria

From `spec.md` User Story 2:

1. **Edit entry name**  
   **Given** a custom menu entry exists  
   **When** a user edits the entry's name from "Old Docs" to "New Docs"  
   **Then** the updated name appears in the navigation menu

2. **Delete entry**  
   **Given** a custom menu entry exists  
   **When** a user deletes the entry  
   **Then** it is removed from the navigation menu and no longer accessible

3. **Edit with invalid URL**  
   **Given** a user is editing a menu entry  
   **When** they change the URL to an invalid format  
   **Then** validation prevents saving and displays an error message

4. **Delete confirmation**  
   **Given** a menu entry is in use  
   **When** a user attempts to delete it  
   **Then** the system confirms deletion and removes the entry

---

## 🧪 Testing Strategy

### Unit Tests (Write First - TDD)
- **Service Layer**: 7 tests (5 update + 2 delete)
- **Controller Layer**: 6 tests (4 PUT + 2 DELETE)
- **Client Layer**: 8 tests (4 dialog + 4 list page)
- **Total**: ~21 new tests

### Manual Testing Checklist
- [ ] Edit menu entry name → verify navigation update
- [ ] Edit menu entry URL → verify link works
- [ ] Delete menu entry → verify removal + confirmation
- [ ] Edit with invalid URL → verify validation error
- [ ] Edit with duplicate name → verify error message
- [ ] Authorization check → verify 401 for non-admins

---

## 🔧 Technology Stack

- **Backend**: ASP.NET Core 8.0, Entity Framework Core, AutoMapper
- **Frontend**: Blazor WebAssembly, MudBlazor
- **Database**: PostgreSQL 13+ OR MySQL 8.0+
- **Testing**: xUnit, Moq, bUnit
- **Architecture**: Clean Architecture (Domain → Application → Infrastructure → Server/Client)

---

## 📚 Additional Resources

- **Constitution**: `.specify/memory/constitution.md` - Project standards and patterns
- **US1 Implementation**: Already complete - provides foundation for US2
- **ASP.NET Core Validation**: https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation
- **MudBlazor Dialogs**: https://mudblazor.com/components/dialog
- **REST API Best Practices**: https://restfulapi.net/

---

## 🚀 Getting Started

1. **Read** `plan.md` for complete overview
2. **Review** `quickstart.md` for implementation steps
3. **Reference** `contracts/menu-entries-api.yaml` for API details
4. **Follow** TDD approach (write tests first)
5. **Implement** backend → frontend → testing
6. **Validate** against acceptance criteria
7. **Deploy** to staging for stakeholder review

---

## 💡 Quick Tips

- ✅ All US1 infrastructure is ready - no new dependencies needed
- ✅ No breaking changes - US1 functionality fully preserved
- ✅ Follow TDD - write tests first, ensure they fail, implement, verify pass
- ✅ Use `quickstart.md` for detailed code examples
- ✅ All architecture standards already met (constitution compliance)
- ✅ Estimated 3-4 days for complete implementation

---

## 📞 Support

For questions or clarification:
1. Check the comprehensive documentation in this directory
2. Review specific sections in `plan.md` or `quickstart.md`
3. Refer to `research.md` for technical decision rationale
4. Contact the IoT Hub Portal development team

---

**Status**: ✅ Planning Complete | 🚀 Ready for Implementation

*Last Updated: 2026-02-01*
