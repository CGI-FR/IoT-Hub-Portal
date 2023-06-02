// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Jobs.AWS
{
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Amazon.GreengrassV2;
    using Amazon.GreengrassV2.Model;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using AutoMapper;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Repositories;
    using Microsoft.Extensions.Logging;
    using Quartz;
    using Quartz.Util;

    [DisallowConcurrentExecution]
    public class SyncGreenGrassDevicesJob : IJob
    {

        private readonly ILogger<SyncGreenGrassDevicesJob> logger;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IEdgeDeviceRepository edgeDeviceRepository;
        private readonly IEdgeDeviceModelRepository edgeDeviceModelRepository;
        private readonly IDeviceTagValueRepository deviceTagValueRepository;
        private readonly IAmazonIoT amazonIoTClient;
        private readonly IAmazonGreengrassV2 amazonGreenGrass;

        public SyncGreenGrassDevicesJob(
            ILogger<SyncGreenGrassDevicesJob> logger,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IEdgeDeviceRepository edgeDeviceRepository,
            IEdgeDeviceModelRepository edgeDeviceModelRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            IAmazonIoT amazonIoTClient,
            IAmazonGreengrassV2 amazonGreenGrass)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.edgeDeviceRepository = edgeDeviceRepository;
            this.edgeDeviceModelRepository = edgeDeviceModelRepository;
            this.deviceTagValueRepository = deviceTagValueRepository;
            this.amazonIoTClient = amazonIoTClient;
            this.amazonGreenGrass = amazonGreenGrass;
            this.logger = logger;
        }


        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                this.logger.LogInformation("Start of sync GreenGrass Devices job");

                await SyncGreenGrassDevicesAsEdgeDevices();

                this.logger.LogInformation("End of sync GreenGrass Devices job");
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Sync GreenGrass Devices job has failed");
            }
        }

        private async Task SyncGreenGrassDevicesAsEdgeDevices()
        {
            var things = await GetAllGreenGrassDevices();

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
                    this.logger.LogInformation($"Cannot import Greengrass device '{thing.ThingName}' since it doesn't have related thing type.");
                    continue;
                }

                //EdgeDeviceModel not find in DB
                var edgeDeviceModel = await this.edgeDeviceModelRepository.GetByNameAsync(thing.ThingTypeName);
                if (edgeDeviceModel == null)
                {
                    this.logger.LogWarning($"Cannot import Greengrass device '{thing.ThingName}'. The EdgeDeviceModel '{thing.ThingTypeName}' doesn't exist");
                    continue;
                }

                //Create or update the Edge Device
                await CreateOrUpdateGreenGrassDevice(thing, edgeDeviceModel);
            }

            foreach (var item in (await this.edgeDeviceRepository.GetAllAsync(
                edgeDevice => !things.Select(x => x.ThingId).Contains(edgeDevice.Id),
                default,
                d => d.Tags,
                d => d.Labels
            )))
            {
                this.edgeDeviceRepository.Delete(item.Id);
            }

            await this.unitOfWork.SaveAsync();
        }

        private async Task<List<DescribeThingResponse>> GetAllGreenGrassDevices()
        {
            var devices = new List<DescribeThingResponse>();

            var nextToken = string.Empty;

            var response = await amazonGreenGrass.ListCoreDevicesAsync(
                new ListCoreDevicesRequest
                {
                    NextToken = nextToken
                });

            foreach (var requestDescribeThing in response.CoreDevices.Select(device => new DescribeThingRequest { ThingName = device.CoreDeviceThingName }))
            {
                try
                {
                    devices.Add(await this.amazonIoTClient.DescribeThingAsync(requestDescribeThing));
                }
                catch (AmazonIoTException e)
                {
                    this.logger.LogWarning($"Cannot import Greengrass device '{requestDescribeThing.ThingName}' due to an error in the Amazon IoT API.", e);
                    continue;
                }
            }

            return devices;
        }

        private async Task CreateOrUpdateGreenGrassDevice(DescribeThingResponse greengrassDevice, EdgeDeviceModel edgeModelDevice)
        {
            var edgeDevice = this.mapper.Map<EdgeDevice>(greengrassDevice);
            var edgeDeviceEntity = await this.edgeDeviceRepository.GetByIdAsync(edgeDevice.Id, d => d.Tags);
            edgeDevice.DeviceModelId = edgeModelDevice.Id;

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
