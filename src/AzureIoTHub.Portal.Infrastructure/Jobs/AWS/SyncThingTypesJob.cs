// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Jobs.AWS
{
    using System.Threading.Tasks;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using Microsoft.Extensions.Logging;
    using Quartz;

    [DisallowConcurrentExecution]
    public class SyncThingTypesJob : IJob
    {

        private readonly ILogger<SyncThingTypesJob> logger;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAmazonIoT amazonIoTClient;
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public SyncThingTypesJob(
            ILogger<SyncThingTypesJob> logger,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IAmazonIoT amazonIoTClient,
            IDeviceModelImageManager awsImageManager)
        {
            this.deviceModelImageManager = awsImageManager;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.amazonIoTClient = amazonIoTClient;
            this.logger = logger;
        }


        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                this.logger.LogInformation("Start of sync Thing Types job");

                await SyncThingTypesAsDeviceModels();

                this.logger.LogInformation("End of sync Thing Types job");
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Sync Thing Types job has failed");
            }
        }

        private async Task SyncThingTypesAsDeviceModels()
        {
            var thingTypes = await GetAllThingTypes();

            thingTypes.ForEach(async thingType =>
            {
                await CreateOrUpdateDeviceModel(thingType);
            });

            //Delete in Database AWS deleted thing types
            await DeleteThingTypes(thingTypes);
        }

        private async Task<List<DescribeThingTypeResponse>> GetAllThingTypes()
        {
            var thingTypes = new List<DescribeThingTypeResponse>();

            var nextToken = string.Empty;

            do
            {
                var request = new ListThingTypesRequest
                {
                    NextToken = nextToken
                };

                var response = await amazonIoTClient.ListThingTypesAsync(request);

                foreach (var thingType in response.ThingTypes)
                {
                    var requestDescribeThingType = new DescribeThingTypeRequest
                    {
                        ThingTypeName = thingType.ThingTypeName,
                    };

                    thingTypes.Add(await this.amazonIoTClient.DescribeThingTypeAsync(requestDescribeThingType));
                }

                nextToken = response.NextToken;
            }
            while (!string.IsNullOrEmpty(nextToken));

            return thingTypes;
        }

        private async Task CreateOrUpdateDeviceModel(DescribeThingTypeResponse thingType)
        {
            if (thingType.ThingTypeMetadata.Deprecated)
            {
                return;
            }

            var deviceModel = this.mapper.Map<DeviceModel>(thingType);

            var existingDeviceModel = await this.unitOfWork.DeviceModelRepository.GetByIdAsync(deviceModel.Id);

            if (existingDeviceModel == null)
            {
                await this.unitOfWork.DeviceModelRepository.InsertAsync(deviceModel);
                _ = await this.deviceModelImageManager.SetDefaultImageToModel(deviceModel.Id);
            }
            else
            {
                _ = this.mapper.Map(deviceModel, existingDeviceModel);
                this.unitOfWork.DeviceModelRepository.Update(existingDeviceModel);
            }
            await this.unitOfWork.SaveAsync();

        }

        private async Task DeleteThingTypes(List<DescribeThingTypeResponse> thingTypes)
        {
            // Get all device models that are not in AWS anymore or that are deprecated
            var deviceModelsToDelete = (await this.unitOfWork.DeviceModelRepository.GetAllAsync())
                .Where(deviceModel => !thingTypes.Any(thingType => deviceModel.Id.Equals(thingType.ThingTypeId, StringComparison.Ordinal)) ||
                    thingTypes.Any(thingType => deviceModel.Id.Equals(thingType.ThingTypeId, StringComparison.Ordinal) && thingType.ThingTypeMetadata.Deprecated))
                .ToList();

            deviceModelsToDelete.ForEach(async deviceModel =>
            {
                await this.deviceModelImageManager.DeleteDeviceModelImageAsync(deviceModel.Id);
                this.unitOfWork.DeviceModelRepository.Delete(deviceModel.Id);
            });

            await this.unitOfWork.SaveAsync();
        }

    }
}
