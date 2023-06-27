// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.AWS.Services
{
    using Amazon.GreengrassV2;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using AutoMapper;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Infrastructure.Common.Helpers;
    using IoTHub.Portal.Infrastructure.Common.Services;
    using IoTHub.Portal.Models.v10;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;

    public class EdgeDevicesService : EdgeDevicesServiceBase, IEdgeDevicesService
    {
        /// <summary>
        /// The device idevice service.
        /// </summary>
        private readonly IExternalDeviceService externalDeviceService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IEdgeEnrollementHelper edgeEnrollementHelper;
        private readonly IEdgeDeviceModelRepository deviceModelRepository;
        private readonly IConfigService configService;
        private readonly ConfigHandler configHandler;
        private readonly IMapper mapper;
        private readonly IAmazonIoT amazonIoTClient;
        private readonly IAmazonGreengrassV2 amazonGreengrass;
        private readonly ILogger<EdgeDevicesService> logger;

        public EdgeDevicesService(
            ConfigHandler configHandler,
            IEdgeEnrollementHelper edgeEnrollementHelper,
            IExternalDeviceService externalDeviceService,
            IDeviceTagService deviceTagService,
            IConfigService configService,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IEdgeDeviceRepository edgeDeviceRepository,
            IEdgeDeviceModelRepository deviceModelRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            ILabelRepository labelRepository,
            IDeviceModelImageManager deviceModelImageManager,
            IAmazonIoT amazonIoTClient,
            IAmazonGreengrassV2 amazonGreengrass,
            ILogger<EdgeDevicesService> logger)
            : base(deviceTagService, edgeDeviceRepository, mapper, deviceModelImageManager, deviceTagValueRepository, labelRepository)
        {
            this.configHandler = configHandler;
            this.edgeEnrollementHelper = edgeEnrollementHelper;
            this.externalDeviceService = externalDeviceService;
            this.configService = configService;
            this.deviceModelRepository = deviceModelRepository;

            this.unitOfWork = unitOfWork;

            this.mapper = mapper;

            this.amazonIoTClient = amazonIoTClient;
            this.amazonGreengrass = amazonGreengrass;
            this.logger = logger;
        }

        /// <summary>
        /// Create a new edge device.
        /// </summary>
        /// <param name="edgeDevice"> the new edge device.</param>
        /// <returns>the result of the operation.</returns>
        public async Task<IoTEdgeDeviceDto> CreateEdgeDevice(IoTEdgeDeviceDto edgeDevice)
        {
            ArgumentNullException.ThrowIfNull(edgeDevice, nameof(edgeDevice));

            //Retrieve Device Model
            var model = await deviceModelRepository.GetByIdAsync(edgeDevice.ModelId);
            if (model == null)
            {
                throw new InvalidOperationException($"Edge model '{edgeDevice.ModelId}' doesn't exist!");
            }

            //Create Thing
            var createThingRequest = mapper.Map<CreateThingRequest>(edgeDevice);
            createThingRequest.ThingTypeName = model.Name;

            try
            {
                var thingResponse = await amazonIoTClient.CreateThingAsync(createThingRequest);
                edgeDevice.DeviceId = thingResponse.ThingId;

                //Create EdgeDevice in DB
                var result = await base.CreateEdgeDeviceInDatabase(edgeDevice);
                await unitOfWork.SaveAsync();

                return result;

            }
            catch (AmazonIoTException e)
            {
                throw new InternalServerErrorException($"Unable to create the thing with device name : {edgeDevice.DeviceName} due to an error in the Amazon IoT API.", e);
            }

        }


        /// <summary>
        /// Update the edge device ant the twin.
        /// </summary>
        /// <param name="edgeDevice">edge device object update.</param>
        /// <returns>device twin updated.</returns>
        public async Task<IoTEdgeDeviceDto> UpdateEdgeDevice(IoTEdgeDeviceDto edgeDevice)
        {
            ArgumentNullException.ThrowIfNull(edgeDevice, nameof(edgeDevice));

            try
            {
                //Update Thing
                _ = await amazonIoTClient.UpdateThingAsync(mapper.Map<UpdateThingRequest>(edgeDevice));

                //Update EdgeDevice in DB
                var result = await UpdateEdgeDeviceInDatabase(edgeDevice);
                await unitOfWork.SaveAsync();

                return result;
            }
            catch (AmazonIoTException e)
            {
                throw new InternalServerErrorException($"Unable to update the thing with device name : {edgeDevice.DeviceName} due to an error in the Amazon IoT API.", e);
            }

        }


        public async Task DeleteEdgeDeviceAsync(string deviceId)
        {
            var device = await base.GetEdgeDevice(deviceId);

            try
            {
                await externalDeviceService.RemoveDeviceCredentials(device);
                await externalDeviceService.DeleteDevice(device.DeviceName);
            }
            catch (AmazonIoTException ex)
            {
                logger.LogWarning("Can not delete the edge device because it doesn't exist in AWS IoT", ex);

            }
            finally
            {
                await DeleteEdgeDeviceInDatabase(deviceId);

                await unitOfWork.SaveAsync();
            }

        }

        /// <summary>
        /// Get edge device with its modules.
        /// </summary>
        /// <param name="edgeDeviceId">device id.</param>
        /// <returns>IoTEdgeDevice object.</returns>
#pragma warning disable CS0108 // Un membre masque un membre hérité ; le mot clé new est manquant
        public async Task<IoTEdgeDeviceDto> GetEdgeDevice(string edgeDeviceId)
#pragma warning restore CS0108 // Un membre masque un membre hérité ; le mot clé new est manquant
        {
            var deviceDto = await base.GetEdgeDevice(edgeDeviceId);

            deviceDto.LastDeployment = await externalDeviceService.RetrieveLastConfiguration(deviceDto);
            deviceDto.RuntimeResponse = deviceDto.LastDeployment?.Status;

            var model = await deviceModelRepository.GetByIdAsync(deviceDto.ModelId);

            deviceDto.Modules = await configService.GetConfigModuleList(model.ExternalIdentifier!);
            deviceDto.NbDevices = await this.GetEdgeDeviceNbDevices(deviceDto);
            deviceDto.NbModules = deviceDto.Modules.Count;
            deviceDto.ConnectionState = deviceDto.RuntimeResponse == CoreDeviceStatus.HEALTHY ? "Connected" : "Disconnected";

            return deviceDto;
        }


        /// <summary>
        /// Executes the module method on the IoT Edge device.
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="deviceId"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public async Task<C2DresultDto> ExecuteModuleMethod(string deviceId, string moduleName, string methodName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Execute the custom module command.
        /// </summary>
        /// <param name="deviceId">the device identifier.</param>
        /// <param name="moduleName">the module name.</param>
        /// <param name="commandName">the command name.</param>
        /// <returns></returns>
        public async Task<C2DresultDto> ExecuteModuleCommand(string deviceId, string moduleName, string commandName)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetEdgeDeviceEnrollementScript(string deviceId, string templateName)
        {
            var template = edgeEnrollementHelper.GetEdgeEnrollementTemplate($"{configHandler.CloudProvider}.{templateName}");

            var device = await GetEdgeDevice(deviceId);

            return await externalDeviceService.CreateEnrollementScript(template, device);
        }

        private async Task<int> GetEdgeDeviceNbDevices(IoTEdgeDeviceDto device)
        {
            try
            {
                var listClientDevices = await amazonGreengrass.ListClientDevicesAssociatedWithCoreDeviceAsync(
                new Amazon.GreengrassV2.Model.ListClientDevicesAssociatedWithCoreDeviceRequest
                {
                    CoreDeviceThingName = device.DeviceName
                });

                return listClientDevices.AssociatedClientDevices.Count;
            }
            catch (AmazonGreengrassV2Exception e)
            {
                throw new InternalServerErrorException($"Can not list Client Devices Associated to {device.DeviceName} Core Device due to an error in the Amazon IoT API.", e);
            }
        }

    }
}
