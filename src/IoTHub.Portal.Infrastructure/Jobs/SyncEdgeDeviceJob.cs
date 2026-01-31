// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class SyncEdgeDeviceJob : IJob
    {
        private readonly IExternalDeviceService externalDeviceService;
        private readonly IEdgeDeviceRepository edgeDeviceRepository;
        private readonly IDeviceTagValueRepository deviceTagValueRepository;
        private readonly IEdgeDeviceModelRepository edgeDeviceModelRepository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<SyncEdgeDeviceJob> logger;

        private readonly IEdgeDevicesService edgeDevicesService;

        private const string ModelId = "modelId";

        public SyncEdgeDeviceJob(IExternalDeviceService externalDeviceService,
            IEdgeDeviceModelRepository edgeDeviceModelRepository,
            IEdgeDevicesService edgeDevicesService,
            IEdgeDeviceRepository edgeDeviceRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILogger<SyncEdgeDeviceJob> logger)
        {
            this.edgeDeviceModelRepository = edgeDeviceModelRepository;
            this.externalDeviceService = externalDeviceService;
            this.edgeDeviceRepository = edgeDeviceRepository;
            this.deviceTagValueRepository = deviceTagValueRepository;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
            this.edgeDevicesService = edgeDevicesService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                this.logger.LogInformation("Start of sync devices job");

                await SyncEdgeDevices();

                this.logger.LogInformation("End of sync devices job");
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Sync devices job has failed");
            }
        }

        private async Task SyncEdgeDevices()
        {
            var deviceTwins = await GetTwinDevices();

            foreach (var twin in deviceTwins)
            {
                try
                {
                    if (!twin.Tags.Contains(ModelId))
                    {
                        this.logger.LogWarning($"The device with wont be synchronized since it doesn't have a model identifier.");
                        continue;
                    }

                    var modelId = twin.Tags[ModelId].ToString();

                    var deviceModel = await this.edgeDeviceModelRepository.GetByIdAsync(modelId);

                    if (deviceModel == null)
                    {
                        this.logger.LogWarning($"The device with wont be synchronized, its model id {modelId} doesn't exist");
                        continue;
                    }

                    await CreateOrUpdateDevice(twin);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"Failed to synchronize device {twin.DeviceId}");
                }
            }

            foreach (var item in (await this.edgeDeviceRepository.GetAllAsync()).Where(edgeDevice => !deviceTwins.Exists(twin => twin.DeviceId == edgeDevice.Id)))
            {
                this.edgeDeviceRepository.Delete(item.Id);
            }

            await this.unitOfWork.SaveAsync();
        }

        private async Task<List<Twin>> GetTwinDevices()
        {
            var twins = new List<Twin>();
            var continuationToken = string.Empty;

            int totalTwinDevices;
            do
            {
                var result = await this.externalDeviceService.GetAllEdgeDevice(continuationToken: continuationToken, pageSize: 100);
                twins.AddRange(result.Items);

                totalTwinDevices = result.TotalItems;
                continuationToken = result.NextPage;

            } while (totalTwinDevices > twins.Count);

            return twins;
        }

        private async Task CreateOrUpdateDevice(Twin twin)
        {
            var twinWithModule = await this.externalDeviceService.GetDeviceTwinWithModule(twin.DeviceId);
            var twinWithClient = await this.externalDeviceService.GetDeviceTwinWithEdgeHubModule(twin.DeviceId);

            var connectionState = twin.ConnectionState == DeviceConnectionState.Connected ? "Connected" : "Disconnected";

            var device = this.mapper.Map<EdgeDevice>(twin,opts =>
            {
                opts.Items["TwinModules"] = twinWithModule;
                opts.Items["TwinClient"] = twinWithClient;
            }) ;

            device.ConnectionState = connectionState;

            var deviceEntity = await this.edgeDeviceRepository.GetByIdAsync(device.Id, d => d.Tags);

            if (deviceEntity == null)
            {
                await this.edgeDeviceRepository.InsertAsync(device);
            }
            else
            {
                if (deviceEntity.Version >= device.Version) return;

                foreach (var deviceTagEntity in deviceEntity.Tags)
                {
                    this.deviceTagValueRepository.Delete(deviceTagEntity.Id);
                }

                _ = this.mapper.Map(device, deviceEntity);
                this.edgeDeviceRepository.Update(deviceEntity);
            }
        }
    }
}
