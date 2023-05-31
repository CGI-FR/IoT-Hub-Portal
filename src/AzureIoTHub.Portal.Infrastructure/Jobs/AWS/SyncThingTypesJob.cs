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
    using AzureIoTHub.Portal.Domain.Repositories;
    using Microsoft.Extensions.Logging;
    using Quartz;

    [DisallowConcurrentExecution]
    public class SyncThingTypesJob : IJob
    {

        private readonly ILogger<SyncThingTypesJob> logger;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IDeviceModelRepository deviceModelRepository;
        private readonly IAmazonIoT amazonIoTClient;
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public SyncThingTypesJob(
            ILogger<SyncThingTypesJob> logger,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IDeviceModelRepository deviceModelRepository,
            IAmazonIoT amazonIoTClient,
            IDeviceModelImageManager awsImageManager)
        {
            this.deviceModelImageManager = awsImageManager;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.deviceModelRepository = deviceModelRepository;
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

            foreach (var thingType in thingTypes)
            {
                var isEdge = await IsEdgeThingType(thingType);

                // Cannot know if the thing type was created for an iotEdge or not, so skipping...
                if (!isEdge.HasValue)
                {
                    continue;
                }

                if (isEdge == true)
                {
                    // TODO: Implement CreateOrUpdateEdgeModel here.
                }
                else
                {
                    await CreateOrUpdateDeviceModel(thingType);
                }
            }

            // Delete in Database AWS deleted thing types
            await DeleteThingTypes(thingTypes);
        }

        private async Task<bool?> IsEdgeThingType(DescribeThingTypeResponse thingType)
        {
            var response = await this.amazonIoTClient.ListTagsForResourceAsync(new ListTagsForResourceRequest
            {
                ResourceArn = thingType.ThingTypeArn
            });

            do
            {
                if (response == null || !response.Tags.Any())
                {
                    return null;
                }

                var iotEdgeTag = response.Tags.Where(c => c.Key.Equals("iotEdge", StringComparison.OrdinalIgnoreCase));

                if (!iotEdgeTag.Any())
                {
                    response = await this.amazonIoTClient.ListTagsForResourceAsync(new ListTagsForResourceRequest
                    {
                        ResourceArn = thingType.ThingTypeArn,
                        NextToken = response.NextToken
                    });

                    continue;
                }

                return bool.TryParse(iotEdgeTag.Single().Value, out var result) ? result : null;

            } while (true);
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

            var existingDeviceModel = await this.deviceModelRepository.GetByIdAsync(deviceModel.Id);

            if (existingDeviceModel == null)
            {
                await this.deviceModelRepository.InsertAsync(deviceModel);
                _ = await this.deviceModelImageManager.SetDefaultImageToModel(deviceModel.Id);
            }
            else
            {
                _ = this.mapper.Map(deviceModel, existingDeviceModel);
                this.deviceModelRepository.Update(existingDeviceModel);
            }

            await this.unitOfWork.SaveAsync();
        }

        private async Task DeleteThingTypes(List<DescribeThingTypeResponse> thingTypes)
        {
            // Get all device models that are not in AWS anymore or that are deprecated
            var deviceModelsToDelete = (await this.deviceModelRepository.GetAllAsync())
                .Where(deviceModel => !thingTypes.Any(thingType => deviceModel.Id.Equals(thingType.ThingTypeId, StringComparison.Ordinal)) ||
                    thingTypes.Any(thingType => deviceModel.Id.Equals(thingType.ThingTypeId, StringComparison.Ordinal) && thingType.ThingTypeMetadata.Deprecated))
                .ToList();

            foreach (var deviceModel in deviceModelsToDelete)
            {
                await this.deviceModelImageManager.DeleteDeviceModelImageAsync(deviceModel.Id);
                this.deviceModelRepository.Delete(deviceModel.Id);
                await this.unitOfWork.SaveAsync();
            }

        }

    }
}
