# Evaluation Report: Schedule Management (018)

**Evaluation Date**: 2026-02-03  
**Spec Version**: Draft  
**Evaluator**: Excavate Evaluator Agent

---

## Summary Table

| Criterion | Score (1-5) | Weight | Weighted Score |
|-----------|-------------|--------|----------------|
| Correctness | 4.0 | 30% | 1.20 |
| Completeness | 3.5 | 30% | 1.05 |
| Technical Quality | 4.5 | 20% | 0.90 |
| Coverage | 4.0 | 20% | 0.80 |
| **Total** | | **100%** | **3.95/5.0** |

---

## Accurate Specifications

### ✅ FR-001: Create Schedules with Time Range and Commands
- **Status**: VERIFIED
- **Evidence**: [SchedulesController.cs](../../src/IoTHub.Portal.Server/Controllers/v1.0/SchedulesController.cs#L22-L32) - `CreateScheduleAsync` method
- **Code**: `await this.scheduleService.CreateSchedule(schedule);`

### ✅ FR-002: Associate Schedules with Parent Planning
- **Status**: VERIFIED
- **Evidence**: [Schedule.cs](../../src/IoTHub.Portal.Domain/Entities/Schedule.cs#L25) - `PlanningId` foreign key
- **Code**: `public string PlanningId { get; set; } = default!;`

### ✅ FR-003: Generate Unique Identifiers
- **Status**: VERIFIED
- **Evidence**: [ScheduleDto.cs](../../src/IoTHub.Portal.Shared/Models/v1.0/ScheduleDto.cs#L11) - Auto-generated GUID
- **Code**: `public string Id { get; set; } = Guid.NewGuid().ToString();`

### ✅ FR-004: Validate Schedule Data
- **Status**: VERIFIED
- **Evidence**: [SchedulesController.cs](../../src/IoTHub.Portal.Server/Controllers/v1.0/SchedulesController.cs#L27)
- **Code**: `ArgumentNullException.ThrowIfNull(schedule, nameof(schedule));`

### ✅ FR-006: Return All Schedules
- **Status**: VERIFIED
- **Evidence**: [SchedulesController.cs](../../src/IoTHub.Portal.Server/Controllers/v1.0/SchedulesController.cs#L82-L88) - `GetSchedules()` endpoint
- **Code**: `return Ok(await this.scheduleService.GetSchedules());`

### ✅ FR-007: Return Detailed Schedule Information
- **Status**: VERIFIED
- **Evidence**: [SchedulesController.cs](../../src/IoTHub.Portal.Server/Controllers/v1.0/SchedulesController.cs#L66-L78) - `GetSchedule(scheduleId)` endpoint

### ✅ FR-008: Return 404 for Non-Existent IDs
- **Status**: VERIFIED
- **Evidence**: [SchedulesController.cs](../../src/IoTHub.Portal.Server/Controllers/v1.0/SchedulesController.cs#L72-L76)
- **Code**: `return StatusCode(StatusCodes.Status404NotFound, e.Message);`

### ✅ FR-009: Update Schedule Configurations
- **Status**: VERIFIED
- **Evidence**: [SchedulesController.cs](../../src/IoTHub.Portal.Server/Controllers/v1.0/SchedulesController.cs#L36-L46)

### ✅ FR-010: Delete Schedules
- **Status**: VERIFIED
- **Evidence**: [SchedulesController.cs](../../src/IoTHub.Portal.Server/Controllers/v1.0/SchedulesController.cs#L51-L60)

### ✅ FR-011: Return 204 No Content on Deletion
- **Status**: VERIFIED
- **Evidence**: [SchedulesController.cs](../../src/IoTHub.Portal.Server/Controllers/v1.0/SchedulesController.cs#L59)
- **Code**: `return NoContent();`

---

## Inaccuracies Found

### ❌ FR-005: Day-Specific Command Configurations NOT Implemented
- **Spec Says**: System MUST support day-specific command configurations (different commands per weekday)
- **Actual**: Schedule entity only has single `CommandId`, no day-specific command mapping
- **Location**: [Schedule.cs](../../src/IoTHub.Portal.Domain/Entities/Schedule.cs)
- **Impact**: HIGH - Major documented feature not present in code

### ⚠️ Missing Name Property
- **Spec Says**: Key entities include "Schedule: Time-based command configuration"
- **Actual**: ScheduleDto has no `Name` property, only `Start`, `End`, `CommandId`, `PlanningId`
- **Location**: [ScheduleDto.cs](../../src/IoTHub.Portal.Shared/Models/v1.0/ScheduleDto.cs)
- **Impact**: MEDIUM - Entity doesn't have a descriptive label

### ⚠️ Incorrect Exception Type
- **Spec Says**: Return 404 appropriately
- **Actual**: Controller catches `DeviceNotFoundException` for schedule operations
- **Location**: [SchedulesController.cs#L75](../../src/IoTHub.Portal.Server/Controllers/v1.0/SchedulesController.cs#L75)
- **Impact**: LOW - Exception naming mismatch (uses DeviceNotFoundException for schedules)

### ⚠️ Route Naming
- **Spec Says**: SchedulesController: Schedule CRUD endpoints
- **Actual**: Route is `api/schedule` (singular)
- **Location**: [SchedulesController.cs#L9](../../src/IoTHub.Portal.Server/Controllers/v1.0/SchedulesController.cs#L9)
- **Impact**: LOW - Minor REST convention (typically plural resources)

### ⚠️ Timezone Handling Not Documented
- **Spec Says**: Edge case mentions "Europe/Paris timezone by default"
- **Actual**: No timezone handling visible in code; Start/End are strings
- **Location**: [Schedule.cs](../../src/IoTHub.Portal.Domain/Entities/Schedule.cs#L11-L18)
- **Impact**: MEDIUM - Timezone handling unclear in implementation

---

## Recommendations

1. **Implement Day-Specific Commands**: The spec documents FR-005 as a MUST requirement, but the current implementation only supports a single CommandId per schedule. Consider:
   - Adding a `DayCommands` dictionary property
   - Or creating a `ScheduleDayCommand` junction entity

2. **Add Name/Description Property**: Schedules should have a friendly name for UI display and identification.

3. **Standardize Exception Types**: Use `ResourceNotFoundException` consistently instead of `DeviceNotFoundException`.

4. **Consider Typed Time Properties**: Replace string-based Start/End with proper `TimeSpan` or `DateTimeOffset` types for better validation.

5. **Document Timezone Strategy**: Clarify whether times are stored in UTC and converted for display, or stored in local time.

---

## Code References

| Component | File | Purpose |
|-----------|------|---------|
| Controller | [SchedulesController.cs](../../src/IoTHub.Portal.Server/Controllers/v1.0/SchedulesController.cs) | API endpoints |
| Service Interface | [IScheduleService.cs](../../src/IoTHub.Portal.Application/Services/IScheduleService.cs) | Business logic contract |
| Service Implementation | [ScheduleService.cs](../../src/IoTHub.Portal.Infrastructure/Services/ScheduleService.cs) | Business logic |
| Domain Entity | [Schedule.cs](../../src/IoTHub.Portal.Domain/Entities/Schedule.cs) | Domain model |
| DTO | [ScheduleDto.cs](../../src/IoTHub.Portal.Shared/Models/v1.0/ScheduleDto.cs) | Data transfer object |
| Unit Tests | [SchedulesControllerTests.cs](../../src/IoTHub.Portal.Tests.Unit/Server/Controllers/v1.0/SchedulesControllerTests.cs) | Controller tests |
| Service Tests | [ScheduleServiceTests.cs](../../src/IoTHub.Portal.Tests.Unit/Server/Services/ScheduleServiceTests.cs) | Service tests |

---

## Verdict

**Overall Assessment**: The specification is mostly accurate but contains a significant gap regarding day-specific command configurations (FR-005). The core CRUD operations are correctly implemented, but the advanced scheduling feature documented in the spec is not present in the current codebase. This represents either a spec-ahead-of-code situation or a missing feature.

**Confidence Level**: MEDIUM-HIGH (75%)

**Action Required**: 
- Either implement FR-005 (day-specific commands) OR
- Update spec to remove this requirement if not planned
