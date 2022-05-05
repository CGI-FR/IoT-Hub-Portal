// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
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

        protected ILogger Logger { get; }

        protected DevicesControllerBase(
            ILogger logger,
            IDeviceService devicesService,
            IDeviceTagService deviceTagService,
            IDeviceTwinMapper<TListItem, TModel> deviceTwinMapper,
            IDeviceProvisioningServiceManager deviceProvisioningServiceManager,
            ITableClientFactory tableClientFactory)
        {
            Logger = logger;
            this.devicesService = devicesService;
            this.deviceTagService = deviceTagService;
            this.deviceTwinMapper = deviceTwinMapper;
            this.deviceProvisioningServiceManager = deviceProvisioningServiceManager;
            this.tableClientFactory = tableClientFactory;
        }

        /// <summary>
        /// Gets the device list.
        /// </summary>
        /// <param name="routeName"></param>
        /// <param name="continuationToken"></param>
        /// <param name="searchText"></param>
        /// <param name="searchStatus"></param>
        /// <param name="searchState"></param>
        /// <param name="pageSize"></param>
        public virtual async Task<PaginationResult<TListItem>> GetItems(
            string routeName = null,
            string continuationToken = null,
            string searchText = null,
            bool? searchStatus = null,
            bool? searchState = null,
            int pageSize = 10)
        {
            var searchTags = new Dictionary<string, string>();

            foreach (var tag in this.deviceTagService.GetAllSearchableTagsNames())
            {
                if (Request.Query.TryGetValue($"tag.{tag}", out var searchTag))
                {
                    searchTags.Add(tag, searchTag.Single());
                }
            }

            var result = await this.devicesService.GetAllDevice(
                continuationToken: continuationToken,
                pageSize: pageSize,
                searchStatus: searchStatus,
                searchText: searchText,
                searchState: searchState,
                searchTags: searchTags,
                excludeDeviceType: "LoRa Concentrator");

            string nextPage = null;

            if (!string.IsNullOrEmpty(result.NextPage))
            {
                nextPage = Url.RouteUrl(new UrlRouteContext
                {
                    RouteName = routeName,
                    Values = new
                    {
                        continuationToken = result.NextPage,
                        searchText,
                        searchState,
                        searchStatus,
                        pageSize
                    }
                });

                var tagsFilterBuilder = new StringBuilder();

                foreach (var tag in searchTags)
                {
                    _ = tagsFilterBuilder.Append(CultureInfo.InvariantCulture, $"&tag.{tag.Key}={tag.Value}");
                }
            }

            return new PaginationResult<TListItem>
            {
                Items = result.Items.Select(x => this.deviceTwinMapper.CreateDeviceListItem(x)),
                TotalItems = result.TotalItems,
                NextPage = nextPage
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
            ArgumentNullException.ThrowIfNull(device, nameof(device));

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Create a new Twin from the form's fields.
                var newTwin = new Twin()
                {
                    DeviceId = device.DeviceID
                };

                this.deviceTwinMapper.UpdateTwin(newTwin, device);

                var status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

                var result = await this.devicesService.CreateDeviceWithTwin(device.DeviceID, false, newTwin, status);

                return Ok(result);
            }
            catch (DeviceAlreadyExistsException e)
            {
                Logger?.LogError($"Create device failed -{device.DeviceID}", e);
                return BadRequest(e.Message);
            }
            catch (InvalidOperationException e)
            {
                Logger?.LogError($"Create device failed - {device.DeviceID} -\n{e.Message}");
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Updates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        public virtual async Task<IActionResult> UpdateDeviceAsync(TModel device)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ArgumentNullException.ThrowIfNull(device, nameof(device));

            // Device status (enabled/disabled) has to be dealt with afterwards
            var currentDevice = await this.devicesService.GetDevice(device.DeviceID);
            currentDevice.Status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

            _ = await this.devicesService.UpdateDevice(currentDevice);

            // Get the current twin from the hub, based on the device ID
            var currentTwin = await this.devicesService.GetDeviceTwin(device.DeviceID);

            // Update the twin properties
            this.deviceTwinMapper.UpdateTwin(currentTwin, device);

            _ = await this.devicesService.UpdateDeviceTwin(device.DeviceID, currentTwin);

            return Ok();
        }

        /// <summary>
        /// Deletes the specified device.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        public virtual async Task<IActionResult> Delete(string deviceID)
        {
            await this.devicesService.DeleteDevice(deviceID);

            return Ok();
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
                return NotFound("Device doesn't exist.");
            }

            if (!item.Tags.Contains("modelId"))
            {
                return BadRequest($"Cannot find device type from device {deviceID}");
            }

            Response<TableEntity> response = await this.tableClientFactory.GetDeviceTemplates()
                                            .GetEntityAsync<TableEntity>("0", item.Tags["modelId"].ToString());

            var modelEntity = response.Value;

            var credentials = await this.deviceProvisioningServiceManager.GetEnrollmentCredentialsAsync(deviceID, modelEntity[nameof(DeviceModel.Name)].ToString());

            return Ok(credentials);
        }
    }
}
