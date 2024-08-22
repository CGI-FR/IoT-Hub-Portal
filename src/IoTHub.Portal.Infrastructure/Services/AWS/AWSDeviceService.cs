// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Services.AWS
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
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
    using Shared.Models.v1._0;
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
            try
            {
                var thingResponse = await this.amazonIoTClient.CreateThingAsync(this.mapper.Map<CreateThingRequest>(device));
                device.DeviceID = thingResponse.ThingId;

                try
                {
                    //Create Thing Shadow
                    var shadowResponse = await this.amazonIotDataClient.UpdateThingShadowAsync(this.mapper.Map<UpdateThingShadowRequest>(device));

                    //Create Thing in DB
                    return await CreateDeviceInDatabase(device);
                }
                catch (AmazonIotDataException e)
                {
                    throw new InternalServerErrorException($"Unable to create/update the thing shadow with device name : {device.DeviceName} due to an error in the Amazon IoT API.", e);
                }

            }
            catch (AmazonIoTException e)
            {
                throw new InternalServerErrorException($"Unable to create the thing with device name : {device.DeviceName} due to an error in the Amazon IoT API.", e);

            }

        }

        public override async Task<DeviceDetails> UpdateDevice(DeviceDetails device)
        {
            try
            {
                //Update Thing
                var thingResponse = await this.amazonIoTClient.UpdateThingAsync(this.mapper.Map<UpdateThingRequest>(device));

                //Update Thing in DB
                return await UpdateDeviceInDatabase(device);
            }
            catch (AmazonIoTException e)
            {
                throw new InternalServerErrorException($"Unable to update the thing with device name : {device.DeviceName} due to an error in the Amazon IoT API.", e);
            }

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
                try
                {
                    //Retrieve all thing principals and detach it before deleting the thing
                    var principals = await this.amazonIoTClient.ListThingPrincipalsAsync(new ListThingPrincipalsRequest
                    {
                        NextToken = string.Empty,
                        ThingName = device.Name
                    });

                    try
                    {
                        foreach (var principal in principals.Principals)
                        {
                            _ = await this.amazonIoTClient.DetachThingPrincipalAsync(new DetachThingPrincipalRequest
                            {
                                Principal = principal,
                                ThingName = device.Name
                            });
                        }
                    }
                    catch (AmazonIoTException e)
                    {
                        this.logger.LogWarning(e, "Can not detach Thing principal because it doesn't exist in AWS IoT");
                    }

                    try
                    {
                        //Delete the thing type after detaching the principal
                        _ = await this.amazonIoTClient.DeleteThingAsync(this.mapper.Map<DeleteThingRequest>(device));
                    }
                    catch (AmazonIoTException e)
                    {
                        this.logger.LogWarning(e, "Can not delete the thing because it doesn't exist in AWS IoT");
                    }

                }
                catch (AmazonIoTException e)
                {
                    this.logger.LogWarning(e, "Can not retreive Thing  because it doesn't exist in AWS IoT");
                }

            }
            catch (AmazonIoTException e)
            {
                this.logger.LogWarning(e, "Can not delete the device because it doesn't exist in AWS IoT");

            }
            finally
            {
                //Delete Thing in DB
                await DeleteDeviceInDatabase(deviceId);
            }

        }
    }
}
