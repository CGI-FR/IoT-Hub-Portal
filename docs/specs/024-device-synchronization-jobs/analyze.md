# Feature: Device Synchronization Jobs

**Category**: Background Jobs & Data Synchronization  
**Status**: Analyzed  

---

## Description

The Device Synchronization Jobs feature provides a comprehensive background job system that maintains consistency between cloud provider device registries (Azure IoT Hub, AWS IoT Core) and the portal's local database. This critical infrastructure ensures that the portal's device inventory accurately reflects the real-time state of devices in the cloud, enabling administrators to manage and monitor their IoT fleet effectively without manual synchronization.

Key capabilities include:

**Azure IoT Hub Synchronization:**
- **SyncDevicesJob**: Synchronizes regular IoT devices (including LoRaWAN devices) from Azure IoT Hub
  - Retrieves all device twins excluding LoRa Concentrators
  - Creates or updates devices based on model type (LoRaWAN vs. standard)
  - Handles device tag synchronization and versioning
  - Removes devices deleted from Azure IoT Hub
  - Validates device model existence before import

- **SyncEdgeDeviceJob**: Synchronizes Azure IoT Edge devices with module configurations
  - Retrieves all edge device twins from Azure IoT Hub
  - Fetches edge module and edgeHub client information
  - Synchronizes connection states (Connected/Disconnected)
  - Updates device tags with proper versioning
  - Cleans up deleted edge devices from local database

- **SyncConcentratorsJob**: Synchronizes LoRa concentrator devices (gateways)
  - Filters devices by "LoRa Concentrator" device type
  - Maintains concentrator inventory for LoRaWAN network management
  - Updates concentrator firmware and configuration data
  - Removes decommissioned concentrators

- **SyncGatewayIDJob**: Maintains a synchronized list of LoRa Gateway IDs
  - Updates in-memory gateway ID list for LoRaWAN operations
  - Provides fast gateway validation without database queries
  - Critical for LoRaWAN message routing and network operations

**AWS IoT Core Synchronization:**
- **SyncThingsJob**: Synchronizes AWS IoT Things as devices or edge devices
  - Retrieves all things from AWS IoT Core
  - Determines device type (edge vs. standard) via thing shadow analysis
  - Maps AWS Thing Types to device models
  - Synchronizes AWS Greengrass Core device status
  - Validates thing shadows for standard devices
  - Removes things deleted from AWS

- **SyncThingTypesJob**: Synchronizes AWS IoT Thing Types as device models
  - Lists and describes all thing types from AWS IoT Core
  - Filters to standard devices (non-edge)
  - Manages deprecated thing types with 5-minute grace period
  - Creates/updates device models with default images
  - Removes deleted or deprecated thing types
  - Handles associated dynamic thing group cleanup

- **SyncGreenGrassDeploymentsJob**: Synchronizes AWS Greengrass V2 deployments as edge device models
  - Lists all Greengrass deployments from AWS
  - Filters deployments associated with dynamic thing groups
  - Creates edge device models from deployment metadata
  - Assigns default device model images
  - Removes deployments deleted from AWS
  - Handles deployments with missing names

This feature provides critical business value by:
- Maintaining real-time device inventory accuracy across multi-cloud environments
- Enabling centralized device management without cloud console access
- Supporting automated device lifecycle management
- Reducing manual synchronization effort and human error
- Providing consistent device state for monitoring and reporting
- Supporting disaster recovery with automatic resynchronization
- Enabling multi-cloud IoT infrastructure management
- Facilitating compliance auditing with accurate device records

The synchronization system uses Quartz.NET for reliable job scheduling with configurable intervals, implements optimistic concurrency with version tracking, handles pagination for large device fleets, and provides comprehensive error logging for troubleshooting. All jobs use the `[DisallowConcurrentExecution]` attribute to prevent race conditions during synchronization.

---

## Code Locations

### Entry Points / Job Definitions

#### Azure Jobs

- `src/IoTHub.Portal.Infrastructure/Jobs/SyncDevicesJob.cs` (Lines 1-165)
  - **Snippet**: Main job for synchronizing standard IoT devices
    ```csharp
    [DisallowConcurrentExecution]
    public class SyncDevicesJob : IJob
    {
        private readonly IExternalDeviceService externalDeviceService;
        private readonly IDeviceModelRepository deviceModelRepository;
        private readonly ILorawanDeviceRepository lorawanDeviceRepository;
        private readonly IDeviceRepository deviceRepository;
        
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                this.logger.LogInformation("Start of sync devices job");
                await SyncDevices();
                this.logger.LogInformation("End of sync devices job");
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Sync devices job has failed");
            }
        }
        
        private async Task SyncDevices()
        {
            var deviceTwins = await GetTwinDevices(); // Paginated retrieval
            
            foreach (var twin in deviceTwins)
            {
                // Validate model existence and support LoRaWAN
                if (deviceModel.SupportLoRaFeatures)
                    await CreateOrUpdateLorawanDevice(twin);
                else
                    await CreateOrUpdateDevice(twin);
            }
            
            // Remove devices deleted from Azure
            foreach (var item in (await this.deviceRepository.GetAllAsync())
                .Where(device => !deviceTwins.Exists(x => x.DeviceId == device.Id)))
            {
                this.deviceRepository.Delete(item.Id);
            }
        }
    }
    ```

- `src/IoTHub.Portal.Infrastructure/Jobs/SyncEdgeDeviceJob.cs` (Lines 1-151)
  - **Snippet**: Job for synchronizing Azure IoT Edge devices
    ```csharp
    [DisallowConcurrentExecution]
    public class SyncEdgeDeviceJob : IJob
    {
        private readonly IExternalDeviceService externalDeviceService;
        private readonly IEdgeDeviceRepository edgeDeviceRepository;
        private readonly IEdgeDevicesService edgeDevicesService;
        
        private async Task CreateOrUpdateDevice(Twin twin)
        {
            var twinWithModule = await this.externalDeviceService.GetDeviceTwinWithModule(twin.DeviceId);
            var twinWithClient = await this.externalDeviceService.GetDeviceTwinWithEdgeHubModule(twin.DeviceId);
            
            var connectionState = twin.ConnectionState == DeviceConnectionState.Connected 
                ? "Connected" : "Disconnected";
            
            var device = this.mapper.Map<EdgeDevice>(twin, opts =>
            {
                opts.Items["TwinModules"] = twinWithModule;
                opts.Items["TwinClient"] = twinWithClient;
            });
            
            device.ConnectionState = connectionState;
            
            // Version-based optimistic concurrency
            if (deviceEntity.Version >= device.Version) return;
        }
    }
    ```

- `src/IoTHub.Portal.Infrastructure/Jobs/SyncConcentratorsJob.cs` (Lines 1-101)
  - Synchronizes LoRa Concentrator devices from Azure IoT Hub
  - Filters by deviceType: "LoRa Concentrator"
  - Handles version-based updates with optimistic concurrency
  - Removes concentrators deleted from cloud

- `src/IoTHub.Portal.Infrastructure/Jobs/SyncGatewayIDJob.cs` (Lines 1-45)
  - **Snippet**: Updates in-memory gateway ID list
    ```csharp
    [DisallowConcurrentExecution]
    public class SyncGatewayIDJob : IJob
    {
        private readonly IExternalDeviceService externalDeviceService;
        private readonly LoRaGatewayIDList gatewayIdList;
        
        private async Task SyncGatewayID()
        {
            this.gatewayIdList.GatewayIds = await this.externalDeviceService.GetAllGatewayID();
        }
    }
    ```

#### AWS Jobs

- `src/IoTHub.Portal.Infrastructure/Jobs/AWS/SyncThingsJob.cs` (Lines 1-251)
  - **Snippet**: Synchronizes AWS IoT Things as devices/edge devices
    ```csharp
    [DisallowConcurrentExecution]
    public class SyncThingsJob : IJob
    {
        private readonly IAmazonIoT amazonIoTClient;
        private readonly IAmazonIotData amazonIoTDataClient;
        private readonly IAmazonGreengrassV2 amazonGreenGrass;
        private readonly IDeviceRepository deviceRepository;
        private readonly IEdgeDeviceRepository edgeDeviceRepository;
        
        private async Task SyncThingsAsDevices()
        {
            var things = await this.externalDeviceService.GetAllThing();
            
            foreach (var thing in things)
            {
                // Validate thing has ThingType
                if (thing.ThingTypeName.IsNullOrWhiteSpace()) continue;
                
                // Determine if edge device via shadow analysis
                bool? isEdge = await externalDeviceService.IsEdgeDeviceModel(
                    this.mapper.Map<ExternalDeviceModelDto>(thing));
                
                if (isEdge == true)
                {
                    // Sync as EdgeDevice with Greengrass status
                    var coreDevice = await amazonGreenGrass.GetCoreDeviceAsync(
                        new GetCoreDeviceRequest() { CoreDeviceThingName = thing.ThingName });
                    edgeDevice.ConnectionState = coreDevice.Status == CoreDeviceStatus.HEALTHY 
                        ? "Connected" : "Disconnected";
                }
                else
                {
                    // Sync as standard Device with shadow validation
                    var thingShadow = await this.amazonIoTDataClient.GetThingShadowAsync(
                        new GetThingShadowRequest() { ThingName = thing.ThingName });
                }
            }
            
            // Delete devices not in AWS
            foreach (var item in (await this.deviceRepository.GetAllAsync(
                device => !things.Select(x => x.ThingId).Contains(device.Id))))
            {
                this.deviceRepository.Delete(item.Id);
            }
        }
    }
    ```

- `src/IoTHub.Portal.Infrastructure/Jobs/AWS/SyncThingTypesJob.cs` (Lines 1-203)
  - **Snippet**: Synchronizes AWS Thing Types as device models
    ```csharp
    [DisallowConcurrentExecution]
    public class SyncThingTypesJob : IJob
    {
        private readonly IDeviceModelRepository deviceModelRepository;
        private readonly IAmazonIoT amazonIoTClient;
        private readonly IDeviceModelImageManager deviceModelImageManager;
        
        private async Task SyncThingTypesAsDeviceModels()
        {
            var thingTypes = await GetAllThingTypes();
            
            foreach (var thingType in thingTypes)
            {
                var isEdge = await externalDeviceService.IsEdgeDeviceModel(
                    this.mapper.Map<ExternalDeviceModelDto>(thingType));
                
                // Only sync non-edge device models
                if (isEdge == false)
                {
                    await CreateOrUpdateDeviceModel(thingType);
                }
            }
            
            // Delete deprecated or removed thing types
            await DeleteThingTypes(thingTypes);
        }
        
        private async Task<List<DescribeThingTypeResponse>> Remove5mnDeprecatedThingTypes(
            List<DescribeThingTypeResponse> thingTypes)
        {
            // Cleanup deprecated thing types after 5-minute grace period
            foreach (var thingType in thingTypes)
            {
                var diffInMinutes = DateTime.Now.Subtract(
                    thingType.ThingTypeMetadata.DeprecationDate).TotalMinutes;
                if (thingType.ThingTypeMetadata.Deprecated && diffInMinutes > 5)
                {
                    await this.amazonIoTClient.DeleteThingTypeAsync(...);
                    await this.amazonIoTClient.DeleteDynamicThingGroupAsync(...);
                }
            }
        }
    }
    ```

- `src/IoTHub.Portal.Infrastructure/Jobs/AWS/SyncGreenGrassDeploymentsJob.cs` (Lines 1-159)
  - **Snippet**: Synchronizes Greengrass deployments as edge models
    ```csharp
    [DisallowConcurrentExecution]
    public class SyncGreenGrassDeploymentsJob : IJob
    {
        private readonly IEdgeDeviceModelRepository edgeDeviceModelRepository;
        private readonly IAmazonGreengrassV2 amazonGreenGrass;
        private readonly IAmazonIoT amazonIoTClient;
        
        private async Task GetAllGreenGrassDeployments()
        {
            var deployments = new List<IoTEdgeModel>();
            
            var response = await this.amazonGreenGrass.ListDeploymentsAsync(
                new ListDeploymentsRequest { 
                    HistoryFilter = DeploymentHistoryFilter.LATEST_ONLY 
                });
            
            foreach (var deployment in response.Deployments)
            {
                // Extract thing group from target ARN
                var awsThingGroupRegex = new Regex(@"/([^/]+)$");
                var matches = awsThingGroupRegex.Match(deployment.TargetArn);
                
                // Verify it's a dynamic thing group (has query string)
                var thingGroup = await this.amazonIoTClient.DescribeThingGroupAsync(...);
                if (thingGroup.QueryString != null)
                {
                    deployments.Add(new IoTEdgeModel
                    {
                        ModelId = deployment.DeploymentId,
                        Name = deployment.DeploymentName ?? deployment.DeploymentId
                    });
                }
            }
        }
    }
    ```

### Job Configuration & Scheduling

- `src/IoTHub.Portal.Infrastructure/Startup/AzureServiceCollectionExtension.cs` (Lines 122-167)
  - **Snippet**: Azure job configuration with Quartz.NET
    ```csharp
    private static IServiceCollection ConfigureSyncJobs(
        this IServiceCollection services, ConfigHandler configuration)
    {
        return services.AddQuartz(q =>
        {
            _ = q.AddJob<SyncDevicesJob>(j => j.WithIdentity(nameof(SyncDevicesJob)))
                .AddTrigger(t => t
                    .WithIdentity($"{nameof(SyncDevicesJob)}")
                    .ForJob(nameof(SyncDevicesJob))
                    .WithSimpleSchedule(s => s
                        .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                        .RepeatForever()));
            
            _ = q.AddJob<SyncEdgeDeviceJob>(j => j.WithIdentity(nameof(SyncEdgeDeviceJob)))
                .AddTrigger(t => t
                    .WithSimpleSchedule(s => s
                        .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                        .RepeatForever()));
            
            // LoRaWAN jobs only enabled when IsLoRaEnabled = true
            if (configuration.IsLoRaEnabled)
            {
                _ = q.AddJob<SyncConcentratorsJob>(...)
                    .AddTrigger(t => t.WithSimpleSchedule(s => s
                        .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                        .RepeatForever()));
                
                _ = q.AddJob<SyncGatewayIDJob>(...)
                    .AddTrigger(t => t.WithSimpleSchedule(s => s
                        .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                        .RepeatForever()));
                
                _ = q.AddJob<SyncLoRaDeviceTelemetryJob>(...)
                    .AddTrigger(t => t.StartAt(DateTimeOffset.Now.AddMinutes(1)));
            }
        });
    }
    ```

- `src/IoTHub.Portal.Infrastructure/Startup/AWSServiceCollectionExtension.cs` (Lines 60-112)
  - **Snippet**: AWS job configuration
    ```csharp
    private static IServiceCollection ConfigureAWSSyncJobs(
        this IServiceCollection services, ConfigHandler configuration)
    {
        return services.AddQuartz(q =>
        {
            _ = q.AddJob<SyncThingTypesJob>(j => j.WithIdentity(nameof(SyncThingTypesJob)))
                .AddTrigger(t => t.WithSimpleSchedule(s => s
                    .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                    .RepeatForever()));
            
            _ = q.AddJob<SyncThingsJob>(j => j.WithIdentity(nameof(SyncThingsJob)))
                .AddTrigger(t => t.WithSimpleSchedule(s => s
                    .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                    .RepeatForever()));
            
            _ = q.AddJob<SyncGreenGrassDeploymentsJob>(...)
                .AddTrigger(t => t.WithSimpleSchedule(s => s
                    .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                    .RepeatForever()));
        });
    }
    ```

### Business Logic & Services

- `src/IoTHub.Portal.Application/Services/IExternalDeviceService.cs` (Interface)
  - Core abstraction for cloud provider device operations
  - Methods: `GetAllDevice()`, `GetAllEdgeDevice()`, `GetAllThing()`, `GetAllGatewayID()`
  - Returns paginated results with continuation tokens
  - Provides device twin retrieval with module information

- `src/IoTHub.Portal.Infrastructure/Services/AzureExternalDeviceService.cs`
  - Azure IoT Hub implementation of IExternalDeviceService
  - Uses Azure IoT Hub SDK for device twin operations
  - Implements pagination with continuation tokens
  - Handles device filtering by type and tags

- `src/IoTHub.Portal.Infrastructure/Services/AWS/AWSExternalDeviceService.cs`
  - AWS IoT Core implementation of IExternalDeviceService
  - Uses AWS SDK for IoT, IoT Data, and Greengrass V2
  - Implements thing enumeration and shadow retrieval
  - Handles edge device detection via shadow analysis

### Data Access

- `src/IoTHub.Portal.Domain/Repositories/IDeviceRepository.cs`
  - Repository interface for Device entity operations
  - Methods: `GetAllAsync()`, `GetByIdAsync()`, `InsertAsync()`, `Update()`, `Delete()`
  - Supports query expressions for filtering

- `src/IoTHub.Portal.Domain/Repositories/ILorawanDeviceRepository.cs`
  - Repository interface for LorawanDevice entity operations
  - Extends IRepository<LorawanDevice>
  - Supports tag relationship loading

- `src/IoTHub.Portal.Domain/Repositories/IEdgeDeviceRepository.cs`
  - Repository interface for EdgeDevice entity operations
  - Supports module and tag relationship loading
  - Handles connection state updates

- `src/IoTHub.Portal.Domain/Repositories/IConcentratorRepository.cs`
  - Repository interface for Concentrator entity operations
  - LoRaWAN gateway-specific device management

- `src/IoTHub.Portal.Domain/Repositories/IDeviceModelRepository.cs`
  - Repository interface for DeviceModel entity operations
  - Methods: `GetByIdAsync()`, `GetByNameAsync()`
  - Used for model validation during sync

- `src/IoTHub.Portal.Domain/Repositories/IEdgeDeviceModelRepository.cs`
  - Repository interface for EdgeDeviceModel entity operations
  - Methods: `GetByNameAsync()` for AWS deployment mapping

- `src/IoTHub.Portal.Domain/Repositories/IDeviceTagValueRepository.cs`
  - Repository interface for DeviceTagValue entity operations
  - Handles tag cleanup during device updates

### Data Models

- `src/IoTHub.Portal.Domain/Entities/Device.cs`
  - Standard device entity with properties:
    - `Id`, `Name`, `DeviceModelId`, `Version`, `IsEnabled`, `ConnectionState`
    - Navigation: `Tags`, `Labels`

- `src/IoTHub.Portal.Domain/Entities/LorawanDevice.cs`
  - LoRaWAN device entity extending Device
  - Additional LoRaWAN-specific properties

- `src/IoTHub.Portal.Domain/Entities/EdgeDevice.cs`
  - Edge device entity with properties:
    - `Id`, `Name`, `DeviceModelId`, `Version`, `ConnectionState`
    - Navigation: `Tags`, `Labels`, `Modules`

- `src/IoTHub.Portal.Domain/Entities/Concentrator.cs`
  - LoRa Concentrator (gateway) entity
  - Properties: `Id`, `Name`, `Version`, `LoraRegion`, `ClientThumbprint`

- `src/IoTHub.Portal.Domain/Entities/DeviceModel.cs`
  - Device model entity with properties:
    - `Id`, `Name`, `Description`, `SupportLoRaFeatures`

- `src/IoTHub.Portal.Domain/Entities/EdgeDeviceModel.cs`
  - Edge device model entity
  - Properties: `Id`, `Name`, `Description`, `ExternalIdentifier`

### Shared Models

- `src/IoTHub.Portal.Shared/Models/v1.0/LoRaGatewayIDList.cs`
  - In-memory singleton for gateway ID list
  - Properties: `GatewayIds` (List<string>)
  - Updated by SyncGatewayIDJob for fast lookups

---

## Dependencies

### Internal Dependencies
- **Domain Layer**:
  - Entity models (Device, EdgeDevice, Concentrator, LorawanDevice)
  - Repository interfaces
  - IUnitOfWork for transaction management
  - Domain exceptions (InternalServerErrorException)

- **Application Layer**:
  - IExternalDeviceService for cloud provider abstraction
  - IEdgeDevicesService for edge module operations
  - IDeviceModelImageManager for model image management
  - IConfigService for configuration retrieval
  - AutoMapper for entity-to-DTO mapping

- **Infrastructure Layer**:
  - Quartz.NET job scheduler
  - ConfigHandler for application settings
  - Logging infrastructure (ILogger<T>)

### External Dependencies
- **Azure SDK**:
  - `Azure.IoT.Hub.Service` - Azure IoT Hub device twin operations
  - Device connection state tracking
  - Module twin retrieval for edge devices

- **AWS SDK**:
  - `AWSSDK.IoT` - Thing and ThingType management
  - `AWSSDK.IotData` - Thing shadow operations
  - `AWSSDK.GreengrassV2` - Greengrass core device status
  - HTTP status code handling for AWS API responses

- **Quartz.NET**:
  - `Quartz` (v3.x) - Job scheduling framework
  - `[DisallowConcurrentExecution]` attribute
  - IJob interface and IJobExecutionContext

- **AutoMapper**:
  - `AutoMapper` - Object-to-object mapping
  - Custom mapping profiles for cloud-to-entity conversion

- **Entity Framework Core**:
  - Database persistence via repositories
  - IUnitOfWork for transaction boundaries
  - Change tracking and concurrency management

### Configuration Dependencies
- **Settings**:
  - `SyncDatabaseJobRefreshIntervalInMinutes` - Job execution frequency (default: 5)
  - `IsLoRaEnabled` - Feature flag for LoRaWAN jobs
  - Cloud provider connection strings (IoT Hub, AWS credentials)

- **Environment Variables**:
  - Azure IoT Hub connection string
  - AWS region, access key, secret key
  - Database connection string

### Feature Dependencies
- **Depends on**:
  - Device Models feature (validates model existence)
  - Device Management feature (uses device repositories)
  - Edge Device Management (synchronizes edge modules)
  - LoRaWAN Management (when enabled, syncs concentrators and gateways)
  - Authentication & Authorization (service principal for cloud APIs)

- **Used by**:
  - Device List/Detail views (displays synchronized devices)
  - Device Monitoring dashboard (uses accurate device counts)
  - Edge Device Configuration (uses synchronized modules)
  - LoRaWAN Gateway Management (uses synchronized gateway list)
  - Metrics Collection Jobs (operates on synchronized device data)

---

## Related Features
- **023-background-job-management** - Job scheduling and monitoring infrastructure
- **001-device-management** - Device CRUD operations on synchronized data
- **002-edge-device-management** - Edge device management with synchronized modules
- **012-lorawan-device-management** - LoRaWAN device and gateway operations
- **017-device-monitoring** - Real-time monitoring of synchronized devices
- **025-metrics-collection-jobs** - Metrics based on synchronized device inventory

---

## Notes
- All synchronization jobs use `[DisallowConcurrentExecution]` to prevent overlapping executions and race conditions
- Jobs implement pagination for large device fleets (100 devices per page) to manage memory and API rate limits
- Version-based optimistic concurrency prevents data loss during simultaneous updates
- AWS jobs include 5-minute deprecation grace period for thing type cleanup to prevent accidental data loss
- LoRaWAN jobs are conditionally registered based on `IsLoRaEnabled` configuration flag
- Comprehensive error logging ensures troubleshooting visibility without job failure
- Jobs handle cloud API failures gracefully (HTTP status codes, exceptions) to maintain partial synchronization
- AWS edge device detection uses shadow analysis to distinguish between standard and edge devices
- Gateway ID synchronization uses in-memory singleton for high-performance lookups during message routing
- Default device model images are automatically assigned during AWS sync operations
