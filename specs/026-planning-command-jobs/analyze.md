# Feature: Planning Command Jobs

**Category**: Automation & Scheduling  
**Status**: Analyzed  

---

## Description

The Planning Command Jobs feature provides an advanced scheduling system that automatically executes LoRaWAN commands on IoT devices based on time-based schedules and calendar-driven planning rules. This critical automation infrastructure enables organizations to implement sophisticated device control workflows, such as automated streetlight control, irrigation scheduling, HVAC automation, and industrial equipment operations, without manual intervention.

Key capabilities include:

**Planning Management**:
- Time-bounded planning periods with start and end dates
- Weekly day-off patterns for non-working days (e.g., weekends, holidays)
- Default commands for off-days (e.g., "lights off" on weekends)
- Active planning validation based on current date
- Planning assignment to hierarchical device layers

**Schedule Management**:
- Multiple time-based schedules per planning
- Start and end time ranges for command execution
- Day-specific command sequences (different commands per weekday)
- Command associations with specific payloads
- Support for overlapping schedules within planning periods

**Device Organization via Layers**:
- Hierarchical device grouping (father-child layer relationships)
- Planning association at layer level (inherited by all devices)
- Dynamic device-to-planning mapping based on layer membership
- Support for devices without layers (excluded from automation)

**Automated Command Execution**:
- Time-zone aware scheduling (Europe/Paris by default)
- Per-minute execution cycle checks current time against all schedules
- Finds appropriate command for current day and time slot
- Executes LoRaWAN commands to devices via cloud provider
- Batch command execution to all devices in affected layers
- Respects day-off periods with default commands

**Job Workflow**:
1. Refresh data from APIs (devices, layers, plannings, schedules)
2. Build planning command database with device associations
3. Evaluate active plannings based on current date
4. Build command schedules for each day of the week
5. Handle day-off periods with planning-specific commands
6. Find current time slot in schedule based on Europe/Paris timezone
7. Execute appropriate command to all devices in affected layers

This feature provides critical business value by:
- Automating repetitive device control operations (reduce manual effort by 90%+)
- Enabling smart city applications (streetlights, traffic control, environmental sensors)
- Supporting energy optimization (automated HVAC, lighting based on schedules)
- Facilitating industrial automation (scheduled machine operations, maintenance windows)
- Reducing operational costs through intelligent device control
- Ensuring consistent device behavior across large fleets
- Supporting compliance with operational policies (operating hours, energy regulations)
- Enabling seasonal and calendar-based automation (holidays, DST transitions)
- Providing audit trail for automated command execution
- Reducing human error in manual device control operations

The planning system uses a hierarchical model: Devices belong to Layers, Layers have Plannings, Plannings contain Schedules, and Schedules specify Commands. This multi-level architecture enables flexible automation rules that can be applied to device groups with inheritance, override capabilities, and reusable planning templates.

---

## Code Locations

### Entry Points / Job Definition

- `src/IoTHub.Portal.Infrastructure/Jobs/SendPlanningCommandJob.cs` (Lines 1-259)
  - **Snippet**: Main job for executing scheduled commands
    ```csharp
    [DisallowConcurrentExecution]
    public class SendPlanningCommandJob : IJob
    {
        private readonly IDeviceService<DeviceDetails> deviceService;
        private readonly ILayerService layerService;
        private readonly IPlanningService planningService;
        private readonly IScheduleService scheduleService;
        private readonly ILoRaWANCommandService loRaWANCommandService;
        
        // Internal data structures
        private readonly List<PlanningCommand> planningCommands = new List<PlanningCommand>();
        public PaginatedResult<DeviceListItem> devices { get; set; }
        public IEnumerable<LayerDto> layers { get; set; }
        public IEnumerable<PlanningDto> plannings { get; set; }
        public IEnumerable<ScheduleDto> schedules { get; set; }
        
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                this.logger.LogInformation("Start of send planning commands job");
                
                await DoWork(this.cancellationTokenSource.Token);
                
                this.logger.LogInformation("End of send planning commands job");
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Send planning commands job has failed");
            }
        }
        
        private async Task DoWork(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested) return;
            
            try
            {
                this.planningCommands.Clear();
                await UpdateAPI();      // Fetch latest data
                UpdateDatabase();       // Build planning database
                await SendCommand();    // Execute commands
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Send planning command has failed");
            }
        }
    }
    ```

  - **Snippet**: Data refresh from APIs
    ```csharp
    public async Task UpdateAPI()
    {
        try
        {
            // Fetch all relevant data from services
            devices = await this.deviceService.GetDevices(pageSize: 10000);
            layers = await this.layerService.GetLayers();
            plannings = await this.planningService.GetPlannings();
            schedules = await this.scheduleService.GetSchedules();
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Update API has failed");
        }
    }
    ```

  - **Snippet**: Planning database construction
    ```csharp
    public void UpdateDatabase()
    {
        // Build device-to-planning associations
        foreach (var device in this.devices.Data)
        {
            if (!string.IsNullOrWhiteSpace(device.LayerId))
                AddNewDevice(device);
        }
    }
    
    public void AddNewDevice(DeviceListItem device)
    {
        var layer = layers.FirstOrDefault(layer => layer.Id == device.LayerId);
        
        if (layer?.Planning is not null and not "None")
        {
            // Find existing planning or create new one
            foreach (var planning in this.planningCommands
                .Where(planning => planning.planningId == layer.Planning))
            {
                planning.listDeviceId.Add(device.DeviceID);
                return;
            }
            
            // Create new planning command structure
            var newPlanning = new PlanningCommand(device.DeviceID, layer.Planning);
            AddCommand(newPlanning);
            this.planningCommands.Add(newPlanning);
        }
    }
    ```

  - **Snippet**: Schedule command building
    ```csharp
    public void AddCommand(PlanningCommand planningCommand)
    {
        var planningData = plannings.FirstOrDefault(
            planning => planning.Id == planningCommand.planningId);
        
        // Only process active plannings
        if (planningData != null && IsPlanningActive(planningData))
        {
            // Add day-off commands to planning
            addPlanningSchedule(planningData, planningCommand);
            
            // Add all schedules associated with this planning
            foreach (var schedule in schedules)
            {
                if (schedule.PlanningId == planningCommand.planningId)
                    addSchedule(schedule, planningCommand);
            }
        }
    }
    
    private bool IsPlanningActive(PlanningDto planning)
    {
        var startDay = DateTime.ParseExact(planning.Start, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var endDay = DateTime.ParseExact(planning.End, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        
        return DateTime.Now >= startDay && DateTime.Now <= endDay;
    }
    ```

  - **Snippet**: Day-off command handling
    ```csharp
    public void addPlanningSchedule(PlanningDto planningData, PlanningCommand planning)
    {
        if (planningData != null)
        {
            // Add day-off commands to specified days
            foreach (var key in planning.commands.Keys)
            {
                // Check if this day is marked as day-off
                if ((planningData.DayOff & key) == planningData.DayOff)
                {
                    var newPayload = new PayloadCommand(
                        getTimeSpan("0:00"),    // All day
                        getTimeSpan("24:00"),   // All day
                        planningData.CommandId);
                    planning.commands[key].Add(newPayload);
                }
            }
        }
    }
    ```

  - **Snippet**: Regular schedule addition
    ```csharp
    public void addSchedule(ScheduleDto schedule, PlanningCommand planning)
    {
        var start = getTimeSpan(schedule.Start);
        var end = getTimeSpan(schedule.End);
        
        foreach (var key in planning.commands.Keys)
        {
            // Skip if day already has day-off command
            if (planning.commands[key].Count == 0)
            {
                var newPayload = new PayloadCommand(start, end, schedule.CommandId);
                planning.commands[key].Add(newPayload);
            }
            // Only add if not a day-off schedule (0:00-24:00)
            else if (planning.commands[key][0].start != getTimeSpan("00:00") || 
                     planning.commands[key][0].end != getTimeSpan("24:00"))
            {
                var newPayload = new PayloadCommand(start, end, schedule.CommandId);
                planning.commands[key].Add(newPayload);
            }
        }
    }
    ```

  - **Snippet**: Time-based command execution
    ```csharp
    public async Task SendCommand()
    {
        // Use Europe/Paris timezone for scheduling
        var timeZoneId = "Europe/Paris";
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var currentTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZone);
        
        var currentDay = currentTime.DayOfWeek;
        var currentHour = currentTime.TimeOfDay;
        
        // Search for the appropriate command at the correct time
        foreach (var planning in this.planningCommands)
        {
            foreach (var schedule in planning.commands[DayConverter.Convert(currentDay)])
            {
                // Check if current time falls within schedule window
                if (schedule.start < currentHour && schedule.end > currentHour)
                {
                    await SendDevicesCommand(planning.listDeviceId, schedule.payloadId);
                }
            }
        }
    }
    
    public async Task SendDevicesCommand(Collection<string> devices, string command)
    {
        // Execute command on all devices in the layer
        foreach (var device in devices)
            await loRaWANCommandService.ExecuteLoRaWANCommand(device, command);
    }
    ```

### Internal Data Structures

- `src/IoTHub.Portal.Infrastructure/Jobs/SendPlanningCommandJob.cs` (Lines 6-36)
  - **Snippet**: Planning command data structures
    ```csharp
    public class PlanningCommand
    {
        public string planningId { get; set; } = default!;
        public Collection<string> listDeviceId { get; } = new Collection<string>();
        public Dictionary<DaysEnumFlag.DaysOfWeek, List<PayloadCommand>> commands { get; } 
            = new Dictionary<DaysEnumFlag.DaysOfWeek, List<PayloadCommand>>();
        
        public PlanningCommand(string listDeviceId, string planningId)
        {
            this.planningId = planningId;
            this.listDeviceId.Add(listDeviceId);
            
            // Initialize command lists for all days of the week
            foreach (DaysEnumFlag.DaysOfWeek day in Enum.GetValues(typeof(DaysEnumFlag.DaysOfWeek)))
            {
                commands.Add(day, new List<PayloadCommand>());
            }
        }
    }
    
    public class PayloadCommand
    {
        public string payloadId { get; set; } = default!;
        public TimeSpan start { get; set; } = default!;
        public TimeSpan end { get; set; } = default!;
        
        public PayloadCommand(TimeSpan start, TimeSpan end, string payloadId)
        {
            this.payloadId = payloadId;
            this.start = start;
            this.end = end;
        }
    }
    ```

### Job Configuration & Scheduling

#### Azure Configuration

- `src/IoTHub.Portal.Infrastructure/Startup/AzureServiceCollectionExtension.cs` (Lines 169-180)
  - **Snippet**: Azure planning command job configuration
    ```csharp
    private static IServiceCollection ConfigureSendingCommands(
        this IServiceCollection services, ConfigHandler configuration)
    {
        return services.AddQuartz(q =>
        {
            _ = q.AddJob<SendPlanningCommandJob>(j => j.WithIdentity(nameof(SendPlanningCommandJob)))
                .AddTrigger(t => t
                    .WithIdentity($"{nameof(SendPlanningCommandJob)}")
                    .ForJob(nameof(SendPlanningCommandJob))
                    .WithSimpleSchedule(s => s
                        .WithIntervalInMinutes(configuration.SendCommandsToDevicesIntervalInMinutes)
                        .RepeatForever()));
        });
    }
    ```

#### AWS Configuration

- `src/IoTHub.Portal.Infrastructure/Startup/AWSServiceCollectionExtension.cs` (Lines 114-126)
  - **Snippet**: AWS planning command job configuration
    ```csharp
    private static IServiceCollection ConfigureAWSSendingCommands(
        this IServiceCollection services, ConfigHandler configuration)
    {
        return services.AddQuartz(q =>
        {
            _ = q.AddJob<SendPlanningCommandJob>(j => j.WithIdentity(nameof(SendPlanningCommandJob)))
                .AddTrigger(t => t
                    .WithIdentity($"{nameof(SendPlanningCommandJob)}")
                    .ForJob(nameof(SendPlanningCommandJob))
                    .WithSimpleSchedule(s => s
                        .WithIntervalInMinutes(configuration.SendCommandsToDevicesIntervalInMinutes)
                        .RepeatForever()));
        });
    }
    ```

### Business Logic & Services

- `src/IoTHub.Portal.Application/Services/IDeviceService<T>.cs` (Interface)
  - Generic device management service
  - Method: `GetDevices(pageSize: int)` - Retrieve device list with pagination
  - Returns: `PaginatedResult<DeviceListItem>` with device metadata

- `src/IoTHub.Portal.Application/Services/ILayerService.cs` (Interface)
  - Layer management service for device organization
  - Method: `GetLayers()` - Retrieve all layers with planning associations
  - Returns: `IEnumerable<LayerDto>` with layer hierarchy and planning IDs

- `src/IoTHub.Portal.Application/Services/IPlanningService.cs` (Lines 1-15)
  - **Snippet**: Planning management service
    ```csharp
    public interface IPlanningService
    {
        Task<PlanningDto> CreatePlanning(PlanningDto planning);
        Task UpdatePlanning(PlanningDto planning);
        Task DeletePlanning(string planningId);
        Task<Planning> GetPlanning(string planningId);
        Task<IEnumerable<PlanningDto>> GetPlannings();
    }
    ```

- `src/IoTHub.Portal.Application/Services/IScheduleService.cs` (Interface)
  - Schedule management service for time-based rules
  - Method: `GetSchedules()` - Retrieve all schedules with time ranges
  - Returns: `IEnumerable<ScheduleDto>` with start/end times and command IDs

- `src/IoTHub.Portal.Application/Services/ILoRaWANCommandService.cs` (Lines 1-13)
  - **Snippet**: LoRaWAN command execution service
    ```csharp
    public interface ILoRaWANCommandService
    {
        Task<DeviceModelCommandDto[]> GetDeviceModelCommandsFromModel(string id);
        Task PostDeviceModelCommands(string id, DeviceModelCommandDto[] commands);
        Task ExecuteLoRaWANCommand(string deviceId, string commandId);
    }
    ```

### Data Models

- `src/IoTHub.Portal.Shared/Models/v1.0/LayerDto.cs` (Lines 1-33)
  - **Snippet**: Layer DTO for device organization
    ```csharp
    public class LayerDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string? Father { get; set; }        // Parent layer for hierarchy
        public string Planning { get; set; }       // Associated planning ID
        public bool hasSub { get; set; }           // Has sub-layers
    }
    ```

- `src/IoTHub.Portal.Shared/Models/v1.0/PlanningDto.cs` (Lines 1-43)
  - **Snippet**: Planning DTO for automation rules
    ```csharp
    public class PlanningDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Start { get; set; }                    // Start date (yyyy-MM-dd)
        public string End { get; set; }                      // End date (yyyy-MM-dd)
        public bool Frequency { get; set; }                  // Repeat frequency
        public DaysEnumFlag.DaysOfWeek DayOff { get; set; }  // Day-off pattern
        public string CommandId { get; set; }                // Day-off command
    }
    ```

- `src/IoTHub.Portal.Shared/Models/v1.0/ScheduleDto.cs` (Lines 1-31)
  - **Snippet**: Schedule DTO for time-based commands
    ```csharp
    public class ScheduleDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Start { get; set; }        // Start time (HH:mm)
        public string End { get; set; }          // End time (HH:mm)
        public string CommandId { get; set; }    // Command to execute
        public string PlanningId { get; set; }   // Associated planning
    }
    ```

- `src/IoTHub.Portal.Shared/Models/v1.0/DeviceListItem.cs` (DTO)
  - Device list item with layer association
  - Properties: `DeviceID`, `LayerId`, `Name`, `Status`, etc.

### Domain Entities

- `src/IoTHub.Portal.Domain/Entities/Layer.cs`
  - Layer entity for database persistence
  - Properties: `Id`, `Name`, `Father`, `PlanningId`
  - Navigation: Parent/child layer relationships

- `src/IoTHub.Portal.Domain/Entities/Planning.cs`
  - Planning entity with date ranges and day-off patterns
  - Properties: `Id`, `Name`, `Start`, `End`, `DayOff`, `CommandId`

- `src/IoTHub.Portal.Domain/Entities/Schedule.cs`
  - Schedule entity with time ranges
  - Properties: `Id`, `Start`, `End`, `CommandId`, `PlanningId`

### Shared Enums & Utilities

- `src/IoTHub.Portal.Shared/Models/v1.0/DaysEnumFlag.cs`
  - **Snippet**: Day-of-week flag enum
    ```csharp
    [Flags]
    public enum DaysOfWeek
    {
        Monday = 1,
        Tuesday = 2,
        Wednesday = 4,
        Thursday = 8,
        Friday = 16,
        Saturday = 32,
        Sunday = 64
    }
    ```

- `src/IoTHub.Portal.Infrastructure/Helpers/DayConverter.cs`
  - Utility to convert .NET DayOfWeek to DaysEnumFlag
  - Used for matching current day to schedule day patterns

---

## Dependencies

### Internal Dependencies
- **Application Layer**:
  - IDeviceService<T> for device list retrieval
  - ILayerService for layer and planning associations
  - IPlanningService for planning definitions
  - IScheduleService for schedule time ranges
  - ILoRaWANCommandService for command execution

- **Domain Layer**:
  - Layer, Planning, Schedule entities
  - DaysEnumFlag enum for day-of-week patterns
  - Device domain models

- **Infrastructure Layer**:
  - Quartz.NET job scheduler
  - ConfigHandler for job intervals
  - DayConverter utility for day matching
  - Logging infrastructure (ILogger<T>)

### External Dependencies
- **Quartz.NET**:
  - `Quartz` (v3.x) - Job scheduling framework
  - `[DisallowConcurrentExecution]` attribute
  - IJob interface and IJobExecutionContext
  - Simple schedule triggers with minute intervals

- **.NET Framework**:
  - `TimeZoneInfo` - Timezone-aware date/time handling
  - `CultureInfo.InvariantCulture` - Date parsing
  - `CancellationTokenSource` - Job cancellation support
  - Collection types (List, Dictionary, Collection)

### Configuration Dependencies
- **Settings**:
  - `SendCommandsToDevicesIntervalInMinutes` - Job execution frequency (default: 1)
  - Timezone configuration (hardcoded: Europe/Paris)
  - Device page size (hardcoded: 10000)

- **Environment Variables**:
  - LoRaWAN network server connection string
  - Cloud provider credentials for command execution
  - Database connection string for planning data

### Feature Dependencies
- **Depends on**:
  - Layer Management feature (device organization)
  - Planning Management feature (automation rules)
  - Schedule Management feature (time-based rules)
  - Device Management feature (device list and metadata)
  - LoRaWAN Command feature (command execution)
  - Device Synchronization Jobs (accurate device data)

- **Used by**:
  - Smart city automation (streetlight control, traffic management)
  - Industrial automation (scheduled operations)
  - Energy management (HVAC, lighting optimization)
  - Agricultural automation (irrigation scheduling)
  - Building automation (access control, climate control)

---

## Related Features
- **023-background-job-management** - Job scheduling infrastructure
- **012-lorawan-device-management** - LoRaWAN device and command management
- **001-device-management** - Device inventory and metadata
- **Layer Management** (TBD) - Hierarchical device organization
- **Planning Management** (TBD) - Automation rule definitions
- **Schedule Management** (TBD) - Time-based schedule configuration

---

## Notes
- Job executes every minute by default (`SendCommandsToDevicesIntervalInMinutes=1`)
- Uses `[DisallowConcurrentExecution]` to prevent overlapping executions
- Timezone is hardcoded to "Europe/Paris" - may need configuration for multi-region deployments
- Device page size is hardcoded to 10000 - may cause memory issues with larger fleets
- Day-off commands take precedence over regular schedules (0:00-24:00 time range)
- Schedule time windows are inclusive on start, exclusive on end (start < currentTime < end)
- Planning dates use "yyyy-MM-dd" format for start/end validation
- Schedule times use "HH:mm" format for time parsing
- Commands execute to all devices in a layer simultaneously (no batching or rate limiting)
- No retry mechanism for failed command executions
- Job failure doesn't stop execution - comprehensive error logging for troubleshooting
- Planning database is rebuilt from scratch on each execution (no caching)
- Active planning check uses current system time, not timezone-aware time
- Supports multiple schedules per day with different time ranges
- Day-of-week enum uses flags for efficient day-off pattern matching
- No audit trail for executed commands (consider adding for compliance)
- Layer hierarchy is loaded but only top-level planning associations are used
- Device-to-layer associations are required for planning participation
- Commands are LoRaWAN-specific - feature not applicable to standard IoT devices
