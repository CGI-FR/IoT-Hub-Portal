---
description: "Task list for Custom Menu Entries feature implementation"
---

# Tasks: Custom Menu Entries

**Feature Branch**: `001-custom-menu-entries`  
**Input**: Design documents from `/specs/001-custom-menu-entries/`  
**Prerequisites**: spec.md (user stories and requirements)

**Architecture Context**: ASP.NET Core + Blazor WebAssembly using Clean Architecture
- **Domain Layer**: Entities, interfaces (IoTHub.Portal.Domain)
- **Application Layer**: Services, interfaces (IoTHub.Portal.Application)
- **Infrastructure Layer**: Repository implementations, service implementations (IoTHub.Portal.Infrastructure)
- **Server Layer**: API Controllers (IoTHub.Portal.Server)
- **Shared Layer**: DTOs, permissions (IoTHub.Portal.Shared)
- **Client Layer**: Blazor UI components (IoTHub.Portal.Client)

**Tests**: Tests are included as this is a production feature requiring quality assurance.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and permissions setup

- [ ] T001 Add MenuEntryRead permission to PortalPermissions enum in src/IoTHub.Portal.Shared/Security/PortalPermissions.cs
- [ ] T002 Add MenuEntryWrite permission to PortalPermissions enum in src/IoTHub.Portal.Shared/Security/PortalPermissions.cs
- [ ] T003 Add policy constants "menuentry:read" and "menuentry:write" to src/IoTHub.Portal.Shared/Security/Policies.cs (if not auto-generated)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain entities and database schema that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T004 [P] Create MenuEntry entity in src/IoTHub.Portal.Domain/Entities/MenuEntry.cs with properties: Id (string), Name (string), Url (string), Order (int), IsEnabled (bool), IsExternal (bool), CreatedAt (DateTime), UpdatedAt (DateTime)
- [ ] T005 [P] Create MenuEntryDto in src/IoTHub.Portal.Shared/Models/v1.0/MenuEntryDto.cs with same properties as entity
- [ ] T006 [P] Create IMenuEntryRepository interface in src/IoTHub.Portal.Domain/Repositories/IMenuEntryRepository.cs with GetAll, GetById, Add, Update, Delete, GetByName methods
- [ ] T007 Create MenuEntry database migration in src/IoTHub.Portal.Postgres/Migrations/ and src/IoTHub.Portal.MySql/Migrations/
- [ ] T008 [P] Create MenuEntryRepository implementation in src/IoTHub.Portal.Infrastructure/Repositories/MenuEntryRepository.cs
- [ ] T009 Register IMenuEntryRepository and MenuEntryRepository in DI container in src/IoTHub.Portal.Infrastructure/Startup/ServiceCollectionExtension.cs (or equivalent)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Create Custom Menu Entry (Priority: P1) 🎯 MVP

**Goal**: Users can create custom menu entries with name and URL, which persist in the database and appear in the navigation menu.

**Independent Test**: Create a menu entry via UI, verify it persists in storage, and confirm it appears in the navigation menu with the correct URL.

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T010 [P] [US1] Unit test for MenuEntry entity validation in src/IoTHub.Portal.Tests.Unit/Domain/Entities/MenuEntryTests.cs
- [ ] T011 [P] [US1] Unit test for MenuEntryRepository in src/IoTHub.Portal.Tests.Unit/Infrastructure/Repositories/MenuEntryRepositoryTests.cs
- [ ] T012 [P] [US1] Unit test for IMenuEntryService.CreateMenuEntry in src/IoTHub.Portal.Tests.Unit/Infrastructure/Services/MenuEntryServiceTests.cs
- [ ] T013 [P] [US1] Unit test for MenuEntriesController POST endpoint in src/IoTHub.Portal.Tests.Unit/Server/Controllers/v1.0/MenuEntriesControllerTests.cs
- [ ] T014 [P] [US1] Unit test for MenuEntriesController GET list endpoint in src/IoTHub.Portal.Tests.Unit/Server/Controllers/v1.0/MenuEntriesControllerTests.cs

### Implementation for User Story 1

**Domain & Application Layer**

- [ ] T015 [P] [US1] Create IMenuEntryService interface in src/IoTHub.Portal.Application/Services/IMenuEntryService.cs with CreateMenuEntry, GetAllMenuEntries methods
- [ ] T016 [P] [US1] Create MenuEntryMapper in src/IoTHub.Portal.Application/Mappers/MenuEntryMapper.cs to map between MenuEntry and MenuEntryDto

**Infrastructure Layer**

- [ ] T017 [US1] Implement MenuEntryService in src/IoTHub.Portal.Infrastructure/Services/MenuEntryService.cs with CreateMenuEntry validation (name required, max 100 chars, URL format validation)
- [ ] T018 [US1] Add business validation in MenuEntryService.CreateMenuEntry to check for duplicate names and enforce max length constraints
- [ ] T019 Register IMenuEntryService and MenuEntryService in DI container in src/IoTHub.Portal.Infrastructure/Startup/ServiceCollectionExtension.cs

**Server Layer (API)**

- [ ] T020 [US1] Create MenuEntriesController in src/IoTHub.Portal.Server/Controllers/v1.0/MenuEntriesController.cs with POST endpoint (requires menuentry:write authorization)
- [ ] T021 [US1] Add GET list endpoint to MenuEntriesController (requires menuentry:read authorization) to retrieve all menu entries
- [ ] T022 [US1] Add validation and error handling to MenuEntriesController endpoints (400 Bad Request for validation errors, 401 Unauthorized, 500 Internal Server Error)

**Client Layer (Blazor UI)**

- [ ] T023 [P] [US1] Create IMenuEntryClientService interface in src/IoTHub.Portal.Client/Services/IMenuEntryClientService.cs with CreateMenuEntry, GetMenuEntries methods
- [ ] T024 [P] [US1] Implement MenuEntryClientService in src/IoTHub.Portal.Client/Services/MenuEntryClientService.cs using HttpClient to call API
- [ ] T025 [US1] Register IMenuEntryClientService and MenuEntryClientService in DI container in src/IoTHub.Portal.Client/Program.cs
- [ ] T026 [P] [US1] Create MenuEntryListPage.razor in src/IoTHub.Portal.Client/Pages/MenuEntries/MenuEntryListPage.razor with MudTable displaying menu entries
- [ ] T027 [P] [US1] Create CreateMenuEntryDialog.razor in src/IoTHub.Portal.Client/Dialogs/MenuEntries/CreateMenuEntryDialog.razor with form for name and URL
- [ ] T028 [US1] Add validation to CreateMenuEntryDialog form (name required, max 100 chars, valid URL format)
- [ ] T029 [US1] Integrate CreateMenuEntryDialog with MenuEntryListPage (add button to open dialog)
- [ ] T030 [US1] Update NavMenu.razor in src/IoTHub.Portal.Client/Shared/NavMenu.razor to dynamically load and display custom menu entries
- [ ] T031 [US1] Implement logic in NavMenu.razor to differentiate external links (open in new tab with target="_blank") from internal links
- [ ] T032 [US1] Add navigation link to MenuEntryListPage in NavMenu.razor under Settings or Admin section (requires menuentry:read permission)

**Client Tests**

- [ ] T033 [P] [US1] Unit test for MenuEntryClientService in src/IoTHub.Portal.Tests.Unit/Client/Services/MenuEntryClientServiceTests.cs
- [ ] T034 [P] [US1] Unit test for MenuEntryListPage in src/IoTHub.Portal.Tests.Unit/Client/Pages/MenuEntries/MenuEntryListPageTests.cs
- [ ] T035 [P] [US1] Unit test for CreateMenuEntryDialog in src/IoTHub.Portal.Tests.Unit/Client/Dialogs/MenuEntries/CreateMenuEntryDialogTests.cs

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently. Users can create menu entries and see them in navigation.

---

## Phase 4: User Story 2 - Edit and Delete Menu Entries (Priority: P2)

**Goal**: Users can modify existing menu entries (change name, URL) and delete entries that are no longer needed.

**Independent Test**: Create a menu entry, edit its properties, verify the changes persist, and then delete it to confirm removal from the navigation menu.

### Tests for User Story 2

- [X] T036 [P] [US2] Unit test for IMenuEntryService.UpdateMenuEntry in src/IoTHub.Portal.Tests.Unit/Infrastructure/Services/MenuEntryServiceTests.cs
- [X] T037 [P] [US2] Unit test for IMenuEntryService.DeleteMenuEntry in src/IoTHub.Portal.Tests.Unit/Infrastructure/Services/MenuEntryServiceTests.cs
- [X] T038 [P] [US2] Unit test for MenuEntriesController PUT endpoint in src/IoTHub.Portal.Tests.Unit/Server/Controllers/v1.0/MenuEntriesControllerTests.cs
- [X] T039 [P] [US2] Unit test for MenuEntriesController DELETE endpoint in src/IoTHub.Portal.Tests.Unit/Server/Controllers/v1.0/MenuEntriesControllerTests.cs

### Implementation for User Story 2

**Domain & Application Layer**

- [X] T040 [US2] Add UpdateMenuEntry and DeleteMenuEntry methods to IMenuEntryService interface in src/IoTHub.Portal.Application/Services/IMenuEntryService.cs

**Infrastructure Layer**

- [X] T041 [US2] Implement UpdateMenuEntry in MenuEntryService with validation (name required, max 100 chars, URL format, duplicate name check)
- [X] T042 [US2] Implement DeleteMenuEntry in MenuEntryService with existence check

**Server Layer (API)**

- [X] T043 [US2] Add GET by ID endpoint to MenuEntriesController in src/IoTHub.Portal.Server/Controllers/v1.0/MenuEntriesController.cs (requires menuentry:read authorization)
- [X] T044 [US2] Add PUT endpoint to MenuEntriesController for updating menu entries (requires menuentry:write authorization)
- [X] T045 [US2] Add DELETE endpoint to MenuEntriesController for deleting menu entries (requires menuentry:write authorization, returns 204 No Content)
- [X] T046 [US2] Add error handling for 404 Not Found when menu entry doesn't exist

**Client Layer (Blazor UI)**

- [X] T047 [P] [US2] Add GetMenuEntryById, UpdateMenuEntry, and DeleteMenuEntry methods to IMenuEntryClientService in src/IoTHub.Portal.Client/Services/IMenuEntryClientService.cs
- [X] T048 [P] [US2] Implement new methods in MenuEntryClientService in src/IoTHub.Portal.Client/Services/MenuEntryClientService.cs
- [X] T049 [P] [US2] Create EditMenuEntryDialog.razor in src/IoTHub.Portal.Client/Dialogs/MenuEntries/EditMenuEntryDialog.razor with pre-filled form
- [X] T050 [US2] Add validation to EditMenuEntryDialog form (same as create: name required, max 100 chars, valid URL)
- [X] T051 [US2] Add Edit button to MenuEntryListPage that opens EditMenuEntryDialog with selected entry
- [X] T052 [US2] Add Delete button to MenuEntryListPage with confirmation dialog before deletion
- [X] T053 [US2] Update MenuEntryListPage to refresh list after edit or delete operations
- [X] T054 [US2] Update NavMenu.razor to handle menu entry deletions gracefully (remove from navigation without breaking UI)

**Client Tests**

- [X] T055 [P] [US2] Unit test for EditMenuEntryDialog in src/IoTHub.Portal.Tests.Unit/Client/Dialogs/MenuEntries/EditMenuEntryDialogTests.cs
- [X] T056 [P] [US2] Update MenuEntryListPageTests to cover edit and delete operations in src/IoTHub.Portal.Tests.Unit/Client/Pages/MenuEntries/MenuEntryListPageTests.cs

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently. Full CRUD operations are available.

---

## Phase 5: User Story 3 - Manage Menu Entry via API (Priority: P2)

**Goal**: Administrators can create, update, and delete custom menu entries programmatically through REST API endpoints, enabling automation and integration.

**Independent Test**: Call API endpoints directly to create, update, and delete menu entries, verifying operations complete successfully and changes reflect in the UI.

### Tests for User Story 3

- [ ] T057 [P] [US3] Integration test for POST /api/menu-entries endpoint in src/IoTHub.Portal.Tests.Unit/Server/Controllers/v1.0/MenuEntriesControllerTests.cs (verify 201 Created response)
- [ ] T058 [P] [US3] Integration test for PUT /api/menu-entries/{id} endpoint (verify 200 OK response)
- [ ] T059 [P] [US3] Integration test for DELETE /api/menu-entries/{id} endpoint (verify 204 No Content response)
- [ ] T060 [P] [US3] Integration test for unauthorized API access (verify 401 Unauthorized response)
- [ ] T061 [P] [US3] Integration test for invalid data validation (verify 400 Bad Request response)

### Implementation for User Story 3

**Server Layer (API Enhancement)**

- [ ] T062 [US3] Update POST endpoint in MenuEntriesController to return 201 Created with location header and created resource
- [ ] T063 [US3] Update PUT endpoint to return 200 OK with updated resource details
- [ ] T064 [US3] Ensure DELETE endpoint returns 204 No Content on success
- [ ] T065 [US3] Add comprehensive error responses: 400 Bad Request (validation), 401 Unauthorized (auth), 404 Not Found (missing entry), 409 Conflict (duplicate name)
- [ ] T066 [US3] Add API documentation comments (XML docs) to all MenuEntriesController endpoints for Swagger/OpenAPI
- [ ] T067 [US3] Verify proper authorization enforcement on all endpoints (menuentry:read for GET, menuentry:write for POST/PUT/DELETE)

**Documentation**

- [ ] T068 [P] [US3] Create API documentation for menu entries endpoints in docs/ or README (if applicable)
- [ ] T069 [P] [US3] Add example cURL commands or Postman collection for API usage

**Checkpoint**: All CRUD operations now available via REST API with proper status codes and authentication.

---

## Phase 6: User Story 4 - Configure Menu Entry Order (Priority: P3)

**Goal**: Users can reorder custom menu entries by dragging and dropping or using up/down controls. The order persists across sessions.

**Independent Test**: Create multiple menu entries, change their order via drag-and-drop or order controls, and verify the new order persists after page refresh.

### Tests for User Story 4

- [ ] T070 [P] [US4] Unit test for IMenuEntryService.UpdateMenuEntryOrder in src/IoTHub.Portal.Tests.Unit/Infrastructure/Services/MenuEntryServiceTests.cs
- [ ] T071 [P] [US4] Unit test for MenuEntriesController PATCH/PUT order endpoint in src/IoTHub.Portal.Tests.Unit/Server/Controllers/v1.0/MenuEntriesControllerTests.cs

### Implementation for User Story 4

**Domain & Application Layer**

- [ ] T072 [US4] Add UpdateMenuEntryOrder method to IMenuEntryService interface in src/IoTHub.Portal.Application/Services/IMenuEntryService.cs

**Infrastructure Layer**

- [ ] T073 [US4] Implement UpdateMenuEntryOrder in MenuEntryService to update Order property for one or more entries
- [ ] T074 [US4] Ensure GetAllMenuEntries returns entries ordered by Order property ascending

**Server Layer (API)**

- [ ] T075 [US4] Add PATCH/PUT endpoint to MenuEntriesController for bulk order updates (accepts array of {id, order} pairs, requires menuentry:write)

**Client Layer (Blazor UI)**

- [ ] T076 [P] [US4] Add UpdateMenuEntryOrder method to IMenuEntryClientService in src/IoTHub.Portal.Client/Services/IMenuEntryClientService.cs
- [ ] T077 [P] [US4] Implement UpdateMenuEntryOrder in MenuEntryClientService in src/IoTHub.Portal.Client/Services/MenuEntryClientService.cs
- [ ] T078 [US4] Add up/down arrow buttons to each row in MenuEntryListPage MudTable for manual reordering
- [ ] T079 [US4] Implement logic to increment/decrement Order values and call API when arrows are clicked
- [ ] T080 [US4] Add drag-and-drop support to MenuEntryListPage using MudBlazor drag-drop components or HTML5 drag API
- [ ] T081 [US4] Update MenuEntryListPage to refresh and reorder list after order changes
- [ ] T082 [US4] Update NavMenu.razor to display custom menu entries in the configured order (order by Order property)

**Client Tests**

- [ ] T083 [P] [US4] Unit test for menu entry ordering in MenuEntryListPageTests in src/IoTHub.Portal.Tests.Unit/Client/Pages/MenuEntries/MenuEntryListPageTests.cs

**Checkpoint**: Menu entries can now be reordered and the order persists across sessions.

---

## Phase 7: User Story 5 - Configure Menu Section Position (Priority: P3)

**Goal**: Administrators can configure where the custom menu section appears relative to existing portal menu sections (e.g., first, last, or between specific sections).

**Independent Test**: Change the section position setting, verify the custom menu section appears in the specified location in the navigation menu.

### Tests for User Story 5

- [ ] T084 [P] [US5] Unit test for MenuSectionConfiguration entity in src/IoTHub.Portal.Tests.Unit/Domain/Entities/MenuSectionConfigurationTests.cs
- [ ] T085 [P] [US5] Unit test for IMenuSectionConfigurationService in src/IoTHub.Portal.Tests.Unit/Infrastructure/Services/MenuSectionConfigurationServiceTests.cs
- [ ] T086 [P] [US5] Unit test for MenuSectionConfigurationController in src/IoTHub.Portal.Tests.Unit/Server/Controllers/v1.0/MenuSectionConfigurationControllerTests.cs (if separate controller)

### Implementation for User Story 5

**Domain & Application Layer**

- [ ] T087 [P] [US5] Create MenuSectionConfiguration entity in src/IoTHub.Portal.Domain/Entities/MenuSectionConfiguration.cs with properties: Id, Position (enum: First, Last, After), AfterSection (string), UpdatedAt
- [ ] T088 [P] [US5] Create MenuSectionConfigurationDto in src/IoTHub.Portal.Shared/Models/v1.0/MenuSectionConfigurationDto.cs
- [ ] T089 [P] [US5] Create IMenuSectionConfigurationRepository interface in src/IoTHub.Portal.Domain/Repositories/IMenuSectionConfigurationRepository.cs
- [ ] T090 [US5] Create database migration for MenuSectionConfiguration in src/IoTHub.Portal.Postgres/Migrations/ and src/IoTHub.Portal.MySql/Migrations/
- [ ] T091 [P] [US5] Create IMenuSectionConfigurationService interface in src/IoTHub.Portal.Application/Services/IMenuSectionConfigurationService.cs
- [ ] T092 [P] [US5] Create MenuSectionConfigurationMapper in src/IoTHub.Portal.Application/Mappers/MenuSectionConfigurationMapper.cs

**Infrastructure Layer**

- [ ] T093 [US5] Implement MenuSectionConfigurationRepository in src/IoTHub.Portal.Infrastructure/Repositories/MenuSectionConfigurationRepository.cs
- [ ] T094 [US5] Implement MenuSectionConfigurationService in src/IoTHub.Portal.Infrastructure/Services/MenuSectionConfigurationService.cs with Get and Update methods
- [ ] T095 [US5] Register repositories and services in DI container in src/IoTHub.Portal.Infrastructure/Startup/ServiceCollectionExtension.cs

**Server Layer (API)**

- [ ] T096 [US5] Add GET and PUT endpoints to MenuEntriesController or create MenuSectionConfigurationController for section position configuration (requires menuentry:write for PUT)

**Client Layer (Blazor UI)**

- [ ] T097 [P] [US5] Create IMenuSectionConfigurationClientService in src/IoTHub.Portal.Client/Services/IMenuSectionConfigurationClientService.cs
- [ ] T098 [P] [US5] Implement MenuSectionConfigurationClientService in src/IoTHub.Portal.Client/Services/MenuSectionConfigurationClientService.cs
- [ ] T099 [US5] Register service in DI container in src/IoTHub.Portal.Client/Program.cs
- [ ] T100 [US5] Add section position configuration UI to MenuEntryListPage or create separate settings page with dropdown/radio buttons for position (First, Last, After)
- [ ] T101 [US5] Add conditional dropdown to select "After" section when "After" position is selected
- [ ] T102 [US5] Update NavMenu.razor to dynamically position custom menu section based on configuration
- [ ] T103 [US5] Implement logic in NavMenu.razor to inject custom menu section at specified position (first, last, or after named section)

**Client Tests**

- [ ] T104 [P] [US5] Unit test for MenuSectionConfigurationClientService in src/IoTHub.Portal.Tests.Unit/Client/Services/MenuSectionConfigurationClientServiceTests.cs
- [ ] T105 [P] [US5] Unit test for NavMenu section positioning logic in src/IoTHub.Portal.Tests.Unit/Client/Shared/NavMenuTests.cs (if applicable)

**Checkpoint**: All user stories are now independently functional. Custom menu section position is configurable.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories, documentation, and final validation

- [ ] T106 [P] Add logging for menu entry operations (create, update, delete, order changes) in MenuEntryService
- [ ] T107 [P] Add logging for API requests in MenuEntriesController
- [ ] T108 [P] Handle edge cases: extremely long names (truncate in UI), broken URLs (user's browser handles), concurrent updates (optimistic concurrency if needed)
- [ ] T109 [P] Implement pagination or virtual scrolling in MenuEntryListPage for handling 50+ entries
- [ ] T110 [P] Add error handling and user-friendly error messages in all client components
- [ ] T111 [P] Performance optimization: cache menu entries in client to avoid repeated API calls on every navigation
- [ ] T112 [P] Add loading indicators in MenuEntryListPage and dialogs during API operations
- [ ] T113 [P] Security review: verify authorization on all endpoints, sanitize URLs to prevent XSS
- [ ] T114 [P] Accessibility review: ensure keyboard navigation works in menu entry list and dialogs, proper ARIA labels
- [ ] T115 [P] Update user documentation with instructions for creating and managing custom menu entries
- [ ] T116 [P] Create admin documentation for API usage and configuration
- [ ] T117 Code cleanup and refactoring: remove unused code, improve naming consistency
- [ ] T118 Run all unit tests and verify 100% pass rate across all layers
- [ ] T119 Run integration tests (if applicable) to validate end-to-end scenarios
- [ ] T120 Manual testing of all user stories in browser to validate UI/UX

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational (Phase 2) - Core functionality
- **User Story 2 (Phase 4)**: Depends on User Story 1 (Phase 3) - Extends CRUD with edit/delete
- **User Story 3 (Phase 5)**: Can start after User Story 2 (Phase 4) - API enhancements and documentation
- **User Story 4 (Phase 6)**: Can start after User Story 1 (Phase 3) - Independent ordering feature
- **User Story 5 (Phase 7)**: Can start after User Story 1 (Phase 3) - Independent positioning feature
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: MUST complete first - provides core create and list functionality
- **User Story 2 (P2)**: Depends on User Story 1 - extends with edit and delete
- **User Story 3 (P2)**: Depends on User Story 2 - enhances API with proper responses
- **User Story 4 (P3)**: Depends on User Story 1 - adds ordering (can be implemented in parallel with US2/US3)
- **User Story 5 (P3)**: Depends on User Story 1 - adds positioning (can be implemented in parallel with US2/US3/US4)

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Domain entities before services
- Services before controllers
- Controllers before client services
- Client services before UI components
- Core implementation before integration

### Parallel Opportunities

- Within **Setup (Phase 1)**: All tasks can run in parallel
- Within **Foundational (Phase 2)**: T004, T005, T006, T008 can run in parallel (entity, DTO, interface, repository)
- Within **User Story Tests**: All test tasks marked [P] can run in parallel
- **User Story 4 and 5**: Can be implemented in parallel after User Story 1 is complete
- **Polish tasks**: Most tasks in Phase 8 marked [P] can run in parallel

---

## Parallel Example: User Story 1 Tests

```bash
# Launch all tests for User Story 1 together:
Task T010: "Unit test for MenuEntry entity validation"
Task T011: "Unit test for MenuEntryRepository"
Task T012: "Unit test for IMenuEntryService.CreateMenuEntry"
Task T013: "Unit test for MenuEntriesController POST endpoint"
Task T014: "Unit test for MenuEntriesController GET list endpoint"
```

## Parallel Example: User Story 1 Domain Layer

```bash
# Launch domain layer tasks together:
Task T015: "Create IMenuEntryService interface"
Task T016: "Create MenuEntryMapper"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T003)
2. Complete Phase 2: Foundational (T004-T009) - CRITICAL: blocks all stories
3. Complete Phase 3: User Story 1 (T010-T035)
4. **STOP and VALIDATE**: Test User Story 1 independently
   - Create menu entries via UI
   - Verify persistence
   - Check navigation menu displays entries correctly
   - Test external vs internal link behavior
5. Deploy/demo if ready - **This is the MVP!**

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
   - **Value delivered**: Users can create and view custom menu entries
3. Add User Story 2 → Test independently → Deploy/Demo
   - **Value delivered**: Full CRUD operations, users can maintain menu entries
4. Add User Story 3 → Test independently → Deploy/Demo
   - **Value delivered**: API automation capabilities for enterprise deployments
5. Add User Story 4 → Test independently → Deploy/Demo
   - **Value delivered**: Customizable menu order for better UX
6. Add User Story 5 → Test independently → Deploy/Demo
   - **Value delivered**: Full navigation customization with section positioning
7. Polish (Phase 8) → Final quality improvements and documentation

### Parallel Team Strategy

With multiple developers:

1. **Team completes Setup + Foundational together** (T001-T009)
2. **Once Foundational is done:**
   - Developer A: User Story 1 (T010-T035) - MUST complete first
   - Developers wait for US1 core components (T004-T022) before starting parallel work
3. **After User Story 1 core is complete:**
   - Developer A: User Story 2 (T036-T056)
   - Developer B: User Story 4 (T070-T083) - can start after US1 foundation
   - Developer C: User Story 5 (T084-T105) - can start after US1 foundation
4. **After User Story 2 is complete:**
   - Developer A: User Story 3 (T057-T069) - enhances API
5. **Final phase:**
   - All developers: Polish tasks (T106-T120) - many can be done in parallel

---

## Notes

- **[P] tasks** = different files, no dependencies, can be parallelized
- **[Story] label** maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing (TDD approach)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- **Authorization**: All endpoints require proper authorization (menuentry:read or menuentry:write)
- **Validation**: Enforce name max length (100 chars), URL format, no duplicates
- **Error Handling**: Use proper HTTP status codes (200, 201, 204, 400, 401, 404, 409, 500)
- **UI/UX**: Use MudBlazor components for consistency, ensure responsive design
- **Testing**: Unit tests for all layers, integration tests for API endpoints
- **Documentation**: XML docs for API, user docs for UI features
- **Avoid**: vague tasks, same file conflicts, cross-story dependencies that break independence

---

## Task Summary

- **Total Tasks**: 120
- **Setup**: 3 tasks
- **Foundational**: 6 tasks
- **User Story 1 (P1)**: 26 tasks (T010-T035)
- **User Story 2 (P2)**: 21 tasks (T036-T056)
- **User Story 3 (P2)**: 13 tasks (T057-T069)
- **User Story 4 (P3)**: 14 tasks (T070-T083)
- **User Story 5 (P3)**: 22 tasks (T084-T105)
- **Polish**: 15 tasks (T106-T120)

**Parallel Opportunities**: 40+ tasks marked [P] can be executed in parallel within their phases

**Estimated Complexity**:
- **Setup + Foundational**: 2-3 days (critical path)
- **User Story 1 (MVP)**: 5-7 days (full stack implementation)
- **User Story 2**: 3-4 days (extends US1)
- **User Story 3**: 2-3 days (API documentation and testing)
- **User Story 4**: 3-4 days (ordering UI and logic)
- **User Story 5**: 4-5 days (new entity and positioning logic)
- **Polish**: 2-3 days (testing, docs, cleanup)
- **Total Estimated**: 21-29 days (single developer, sequential)
- **With 3 developers**: 12-16 days (parallelizing US4, US5, and polish tasks)

**Suggested MVP Scope**: Complete only Setup + Foundational + User Story 1 (T001-T035) for initial release. This provides the core value of custom menu entries.
