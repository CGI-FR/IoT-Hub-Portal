# Implementation Plan: Fix Device Import Data Overwrite Bug

**Branch**: `copilot/analyze-issue-3251` | **Date**: 2026-01-30 | **Spec**: [Issue #3251](https://github.com/CGI-FR/IoT-Hub-Portal/issues/3251)
**Input**: GitHub Issue #3251 - Bug: Import device - data overwritten

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Fix critical data loss bug where imported LoRaWAN device data is overwritten during synchronization. When devices are imported via CSV, only a subset of fields (Id, Name, ModelId, assetId, locationCode, supportLoRaFeatures, AppKey, AppEUI, AppSKey, NwkSKey, DevAddr, GatewayID) are retained. Other CSV-provided data is lost because:
1. Import process doesn't push all fields to Azure IoT Hub
2. Synchronization job then overwrites database with incomplete data from IoT Hub
3. Some fields inherit unwanted device model defaults

Technical approach: Modify import flow to properly persist all CSV fields to IoT Hub desired properties, and refine synchronization mapping to preserve explicitly imported values while avoiding model default overrides.

## Technical Context

**Language/Version**: C# / .NET 8.0  
**Primary Dependencies**: 
- ASP.NET Core (Backend API)
- Blazor WebAssembly (Frontend)
- AutoMapper (Object mapping)
- Azure IoT Hub SDK (Device management)
- CsvHelper (CSV import/export)
- Entity Framework Core (Database ORM)
- Quartz.NET (Job scheduling for sync operations)

**Storage**: PostgreSQL or MySQL (configurable via Entity Framework)  
**Testing**: xUnit with Moq for unit tests  
**Target Platform**: Linux/Windows server (containerized via Docker), Web browser for frontend  
**Project Type**: Web application (separate backend/frontend)  
**Performance Goals**: Handle CSV imports with 1000+ devices, sync job completion within 5 minutes for typical deployments  
**Constraints**: 
- Must maintain Azure IoT Hub as source of truth for device state
- Cannot break existing CSV import/export format for backward compatibility
- Synchronization runs on scheduled intervals (via Quartz job)
- Must handle both OTAA and ABP LoRaWAN authentication modes

**Scale/Scope**: 
- IoT Hub Portal manages thousands of IoT devices
- Bug affects LoRaWAN device import specifically
- Impacts ~30 LoRaWAN-specific properties in LoRaDeviceBase class
- Core files: ExportManager.cs, SyncDevicesJob.cs, DeviceProfile.cs (AutoMapper), LoRaWanDeviceService.cs

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Initial Status (Pre-Phase 0)**: ✅ PASS (Constitution template is empty/unconfigured for this project)

**Post-Phase 1 Status**: ✅ PASS (Constitution template remains empty/unconfigured)

The project constitution file exists but contains only placeholder templates without concrete principles or constraints. Since no enforceable rules are defined, there are no violations to justify. 

**Design Review**:
- ✅ No new architectural layers introduced
- ✅ No new dependencies added
- ✅ Follows existing patterns (AutoMapper, CsvHelper, LoRaWAN service layer)
- ✅ Backward compatible (old CSV files work with defaults)
- ✅ No breaking API changes
- ✅ Maintains Azure IoT Hub as source of truth
- ✅ Unit tests follow existing xUnit + Moq patterns

This implementation will:
- Follow existing codebase patterns and conventions
- Maintain backward compatibility with CSV format
- Preserve Azure IoT Hub as source of truth architecture
- Add appropriate unit tests following xUnit patterns already in use

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
# Web application structure (Blazor WebAssembly + ASP.NET Core Backend)
backend/src/
├── IoTHub.Portal.Server/           # ASP.NET Core API
│   ├── Controllers/v1.0/           # REST API endpoints
│   │   ├── AdminController.cs      # Contains device import endpoint
│   │   └── LoRaWAN/               
│   │       └── LoRaWANDevicesController.cs
│   └── Managers/
│       └── ExportManager.cs        # CSV import/export logic (PRIMARY FIX LOCATION)
├── IoTHub.Portal.Infrastructure/   # Data access & external services
│   ├── Services/
│   │   └── LoRaWanDeviceService.cs # Device CRUD operations
│   └── Jobs/
│       └── SyncDevicesJob.cs       # Scheduled sync job (SECONDARY FIX LOCATION)
├── IoTHub.Portal.Application/      # Business logic layer
│   ├── Managers/
│   │   └── IExportManager.cs       # Import/export interface
│   └── Mappers/
│       └── DeviceProfile.cs        # AutoMapper profiles (TERTIARY FIX LOCATION)
├── IoTHub.Portal.Domain/           # Domain entities
│   └── Entities/
│       └── LorawanDevice.cs        # LoRaWAN device entity
└── IoTHub.Portal.Shared/           # Shared DTOs
    └── Models/v1.0/LoRaWAN/
        ├── LoRaDeviceDetails.cs    # DTO for device details
        └── LoRaDeviceBase.cs       # Base class with LoRaWAN properties

frontend/src/IoTHub.Portal.Client/  # Blazor WebAssembly
├── Pages/Devices/
│   └── ImportReportDialog.razor    # UI for import results
└── Services/
    └── DeviceClientService.cs      # API client

tests/
├── IoTHub.Portal.Tests.Unit/
│   ├── Server/Managers/
│   │   └── ExportManagerTests.cs   # Tests for import/export (NEEDS UPDATE)
│   └── Infrastructure/Jobs/
│       └── SyncDevicesJobTests.cs  # Tests for sync job (NEEDS UPDATE)
└── IoTHub.Portal.Tests.E2E/        # End-to-end tests
```

**Structure Decision**: Standard ASP.NET Core layered architecture with:
- **Server**: REST API controllers and managers
- **Infrastructure**: Data access, external services (Azure IoT Hub), and scheduled jobs
- **Application**: Business logic and cross-cutting concerns (mappers)
- **Domain**: Core entities and business rules
- **Shared**: DTOs shared between client and server
- **Client**: Blazor WebAssembly frontend

Bug fix requires changes primarily in Infrastructure and Server layers, with test updates.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No violations detected. Constitution is not yet configured with concrete principles.

---

## Phase 2: Task Generation Planning

**Status**: Ready for `/speckit.tasks` command

### Implementation Scope

**Primary Changes**:
1. **ExportManager.cs** - Add 14 property reads in `ImportLoRaDevice()` method
2. **ExportManagerTests.cs** - Add/update unit tests for complete property import

**Secondary Review** (No changes expected, but verify):
3. **DeviceProfile.cs** - Confirm AutoMapper correctly maps all properties (already done)
4. **SyncDevicesJob.cs** - Confirm sync logic preserves data (already correct)

### Estimated Complexity

**Size**: Small (single method modification + tests)  
**Risk**: Low (localized change, backward compatible)  
**Testing**: Unit + manual verification  
**Estimated effort**: 2-4 hours development + 2 hours testing

### Task Breakdown Preview

Expected tasks for `/speckit.tasks`:
1. Add LoRaWAN configuration property reads to CSV import
2. Update unit tests for complete property coverage
3. Add backward compatibility test for minimal CSV
4. Manual verification: Import → IoT Hub → Sync → Database
5. Update documentation if needed

### Dependencies

- ✅ Phase 0 (Research) - Complete
- ✅ Phase 1 (Design) - Complete
- ✅ Agent context - Updated
- ⏭️ Next: Run `/speckit.tasks` to generate actionable tasks

### Success Criteria

Fix is successful when:
- [ ] All 14 LoRaWAN configuration properties are read from CSV
- [ ] Properties are persisted to Azure IoT Hub desired properties
- [ ] Sync job retrieves complete data from IoT Hub
- [ ] Database retains all imported values after sync
- [ ] Unit tests achieve >95% coverage on modified methods
- [ ] Backward compatibility maintained for old CSV files
- [ ] Export → Import → Export roundtrip is lossless

---

## Completion Summary

**Planning Phase Complete**: 2026-01-30

### Artifacts Generated

✅ **Phase 0 Outputs**:
- `research.md` - Root cause analysis and technical decisions

✅ **Phase 1 Outputs**:
- `data-model.md` - Data structures and flow documentation
- `contracts/device-import-api.md` - API contract specification
- `quickstart.md` - Implementation guide
- `.github/agents/copilot-instructions.md` - Updated agent context

✅ **Planning Artifacts**:
- `plan.md` - This comprehensive implementation plan (updated)

### Next Steps

1. **Run task generation**: Execute `/speckit.tasks` command to break down implementation into actionable tasks
2. **Implementation**: Use generated `tasks.md` for step-by-step implementation
3. **Testing**: Follow test plans in `quickstart.md` and `research.md`
4. **Verification**: Ensure success criteria met before closing issue

### Key Insights

**Root Cause**: Import writes only 5 authentication properties to IoT Hub; sync overwrites database with incomplete data.

**Solution**: Expand import to write all 14 LoRaWAN configuration properties to IoT Hub.

**Impact**: Zero breaking changes, backward compatible, minimal code footprint (14 lines + tests).

**Branch**: `copilot/analyze-issue-3251`  
**Issue**: [#3251](https://github.com/CGI-FR/IoT-Hub-Portal/issues/3251)  
**Milestone**: v6.0
