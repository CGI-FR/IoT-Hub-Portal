// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Services.AWS
{
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Application.Services.AWS;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities.AWS;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Models.v10.AWS;
    using Microsoft.AspNetCore.Http;

    public class ThingTypeService : IThingTypeService
    {
        private readonly IThingTypeRepository thingTypeRepository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAmazonIoT amazonIoTClient;
        private readonly IDeviceModelImageManager thingTypeImageManager;


        public ThingTypeService(
            IAmazonIoT amazonIoTClient,
            IThingTypeRepository thingTypeRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IDeviceModelImageManager thingTypeImageManager

        )
        {
            this.thingTypeRepository = thingTypeRepository;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.amazonIoTClient = amazonIoTClient;
            this.thingTypeImageManager = thingTypeImageManager;
        }

        public async Task<ThingTypeDto> CreateThingType(ThingTypeDto thingType)
        {
            ArgumentNullException.ThrowIfNull(thingType, nameof(thingType));

            var createThingTypeRequest = this.mapper.Map<CreateThingTypeRequest>(thingType);

            var response = await this.amazonIoTClient.CreateThingTypeAsync(createThingTypeRequest);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalServerErrorException("The creation of the thing type failed due to an error in the Amazon IoT API.");
            }
            else
            {
                thingType.ThingTypeID = response.ThingTypeId;
                return await CreateThingTypeInDatabase(thingType);
            }
        }
        private async Task<ThingTypeDto> CreateThingTypeInDatabase(ThingTypeDto thingType)
        {
            var thingTypeEntity = this.mapper.Map<ThingType>(thingType);

            await this.thingTypeRepository.InsertAsync(thingTypeEntity);
            await this.unitOfWork.SaveAsync();

            _ = this.thingTypeImageManager.SetDefaultImageToModel(thingType.ThingTypeID);

            return thingType;
        }

        public Task<string> GetDeviceModelAvatar(string deviceModelId)
        {
            return Task.Run(() => this.thingTypeImageManager.ComputeImageUrl(deviceModelId).ToString());
        }

        public Task<string> UpdateDeviceModelAvatar(string deviceModelId, IFormFile file)
        {
            return Task.Run(() => this.thingTypeImageManager.ChangeDeviceModelImageAsync(deviceModelId, file.OpenReadStream()));
        }

        public Task DeleteDeviceModelAvatar(string deviceModelId)
        {
            return this.thingTypeImageManager.DeleteDeviceModelImageAsync(deviceModelId);
        }
    }
}
