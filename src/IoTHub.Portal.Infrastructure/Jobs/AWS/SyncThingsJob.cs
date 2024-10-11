// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Jobs.AWS
{
    [DisallowConcurrentExecution]
    public class SyncThingsJob : IJob
    {

        private readonly ILogger<SyncThingsJob> logger;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IDeviceRepository deviceRepository;
        private readonly IEdgeDeviceRepository edgeDeviceRepository;
        private readonly IDeviceModelRepository deviceModelRepository;
        private readonly IEdgeDeviceModelRepository edgeDeviceModelRepository;
        private readonly IDeviceTagValueRepository deviceTagValueRepository;
        private readonly IAmazonIoT amazonIoTClient;
        private readonly IAmazonIotData amazonIoTDataClient;
        private readonly IAmazonGreengrassV2 amazonGreenGrass;
        private readonly IExternalDeviceService externalDeviceService;

        public SyncThingsJob(
            ILogger<SyncThingsJob> logger,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IDeviceRepository deviceRepository,
            IEdgeDeviceRepository edgeDeviceRepository,
            IDeviceModelRepository deviceModelRepository,
            IEdgeDeviceModelRepository edgeDeviceModelRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            IAmazonIoT amazonIoTClient,
            IAmazonIotData amazonIoTDataClient,
            IAmazonGreengrassV2 amazonGreenGrass,
            IExternalDeviceService externalDeviceService)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.deviceRepository = deviceRepository;
            this.edgeDeviceRepository = edgeDeviceRepository;
            this.deviceModelRepository = deviceModelRepository;
            this.edgeDeviceModelRepository = edgeDeviceModelRepository;
            this.deviceTagValueRepository = deviceTagValueRepository;
            this.amazonIoTClient = amazonIoTClient;
            this.amazonIoTDataClient = amazonIoTDataClient;
            this.amazonGreenGrass = amazonGreenGrass;
            this.logger = logger;
            this.externalDeviceService = externalDeviceService;
        }


        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                this.logger.LogInformation("Start of sync Things job");

                await SyncThingsAsDevices();

                this.logger.LogInformation("End of sync Things job");
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Sync Things job has failed");
            }
        }

        private async Task SyncThingsAsDevices()
        {
            var things = await this.externalDeviceService.GetAllThing();

            foreach (var thing in things)
            {
                //Thing error
                if (thing.HttpStatusCode != HttpStatusCode.OK)
                {
                    this.logger.LogWarning($"Cannot import device '{thing.ThingName}' due to an error in the Amazon IoT API : {thing.HttpStatusCode}");
                    continue;
                }

                //ThingType not specified
                if (thing.ThingTypeName.IsNullOrWhiteSpace())
                {
                    this.logger.LogInformation($"Cannot import device '{thing.ThingName}' since it doesn't have related thing type.");
                    continue;
                }

                bool? isEdge;
                //Retrieve ThingType to know if it's an iotEdge
                try
                {
                    isEdge = await externalDeviceService.IsEdgeDeviceModel(this.mapper.Map<ExternalDeviceModelDto>(thing));
                }
                catch (AmazonIoTException e)
                {
                    this.logger.LogWarning($"Cannot import device '{thing.ThingName}' due to an error retrieving thing shadow in the Amazon IoT Data API.", e);
                    continue;
                }

                // Cannot know if the thing type was created for an iotEdge or not, so skipping...
                if (!isEdge.HasValue)
                {
                    continue;
                }

                // EdgeDevice
                if (isEdge == true)
                {
                    //EdgeDeviceModel not find in DB
                    var edgeDeviceModel = await this.edgeDeviceModelRepository.GetByNameAsync(thing.ThingTypeName);
                    if (edgeDeviceModel == null)
                    {
                        this.logger.LogWarning($"Cannot import Edge device '{thing.ThingName}'. The EdgeDeviceModel '{thing.ThingTypeName}' doesn't exist");
                        continue;
                    }

                    //EdgeDevice map
                    var edgeDevice = this.mapper.Map<EdgeDevice>(thing);
                    edgeDevice.DeviceModelId = edgeDeviceModel.Id;
                    //Get EdgeDevice ConnectionState
                    try
                    {
                        var coreDevice = await amazonGreenGrass.GetCoreDeviceAsync(new GetCoreDeviceRequest() { CoreDeviceThingName = thing.ThingName });
                        edgeDevice.ConnectionState = coreDevice.HttpStatusCode != HttpStatusCode.OK
                            ? "Disconnected"
                            : coreDevice.Status == CoreDeviceStatus.HEALTHY ? "Connected" : "Disconnected";
                    }
                    //Disconnected if unable to retrieve
                    catch (AmazonGreengrassV2Exception)
                    {
                        edgeDevice.ConnectionState = "Disconnected";
                    }

                    //Create or update the edge device
                    await CreateOrUpdateEdgeDevice(edgeDevice);
                }
                //Device
                else
                {
                    //DeviceModel not find in DB
                    var deviceModel = await this.deviceModelRepository.GetByNameAsync(thing.ThingTypeName);
                    if (deviceModel == null)
                    {
                        this.logger.LogWarning($"Cannot import device '{thing.ThingName}'. The ThingType '{thing.ThingTypeName}' doesn't exist");
                        continue;
                    }

                    //ThingShadow not specified
                    var thingShadowRequest = new GetThingShadowRequest()
                    {
                        ThingName = thing.ThingName
                    };
                    try
                    {
                        var thingShadow = await this.amazonIoTDataClient.GetThingShadowAsync(thingShadowRequest);
                        if (thingShadow.HttpStatusCode != HttpStatusCode.OK)
                        {
                            if (thingShadow.HttpStatusCode.Equals(HttpStatusCode.NotFound))
                                this.logger.LogInformation($"Cannot import device '{thing.ThingName}' since it doesn't have related classic thing shadow");
                            else
                                this.logger.LogWarning($"Cannot import device '{thing.ThingName}' due to an error retrieving thing shadow in the Amazon IoT API : {thingShadow.HttpStatusCode}");
                            continue;
                        }
                    }
                    catch (AmazonIotDataException e)
                    {
                        this.logger.LogWarning($"Cannot import device '{thing.ThingName}' due to an error retrieving thing shadow in the Amazon IoT Data API.", e);
                        continue;
                    }

                    //Device
                    var device = this.mapper.Map<Device>(thing);
                    device.DeviceModelId = deviceModel.Id;

                    //Create or update the thing
                    await CreateOrUpdateDevice(device);
                }
            }

            //Delete Device don't exist on AWS
            foreach (var item in (await this.deviceRepository.GetAllAsync(
                device => !things.Select(x => x.ThingId).Contains(device.Id),
                default,
                d => d.Tags,
                d => d.Labels
            )))
            {
                this.deviceRepository.Delete(item.Id);
            }

            //Delete Edge Device don't exist on AWS
            foreach (var item in (await this.edgeDeviceRepository.GetAllAsync(
                device => !things.Select(x => x.ThingId).Contains(device.Id),
                default,
                d => d.Tags,
                d => d.Labels
            )))
            {
                this.edgeDeviceRepository.Delete(item.Id);
            }

            await this.unitOfWork.SaveAsync();
        }

        private async Task CreateOrUpdateDevice(Device device)
        {
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

                _ = this.mapper.Map(device, deviceEntity);
                this.deviceRepository.Update(deviceEntity);
            }
        }

        private async Task CreateOrUpdateEdgeDevice(EdgeDevice edgeDevice)
        {
            var edgeDeviceEntity = await this.edgeDeviceRepository.GetByIdAsync(edgeDevice.Id, d => d.Tags);

            if (edgeDeviceEntity == null)
            {
                await this.edgeDeviceRepository.InsertAsync(edgeDevice);
            }
            else
            {
                if (edgeDeviceEntity.Version >= edgeDevice.Version) return;

                foreach (var deviceTagEntity in edgeDeviceEntity.Tags)
                {
                    this.deviceTagValueRepository.Delete(deviceTagEntity.Id);
                }
                _ = this.mapper.Map(edgeDevice, edgeDeviceEntity);
                this.edgeDeviceRepository.Update(edgeDeviceEntity);
            }
        }
    }
}
