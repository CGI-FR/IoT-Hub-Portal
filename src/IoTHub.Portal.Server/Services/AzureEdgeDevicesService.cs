// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Services
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using IoTHub.Portal.Application.Helpers;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Infrastructure.Helpers;
    using IoTHub.Portal.Infrastructure.Services;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json;
    using Shared.Models.v1._0;

    public class AzureEdgeDevicesService : EdgeDevicesServiceBase, IEdgeDevicesService
    {
        /// <summary>
        /// The device idevice service.
        /// </summary>
        private readonly IExternalDeviceService externalDevicesService;

        private readonly IDeviceTagService deviceTagService;

        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IEdgeDeviceRepository edgeDeviceRepository;
        private readonly IDeviceTagValueRepository deviceTagValueRepository;
        private readonly ILabelRepository labelRepository;
        private readonly IDeviceModelImageManager deviceModelImageManager;
        private readonly IEdgeEnrollementHelper edgeEnrollementHelper;

        private readonly ConfigHandler configHandler;

        public AzureEdgeDevicesService(
            ConfigHandler configHandler,
            IEdgeEnrollementHelper edgeEnrollementHelper,
            IExternalDeviceService externalDevicesService,
            IDeviceTagService deviceTagService,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IEdgeDeviceRepository edgeDeviceRepository,
            IDeviceTagValueRepository deviceTagValueRepository,
            ILabelRepository labelRepository,
            IDeviceModelImageManager deviceModelImageManager)
            : base(deviceTagService, edgeDeviceRepository, mapper, deviceModelImageManager, deviceTagValueRepository, labelRepository)
        {
            this.configHandler = configHandler;
            this.edgeEnrollementHelper = edgeEnrollementHelper;
            this.externalDevicesService = externalDevicesService;
            this.deviceTagService = deviceTagService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.edgeDeviceRepository = edgeDeviceRepository;
            this.deviceTagValueRepository = deviceTagValueRepository;
            this.labelRepository = labelRepository;
            this.deviceModelImageManager = deviceModelImageManager;
        }

        /// <summary>
        /// Create a new edge device.
        /// </summary>
        /// <param name="edgeDevice"> the new edge device.</param>
        /// <returns>the result of the operation.</returns>
        public async Task<IoTEdgeDevice> CreateEdgeDevice(IoTEdgeDevice edgeDevice)
        {
            ArgumentNullException.ThrowIfNull(edgeDevice, nameof(edgeDevice));

            var deviceTwin = new Twin(edgeDevice.DeviceId);

            if (edgeDevice.Tags != null)
            {
                foreach (var tag in edgeDevice.Tags)
                {
                    DeviceHelper.SetTagValue(deviceTwin, tag.Key, tag.Value);
                }
            }

            DeviceHelper.SetTagValue(deviceTwin, nameof(IoTEdgeDevice.ModelId), edgeDevice.ModelId);
            DeviceHelper.SetTagValue(deviceTwin, nameof(IoTEdgeDevice.DeviceName), edgeDevice.DeviceName);

            _ = await this.externalDevicesService.CreateDeviceWithTwin(edgeDevice.DeviceId, true, deviceTwin, DeviceStatus.Enabled);

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

            var device = await this.externalDevicesService.GetDevice(edgeDevice.DeviceId);

            if (Enum.TryParse(edgeDevice.Status, out DeviceStatus status))
            {
                device.Status = status;
            }

            _ = await this.externalDevicesService.UpdateDevice(device);

            var deviceTwin = await this.externalDevicesService.GetDeviceTwin(edgeDevice.DeviceId);

            _ = await this.externalDevicesService.UpdateDeviceTwin(deviceTwin);

            var result = await UpdateEdgeDeviceInDatabase(edgeDevice);

            await this.unitOfWork.SaveAsync();

            return result;
        }


        public async Task DeleteEdgeDeviceAsync(string deviceId)
        {
            await this.externalDevicesService.DeleteDevice(deviceId);

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

            var deviceTwinWithModules = await this.externalDevicesService.GetDeviceTwinWithModule(edgeDeviceId);

            deviceDto.LastDeployment = await this.externalDevicesService.RetrieveLastConfiguration(deviceDto);

            deviceDto.Modules = DeviceHelper.RetrieveModuleList(deviceTwinWithModules);
            deviceDto.RuntimeResponse = DeviceHelper.RetrieveRuntimeResponse(deviceTwinWithModules);

            deviceDto.Status = deviceTwinWithModules.Status?.ToString();

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
            ArgumentNullException.ThrowIfNull(moduleName, nameof(moduleName));

            if (!methodName.Equals("RestartModule", StringComparison.Ordinal))
            {
                return await ExecuteModuleCommand(deviceId, moduleName, methodName);
            }

            var method = new CloudToDeviceMethod(methodName);

            var payload = JsonConvert.SerializeObject(new
            {
                id = moduleName,
                schemaVersion = "1.0"
            });

            _ = method.SetPayloadJson(payload);

            var result = await this.externalDevicesService.ExecuteC2DMethod(deviceId, method);

            return new C2Dresult()
            {
                Payload = result.GetPayloadAsJson(),
                Status = result.Status
            };
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
            ArgumentNullException.ThrowIfNull(deviceId, nameof(deviceId));
            ArgumentNullException.ThrowIfNull(moduleName, nameof(moduleName));
            ArgumentNullException.ThrowIfNull(commandName, nameof(commandName));

            var method = new CloudToDeviceMethod(commandName);
            var payload = "{}";

            _ = method.SetPayloadJson(payload);

            var result = await this.externalDevicesService.ExecuteCustomCommandC2DMethod(deviceId,moduleName, method);

            return new C2Dresult()
            {
                Payload = result.GetPayloadAsJson(),
                Status = result.Status
            };
        }

        public async Task<string> GetEdgeDeviceEnrollementScript(string deviceId, string templateName)
        {
            var template = edgeEnrollementHelper.GetEdgeEnrollementTemplate($"{configHandler.CloudProvider}.{templateName}");

            var device = await GetEdgeDevice(deviceId);

            return await this.externalDevicesService.CreateEnrollementScript(template, device);
        }
    }
}
