---
description: "Task list for improving device status display"
---

# Tasks: Improve Device Status Display

**Feature**: Remove confusing Connection State and Last Status Update columns from device and gateway views
**Input**: Design documents from `/specs/001-device-status/`
**Prerequisites**: spec.md (user stories and requirements)

**Tests**: Tests are included as this is a UI change that needs validation

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

This is a Blazor web application with the following structure:
- Frontend: `/src/IoTHub.Portal.Client/`
- Shared Models: `/src/IoTHub.Portal.Shared/Models/`
- Unit Tests: `/src/IoTHub.Portal.Tests.Unit/`
- E2E Tests: `/src/IoTHub.Portal.Tests.E2E/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project validation and preparation

- [X] T001 Verify project builds successfully with `dotnet build src/IoTHub.Portal.sln`
- [X] T002 Verify existing tests pass with `dotnet test src/IoTHub.Portal.sln`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: No foundational changes needed - this is a removal task with no blocking prerequisites

**Checkpoint**: Setup complete - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - View Device Status Without Confusion (Priority: P1) 🎯 MVP

**Goal**: Remove the misleading "Connection State" column from the device list view so LoRaWAN devices don't appear as "disconnected" when functioning normally

**Independent Test**: Navigate to `/devices` page and verify that:
1. "Connection State" column is NOT present in the table headers
2. Devices are displayed without connection state indicators in the list
3. All other columns remain functional (Device, Allowed, Last activity time, See details, Delete)
4. The page loads without errors

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T003 [P] [US1] Add test to verify Connection State column is not present in DeviceListPage in src/IoTHub.Portal.Tests.Unit/Client/Pages/Devices/DevicesListPageTests.cs
- [X] T004 [P] [US1] Update existing DeviceListPage tests to not expect IsConnected property in src/IoTHub.Portal.Tests.Unit/Client/Pages/Devices/DevicesListPageTests.cs

### Implementation for User Story 1

- [X] T005 [US1] Remove IsConnected property from DeviceListItem model in src/IoTHub.Portal.Shared/Models/v1.0/DeviceListItem.cs
- [X] T006 [US1] Remove Connection State column header and data cells from DeviceListPage.razor in src/IoTHub.Portal.Client/Pages/Devices/DeviceListPage.razor
- [X] T007 [US1] Update ColGroup column definitions in DeviceListPage.razor to reflect removed column in src/IoTHub.Portal.Client/Pages/Devices/DeviceListPage.razor
- [X] T008 [US1] Run unit tests for DeviceListPage to verify changes with `dotnet test src/IoTHub.Portal.Tests.Unit/Client/Pages/Devices/DevicesListPageTests.cs`
- [X] T009 [US1] Manual smoke test: Load `/devices` page and verify Connection State column is removed

**Checkpoint**: At this point, User Story 1 should be fully functional - device list shows no Connection State column

---

## Phase 4: User Story 2 - View Accurate Device Activity Timestamp (Priority: P2)

**Goal**: Remove the "Last Status Update" column that shows Device Twin update time (misleading) and ensure activity information uses lastActivityTime

**Independent Test**: Navigate to `/devices` page and verify that:
1. "Last Status Update" column is NOT present in the table headers
2. "Last activity time" column is present and displays accurate timestamps
3. Activity timestamp reflects actual device communication, not Twin updates
4. All other functionality remains intact

### Tests for User Story 2

- [X] T010 [P] [US2] Add test to verify Last Status Update column is not present in DeviceListPage in src/IoTHub.Portal.Tests.Unit/Client/Pages/Devices/DevicesListPageTests.cs
- [X] T011 [P] [US2] Add test to verify LastActivityTime is displayed correctly in DeviceListPage in src/IoTHub.Portal.Tests.Unit/Client/Pages/Devices/DevicesListPageTests.cs
- [X] T012 [P] [US2] Update existing tests to not reference StatusUpdatedTime property in src/IoTHub.Portal.Tests.Unit/Client/Pages/Devices/DevicesListPageTests.cs

### Implementation for User Story 2

- [X] T013 [US2] Remove StatusUpdatedTime property from DeviceListItem model in src/IoTHub.Portal.Shared/Models/v1.0/DeviceListItem.cs
- [X] T014 [US2] Verify LastActivityTime property exists and is properly used in DeviceListPage.razor in src/IoTHub.Portal.Client/Pages/Devices/DeviceListPage.razor
- [X] T015 [US2] Add null handling for LastActivityTime in DeviceListPage.razor to display "No activity recorded" when null in src/IoTHub.Portal.Client/Pages/Devices/DeviceListPage.razor
- [X] T016 [US2] Run unit tests for DeviceListPage to verify changes with `dotnet test src/IoTHub.Portal.Tests.Unit/Client/Pages/Devices/DevicesListPageTests.cs`
- [X] T017 [US2] Manual smoke test: Load `/devices` page and verify Last Status Update column is removed and LastActivityTime displays correctly

**Checkpoint**: At this point, User Stories 1 AND 2 should both work - device list shows accurate activity time without confusing columns

---

## Phase 5: User Story 3 - View Gateway Status Without Confusion (Priority: P2)

**Goal**: Apply the same column removal to gateway list view for consistency across the platform

**Independent Test**: Navigate to `/edge/devices` page and verify that:
1. "Connection State" column is NOT present (Status column showing "Connected/Disconnected" should remain)
2. Gateway list displays correctly with all other columns functional
3. Activity information is accurate
4. No errors occur when loading the page

### Tests for User Story 3

- [X] T018 [P] [US3] Add test to verify gateway list displays correctly without Connection State issues in src/IoTHub.Portal.Tests.Unit/Client/Pages/EdgeDevices/EdgeDeviceListPageTests.cs
- [X] T019 [P] [US3] Update existing EdgeDeviceListPage tests to verify correct Status field usage in src/IoTHub.Portal.Tests.Unit/Client/Pages/EdgeDevices/EdgeDeviceListPageTests.cs

### Implementation for User Story 3

- [X] T020 [US3] Review IoTEdgeListItem model in src/IoTHub.Portal.Shared/Models/v1.0/IoTEdgeListItem.cs and verify Status field is correctly used (no changes needed - Status field is already correct)
- [X] T021 [US3] Verify EdgeDeviceListPage.razor uses Status field correctly and doesn't have misleading columns in src/IoTHub.Portal.Client/Pages/EdgeDevices/EdgeDeviceListPage.razor
- [X] T022 [US3] Review EdgeDeviceDetailPage.razor for any ConnectionState references that need clarification in src/IoTHub.Portal.Client/Pages/EdgeDevices/EdgeDeviceDetailPage.razor (found at line 311)
- [X] T023 [US3] Add comment to EdgeDeviceDetailPage.razor explaining ConnectionState usage in detail page context in src/IoTHub.Portal.Client/Pages/EdgeDevices/EdgeDeviceDetailPage.razor
- [X] T024 [US3] Run unit tests for EdgeDeviceListPage to verify no regressions with `dotnet test src/IoTHub.Portal.Tests.Unit/Client/Pages/EdgeDevices/EdgeDeviceListPageTests.cs`
- [X] T025 [US3] Manual smoke test: Load `/edge/devices` page and verify gateway list displays correctly

**Checkpoint**: All user stories should now be independently functional - both device and gateway lists show accurate information

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and final validation

- [X] T026 [P] Run full test suite to ensure no regressions with `dotnet test src/IoTHub.Portal.sln`
- [X] T027 [P] Manual testing: Test device list with LoRaWAN devices (devices that send data infrequently)
- [X] T028 [P] Manual testing: Test device list with devices that have never sent data (null lastActivityTime)
- [X] T029 [P] Manual testing: Test gateway list with various gateway states
- [X] T030 Update README or user documentation if device/gateway list columns were documented in docs/
- [X] T031 Code cleanup: Search for any remaining references to removed properties (IsConnected, StatusUpdatedTime) across the codebase
- [X] T032 Final validation: Build and run application with `dotnet run` and verify all pages work correctly

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: No blocking work needed
- **User Stories (Phase 3-5)**: Can proceed immediately after Setup
  - User stories can proceed in parallel (if staffed)
  - Or sequentially in priority order (US1 → US2 → US3)
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Setup - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Setup - Independent from US1, works on different model property
- **User Story 3 (P3)**: Can start after Setup - Independent from US1/US2, works on different page/model

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Model changes before UI changes
- Unit tests before manual smoke tests
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All three user stories can start in parallel after Setup (if team capacity allows)
- All tests for a user story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members
- Polish tasks marked [P] can run in parallel

---

## Parallel Example: All User Stories

Since these stories work on different files, they can all be done in parallel:

```bash
# Developer A: User Story 1 (Device List - Connection State)
Task: "Remove IsConnected property from DeviceListItem model"
Task: "Remove Connection State column from DeviceListPage.razor"

# Developer B: User Story 2 (Device List - Last Status Update)
Task: "Remove StatusUpdatedTime property from DeviceListItem model"
Task: "Add null handling for LastActivityTime in DeviceListPage.razor"

# Developer C: User Story 3 (Gateway List)
Task: "Review IoTEdgeListItem model and EdgeDeviceListPage.razor"
Task: "Add clarification comments for ConnectionState in detail page"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 3: User Story 1 (Remove Connection State from device list)
3. **STOP and VALIDATE**: Test that device list works without Connection State column
4. Deploy/demo if ready - This alone solves the critical user confusion issue

### Incremental Delivery

1. Complete Setup → Foundation ready (no blocking work)
2. Add User Story 1 → Test independently → Deploy/Demo (MVP! - Removes main confusion)
3. Add User Story 2 → Test independently → Deploy/Demo (Better info, removes second confusing column)
4. Add User Story 3 → Test independently → Deploy/Demo (Consistency across platform)
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup together (quick - just verify builds)
2. Once Setup is done:
   - Developer A: User Story 1 (Device Connection State)
   - Developer B: User Story 2 (Device Last Status Update)
   - Developer C: User Story 3 (Gateway consistency)
3. Stories complete and integrate independently (different files, no conflicts)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- This is primarily a **removal** task - we're making the UI simpler and less confusing
- Focus on not breaking existing functionality while removing misleading information
- The LastActivityTime field already exists - we're just ensuring it's the primary activity indicator
