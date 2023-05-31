// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Threading.Tasks;
    using Amazon.IoT.Model;
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Application.Services.AWS;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Infrastructure.Helpers;
    using AzureIoTHub.Portal.Models.v10;

    public class AWSEdgeDevicesService : EdgeDevicesServiceBase, IEdgeDevicesService
    {
        /// <summary>
        /// The device idevice service.
        /// </summary>
        private readonly IAWSExternalDeviceService awsExternalDevicesService;
        private readonly IExternalDeviceService externalDeviceService;

        private readonly IUnitOfWork unitOfWork;
        private readonly IEdgeDeviceRepository edgeDeviceRepository;
        private readonly IEdgeEnrollementHelper edgeEnrollementHelper;
        private readonly IEdgeDeviceModelRepository deviceModelRepository;

        private readonly ConfigHandler configHandler;

        public AWSEdgeDevicesService(
            ConfigHandler configHandler,
            IEdgeEnrollementHelper edgeEnrollementHelper,
            IExternalDeviceService externalDeviceService,
            IAWSExternalDeviceService awsExternalDevicesService,
            IDeviceTagService deviceTagService,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IEdgeDeviceRepository edgeDeviceRepository,
            IEdgeDeviceModelRepository deviceModelRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            ILabelRepository labelRepository,
            IDeviceModelImageManager deviceModelImageManager)
            : base(deviceTagService, edgeDeviceRepository, mapper, deviceModelImageManager, deviceTagValueRepository, labelRepository)
        {
            this.configHandler = configHandler;
            this.edgeEnrollementHelper = edgeEnrollementHelper;
            this.awsExternalDevicesService = awsExternalDevicesService;
            this.externalDeviceService = externalDeviceService;
            this.deviceModelRepository = deviceModelRepository;

            this.unitOfWork = unitOfWork;
            this.edgeDeviceRepository = edgeDeviceRepository;
        }

        /// <summary>
        /// Create a new edge device.
        /// </summary>
        /// <param name="edgeDevice"> the new edge device.</param>
        /// <returns>the result of the operation.</returns>
        public async Task<IoTEdgeDevice> CreateEdgeDevice(IoTEdgeDevice edgeDevice)
        {
            ArgumentNullException.ThrowIfNull(edgeDevice, nameof(edgeDevice));

            var model = await this.deviceModelRepository.GetByIdAsync(edgeDevice.ModelId);

            if (model == null)
            {
                throw new InvalidOperationException($"Edge model '{edgeDevice.ModelId}' doesn't exist!");
            }

            var response = await this.awsExternalDevicesService.CreateDevice(new CreateThingRequest
            {
                ThingName = edgeDevice.DeviceName,
                ThingTypeName = model.Name
            });

            edgeDevice.DeviceId = response.ThingId;
            var result = await base.CreateEdgeDeviceInDatabase(edgeDevice);

            await this.unitOfWork.SaveAsync();

            return result;
        }


        /// <summary>
        /// Update the edge device ant the twin.
        /// </summary>
        /// <param name="edgeDevice">edge device object update.</param>
        /// <returns>device twin updated.</returns>
        public async Task<IoTEdgeDevice> UpdateEdgeDevice(IoTEdgeDevice edgeDevice)
        {
            ArgumentNullException.ThrowIfNull(edgeDevice, nameof(edgeDevice));

            _ = await this.awsExternalDevicesService.GetDevice(edgeDevice.DeviceId);

            // TODO
            var result = await UpdateEdgeDeviceInDatabase(edgeDevice);

            await this.unitOfWork.SaveAsync();

            return result;
        }


        public async Task DeleteEdgeDeviceAsync(string deviceId)
        {
            var device = await base.GetEdgeDevice(deviceId);

            await this.externalDeviceService.RemoveDeviceCredentials(device);
            await this.externalDeviceService.DeleteDevice(device.DeviceName);

            await DeleteEdgeDeviceInDatabase(deviceId);

            await this.unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Get edge device with its modules.
        /// </summary>
        /// <param name="edgeDeviceId">device id.</param>
        /// <returns>IoTEdgeDevice object.</returns>
        public async Task<IoTEdgeDevice> GetEdgeDevice(string edgeDeviceId)
        {
            var deviceDto = await base.GetEdgeDevice(edgeDeviceId);

            // TODO

            return deviceDto;
        }


        /// <summary>
        /// Executes the module method on the IoT Edge device.
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="deviceId"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public async Task<C2Dresult> ExecuteModuleMethod(string deviceId, string moduleName, string methodName)
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
        public async Task<C2Dresult> ExecuteModuleCommand(string deviceId, string moduleName, string commandName)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetEdgeDeviceEnrollementScript(string deviceId, string templateName)
        {
            var template = edgeEnrollementHelper.GetEdgeEnrollementTemplate($"{configHandler.CloudProvider}.{templateName}");
            var device = await this.edgeDeviceRepository.GetByIdAsync(deviceId);

            return await this.externalDeviceService.CreateEnrollementScript(template, device);
        }
    }
}
