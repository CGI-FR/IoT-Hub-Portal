# Feature: Metrics Collection Jobs

**Category**: Monitoring & Observability  
**Status**: Analyzed  

---

## Description

The Metrics Collection Jobs feature provides a comprehensive background job system that continuously collects, aggregates, and exports telemetry metrics about the IoT device fleet to monitoring systems. This critical observability infrastructure enables real-time monitoring dashboards, alerting systems, and capacity planning by exposing key performance indicators (KPIs) through Prometheus-compatible metrics endpoints.

Key capabilities include:

**Metric Loader Jobs** (Data Collection):
- **DeviceMetricLoaderJob**: Collects device inventory metrics
  - Total device count across all device types
  - Connected device count (devices with active connections)
  - Retrieves metrics from cloud providers (Azure IoT Hub, AWS IoT Core)
  - Updates shared PortalMetric singleton for export
  - Error handling for cloud API failures

- **EdgeDeviceMetricLoaderJob**: Collects edge device operational metrics
  - Total edge device count (IoT Edge, Greengrass)
  - Connected edge device count (healthy edge devices)
  - Failed deployment count for edge modules
  - Multi-source data aggregation (device service + config service)
  - Supports both Azure IoT Edge and AWS Greengrass

- **ConcentratorMetricLoaderJob**: Collects LoRaWAN gateway metrics
  - Total concentrator (LoRa gateway) count
  - Critical for LoRaWAN network health monitoring
  - Provides network coverage visibility

**Metric Exporter Jobs** (Prometheus Export):
- **DeviceMetricExporterJob**: Exports device metrics to Prometheus
  - Exposes `iot_hub_portal_device_count` counter
  - Exposes `iot_hub_portal_connected_device_count` counter
  - Updates Prometheus metrics from PortalMetric singleton
  - Uses `prometheus-net` library for metric publishing

- **EdgeDeviceMetricExporterJob**: Exports edge device metrics to Prometheus
  - Exposes `iot_hub_portal_edge_device_count` counter
  - Exposes `iot_hub_portal_connected_edge_device_count` counter
  - Exposes `iot_hub_portal_failed_deployment_count` counter
  - Critical for edge infrastructure monitoring

- **ConcentratorMetricExporterJob**: Exports LoRaWAN metrics to Prometheus
  - Exposes `iot_hub_portal_concentrator_count` counter
  - Enables LoRaWAN network coverage monitoring

**Special-Purpose Jobs**:
- **SyncLoRaDeviceTelemetryJob**: Real-time LoRaWAN telemetry ingestion
  - Listens to Azure Event Hub for LoRaWAN device messages
  - Uses EventProcessorClient for scalable event processing
  - Maintains checkpoint state in Azure Blob Storage
  - Processes telemetry events via IDeviceService
  - Runs continuously until cancellation
  - Critical for real-time LoRaWAN device monitoring

This feature provides critical business value by:
- Enabling real-time operational dashboards (Grafana, Prometheus)
- Supporting proactive alerting on device connectivity issues
- Facilitating capacity planning with historical trend analysis
- Providing SLA compliance monitoring (uptime, connectivity)
- Enabling troubleshooting with detailed device health metrics
- Supporting multi-cloud observability (Azure + AWS unified metrics)
- Reducing mean time to detection (MTTD) for infrastructure issues
- Enabling automated incident response through metric-based triggers
- Providing evidence for compliance auditing (device inventory, uptime)
- Supporting business intelligence with IoT fleet analytics

The metrics collection system follows a two-stage architecture: Loader jobs periodically fetch metrics from cloud providers and update an in-memory PortalMetric singleton, while Exporter jobs synchronously read from the singleton and update Prometheus counters. This separation ensures cloud API failures don't disrupt metric serving, provides consistent metric timestamps, and reduces cloud API call frequency.

---

## Code Locations

### Entry Points / Job Definitions

#### Device Metric Jobs

- `src/IoTHub.Portal.Infrastructure/Jobs/DeviceMetricLoaderJob.cs` (Lines 1-54)
  - **Snippet**: Loads device count metrics from cloud providers
    ```csharp
    [DisallowConcurrentExecution]
    public class DeviceMetricLoaderJob : IJob
    {
        private readonly ILogger<DeviceMetricLoaderJob> logger;
        private readonly PortalMetric portalMetric;
        private readonly IExternalDeviceService externalDeviceService;
        
        public async Task Execute(IJobExecutionContext context)
        {
            this.logger.LogInformation("Start loading devices metrics");
            
            await LoadDevicesCountMetric();
            await LoadConnectedDevicesCountMetric();
            
            this.logger.LogInformation("End loading devices metrics");
        }
        
        private async Task LoadDevicesCountMetric()
        {
            try
            {
                this.portalMetric.DeviceCount = await this.externalDeviceService.GetDevicesCount();
            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load devices count metric: {e.Detail}", e);
            }
        }
        
        private async Task LoadConnectedDevicesCountMetric()
        {
            try
            {
                this.portalMetric.ConnectedDeviceCount = 
                    await this.externalDeviceService.GetConnectedDevicesCount();
            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load connected devices count metric: {e.Detail}", e);
            }
        }
    }
    ```

- `src/IoTHub.Portal.Infrastructure/Jobs/DeviceMetricExporterJob.cs` (Lines 1-33)
  - **Snippet**: Exports device metrics to Prometheus
    ```csharp
    [DisallowConcurrentExecution]
    public class DeviceMetricExporterJob : IJob
    {
        private readonly ILogger<DeviceMetricExporterJob> logger;
        private readonly PortalMetric portalMetric;
        
        private readonly Counter deviceCounter = Metrics.CreateCounter(
            MetricName.DeviceCount, "Devices count");
        private readonly Counter connectedDeviceCounter = Metrics.CreateCounter(
            MetricName.ConnectedDeviceCount, "Connected devices count");
        
        public Task Execute(IJobExecutionContext context)
        {
            this.logger.LogInformation("Start exporting devices metrics");
            
            // Update Prometheus counters from shared PortalMetric
            this.deviceCounter.IncTo(this.portalMetric.DeviceCount);
            this.connectedDeviceCounter.IncTo(this.portalMetric.ConnectedDeviceCount);
            
            this.logger.LogInformation("End exporting devices metrics");
            
            return Task.CompletedTask;
        }
    }
    ```

#### Edge Device Metric Jobs

- `src/IoTHub.Portal.Infrastructure/Jobs/EdgeDeviceMetricLoaderJob.cs` (Lines 1-70)
  - **Snippet**: Loads edge device and deployment metrics
    ```csharp
    [DisallowConcurrentExecution]
    public class EdgeDeviceMetricLoaderJob : IJob
    {
        private readonly ILogger<EdgeDeviceMetricLoaderJob> logger;
        private readonly PortalMetric portalMetric;
        private readonly IExternalDeviceService externalDeviceService;
        private readonly IConfigService configService;
        
        public async Task Execute(IJobExecutionContext context)
        {
            this.logger.LogInformation("Start loading edge devices metrics");
            
            await LoadEdgeDevicesCountMetric();
            await LoadConnectedEdgeDevicesCountMetric();
            await LoadFailedDeploymentsCountMetric();
            
            this.logger.LogInformation("End loading edge devices metrics");
        }
        
        private async Task LoadEdgeDevicesCountMetric()
        {
            try
            {
                this.portalMetric.EdgeDeviceCount = 
                    await this.externalDeviceService.GetEdgeDevicesCount();
            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load edge devices count metric: {e.Detail}", e);
            }
        }
        
        private async Task LoadConnectedEdgeDevicesCountMetric()
        {
            try
            {
                this.portalMetric.ConnectedEdgeDeviceCount = 
                    await this.externalDeviceService.GetConnectedEdgeDevicesCount();
            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load connected edge devices count metric: {e.Detail}", e);
            }
        }
        
        private async Task LoadFailedDeploymentsCountMetric()
        {
            try
            {
                this.portalMetric.FailedDeploymentCount = 
                    await configService.GetFailedDeploymentsCount();
            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load failed deployments count metric: {e.Detail}", e);
            }
        }
    }
    ```

- `src/IoTHub.Portal.Infrastructure/Jobs/EdgeDeviceMetricExporterJob.cs` (Lines 1-36)
  - **Snippet**: Exports edge device metrics to Prometheus
    ```csharp
    [DisallowConcurrentExecution]
    public class EdgeDeviceMetricExporterJob : IJob
    {
        private readonly ILogger<EdgeDeviceMetricExporterJob> logger;
        private readonly PortalMetric portalMetric;
        
        private readonly Counter edgeDeviceCounter = Metrics.CreateCounter(
            MetricName.EdgeDeviceCount, "Edge devices count");
        private readonly Counter connectedEdgeDeviceCounter = Metrics.CreateCounter(
            MetricName.ConnectedEdgeDeviceCount, "Connected edge devices count");
        private readonly Counter failedDeploymentCount = Metrics.CreateCounter(
            MetricName.FailedDeploymentCount, "Failed deployments count");
        
        public Task Execute(IJobExecutionContext context)
        {
            this.logger.LogInformation("Start exporting edge devices metrics");
            
            this.edgeDeviceCounter.IncTo(this.portalMetric.EdgeDeviceCount);
            this.connectedEdgeDeviceCounter.IncTo(this.portalMetric.ConnectedEdgeDeviceCount);
            this.failedDeploymentCount.IncTo(this.portalMetric.FailedDeploymentCount);
            
            this.logger.LogInformation("End exporting edge devices metrics");
            
            return Task.CompletedTask;
        }
    }
    ```

#### Concentrator Metric Jobs

- `src/IoTHub.Portal.Infrastructure/Jobs/ConcentratorMetricLoaderJob.cs` (Lines 1-41)
  - **Snippet**: Loads LoRa concentrator count metrics
    ```csharp
    [DisallowConcurrentExecution]
    public class ConcentratorMetricLoaderJob : IJob
    {
        private readonly ILogger<ConcentratorMetricLoaderJob> logger;
        private readonly PortalMetric portalMetric;
        private readonly IExternalDeviceService externalDeviceService;
        
        public async Task Execute(IJobExecutionContext context)
        {
            this.logger.LogInformation("Start loading concentrators metrics");
            
            await LoadConcentratorsCountMetric();
            
            this.logger.LogInformation("End loading concentrators metrics");
        }
        
        private async Task LoadConcentratorsCountMetric()
        {
            try
            {
                this.portalMetric.ConcentratorCount = 
                    await this.externalDeviceService.GetConcentratorsCount();
            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load concentrators count metric: {e.Detail}", e);
            }
        }
    }
    ```

- `src/IoTHub.Portal.Infrastructure/Jobs/ConcentratorMetricExporterJob.cs` (Lines 1-31)
  - **Snippet**: Exports concentrator metrics to Prometheus
    ```csharp
    [DisallowConcurrentExecution]
    public class ConcentratorMetricExporterJob : IJob
    {
        private readonly ILogger<ConcentratorMetricExporterJob> logger;
        private readonly PortalMetric portalMetric;
        
        private readonly Counter concentratorCounter = Metrics.CreateCounter(
            MetricName.ConcentratorCount, "Concentrators count");
        
        public Task Execute(IJobExecutionContext context)
        {
            this.logger.LogInformation("Start exporting concentrators metrics");
            
            this.concentratorCounter.IncTo(this.portalMetric.ConcentratorCount);
            
            this.logger.LogInformation("End exporting concentrators metrics");
            
            return Task.CompletedTask;
        }
    }
    ```

#### LoRaWAN Telemetry Job

- `src/IoTHub.Portal.Infrastructure/Jobs/SyncLoRaDeviceTelemetryJob.cs` (Lines 1-72)
  - **Snippet**: Real-time LoRaWAN telemetry event processing
    ```csharp
    public class SyncLoRaDeviceTelemetryJob : IJob
    {
        private readonly ILogger<SyncLoRaDeviceTelemetryJob> logger;
        private readonly ConfigHandler configHandler;
        private readonly BlobServiceClient blobServiceClient;
        private readonly IDeviceService<LoRaDeviceDetails> deviceService;
        
        private const string CHECKPOINTS_BLOBSTORAGE_NAME = "iothub-portal-events-checkpoints";
        
        public async Task Execute(IJobExecutionContext context)
        {
            var storageClient = this.blobServiceClient.GetBlobContainerClient(
                CHECKPOINTS_BLOBSTORAGE_NAME);
            
            _ = await storageClient.CreateIfNotExistsAsync();
            
            // Create Event Hub processor with checkpoint storage
            var processor = new EventProcessorClient(
                storageClient,
                this.configHandler.AzureIoTHubEventHubConsumerGroup,
                this.configHandler.AzureIoTHubEventHubEndpoint);
            
            try
            {
                processor.ProcessEventAsync += ProcessEventHandler;
                processor.ProcessErrorAsync += ProcessErrorHandler;
                
                try
                {
                    await processor.StartProcessingAsync(context.CancellationToken);
                    // Run until cancellation
                    await Task.Delay(Timeout.Infinite, context.CancellationToken);
                }
                finally
                {
                    await processor.StopProcessingAsync();
                }
            }
            finally
            {
                processor.ProcessEventAsync -= ProcessEventHandler;
                processor.ProcessErrorAsync -= ProcessErrorHandler;
            }
        }
        
        private async Task ProcessEventHandler(ProcessEventArgs args)
        {
            // Delegate telemetry processing to device service
            await this.deviceService.ProcessTelemetryEvent(args.Data);
        }
        
        private Task ProcessErrorHandler(ProcessErrorEventArgs args)
        {
            this.logger.LogError(args.Exception, 
                $"Error in the EventProcessorClient on {nameof(SyncLoRaDeviceTelemetryJob)}: Operation {args.Operation}");
            
            return Task.CompletedTask;
        }
    }
    ```

### Job Configuration & Scheduling

#### Azure Metrics Configuration

- `src/IoTHub.Portal.Infrastructure/Startup/AzureServiceCollectionExtension.cs` (Lines 110-120)
  - **Snippet**: Azure metrics job configuration
    ```csharp
    private static IServiceCollection ConfigureMetrics(
        this IServiceCollection services, ConfigHandler configuration)
    {
        return services.AddQuartz(q =>
        {
            q.AddMetricJob<DeviceMetricLoaderJob, DeviceMetricExporterJob>(
                configuration.MetricLoaderRefreshIntervalInMinutes,
                configuration.MetricExporterRefreshIntervalInSeconds);
            
            q.AddMetricJob<EdgeDeviceMetricLoaderJob, EdgeDeviceMetricExporterJob>(
                configuration.MetricLoaderRefreshIntervalInMinutes,
                configuration.MetricExporterRefreshIntervalInSeconds);
            
            if (configuration.IsLoRaEnabled)
            {
                q.AddMetricJob<ConcentratorMetricLoaderJob, ConcentratorMetricExporterJob>(
                    configuration.MetricLoaderRefreshIntervalInMinutes,
                    configuration.MetricExporterRefreshIntervalInSeconds);
            }
        });
    }
    ```

#### AWS Metrics Configuration

- `src/IoTHub.Portal.Infrastructure/Startup/AWSServiceCollectionExtension.cs` (Lines 60-111)
  - **Snippet**: AWS metrics job configuration within sync jobs
    ```csharp
    private static IServiceCollection ConfigureAWSSyncJobs(
        this IServiceCollection services, ConfigHandler configuration)
    {
        return services.AddQuartz(q =>
        {
            // Thing and deployment sync jobs...
            
            _ = q.AddJob<DeviceMetricLoaderJob>(j => j.WithIdentity(nameof(DeviceMetricLoaderJob)))
                .AddTrigger(t => t
                    .WithIdentity($"{nameof(DeviceMetricLoaderJob)}")
                    .ForJob(nameof(DeviceMetricLoaderJob))
                    .WithSimpleSchedule(s => s
                        .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                        .RepeatForever()));
            
            _ = q.AddJob<DeviceMetricExporterJob>(j => j.WithIdentity(nameof(DeviceMetricExporterJob)))
                .AddTrigger(t => t
                    .WithIdentity($"{nameof(DeviceMetricExporterJob)}")
                    .ForJob(nameof(DeviceMetricExporterJob))
                    .WithSimpleSchedule(s => s
                        .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                        .RepeatForever()));
            
            _ = q.AddJob<EdgeDeviceMetricLoaderJob>(j => j.WithIdentity(nameof(EdgeDeviceMetricLoaderJob)))
                .AddTrigger(t => t
                    .WithSimpleSchedule(s => s
                        .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                        .RepeatForever()));
        });
    }
    ```

#### Quartz Extension Helper

- `src/IoTHub.Portal.Infrastructure/Extenstions/QuartzConfiguratorExtension.cs` (Lines 1-30)
  - **Snippet**: Generic metric job configuration helper
    ```csharp
    public static class QuartzConfiguratorExtension
    {
        public static IServiceCollectionQuartzConfigurator AddMetricJob<TLoaderService, TExporterService>(
            this IServiceCollectionQuartzConfigurator q,
            int loaderIntervalInMinutes,
            int exporterIntervalInSeconds)
            where TLoaderService : class, IJob
            where TExporterService : class, IJob
        {
            // Configure loader job (data collection)
            _ = q.AddJob<TExporterService>(j => j.WithIdentity(typeof(TExporterService).Name))
                .AddTrigger(t => t
                    .WithIdentity($"{typeof(TExporterService).Name}")
                    .ForJob(typeof(TExporterService).Name)
                    .WithSimpleSchedule(s => s
                        .WithIntervalInSeconds(exporterIntervalInSeconds)
                        .RepeatForever()));
            
            // Configure exporter job (Prometheus export)
            _ = q.AddJob<TLoaderService>(j => j.WithIdentity(typeof(TLoaderService).Name))
                .AddTrigger(t => t
                    .WithIdentity($"{typeof(TLoaderService).Name}")
                    .ForJob(typeof(TLoaderService).Name)
                    .WithSimpleSchedule(s => s
                        .WithIntervalInMinutes(loaderIntervalInMinutes)
                        .RepeatForever()));
            
            return q;
        }
    }
    ```

### Business Logic & Services

- `src/IoTHub.Portal.Application/Services/IExternalDeviceService.cs` (Interface)
  - Core abstraction for cloud provider metric retrieval
  - Methods:
    - `GetDevicesCount()` - Total device count
    - `GetConnectedDevicesCount()` - Connected device count
    - `GetEdgeDevicesCount()` - Total edge device count
    - `GetConnectedEdgeDevicesCount()` - Connected edge device count
    - `GetConcentratorsCount()` - Total concentrator count

- `src/IoTHub.Portal.Infrastructure/Services/AzureExternalDeviceService.cs`
  - Azure IoT Hub implementation of metric retrieval
  - Uses Azure IoT Hub service SDK for device queries
  - Implements connection state filtering

- `src/IoTHub.Portal.Infrastructure/Services/AWS/AWSExternalDeviceService.cs`
  - AWS IoT Core implementation of metric retrieval
  - Uses AWS SDK for IoT and Greengrass APIs
  - Aggregates counts from thing and core device queries

- `src/IoTHub.Portal.Application/Services/IConfigService.cs`
  - Configuration and deployment management service
  - Method: `GetFailedDeploymentsCount()` - Failed edge deployment count
  - Queries edge device deployment status

- `src/IoTHub.Portal.Application/Services/IDeviceService<T>.cs`
  - Generic device management service
  - Method: `ProcessTelemetryEvent(EventData)` - Process LoRaWAN telemetry
  - Used by SyncLoRaDeviceTelemetryJob

### Data Models

- `src/IoTHub.Portal.Shared/Models/v1.0/PortalMetric.cs` (Lines 1-20)
  - **Snippet**: Shared singleton for metric storage
    ```csharp
    public class PortalMetric
    {
        public int DeviceCount { get; set; }
        public int ConnectedDeviceCount { get; set; }
        public int EdgeDeviceCount { get; set; }
        public int ConnectedEdgeDeviceCount { get; set; }
        public int FailedDeploymentCount { get; set; }
        public int ConcentratorCount { get; set; }
    }
    ```

- `src/IoTHub.Portal.Domain/Shared/Constants/MetricName.cs` (Lines 1-24)
  - **Snippet**: Prometheus metric name constants
    ```csharp
    public static class MetricName
    {
        private const string Prefix = "iot_hub_portal";
        
        public const string DeviceCount = $"{Prefix}_device_count";
        public const string ConnectedDeviceCount = $"{Prefix}_connected_device_count";
        public const string EdgeDeviceCount = $"{Prefix}_edge_device_count";
        public const string ConnectedEdgeDeviceCount = $"{Prefix}_connected_edge_device_count";
        public const string FailedDeploymentCount = $"{Prefix}_failed_deployment_count";
        public const string ConcentratorCount = $"{Prefix}_concentrator_count";
        public const string ConnectedConcentratorCount = $"{Prefix}_connected_concentrator_count";
    }
    ```

---

## Dependencies

### Internal Dependencies
- **Application Layer**:
  - IExternalDeviceService for metric data retrieval
  - IConfigService for deployment metrics
  - IDeviceService<T> for telemetry processing
  - PortalMetric singleton for metric sharing

- **Domain Layer**:
  - MetricName constants for metric naming
  - InternalServerErrorException for error handling
  - Domain exceptions for cloud API failures

- **Infrastructure Layer**:
  - Quartz.NET job scheduler
  - ConfigHandler for application settings
  - Logging infrastructure (ILogger<T>)
  - Azure Event Hub integration (SyncLoRaDeviceTelemetryJob)

### External Dependencies
- **Prometheus (prometheus-net)**:
  - `prometheus-net` (v8.x) - Prometheus metric library
  - `Metrics.CreateCounter()` - Counter creation
  - `Counter.IncTo()` - Counter update method
  - Metric endpoint `/metrics` exposed by ASP.NET Core

- **Azure Event Hub SDK**:
  - `Azure.Messaging.EventHubs.Processor` - Event processing
  - `EventProcessorClient` - Scalable event consumer
  - `BlobServiceClient` - Checkpoint storage
  - Consumer group and endpoint configuration

- **Quartz.NET**:
  - `Quartz` (v3.x) - Job scheduling framework
  - `[DisallowConcurrentExecution]` attribute
  - IJob interface and IJobExecutionContext
  - Simple schedule triggers

- **Azure Storage SDK**:
  - `Azure.Storage.Blobs` - Blob storage for checkpoints
  - BlobServiceClient for container operations
  - Checkpoint persistence for event processing

### Configuration Dependencies
- **Settings**:
  - `MetricLoaderRefreshIntervalInMinutes` - Loader job frequency (default: 5)
  - `MetricExporterRefreshIntervalInSeconds` - Exporter job frequency (default: 30)
  - `SyncDatabaseJobRefreshIntervalInMinutes` - AWS loader frequency (default: 5)
  - `IsLoRaEnabled` - Feature flag for concentrator metrics
  - `AzureIoTHubEventHubConsumerGroup` - Event Hub consumer group
  - `AzureIoTHubEventHubEndpoint` - Event Hub connection string

- **Environment Variables**:
  - Azure IoT Hub connection string (for device queries)
  - Azure Event Hub endpoint (for telemetry ingestion)
  - Azure Blob Storage connection string (for checkpoints)
  - AWS credentials (for AWS metric retrieval)
  - Prometheus endpoint configuration

### Feature Dependencies
- **Depends on**:
  - Device Synchronization Jobs (operates on synchronized device data)
  - External Device Services (cloud provider metric APIs)
  - Edge Device Management (deployment failure metrics)
  - LoRaWAN Management (concentrator and telemetry metrics)
  - Configuration Management (failed deployment counts)

- **Used by**:
  - Prometheus monitoring systems (scrapes `/metrics` endpoint)
  - Grafana dashboards (visualizes metrics over time)
  - Alerting systems (AlertManager, PagerDuty)
  - Capacity planning tools (historical trend analysis)
  - Operational dashboards (real-time device health)
  - SLA reporting (uptime and connectivity metrics)

---

## Related Features
- **023-background-job-management** - Job scheduling and monitoring infrastructure
- **024-device-synchronization-jobs** - Provides synchronized device data for metrics
- **001-device-management** - Device inventory being measured
- **002-edge-device-management** - Edge device and deployment metrics
- **012-lorawan-device-management** - LoRaWAN concentrator and telemetry metrics
- **017-device-monitoring** - Consumes metrics for monitoring dashboards

---

## Notes
- **Two-stage architecture**: Loader jobs fetch from cloud APIs, Exporter jobs update Prometheus from PortalMetric singleton
- Loader jobs run less frequently (minutes) to reduce cloud API costs, Exporter jobs run frequently (seconds) for metric freshness
- PortalMetric singleton ensures atomic metric updates and consistent timestamps across all exported metrics
- All jobs use `[DisallowConcurrentExecution]` to prevent race conditions on PortalMetric updates
- Comprehensive error handling in loader jobs prevents single metric failures from blocking other metrics
- SyncLoRaDeviceTelemetryJob runs continuously (infinite loop) until job cancellation
- Event Hub checkpoint storage ensures exactly-once telemetry processing with consumer group isolation
- Prometheus counters use `IncTo()` for absolute value updates rather than increments
- Concentrator metrics only registered when `IsLoRaEnabled=true` to avoid unnecessary jobs
- Metric names follow Prometheus naming conventions with `iot_hub_portal_` prefix
- Connected device metrics provide real-time infrastructure health visibility
- Failed deployment metrics enable proactive edge infrastructure troubleshooting
- Metrics are exposed via standard `/metrics` endpoint for Prometheus scraping
