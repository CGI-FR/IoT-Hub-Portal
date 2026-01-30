// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Jobs
{
    using Device = Domain.Entities.Device;

    [DisallowConcurrentExecution]
    public class SyncDevicesJob : IJob
    {
        private readonly IExternalDeviceService externalDeviceService;
        private readonly IDeviceModelRepository deviceModelRepository;
        private readonly ILorawanDeviceRepository lorawanDeviceRepository;
        private readonly IDeviceRepository deviceRepository;
        private readonly IDeviceTagValueRepository deviceTagValueRepository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<SyncDevicesJob> logger;

        private const string ModelId = "modelId";

        public SyncDevicesJob(IExternalDeviceService externalDeviceService,
            IDeviceModelRepository deviceModelRepository,
            ILorawanDeviceRepository lorawanDeviceRepository,
            IDeviceRepository deviceRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILogger<SyncDevicesJob> logger)
        {
            this.externalDeviceService = externalDeviceService;
            this.deviceModelRepository = deviceModelRepository;
            this.lorawanDeviceRepository = lorawanDeviceRepository;
            this.deviceRepository = deviceRepository;
            this.deviceTagValueRepository = deviceTagValueRepository;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

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
            var deviceTwins = await GetTwinDevices();

            foreach (var twin in deviceTwins)
            {
                if (!twin.Tags.Contains(ModelId))
                {
                    this.logger.LogInformation($"Cannot import device '{twin.DeviceId}' since it doesn't have a model identifier.");
                    continue;
                }

                var deviceModel = await this.deviceModelRepository.GetByIdAsync(twin.Tags[ModelId]?.ToString() ?? string.Empty);

                if (deviceModel == null)
                {
                    this.logger.LogWarning($"The device with wont be synched, its model id {twin.Tags[ModelId]?.ToString()} doesn't exist");
                    continue;
                }

                if (deviceModel.SupportLoRaFeatures)
                {
                    await CreateOrUpdateLorawanDevice(twin);
                }
                else
                {
                    await CreateOrUpdateDevice(twin);
                }
            }

            foreach (var item in (await this.deviceRepository.GetAllAsync()).Where(device => !deviceTwins.Exists(x => x.DeviceId == device.Id)))
            {
                this.deviceRepository.Delete(item.Id);
            }

            foreach (var item in (await this.lorawanDeviceRepository.GetAllAsync()).Where(lorawanDevice => !deviceTwins.Exists(x => x.DeviceId == lorawanDevice.Id)))
            {
                this.lorawanDeviceRepository.Delete(item.Id);
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
                var result = await this.externalDeviceService.GetAllDevice(continuationToken: continuationToken,excludeDeviceType: "LoRa Concentrator", pageSize: 100);
                twins.AddRange(result.Items);

                totalTwinDevices = result.TotalItems;
                continuationToken = result.NextPage;

            } while (totalTwinDevices > twins.Count);

            return twins;
        }

        private async Task CreateOrUpdateLorawanDevice(Twin twin)
        {
            var lorawanDevice = this.mapper.Map<LorawanDevice>(twin);

            var lorawanDeviceEntity = await this.lorawanDeviceRepository.GetByIdAsync(lorawanDevice.Id, d => d.Tags);
            if (lorawanDeviceEntity == null)
            {
                await this.lorawanDeviceRepository.InsertAsync(lorawanDevice);
            }
            else
            {
                if (lorawanDeviceEntity.Version >= lorawanDevice.Version) return;

                foreach (var deviceTagEntity in lorawanDeviceEntity.Tags)
                {
                    this.deviceTagValueRepository.Delete(deviceTagEntity.Id);
                }

                // Update only properties that are present in the Twin
                // Base device properties (always updated from Twin)
                lorawanDeviceEntity.Name = lorawanDevice.Name;
                lorawanDeviceEntity.DeviceModelId = lorawanDevice.DeviceModelId;
                lorawanDeviceEntity.Version = lorawanDevice.Version;
                lorawanDeviceEntity.IsConnected = lorawanDevice.IsConnected;
                lorawanDeviceEntity.IsEnabled = lorawanDevice.IsEnabled;
                lorawanDeviceEntity.StatusUpdatedTime = lorawanDevice.StatusUpdatedTime;
                lorawanDeviceEntity.LastActivityTime = lorawanDevice.LastActivityTime;
                lorawanDeviceEntity.LayerId = lorawanDevice.LayerId;
                lorawanDeviceEntity.Tags = lorawanDevice.Tags;

                // Update LoRaWAN properties only if they exist in the Twin's desired properties
                // OTAA/ABP authentication settings
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.AppEUI)))
                    lorawanDeviceEntity.AppEUI = lorawanDevice.AppEUI;
                
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.AppKey)))
                    lorawanDeviceEntity.AppKey = lorawanDevice.AppKey;
                
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.AppSKey)))
                    lorawanDeviceEntity.AppSKey = lorawanDevice.AppSKey;
                
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.NwkSKey)))
                    lorawanDeviceEntity.NwkSKey = lorawanDevice.NwkSKey;
                
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.DevAddr)))
                    lorawanDeviceEntity.DevAddr = lorawanDevice.DevAddr;
                
                // Update UseOTAA based on AppEUI presence in Twin
                // Only update if AppEUI exists in Twin to avoid overwriting database value
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.AppEUI)))
                    lorawanDeviceEntity.UseOTAA = lorawanDevice.UseOTAA;

                // Other LoRaWAN configuration properties (only update if present in Twin)
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.SensorDecoder)))
                    lorawanDeviceEntity.SensorDecoder = lorawanDevice.SensorDecoder;
                
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.ClassType)))
                    lorawanDeviceEntity.ClassType = lorawanDevice.ClassType;
                
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.PreferredWindow)))
                    lorawanDeviceEntity.PreferredWindow = lorawanDevice.PreferredWindow;
                
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.Deduplication)))
                    lorawanDeviceEntity.Deduplication = lorawanDevice.Deduplication;
                
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.RX1DROffset)))
                    lorawanDeviceEntity.RX1DROffset = lorawanDevice.RX1DROffset;
                
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.RX2DataRate)))
                    lorawanDeviceEntity.RX2DataRate = lorawanDevice.RX2DataRate;
                
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.RXDelay)))
                    lorawanDeviceEntity.RXDelay = lorawanDevice.RXDelay;
                
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.ABPRelaxMode)))
                    lorawanDeviceEntity.ABPRelaxMode = lorawanDevice.ABPRelaxMode;
                
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.FCntUpStart)))
                    lorawanDeviceEntity.FCntUpStart = lorawanDevice.FCntUpStart;
                
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.FCntDownStart)))
                    lorawanDeviceEntity.FCntDownStart = lorawanDevice.FCntDownStart;
                
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.FCntResetCounter)))
                    lorawanDeviceEntity.FCntResetCounter = lorawanDevice.FCntResetCounter;
                
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.Supports32BitFCnt)))
                    lorawanDeviceEntity.Supports32BitFCnt = lorawanDevice.Supports32BitFCnt;
                
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.KeepAliveTimeout)))
                    lorawanDeviceEntity.KeepAliveTimeout = lorawanDevice.KeepAliveTimeout;
                
                if (twin.Properties.Desired.Contains(nameof(LoRaDeviceDetails.Downlink)))
                    lorawanDeviceEntity.Downlink = lorawanDevice.Downlink;

                // Update reported properties only if they exist in Twin (as they come from the device)
                // AlreadyLoggedInOnce is set based on DevAddr presence in reported properties
                if (twin.Properties.Reported.Contains("DevAddr"))
                    lorawanDeviceEntity.AlreadyLoggedInOnce = lorawanDevice.AlreadyLoggedInOnce;
                
                if (twin.Properties.Reported.Contains(nameof(LoRaDeviceDetails.GatewayID)))
                    lorawanDeviceEntity.GatewayID = lorawanDevice.GatewayID;
                
                if (twin.Properties.Reported.Contains(nameof(LoRaDeviceDetails.DataRate)))
                    lorawanDeviceEntity.DataRate = lorawanDevice.DataRate;
                
                if (twin.Properties.Reported.Contains(nameof(LoRaDeviceDetails.TxPower)))
                    lorawanDeviceEntity.TxPower = lorawanDevice.TxPower;
                
                if (twin.Properties.Reported.Contains(nameof(LoRaDeviceDetails.NbRep)))
                    lorawanDeviceEntity.NbRep = lorawanDevice.NbRep;
                
                if (twin.Properties.Reported.Contains(nameof(LoRaDeviceDetails.ReportedRX2DataRate)))
                    lorawanDeviceEntity.ReportedRX2DataRate = lorawanDevice.ReportedRX2DataRate;
                
                if (twin.Properties.Reported.Contains(nameof(LoRaDeviceDetails.ReportedRX1DROffset)))
                    lorawanDeviceEntity.ReportedRX1DROffset = lorawanDevice.ReportedRX1DROffset;
                
                if (twin.Properties.Reported.Contains(nameof(LoRaDeviceDetails.ReportedRXDelay)))
                    lorawanDeviceEntity.ReportedRXDelay = lorawanDevice.ReportedRXDelay;

                this.lorawanDeviceRepository.Update(lorawanDeviceEntity);
            }
        }

        private async Task CreateOrUpdateDevice(Twin twin)
        {
            var device = this.mapper.Map<Device>(twin);

            var deviceEntity = await this.deviceRepository.GetByIdAsync(device.Id, d => d.Tags);
            if (deviceEntity == null)
            {
                await this.deviceRepository.InsertAsync(device);
            }
            else
            {
                if (deviceEntity.Version >= device.Version) return;

                foreach (var deviceTagEntity in deviceEntity.Tags)
                {
                    this.deviceTagValueRepository.Delete(deviceTagEntity.Id);
                }

                // Update only core properties from Twin
                deviceEntity.Name = device.Name;
                deviceEntity.DeviceModelId = device.DeviceModelId;
                deviceEntity.Version = device.Version;
                deviceEntity.IsConnected = device.IsConnected;
                deviceEntity.IsEnabled = device.IsEnabled;
                deviceEntity.StatusUpdatedTime = device.StatusUpdatedTime;
                deviceEntity.LastActivityTime = device.LastActivityTime;
                deviceEntity.LayerId = device.LayerId;
                deviceEntity.Tags = device.Tags;

                this.deviceRepository.Update(deviceEntity);
            }
        }
    }
}
