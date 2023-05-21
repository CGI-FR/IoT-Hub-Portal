// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Services
{
    using System.Threading.Tasks;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Shared;
    using ResourceAlreadyExistsException = Amazon.IoT.Model.ResourceAlreadyExistsException;
    using ResourceNotFoundException = Amazon.IoT.Model.ResourceNotFoundException;

    public class AwsExternalDeviceService : IExternalDeviceServiceV2
    {
        private readonly IMapper mapper;
        private readonly IAmazonIoT amazonIoTClient;

        public AwsExternalDeviceService(IMapper mapper,
            IAmazonIoT amazonIoTClient)
        {
            this.mapper = mapper;
            this.amazonIoTClient = amazonIoTClient;
        }

        public async Task<ExternalDeviceModelDto> CreateDeviceModel(ExternalDeviceModelDto deviceModel)
        {
            try
            {
                var thingType = this.mapper.Map<CreateThingTypeRequest>(deviceModel);

                var createThingTypeRequest = this.mapper.Map<CreateThingTypeRequest>(thingType);

                var response = await this.amazonIoTClient.CreateThingTypeAsync(createThingTypeRequest);

                deviceModel.Id = response.ThingTypeId;
                return deviceModel;
            }
            catch (ResourceAlreadyExistsException e)
            {
                throw new InternalServerErrorException($"Unable to create the device model {deviceModel.Name}: {e.Message}", e);
            }
        }

        public async Task DeleteDeviceModel(ExternalDeviceModelDto deviceModel)
        {
            try
            {
                var deprecated = new DeprecateThingTypeRequest()
                {
                    ThingTypeName = deviceModel.Name,
                    UndoDeprecate = false
                };

                _ = await this.amazonIoTClient.DeprecateThingTypeAsync(deprecated);
            }
            catch (ResourceNotFoundException e)
            {
                throw new InternalServerErrorException($"Unable to delete the device model {deviceModel.Name}: {e.Message}", e);
            }
        }
    }
}
