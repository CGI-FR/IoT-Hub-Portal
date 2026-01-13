// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Jobs.AWS
{
    [DisallowConcurrentExecution]
    public class SyncThingTypesJob : IJob
    {

        private readonly ILogger<SyncThingTypesJob> logger;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IDeviceModelRepository deviceModelRepository;
        private readonly IAmazonIoT amazonIoTClient;
        private readonly IDeviceModelImageManager deviceModelImageManager;
        private readonly IExternalDeviceService externalDeviceService;

        public SyncThingTypesJob(
            ILogger<SyncThingTypesJob> logger,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IDeviceModelRepository deviceModelRepository,
            IAmazonIoT amazonIoTClient,
            IDeviceModelImageManager awsImageManager,
            IExternalDeviceService externalDeviceService)
        {
            this.deviceModelImageManager = awsImageManager;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.deviceModelRepository = deviceModelRepository;
            this.amazonIoTClient = amazonIoTClient;
            this.externalDeviceService = externalDeviceService;
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
                var isEdge = await externalDeviceService.IsEdgeDeviceModel(this.mapper.Map<ExternalDeviceModelDto>(thingType));

                // Cannot know if the thing type was created for an iotEdge or not, so skipping...
                if (!isEdge.HasValue)
                {
                    continue;
                }

                if (isEdge == false)
                {
                    await CreateOrUpdateDeviceModel(thingType);
                }
            }

            // Delete in Database AWS deleted thing types
            await DeleteThingTypes(thingTypes);
        }


        private async Task<List<DescribeThingTypeResponse>> Remove5MnDeprecatedThingTypes(List<DescribeThingTypeResponse> thingTypes)
        {
            var awsThingTypes = new List< DescribeThingTypeResponse>();

            foreach (var thingType in thingTypes)
            {
                var diffInMinutes = DateTime.Now.Subtract(thingType.ThingTypeMetadata.DeprecationDate).TotalMinutes;
                if (thingType.ThingTypeMetadata.Deprecated && diffInMinutes > 5)
                {
                    try
                    {
                        _ = await this.amazonIoTClient.DeleteThingTypeAsync(new DeleteThingTypeRequest
                        {
                            ThingTypeName = thingType.ThingTypeName
                        });

                        _ = await this.amazonIoTClient.DeleteDynamicThingGroupAsync(new DeleteDynamicThingGroupRequest
                        {
                            ThingGroupName = thingType.ThingTypeName
                        });
                    }
                    catch (AmazonIoTException e)
                    {
                        throw new InternalServerErrorException("Unable to Delete The thing type due to an error in the Amazon IoT API.", e);
                    }
                }
                else
                {
                    awsThingTypes.Add(thingType);
                }
            }

            return awsThingTypes;
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

                try
                {
                    var response = await amazonIoTClient.ListThingTypesAsync(request);

                    foreach (var thingType in response.ThingTypes)
                    {
                        var requestDescribeThingType = new DescribeThingTypeRequest
                        {
                            ThingTypeName = thingType.ThingTypeName,
                        };

                        try
                        {
                            thingTypes.Add(await this.amazonIoTClient.DescribeThingTypeAsync(requestDescribeThingType));
                        }
                        catch (AmazonIoTException e)
                        {
                            throw new InternalServerErrorException("Unable to Describe The thing type due to an error in the Amazon IoT API.", e);
                        }
                    }

                    nextToken = response.NextToken;
                }
                catch (AmazonIoTException e)
                {
                    throw new InternalServerErrorException("Unable to list Thing types due to an error in the Amazon IoT API.", e);
                }
            }
            while (!string.IsNullOrEmpty(nextToken));

            thingTypes = await Remove5MnDeprecatedThingTypes(thingTypes);

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
