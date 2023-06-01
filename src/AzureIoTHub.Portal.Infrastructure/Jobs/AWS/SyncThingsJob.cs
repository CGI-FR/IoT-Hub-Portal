// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Jobs.AWS
{
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using Amazon.IotData;
    using Amazon.IotData.Model;
    using AutoMapper;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Repositories;
    using Microsoft.Extensions.Logging;
    using Quartz;
    using Quartz.Util;

    [DisallowConcurrentExecution]
    public class SyncThingsJob : IJob
    {

        private readonly ILogger<SyncThingsJob> logger;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IDeviceRepository deviceRepository;
        private readonly IDeviceModelRepository deviceModelRepository;
        private readonly IDeviceTagValueRepository deviceTagValueRepository;
        private readonly IAmazonIoT amazonIoTClient;
        private readonly IAmazonIotData amazonIoTDataClient;

        public SyncThingsJob(
            ILogger<SyncThingsJob> logger,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IDeviceRepository deviceRepository,
            IDeviceModelRepository deviceModelRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            IAmazonIoT amazonIoTClient,
            IAmazonIotData amazonIoTDataClient)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.deviceRepository = deviceRepository;
            this.deviceModelRepository = deviceModelRepository;
            this.deviceTagValueRepository = deviceTagValueRepository;
            this.amazonIoTClient = amazonIoTClient;
            this.amazonIoTDataClient = amazonIoTDataClient;
            this.logger = logger;
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
            var things = await GetAllThings();

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

                //ThingType not find in DB
                var deviceModel = this.deviceModelRepository.GetByName(thing.ThingTypeName);
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

                //Create or update the thing
                await CreateOrUpdateThing(thing, deviceModel);
            }

            foreach (var item in (await this.deviceRepository.GetAllAsync(
                device => !things.Select(x => x.ThingId).Contains(device.Id),
                default,
                d => d.Tags,
                d => d.Labels
            )))
            {
                this.deviceRepository.Delete(item.Id);
            }

            await this.unitOfWork.SaveAsync();
        }

        private async Task<List<DescribeThingResponse>> GetAllThings()
        {
            var things = new List<DescribeThingResponse>();

            var response = await amazonIoTClient.ListThingsAsync();

            foreach (var requestDescribeThing in response.Things.Select(thing => new DescribeThingRequest { ThingName = thing.ThingName }))
            {
                try
                {
                    things.Add(await this.amazonIoTClient.DescribeThingAsync(requestDescribeThing));
                }
                catch (AmazonIoTException e)
                {
                    this.logger.LogWarning($"Cannot import device '{requestDescribeThing.ThingName}' due to an error in the Amazon IoT API.", e);
                    continue;
                }
            }

            return things;
        }

        private async Task CreateOrUpdateThing(DescribeThingResponse thing, DeviceModel deviceModel)
        {
            var device = this.mapper.Map<Device>(thing);
            var deviceEntity = await this.deviceRepository.GetByIdAsync(device.Id, d => d.Tags);
            device.DeviceModelId = deviceModel.Id;

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
    }
}
