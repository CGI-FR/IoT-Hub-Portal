// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.v1._0
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Hellang.Middleware.ProblemDetails;
    using IoTHub.Portal.Application.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.Logging;
    using Shared;
    using Shared.Models.v1._0;

    public abstract class DevicesControllerBase<TDto> : ControllerBase
        where TDto : IDeviceDetails
    {
        private readonly IDeviceService<TDto> deviceService;

        protected ILogger Logger { get; }

        protected DevicesControllerBase(
            ILogger logger,
            IDeviceService<TDto> deviceService)
        {
            Logger = logger;
            this.deviceService = deviceService;
        }

        /// <summary>
        /// Gets the device list.
        /// </summary>
        /// <param name="routeName"></param>
        /// <param name="searchText"></param>
        /// <param name="searchStatus"></param>
        /// <param name="searchState"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="orderBy"></param>
        /// <param name="modelId"></param>
        /// <param name="labels"></param>
        protected async Task<PaginationResult<DeviceListItem>> GetItems(
            string routeName = null,
            string searchText = null,
            bool? searchStatus = null,
            bool? searchState = null,
            int pageSize = 10,
            int pageNumber = 0,
            string[] orderBy = null,
            string modelId = null,
            string[] labels = null)
        {

            var paginatedDevices = await this.deviceService.GetDevices(
                searchText,
                searchStatus,
                searchState,
                pageSize,
                pageNumber,
                orderBy,
                GetTagsFromQueryString(Request.Query),
                modelId,
                labels?.ToList());

            var nextPage = string.Empty;

            if (paginatedDevices.HasNextPage)
            {
                nextPage = Url.RouteUrl(new UrlRouteContext
                {
                    RouteName = routeName,
                    Values = new
                    {
                        searchText,
                        searchStatus,
                        searchState,
                        pageSize,
                        pageNumber = pageNumber + 1,
                        orderBy
                    }
                });
            }

            return new PaginationResult<DeviceListItem>
            {
                Items = paginatedDevices.Data,
                TotalItems = paginatedDevices.TotalCount,
                NextPage = nextPage
            };
        }

        /// <summary>
        /// Gets the specified device.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        public virtual async Task<TDto> GetItem(string deviceID)
        {
            return await this.deviceService.GetDevice(deviceID);
        }

        /// <summary>
        /// Creates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        public virtual async Task<IActionResult> CreateDeviceAsync(TDto device)
        {
            ArgumentNullException.ThrowIfNull(device, nameof(device));

            if (!ModelState.IsValid)
            {
                var validation = new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status422UnprocessableEntity
                };

                throw new ProblemDetailsException(validation);
            }

            _ = await this.deviceService.CreateDevice(device);

            return Ok(device);
        }

        /// <summary>
        /// Updates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        public virtual async Task<IActionResult> UpdateDeviceAsync(TDto device)
        {

            ArgumentNullException.ThrowIfNull(device, nameof(device));

            if (!ModelState.IsValid)
            {
                var validation = new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status422UnprocessableEntity
                };

                throw new ProblemDetailsException(validation);
            }

            _ = await this.deviceService.UpdateDevice(device);

            return Ok();
        }

        /// <summary>
        /// Deletes the specified device.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        public virtual async Task<IActionResult> Delete(string deviceID)
        {
            await this.deviceService.DeleteDevice(deviceID);

            return Ok();
        }

        public virtual Task<IEnumerable<LabelDto>> GetAvailableLabels()
        {
            return this.deviceService.GetAvailableLabels();
        }

        /// <summary>
        /// Returns the device enrollment credentials.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        public virtual async Task<ActionResult<DeviceCredentials>> GetCredentials(string deviceID)
        {
            var device = await this.deviceService.GetDevice(deviceID);

            if (device == null)
            {
                return NotFound();
            }

            return Ok(await this.deviceService.GetCredentials(device));
        }

        private static Dictionary<string, string> GetTagsFromQueryString(IQueryCollection queryCollection)
        {
            return queryCollection
                .Where(pair => pair.Key.StartsWith("tag.", StringComparison.InvariantCulture))
                .Select(pair => new
                {
                    Tag = pair.Key.Split(new[] { "tag." }, StringSplitOptions.None).Skip(1).FirstOrDefault(),
                    Value = pair.Value.ToString()
                })
                .ToDictionary(arg => arg.Tag, arg => arg.Value);
        }
    }
}
