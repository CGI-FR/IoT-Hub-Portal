# Feature Analysis: Schedule Management

## Overview
The Schedule Management feature provides CRUD operations for managing schedule entities in the IoT Hub Portal. It enables users to create, read, update, and delete schedule configurations with role-based access control for IoT device scheduling.

## Feature Identification

### Primary Components
- **Controller**: `SchedulesController.cs` (v1.0 API)
- **Service Interface**: `IScheduleService`
- **DTO**: `ScheduleDto` (data transfer object)
- **Entity**: `Schedule` (domain model)

### API Endpoints

#### POST /api/schedule
- **Purpose**: Creates a new schedule
- **Authorization**: `schedule:write` permission required
- **Request Body**: `ScheduleDto`
- **Response**: 200 OK with created `ScheduleDto`
- **Validation**: ArgumentNullException for null input

#### PUT /api/schedule
- **Purpose**: Updates an existing schedule
- **Authorization**: `schedule:write` permission required
- **Request Body**: `ScheduleDto`
- **Response**: 200 OK
- **Status Codes**: 200 OK

#### DELETE /api/schedule/{scheduleId}
- **Purpose**: Deletes a schedule by ID
- **Authorization**: `schedule:write` permission required
- **Route Parameter**: `scheduleId` (string)
- **Response**: 204 No Content
- **Status Codes**: 204 No Content

#### GET /api/schedule/{scheduleId}
- **Purpose**: Retrieves a specific schedule by ID
- **Authorization**: `schedule:read` permission required
- **Route Parameter**: `scheduleId` (string)
- **Response**: 200 OK with `ScheduleDto`, 404 Not Found
- **Status Codes**: 200 OK, 404 Not Found

#### GET /api/schedule
- **Purpose**: Retrieves all schedules
- **Authorization**: `schedule:read` permission required
- **Response**: 200 OK with `IEnumerable<ScheduleDto>`
- **Status Codes**: 200 OK

## Technical Details

### Dependencies
```csharp
- IScheduleService: Core business logic for schedule operations
```

### Data Flow
1. **Create**: Validate input → Service creates schedule → Return DTO
2. **Update**: Service updates schedule → Return OK
3. **Delete**: Service deletes schedule → Return No Content
4. **Get Single**: Service retrieves schedule → Return DTO or 404
5. **Get All**: Service retrieves all schedules → Return collection

### Security Model
- **Authentication**: Required (Authorize attribute on controller)
- **Authorization**: 
  - Write operations: `schedule:write` permission
  - Read operations: `schedule:read` permission
- **Fine-grained**: Policy-based authorization per endpoint

### Error Handling
- `DeviceNotFoundException` caught and returns 404 (inconsistent exception type)
- ArgumentNullException validation on create
- Exception message passed to client on 404

## Key Observations

### Strengths
- RESTful API design with proper HTTP verbs
- Clear separation of controller and service layers
- Role-based access control at method level
- Proper HTTP status codes (200, 204, 404)
- Null validation on create operation
- Consistent naming conventions

### Design Decisions
- Schedule ID is string-based (not GUID or int)
- Update uses PUT (full replacement pattern)
- Delete returns 204 No Content (correct REST practice)
- List returns all items (no pagination)
- Service returns domain entity on get, DTO on list
- Nearly identical pattern to PlanningsController

### Potential Issues
1. **Exception Naming**: Uses `DeviceNotFoundException` instead of `ScheduleNotFoundException`
2. **No Pagination**: List endpoint returns all schedules (scalability concern)
3. **No Filtering**: List endpoint has no query parameters (e.g., by date range, device)
4. **No Validation**: Update has no null check (unlike create)
5. **Inconsistent Return**: Create returns DTO, update returns empty OK
6. **No Conflict Handling**: No handling for duplicate schedule IDs or overlapping schedules
7. **No Audit**: No logging of CRUD operations
8. **Code Duplication**: Nearly identical to PlanningsController (pattern reuse opportunity)

## Domain Model

### Core Entities
- **Schedule**: Domain entity (likely includes timing, recurrence, actions)
- **ScheduleDto**: Data transfer object for API communication

### Expected Properties (Based on Domain)
- Schedule ID (unique identifier)
- Schedule timing (start time, end time, recurrence)
- Associated device or device group
- Actions to execute
- Status/state information
- Metadata (created, updated timestamps)

### Entity Relationships
- Schedule likely relates to Devices or Device Groups
- May relate to Planning entities
- May have associated Commands or Actions

## Integration Points

### Service Contract
```csharp
interface IScheduleService
{
    Task<ScheduleDto> CreateSchedule(ScheduleDto schedule);
    Task UpdateSchedule(ScheduleDto schedule);
    Task DeleteSchedule(string scheduleId);
    Task<Schedule> GetSchedule(string scheduleId);
    Task<IEnumerable<ScheduleDto>> GetSchedules();
}
```

### Data Transformation
- Service returns `Schedule` entity on single get
- Service returns `ScheduleDto` collection on list
- Controller responsible for JSON serialization

### Expected Integrations
- Device management (schedule applies to devices)
- Command execution (schedules trigger commands)
- Time zone management (for correct timing)
- Notification system (for schedule events)

## Business Rules
1. Schedule ID is required for update/delete/get operations
2. Write operations require elevated permissions vs read
3. All operations require authentication
4. Schedule objects cannot be null on creation
5. Deleted schedules return 204 with no body
6. Non-existent schedule returns 404 with error message
7. Schedules likely have temporal constraints (start < end)
8. Schedule execution likely managed by background service

## Testing Considerations

### Test Scenarios
- Create schedule with valid data
- Create schedule with null data
- Create schedule with invalid timing
- Update existing schedule
- Delete existing schedule
- Delete active/running schedule
- Get schedule by valid ID
- Get schedule by invalid ID (404)
- Get all schedules (empty and populated)
- Authorization checks for read/write permissions
- Concurrent schedule creation
- Overlapping schedule handling

### Edge Cases
- Schedule ID that doesn't exist (delete/update/get)
- Empty schedule list
- Duplicate schedule creation
- Concurrent updates to same schedule
- Invalid schedule data format
- Very long schedule ID strings
- Past start times
- Invalid recurrence patterns
- Time zone edge cases (DST transitions)

## Configuration
- API Version: 1.0
- Base Route: `/api/schedule`
- API Explorer Group: "IoT Schedule"
- Authorization Policy Names: `schedule:read`, `schedule:write`

## Metrics & Logging
- No explicit logging in controller
- Exception messages exposed to client (security consideration)
- No performance metrics
- No audit trail for CRUD operations
- No schedule execution metrics

## Schedule Execution Context
- Controller handles CRUD only (not execution)
- Execution likely handled by separate background service
- No visibility into schedule execution status from this endpoint
- May need separate endpoint for execution history/logs

## Future Considerations

### Immediate Improvements
1. Add pagination to list endpoint (page size, page number)
2. Add filtering (by date range, device, status)
3. Add sorting capabilities
4. Fix exception type (use ScheduleNotFoundException)
5. Add null check to update method
6. Add comprehensive logging for audit trail
7. Return created resource location in POST response
8. Add schedule validation (timing, recurrence patterns)

### Scalability
1. Implement caching for frequently accessed schedules
2. Add search capabilities (by device, date range)
3. Consider bulk operations API
4. Add rate limiting for write operations
5. Optimize list query (projection, indexes)

### Feature Enhancements
1. Add PATCH for partial updates
2. Add batch operations (enable/disable multiple)
3. Add schedule validation rules
4. Add schedule versioning/history
5. Add schedule status/lifecycle management (draft, active, completed, failed)
6. Add schedule execution history endpoint
7. Add schedule conflict detection
8. Add schedule simulation/dry-run
9. Add calendar view endpoint
10. Add time zone support
11. Add recurrence pattern builder
12. Add schedule templates
13. Add schedule dependencies (chain schedules)
14. Add notification preferences

### Security
1. Add input validation middleware
2. Sanitize error messages (don't expose internals)
3. Add request/response logging
4. Implement optimistic concurrency control
5. Add field-level permissions
6. Validate schedule ownership/access rights

### Integration
1. Add webhook support for schedule events
2. Add integration with external calendar systems
3. Add schedule import/export
4. Add schedule synchronization with device twins
5. Add schedule analytics and reporting

### Pattern Recognition
- This controller follows identical pattern to PlanningsController
- Consider creating a base generic controller for CRUD operations
- Reduces code duplication and maintenance burden
- Ensures consistent behavior across similar features
