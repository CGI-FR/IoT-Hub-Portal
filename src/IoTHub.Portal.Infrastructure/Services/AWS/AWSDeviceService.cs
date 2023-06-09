// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Services.AWS
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using Models.v10;
    using AutoMapper;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Domain;
    using Amazon.IoT.Model;
    using Amazon.IotData.Model;
    using IoTHub.Portal.Application.Managers;
    using Infrastructure;
    using Microsoft.Extensions.Logging;
    using IoTHub.Portal.Domain.Exceptions;
    using Amazon.IoT;
    using Amazon.IotData;
    using ResourceNotFoundException = Domain.Exceptions.ResourceNotFoundException;

    public class AWSDeviceService : DeviceService
    {
        private readonly IMapper mapper;
        private readonly IDeviceRepository deviceRepository;
        private readonly IAmazonIoT amazonIoTClient;
        private readonly IAmazonIotData amazonIotDataClient;
        private readonly ILogger<AWSDeviceService> logger;


        public AWSDeviceService(PortalDbContext portalDbContext,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IDeviceRepository deviceRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            ILabelRepository labelRepository,
            IDeviceTagService deviceTagService,
            IDeviceModelImageManager deviceModelImageManager,
            IAmazonIoT amazonIoTClient,
            IAmazonIotData amazonIotDataClient,
            IExternalDeviceService externalDeviceService,
            ILogger<AWSDeviceService> logger)
            : base(mapper, unitOfWork, deviceRepository, deviceTagValueRepository, labelRepository, externalDeviceService, deviceTagService, deviceModelImageManager, null!, portalDbContext, logger)
        {
            this.mapper = mapper;
            this.deviceRepository = deviceRepository;
            this.amazonIoTClient = amazonIoTClient;
            this.amazonIotDataClient = amazonIotDataClient;
            this.logger = logger;
        }

        public override async Task<DeviceDetails> CreateDevice(DeviceDetails device)
        {
            //Create Thing
            var thingResponse = await this.amazonIoTClient.CreateThingAsync(this.mapper.Map<CreateThingRequest>(device));
            if (thingResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalServerErrorException($"Unable to create the thing with device name : {device.DeviceName} due to an error in the Amazon IoT API : {thingResponse.HttpStatusCode}");
            }
            device.DeviceID = thingResponse.ThingId;

            //Create Thing Shadow
            var shadowResponse = await this.amazonIotDataClient.UpdateThingShadowAsync(this.mapper.Map<UpdateThingShadowRequest>(device));
            if (shadowResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalServerErrorException($"Unable to create/update the thing shadow with device name : {device.DeviceName} due to an error in the Amazon IoT API : {shadowResponse.HttpStatusCode}");
            }

            //Create Thing in DB
            return await CreateDeviceInDatabase(device);
        }

        public override async Task<DeviceDetails> UpdateDevice(DeviceDetails device)
        {
            //Update Thing
            var thingResponse = await this.amazonIoTClient.UpdateThingAsync(this.mapper.Map<UpdateThingRequest>(device));
            if (thingResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InternalServerErrorException($"Unable to update the thing with device name : {device.DeviceName} due to an error in the Amazon IoT API : {thingResponse.HttpStatusCode}");
            }

            //Update Thing in DB
            return await UpdateDeviceInDatabase(device);
        }

        public override async Task DeleteDevice(string deviceId)
        {
            //Get device in DB
            var device = await deviceRepository.GetByIdAsync(deviceId);

            if (device == null)
            {
                throw new ResourceNotFoundException($"The device with id {deviceId} doesn't exist");
            }

            try
            {
                //Retrieve all thing principals and detach it before deleting the thing
                var principals = await this.amazonIoTClient.ListThingPrincipalsAsync(new ListThingPrincipalsRequest
                {
                    NextToken = string.Empty,
                    ThingName = device.Name
                });

                if (principals.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new InternalServerErrorException($"Unable to retreive Thing {device.Name} principals due to an error in the Amazon IoT API : {principals.HttpStatusCode}");
                }

                foreach (var principal in principals.Principals)
                {
                    var detachPrincipal = await this.amazonIoTClient.DetachThingPrincipalAsync(new DetachThingPrincipalRequest
                    {
                        Principal = principal,
                        ThingName = device.Name
                    });

                    if (detachPrincipal.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new InternalServerErrorException($"Unable to detach Thing {device.Name} principal due to an error in the Amazon IoT API : {detachPrincipal.HttpStatusCode}");
                    }
                }

                //Delete the thing type after detaching the principal
                var deleteResponse = await this.amazonIoTClient.DeleteThingAsync(this.mapper.Map<DeleteThingRequest>(device));
                if (deleteResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new InternalServerErrorException($"Unable to delete the thing with device name : {device.Name} due to an error in the Amazon IoT API : {deleteResponse.HttpStatusCode}");
                }
            }
            catch (AmazonIoTException e)
            {
                this.logger.LogWarning($"Can not the device {deviceId} because it doesn't exist in AWS IoT", e);

            }
            finally
            {
                //Delete Thing in DB
                await DeleteDeviceInDatabase(deviceId);
            }

        }
    }
}
