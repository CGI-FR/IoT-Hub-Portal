// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v10.Device;
    using AzureIoTHub.Portal.Shared.Models.v10.DeviceModel;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;

    public abstract class DevicesControllerBase<TListItem, TModel> : ControllerBase
        where TListItem : DeviceListItem
        where TModel : DeviceDetails
    {
        private readonly IDeviceService devicesService;
        private readonly IDeviceTagService deviceTagService;
        private readonly IDeviceTwinMapper<TListItem, TModel> deviceTwinMapper;
        private readonly ITableClientFactory tableClientFactory;
        private readonly IDeviceProvisioningServiceManager deviceProvisioningServiceManager;

        protected ILogger Logger { get; private set; }

        public DevicesControllerBase(
            ILogger logger,
            IDeviceService devicesService,
            IDeviceTagService deviceTagService,
            IDeviceTwinMapper<TListItem, TModel> deviceTwinMapper,
            IDeviceProvisioningServiceManager deviceProvisioningServiceManager,
            ITableClientFactory tableClientFactory)
        {
            this.Logger = logger;
            this.devicesService = devicesService;
            this.deviceTagService = deviceTagService;
            this.deviceTwinMapper = deviceTwinMapper;
            this.deviceProvisioningServiceManager = deviceProvisioningServiceManager;
            this.tableClientFactory = tableClientFactory;
        }

        /// <summary>
        /// Gets the device list.
        /// </summary>
        public virtual async Task<PaginationResult<TListItem>> GetItems(string continuationToken, int pageSize)
        {
            var result = await this.devicesService.GetAllDevice(
                continuationToken: continuationToken,
                pageSize: pageSize,
                excludeDeviceType: "LoRa Concentrator");

            var tagList = this.deviceTagService.GetAllSearchableTagsNames();

            return new PaginationResult<TListItem>
            {
                Items = result.Items.Select(x => this.deviceTwinMapper.CreateDeviceListItem(x, tagList)),
                TotalItems = result.TotalItems,
                NextPage = !string.IsNullOrEmpty(result.NextPage) ? base.Url.ActionLink(nameof(GetItems), values: new
                {
                    continuationToken = result.NextPage,
                    pageSize
                }) : null
            };
        }

        /// <summary>
        /// Gets the specified device.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        public virtual async Task<TModel> GetItem(string deviceID)
        {
            var item = await this.devicesService.GetDeviceTwin(deviceID);
            var tagList = this.deviceTagService.GetAllTagsNames();

            return this.deviceTwinMapper.CreateDeviceDetails(item, tagList);
        }

        /// <summary>
        /// Creates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        public virtual async Task<IActionResult> CreateDeviceAsync(TModel device)
        {
            try
            {
                if (!this.ModelState.IsValid)
                {
                    return this.BadRequest(this.ModelState);
                }

                // Create a new Twin from the form's fields.
                var newTwin = new Twin()
                {
                    DeviceId = device.DeviceID
                };

                this.deviceTwinMapper.UpdateTwin(newTwin, device);

                var status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

                var result = await this.devicesService.CreateDeviceWithTwin(device.DeviceID, false, newTwin, status);

                return this.Ok(result);
            }
            catch (DeviceAlreadyExistsException e)
            {
                this.Logger?.LogError($"Create device failed -{device.DeviceID}", e);
                return this.BadRequest(e.Message);
            }
            catch (InvalidOperationException e)
            {
                this.Logger?.LogError($"Create device failed - {device.DeviceID} -\n{e.Message}");
                return this.BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Updates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        public virtual async Task<IActionResult> UpdateDeviceAsync(TModel device)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Device status (enabled/disabled) has to be dealt with afterwards
            var currentDevice = await this.devicesService.GetDevice(device.DeviceID);
            currentDevice.Status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

            _ = await this.devicesService.UpdateDevice(currentDevice);

            // Get the current twin from the hub, based on the device ID
            var currentTwin = await this.devicesService.GetDeviceTwin(device.DeviceID);

            // Update the twin properties
            this.deviceTwinMapper.UpdateTwin(currentTwin, device);

            _ = await this.devicesService.UpdateDeviceTwin(device.DeviceID, currentTwin);

            return this.Ok();
        }

        /// <summary>
        /// Deletes the specified device.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        public virtual async Task<IActionResult> Delete(string deviceID)
        {
            await this.devicesService.DeleteDevice(deviceID);

            return this.Ok();
        }

        /// <summary>
        /// Returns the device enrollment credentials.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        public virtual async Task<ActionResult<EnrollmentCredentials>> GetCredentials(string deviceID)
        {
            var item = await this.devicesService.GetDeviceTwin(deviceID);

            if (item == null)
            {
                return this.NotFound("Device doesn't exist.");
            }

            if (!item.Tags.Contains("modelId"))
            {
                return this.BadRequest($"Cannot find device type from device {deviceID}");
            }

            Response<TableEntity> response = await this.tableClientFactory.GetDeviceTemplates()
                                            .GetEntityAsync<TableEntity>("0", item.Tags["modelId"].ToString());

            var modelEntity = response.Value;

            var credentials = await this.deviceProvisioningServiceManager.GetEnrollmentCredentialsAsync(deviceID, modelEntity[nameof(DeviceModel.Name)].ToString());

            return this.Ok(credentials);
        }
    }
}
